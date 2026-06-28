import os
import json
import time

from config import SQS_QUEUE_URL
from aws_service import receive_messages, delete_message, download_audio, upload_audio
from db_service import get_patient_context, update_journal_entry
from ai_service import transcribe_audio, generate_advice, generate_tts_audio

def process_message(message_body):
    print(f"Procesando mensaje: {message_body}")
    data = json.loads(message_body)
    entry_id = data.get("EntryId")
    s3_key = data.get("S3Key")
    
    if not entry_id or not s3_key:
        print("Mensaje inválido. Faltan datos.")
        return

    # 1. Descargar audio de S3
    print(f"Descargando {s3_key} de S3...")
    local_audio_path = f"/tmp/{entry_id}_input.m4a"
    download_audio(s3_key, local_audio_path)

    # 2. Whisper (STT)
    print("Transcribiendo audio...")
    transcribed_text = transcribe_audio(local_audio_path)
    print(f"Transcripción: {transcribed_text}")

    # 3. Obtener contexto de BD
    print("Obteniendo contexto del paciente...")
    paciente, estado, nota = get_patient_context(entry_id)

    # 4. LLM (Dynamic Prompt)
    print("Generando consejo con GPT-4o-mini...")
    consejo, is_emergency = generate_advice(transcribed_text, paciente, estado, nota)
    print(f"Respuesta IA: {consejo} | Emergencia: {is_emergency}")

    # 5. TTS (Audio de respuesta)
    print("Generando audio de respuesta (TTS)...")
    local_reply_path = f"/tmp/{entry_id}_reply.mp3"
    generate_tts_audio(consejo, local_reply_path)

    # 6. Subir respuesta a S3
    print("Subiendo respuesta a S3...")
    reply_s3_key = f"respuestas/reply_{entry_id}.mp3"
    upload_audio(local_reply_path, reply_s3_key)

    # 7. Actualizar Base de Datos
    print("Actualizando BD...")
    update_journal_entry(entry_id, reply_s3_key, consejo, transcribed_text, is_emergency)
    
    # Limpiar temp
    if os.path.exists(local_audio_path): os.remove(local_audio_path)
    if os.path.exists(local_reply_path): os.remove(local_reply_path)
    
    print("Proceso completado exitosamente.")

def main():
    print("Iniciando Worker de Journal...")
    print(f"Escuchando en SQS: {SQS_QUEUE_URL}")
    while True:
        try:
            messages = receive_messages()
            if not messages:
                continue
                
            for message in messages:
                process_message(message['Body'])
                delete_message(message['ReceiptHandle'])
                print("Mensaje borrado de la cola.")
                
        except Exception as e:
            print(f"Error en el worker: {e}")
            time.sleep(5)

if __name__ == "__main__":
    main()
