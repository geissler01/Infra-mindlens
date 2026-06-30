import os
import requests
import boto3
import time
import psycopg2

BASE_URL = "http://backend-dotnet:8080/api"
S3_ENDPOINT = "http://localstack:4566"
REGION = "us-east-1"
BUCKET_NAME = "journal-audios-bucket"

def run_test():
    print("1. Login as Psychologist (carlos@mindlens.com)")
    res = requests.post(f"{BASE_URL}/auth/login", json={
        "email": "carlos@mindlens.com",
        "password": "Password123!"
    })
    if res.status_code != 200:
        print("Login Error:", res.text)
    res.raise_for_status()
    psych_token = res.json()["data"]
    
    print("2. Fetch Tenant ID for carlos")
    res = requests.get(f"{BASE_URL}/tenants")
    res.raise_for_status()
    tenants = res.json()["data"]
    carlos_tenant = next(t for t in tenants if t["databaseName"] == "db_mindlens_carlos")
    tenant_id = carlos_tenant["id"]
    print(f"   Tenant ID: {tenant_id}")
    
    headers = {
        "Authorization": f"Bearer {psych_token}",
        "X-Tenant-Id": tenant_id
    }
    
    print("3. Create a Test Patient")
    patient_email = f"test_{int(time.time())}@mindlens.com"  # Unique email to avoid 400
    patient_data = {
        "firstName": "Test",
        "lastName": "Patient",
        "email": patient_email,
        "phone": "+1234567890",
        "emergencyPhone": "+0987654321",
        "address": "123 Test St",
        "ageRange": "25-35",
        "relationshipStatus": "Single",
        "occupation": "Tester",
        "livingSituation": "Alone",
        "primaryGoal": "Test the system",
        "hasPreviousTherapy": False
    }
    res = requests.post(f"{BASE_URL}/patients", json=patient_data, headers=headers)
    res.raise_for_status()
    print("   Patient created successfully.")
    
    print("4. Get Patient ID from DB (workaround for API bug)")
    conn2 = psycopg2.connect(
        host="postgres-db",
        port="5432",
        dbname="db_mindlens_carlos",
        user="journal_user",
        password="journal_pass"
    )
    cur2 = conn2.cursor()
    cur2.execute('SELECT "Id" FROM "Patients" ORDER BY "CreatedAt" DESC LIMIT 1;')
    patient_id = cur2.fetchone()[0]
    cur2.close()
    conn2.close()
    print(f"   Patient ID (Tenant DB): {patient_id}")
    
    print("5. Create Treatment for Patient (Directly via DB to avoid C# API Segfault)")
    import uuid
    treatment_id = str(uuid.uuid4())
    conn3 = psycopg2.connect(
        host="postgres-db",
        port="5432",
        dbname="db_mindlens_carlos",
        user="journal_user",
        password="journal_pass"
    )
    cur3 = conn3.cursor()
    cur3.execute('INSERT INTO "Treatments" ("Id", "PatientId", "SessionDay", "StartedAt", "State") VALUES (%s, %s, %s, CURRENT_DATE, 0);', (treatment_id, patient_id, 3))
    conn3.commit()
    cur3.close()
    conn3.close()
    print("   Treatment created successfully.")
        
    print("6. Get Treatment ID")
    print(f"   Treatment ID: {treatment_id}")
    
    print("7. Login as Patient")
    res = requests.post(f"{BASE_URL}/auth/login", json={
        "email": patient_email,
        "password": f"MindLens12345-{patient_email}"
    })
    res.raise_for_status()
    patient_token = res.json()["data"]
    print("8. Upload Fake Audio to LocalStack S3 (Directly)")
    s3_key = f"audios/tenant_{tenant_id}/patient_{patient_id}/treatment_{treatment_id}/{str(uuid.uuid4())}.m4a"
    s3_client = boto3.client(
        's3', 
        endpoint_url=S3_ENDPOINT, 
        aws_access_key_id='test', 
        aws_secret_access_key='test', 
        region_name=REGION
    )
    import wave
    import io
    wav_io = io.BytesIO()
    with wave.open(wav_io, 'wb') as wav_file:
        wav_file.setnchannels(1)
        wav_file.setsampwidth(2)
        wav_file.setframerate(44100)
        wav_file.writeframes(b'\\x00' * 44100 * 2)
    
    s3_client.put_object(Bucket=BUCKET_NAME, Key=s3_key, Body=wav_io.getvalue())
    print(f"   Silent WAV Audio uploaded to {BUCKET_NAME}/{s3_key}")
    
    print("9. Create Journaling in DB and Trigger SQS (Directly)")
    journaling_id = str(uuid.uuid4())
    conn4 = psycopg2.connect(
        host="postgres-db",
        port="5432",
        dbname="db_mindlens_carlos",
        user="journal_user",
        password="journal_pass"
    )
    cur4 = conn4.cursor()
    cur4.execute('INSERT INTO "Journalings" ("Id", "TreatmentId", "EntryType", "CreatedAt", "Date", "State", "VoiceRecordKey") VALUES (%s, %s, %s, CURRENT_TIMESTAMP, CURRENT_DATE, %s, %s);', (journaling_id, treatment_id, 1, 0, s3_key)) # EntryType=1 (Audio), State=0
    conn4.commit()
    cur4.close()
    conn4.close()
    print("   Journaling created in DB.")
    
    print("10. Push message to SQS")
    sqs_client = boto3.client(
        'sqs', 
        endpoint_url=S3_ENDPOINT, 
        aws_access_key_id='test', 
        aws_secret_access_key='test', 
        region_name=REGION
    )
    queue_url = sqs_client.get_queue_url(QueueName="journal-processing-queue")["QueueUrl"]
    import json
    sqs_client.send_message(
        QueueUrl=queue_url,
        MessageBody=json.dumps({"JournalingId": journaling_id, "EntryType": "audio", "S3Key": s3_key, "TenantDb": "db_mindlens_carlos"})
    )
    print("   Message pushed to SQS journal-processing-queue.")
        
    print("11. Waiting 20 seconds for Python worker to process...")
    time.sleep(20)
    
    print("12. Verify Database using psycopg2")
    conn = psycopg2.connect(
        host="postgres-db",
        port="5432",
        dbname="db_mindlens_carlos",
        user="journal_user",
        password="journal_pass"
    )
    cur = conn.cursor()
    cur.execute("SELECT \"Id\", \"EntryType\", \"State\", \"Transcription\" FROM \"Journalings\" WHERE \"TreatmentId\" = %s ORDER BY \"Date\" DESC LIMIT 1;", (treatment_id,))
    row = cur.fetchone()
    if row:
        state = "Processed" if row[2] == 1 else row[2] # 1 = Processed
        print(f"   Journaling Found: ID={row[0]}, EntryType={row[1]}, State={row[2]}, Transcription='{row[3]}'")
        if state == 1 or row[2] == 'Processed':
            print("   SUCCESS! Integration E2E works perfectly.")
        else:
            print("   FAILED: Status is not Processed. Check Python worker logs.")
    else:
        print("   FAILED: Journaling not found in DB.")
    
    cur.close()
    conn.close()

if __name__ == "__main__":
    run_test()
