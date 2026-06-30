import os
import json
import psycopg2
from psycopg2.extras import RealDictCursor
from datetime import date, timedelta
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

def procesar_tenant(tenant_db, target_date_str=None):
    conn = psycopg2.connect(host=DB_HOST, database=tenant_db, user=DB_USER, password=DB_PASS)
    cur = conn.cursor(cursor_factory=RealDictCursor)
    
    if target_date_str:
        target_date = date.fromisoformat(target_date_str)
    else:
        target_date = date.today()
        
    # Extraer registros de los últimos 7 días terminando en la fecha de la cita
    started_at = target_date - timedelta(days=7)
    finished_at = target_date
    
    # Obtener tratamientos activos (para la demo, asumiremos que todos los activos tienen cita hoy)
    # En producción, aquí se filtraría por los que realmente tienen cita hoy.
    cur.execute("SELECT id, patient_id FROM treatments WHERE state = 'ACTIVE'")
    treatments = cur.fetchall()
    
    for t in treatments:
        treatment_id = t['id']
        
        # Extraer vectores de los ultimos 7 dias
        query = """
            SELECT jr.pillar_type, jr.analyzed_content, jr.severity_score, j.date
            FROM journaling_register jr
            JOIN journalings j ON jr.journaling_id = j.id
            WHERE j.treatment_id = %s 
            AND j.date > %s AND j.date <= %s
            ORDER BY j.date ASC
        """
        cur.execute(query, (treatment_id, started_at, finished_at))
        registros = cur.fetchall()
        
        query_answers = """
            SELECT q.question, ja.transcription, j.date
            FROM journaling_answers ja
            JOIN questions q ON ja.question_id = q.id
            JOIN journalings j ON ja.journaling_id = j.id
            WHERE j.treatment_id = %s
            AND j.date > %s AND j.date <= %s
            ORDER BY j.date ASC
        """
        cur.execute(query_answers, (treatment_id, started_at, finished_at))
        respuestas = cur.fetchall()
        
        if not registros and not respuestas:
            print(f"[{tenant_db}] Tratamiento {treatment_id}: Sin registros ni respuestas en los últimos 7 días.")
            continue
            
        print(f"[{tenant_db}] Tratamiento {treatment_id}: Procesando Flash Briefing con {len(registros)} registros y {len(respuestas)} respuestas.")
        
        try:
            textos_totales = []
            if registros:
                textos_totales.append("--- REGISTROS DE LOS PILARES ---")
                for r in registros:
                    textos_totales.append(f"[{r['date']} - {r['pillar_type']}]: {r['analyzed_content']}")
            
            if respuestas:
                textos_totales.append("\n--- RESPUESTAS A CONSIGNAS/PREGUNTAS DEL PSICÓLOGO ---")
                for ans in respuestas:
                    textos_totales.append(f"[{ans['date']}] Pregunta: {ans['question']}\nRespuesta del paciente: {ans['transcription']}")
                
            contexto_semanal = "\n".join(textos_totales)
            
            prompt_pre_sesion = f"""
Eres un sistema de preparación de sesiones para seguimiento psicológico.

Tu tarea es generar un "Flash Briefing" que será leído por el psicólogo minutos antes de la sesión.

Dispones de:
- registros diarios del paciente;
- temas recurrentes de los últimos días;
- respuestas a preguntas o seguimientos indicados previamente por el psicólogo.

OBJETIVOS:

- Destacar temas de mayor intensidad o recurrencia.
- Señalar cambios recientes o variaciones relevantes.
- Identificar dificultades predominantes.
- Mencionar respuestas importantes a preguntas o seguimientos del terapeuta.
- Destacar recursos, avances o estrategias reportadas si son relevantes.

PRIORIZA:

1. Temas de alta severidad.
2. Cambios recientes.
3. Patrones recurrentes.
4. Respuestas relevantes a consignas terapéuticas.
5. Mejoras o recursos personales.

REGLAS:

- Lenguaje profesional, objetivo y descriptivo.
- No diagnostiques.
- No sugieras tratamientos.
- No hagas recomendaciones.
- No interpretes información no presente.
- No repitas información similar.
- Omite información poco relevante.

FORMATO:

- 4 a 6 viñetas.
- Máximo 120 caracteres por viñeta.
- Máximo 600 caracteres totales.
- Sin introducción ni conclusión.
- Debe poder leerse en menos de 30 segundos.

INFORMACIÓN DISPONIBLE:

{contexto_semanal}

Genera únicamente el Flash Briefing.
"""
            res_reporte = openai_client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "system", "content": prompt_pre_sesion}]
            )
            flash_briefing = res_reporte.choices[0].message.content.strip()
            
            # Insertar reporte pre sesión
            cur.execute("""
                INSERT INTO pre_session_reports (treatment_id, flash_briefing)
                VALUES (%s, %s)
            """, (treatment_id, flash_briefing))
            
            conn.commit()
            print(f"[{tenant_db}] Tratamiento {treatment_id}: Flash Briefing generado exitosamente.")
            
        except Exception as e:
            conn.rollback()
            print(f"[{tenant_db}] Error creando reporte para tratamiento {treatment_id}: {e}")
            continue
            
    conn.close()

def lambda_handler(event, context):
    print("Iniciando Lambda Reporte Pre-Sesión (Flash Briefing)...")
    
    target_date = None
    if event and 'target_date' in event:
        target_date = event['target_date']
        print(f"SIMULACION ACTIVADA: Cita programada para el -> {target_date}")
        
    tenants = obtener_tenants()
    for tenant in tenants:
        try:
            procesar_tenant(tenant, target_date)
        except Exception as e:
            print(f"Error critico en {tenant}: {e}")
            
    return {
        'statusCode': 200,
        'body': json.dumps('Reportes pre-sesion generados exitosamente.')
    }

if __name__ == "__main__":
    lambda_handler(None, None)
