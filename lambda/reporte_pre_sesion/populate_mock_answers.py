import os
import json
import random
import psycopg2
from psycopg2.extras import RealDictCursor
from openai import OpenAI

DB_HOST = os.getenv("MASTER_DB_HOST", "db")
DB_USER = os.getenv("POSTGRES_USER", "journal_user")
DB_PASS = os.getenv("POSTGRES_PASSWORD", "journal_pass")
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "")

openai_client = OpenAI(api_key=OPENAI_API_KEY) if OPENAI_API_KEY else None

def obtener_tenants():
    conn = psycopg2.connect(host=DB_HOST, database="master_db", user=DB_USER, password=DB_PASS)
    cur = conn.cursor(cursor_factory=RealDictCursor)
    cur.execute("SELECT database_name FROM tenants WHERE state = 'ACTIVE'")
    tenants = [row['database_name'] for row in cur.fetchall()]
    conn.close()
    return tenants

def procesar_tenant(tenant_db):
    conn = psycopg2.connect(host=DB_HOST, database=tenant_db, user=DB_USER, password=DB_PASS)
    cur = conn.cursor(cursor_factory=RealDictCursor)
    
    cur.execute("""
        SELECT t.id as treatment_id, p.id as patient_id, p.primary_goal, p.occupation_type, p.age_range 
        FROM treatments t
        JOIN patients p ON t.patient_id = p.id
        WHERE t.state = 'ACTIVE'
    """)
    treatments = cur.fetchall()
    
    for t in treatments:
        treatment_id = t['treatment_id']
        goal = t['primary_goal']
        occupation = t['occupation_type']
        
        # 1. Verificar si ya tiene preguntas asignadas
        cur.execute("""
            SELECT q.id, q.question 
            FROM questions q
            JOIN treatments_questions tq ON q.id = tq.question_id
            WHERE tq.treatment_id = %s
        """, (treatment_id,))
        preguntas_existentes = cur.fetchall()
        
        if not preguntas_existentes:
            print(f"[{tenant_db}] Tratamiento {treatment_id}: Generando consignas clínicas...")
            prompt_q = f"""
Eres un psicólogo clínico. Tu paciente es un {occupation} cuyo objetivo de terapia es: "{goal}".
Escribe entre 1 y 3 preguntas cortas (consignas o tareas de seguimiento) que el paciente deba responder en su diario durante la semana.
Devuelve las preguntas en formato JSON como una lista de strings: ["pregunta 1", "pregunta 2"]
            """
            res_q = openai_client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "user", "content": prompt_q}],
                response_format={ "type": "json_object" }
            )
            
            # Forzar estructura de lista
            try:
                data = json.loads(res_q.choices[0].message.content)
                if isinstance(data, dict):
                    preguntas_nuevas = list(data.values())[0]
                else:
                    preguntas_nuevas = data
                if not isinstance(preguntas_nuevas, list):
                    preguntas_nuevas = [str(preguntas_nuevas)]
            except Exception as e:
                print(f"Error parseando JSON: {e}")
                preguntas_nuevas = [f"¿Cómo te sentiste hoy respecto a: {goal}?"]
            
            # Insertar en DB
            for pregunta_texto in preguntas_nuevas[:3]:
                cur.execute("INSERT INTO questions (question, type) VALUES (%s, %s) RETURNING id", (pregunta_texto, 'homework'))
                q_id = cur.fetchone()['id']
                cur.execute("INSERT INTO treatments_questions (treatment_id, question_id) VALUES (%s, %s)", (treatment_id, q_id))
                preguntas_existentes.append({'id': q_id, 'question': pregunta_texto})
            conn.commit()
            
        print(f"[{tenant_db}] Tratamiento {treatment_id}: Tiene {len(preguntas_existentes)} consignas asignadas.")
        
        # 2. Iterar sobre journalings y simular respuestas voluntarias
        cur.execute("SELECT id, date, transcription FROM journalings WHERE treatment_id = %s ORDER BY date ASC", (treatment_id,))
        journalings = cur.fetchall()
        
        for j in journalings:
            journaling_id = j['id']
            transcription = j['transcription']
            
            for p in preguntas_existentes:
                q_id = p['id']
                pregunta_texto = p['question']
                
                # Verificar si ya la respondió
                cur.execute("SELECT id FROM journaling_answers WHERE journaling_id = %s AND question_id = %s", (journaling_id, q_id))
                if cur.fetchone():
                    continue # Ya la tiene
                
                # Simular voluntariedad: 30% de probabilidad de responder
                if random.random() > 0.30:
                    continue
                    
                print(f"[{tenant_db}] Simulando respuesta del paciente {treatment_id} para el día {j['date']}...")
                prompt_a = f"""
Eres un paciente en terapia. Tu psicólogo te dejó esta tarea/consigna para la semana: "{pregunta_texto}".
Este es tu registro diario general de hoy (para que entiendas tu contexto emocional):
"{transcription}"

Escribe una respuesta MUY CORTA (1 a 3 oraciones) a la consigna de tu psicólogo. Actúa como el paciente.
No saludes, solo da tu respuesta directa.
                """
                res_a = openai_client.chat.completions.create(
                    model="gpt-4o-mini",
                    messages=[{"role": "user", "content": prompt_a}]
                )
                respuesta_texto = res_a.choices[0].message.content.strip()
                
                cur.execute("""
                    INSERT INTO journaling_answers (journaling_id, question_id, entry_type, transcription, status) 
                    VALUES (%s, %s, 'text', %s, 'completed')
                """, (journaling_id, q_id, respuesta_texto))
                conn.commit()
                
    conn.close()

def main():
    print("==========================================================")
    print("    GENERADOR DINÁMICO DE RESPUESTAS A CONSIGNAS          ")
    print("==========================================================")
    
    tenants = obtener_tenants()
    for tenant in tenants:
        try:
            procesar_tenant(tenant)
        except Exception as e:
            print(f"Error critico poblando {tenant}: {e}")
            
    print("\n==========================================================")
    print(" POBLACIÓN DINÁMICA COMPLETADA CON ÉXITO")
    print("==========================================================")

if __name__ == "__main__":
    main()
