import json
from openai import OpenAI
from src.config.settings import settings
import subprocess
import os

openai_client = OpenAI(api_key=settings.OPENAI_API_KEY) if settings.OPENAI_API_KEY else None

def transcribe_audio(file_path):
    if not openai_client:
        return "AVISO: No hay OPENAI_API_KEY. Usando transcripción simulada."
        
    normalized_path = file_path + "_normalized.mp3"
    try:
        # Convertir a MP3 usando ffmpeg (ignora la extensión original que podría estar mal)
        subprocess.run(["ffmpeg", "-y", "-i", file_path, normalized_path], stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)
        
        with open(normalized_path, "rb") as audio_file:
            transcription = openai_client.audio.transcriptions.create(
                model="whisper-1", 
                file=audio_file
            )
        return transcription.text
    finally:
        if os.path.exists(normalized_path):
            os.remove(normalized_path)

def generate_advice(transcribed_text, patient_context, nota):
    if not openai_client:
        return "Simulación: Respira profundo, todo saldrá bien.", False

    # Extraer valores seguros del diccionario de contexto (evitando PII)
    edad = patient_context.get("AgeRange", "No especificada")
    ocupacion = patient_context.get("Occupation", "No especificada")
    relacion = patient_context.get("RelationshipStatus", "No especificada")
    vivienda = patient_context.get("LivingSituation", "No especificada")
    meta = patient_context.get("PrimaryGoal", "Mejorar bienestar emocional")
    terapia_previa = "Sí" if patient_context.get("HasPreviousTherapy") else "No"
    estado = patient_context.get("State", "Desconocido")

    system_prompt = f"""Eres un asistente de apoyo emocional de primera línea. El paciente acaba de grabar un diario de voz (o texto). Tu objetivo es brindarle un consejo breve, puntual y accionable (máximo 2 párrafos cortos). NO eres su psicólogo, eres una IA de apoyo entre sesiones.
IMPORTANTE: Tu respuesta será convertida a audio y reproducida al paciente, así que debes hablar con un tono muy cálido, empático, conversacional y natural. Evita formatos raros, listas o enumeraciones.

Contexto Clínico del Paciente (Usa esto sutilmente para empatizar mejor):
- Edad: {edad}
- Ocupación: {ocupacion}
- Estado Civil: {relacion}
- Situación de Vivienda: {vivienda}
- Terapia Previa: {terapia_previa}
- Meta Principal del Tratamiento: {meta}
- Estado del Tratamiento: {estado}
- Nota reciente del Psicólogo: {nota}

Debes responder ÚNICAMENTE con un JSON válido usando esta estructura:
{{
  "consejo_texto": "Mensaje hablado para el paciente...",
  "is_emergency": false (true si hay ideación suicida, violencia inminente o crisis severa)
}}
"""
    response = openai_client.chat.completions.create(
        model="gpt-4o-mini",
        messages=[
            {"role": "system", "content": system_prompt},
            {"role": "user", "content": transcribed_text}
        ],
        response_format={ "type": "json_object" }
    )
    
    llm_output = json.loads(response.choices[0].message.content)
    consejo = llm_output.get("consejo_texto", "Gracias por compartir.")
    is_emergency = llm_output.get("is_emergency", False)
    
    return consejo, is_emergency

def generate_tts_audio(text, output_path):
    if not openai_client:
        with open(output_path, 'w') as f:
            f.write("Audio fake")
        return

    tts_response = openai_client.audio.speech.create(
        model="tts-1",
        voice="alloy",
        input=text
    )
    with open(output_path, 'wb') as f:
        for chunk in tts_response.iter_bytes():
            f.write(chunk)
