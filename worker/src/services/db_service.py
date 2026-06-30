import psycopg2
import psycopg2.extras
from src.config.settings import settings

def get_connection(tenant_db=None):
    db_name = tenant_db if tenant_db else settings.TENANT_DB_NAME
    return psycopg2.connect(
        host=settings.TENANT_DB_HOST,
        database=db_name,
        user=settings.TENANT_DB_USER,
        password=settings.TENANT_DB_PASS
    )

def get_patient_context(entry_id, tenant_db=None):
    conn = get_connection(tenant_db)
    cur = conn.cursor(cursor_factory=psycopg2.extras.DictCursor)
    
    # Obtener info clínica sin PII (evitamos nombre, teléfono, email, dirección)
    cur.execute("""
        SELECT 
            p."AgeRange", 
            p."Occupation", 
            p."RelationshipStatus", 
            p."LivingSituation", 
            p."PrimaryGoal", 
            p."HasPreviousTherapy",
            t."State" 
        FROM "Journalings" j
        JOIN "Treatments" t ON j."TreatmentId" = t."Id"
        JOIN "Patients" p ON t."PatientId" = p."Id"
        WHERE j."Id" = %s
    """, (entry_id,))
    context_data = cur.fetchone()
    
    # Manejar si notes existe (por simplicidad, asumiendo que pueda no existir la tabla)
    try:
        cur.execute("""
            SELECT "Message" FROM "Notes" 
            WHERE "TreatmentId" = (SELECT "TreatmentId" FROM "Journalings" WHERE "Id" = %s)
            ORDER BY "CreatedAt" DESC LIMIT 1
        """, (entry_id,))
        note_data = cur.fetchone()
    except psycopg2.errors.UndefinedTable:
        conn.rollback()
        note_data = None
    
    cur.close()
    conn.close()

    patient_context = dict(context_data) if context_data else {}
    nota = note_data['Message'] if note_data else "Ninguna"

    return patient_context, nota

def get_journaling_text(entry_id, tenant_db=None):
    conn = get_connection(tenant_db)
    cur = conn.cursor(cursor_factory=psycopg2.extras.DictCursor)
    cur.execute('SELECT "Transcription" FROM "Journalings" WHERE "Id" = %s', (entry_id,))
    row = cur.fetchone()
    cur.close()
    conn.close()
    return row['Transcription'] if row else ""

def update_journal_entry(entry_id, reply_s3_key, consejo, transcribed_text, is_emergency, tenant_db=None):
    conn = get_connection(tenant_db)
    cur = conn.cursor()
    cur.execute(
        """
        UPDATE "Journalings" 
        SET "State" = 1, 
            "AiReplyKey" = %s, 
            "AiReplyText" = %s, 
            "Transcription" = %s
        WHERE "Id" = %s
        """,
        (reply_s3_key, consejo, transcribed_text, entry_id)
    )
    conn.commit()
    cur.close()
    conn.close()
