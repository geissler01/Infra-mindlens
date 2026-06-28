import psycopg2
import psycopg2.extras
from config import DB_HOST, DB_NAME, DB_USER, DB_PASS

def get_connection():
    return psycopg2.connect(
        host=DB_HOST,
        database=DB_NAME,
        user=DB_USER,
        password=DB_PASS
    )

def get_patient_context(entry_id):
    conn = get_connection()
    cur = conn.cursor(cursor_factory=psycopg2.extras.DictCursor)
    
    # Obtener info básica
    cur.execute("""
        SELECT p.global_user_id, t.state 
        FROM journalings j
        JOIN treatments t ON j.treatment_id = t.id
        JOIN patients p ON t.patient_id = p.id
        WHERE j.id = %s
    """, (entry_id,))
    context_data = cur.fetchone()
    
    # Obtener nota reciente
    cur.execute("""
        SELECT message FROM notes 
        WHERE treatment_id = (SELECT treatment_id FROM journalings WHERE id = %s)
        ORDER BY created_at DESC LIMIT 1
    """, (entry_id,))
    note_data = cur.fetchone()
    
    cur.close()
    conn.close()

    paciente = context_data['global_user_id'] if context_data else "Desconocido"
    estado = context_data['state'] if context_data else "Desconocido"
    nota = note_data['message'] if note_data else "Ninguna"

    return paciente, estado, nota

def update_journal_entry(entry_id, reply_s3_key, consejo, transcribed_text, is_emergency):
    conn = get_connection()
    cur = conn.cursor()
    cur.execute(
        """
        UPDATE journalings 
        SET status = 'completed', 
            ai_reply_key = %s, 
            ai_reply_text = %s, 
            transcription = %s,
            is_emergency = %s
        WHERE id = %s
        """,
        (reply_s3_key, consejo, transcribed_text, is_emergency, entry_id)
    )
    conn.commit()
    cur.close()
    conn.close()
