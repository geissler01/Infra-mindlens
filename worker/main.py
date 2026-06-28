import os
import json
import time
import boto3
import psycopg2

# Configuración desde Variables de Entorno
SQS_QUEUE_URL = os.getenv('SQS_QUEUE_URL', '')
DB_HOST = os.getenv('DB_HOST', 'localhost')
DB_PASS = os.getenv('DB_PASS', 'postgres')

# Inicializar clientes
sqs = boto3.client('sqs', region_name='us-east-1')

def get_db_connection():
    return psycopg2.connect(
        host=DB_HOST,
        database="journaldb",
        user="postgres",
        password=DB_PASS
    )

def process_message(message_body):
    print(f"Procesando mensaje: {message_body}")
    data = json.loads(message_body)
    entry_id = data.get("EntryId")
    texto = data.get("Texto")
    
    # SIMULAR PROCESAMIENTO IA (Worker asíncrono)
    print("Iniciando análisis con 'IA'...")
    time.sleep(5)  # Espera 5 segundos
    respuesta = f"IA Simulada: He analizado tu texto ('{texto}') y todo estará bien."
    
    # Actualizar RDS
    print(f"Actualizando BD para el EntryId {entry_id}...")
    conn = get_db_connection()
    cur = conn.cursor()
    cur.execute(
        "UPDATE \"JournalEntries\" SET \"Status\" = %s, \"Respuesta\" = %s WHERE \"Id\" = %s",
        ('completed', respuesta, entry_id)
    )
    conn.commit()
    cur.close()
    conn.close()
    print("BD Actualizada exitosamente.")

def main():
    print("Iniciando Worker de Journal...")
    print(f"Escuchando en SQS: {SQS_QUEUE_URL}")
    while True:
        try:
            # Long Polling de SQS (espera hasta 20 segundos por un mensaje sin gastar CPU)
            response = sqs.receive_message(
                QueueUrl=SQS_QUEUE_URL,
                MaxNumberOfMessages=1,
                WaitTimeSeconds=20
            )
            
            messages = response.get('Messages', [])
            if not messages:
                continue
                
            for message in messages:
                # 1. Procesar
                process_message(message['Body'])
                
                # 2. Borrar mensaje de la cola (¡Muy importante en SQS!)
                sqs.delete_message(
                    QueueUrl=SQS_QUEUE_URL,
                    ReceiptHandle=message['ReceiptHandle']
                )
                print("Mensaje borrado de la cola.")
                
        except Exception as e:
            print(f"Error en el worker: {e}")
            time.sleep(5) # Evitar loops infinitos rápidos si hay error de conexión

if __name__ == "__main__":
    main()
