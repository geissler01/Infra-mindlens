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
        
    started_at = target_date - timedelta(days=7)
    finished_at = target_date
    
    # Obtener tratamientos activos
    cur.execute("SELECT id, patient_id FROM treatments WHERE state = 'ACTIVE'")
    treatments = cur.fetchall()
    
    for t in treatments:
        treatment_id = t['id']
        
        # Extraer todos los vectores de la ultima semana para este tratamiento
        query = """
            SELECT jr.pillar_type, jr.analyzed_content, jr.severity_score
            FROM journaling_register jr
            JOIN journalings j ON jr.journaling_id = j.id
            WHERE j.treatment_id = %s 
            AND j.date > %s AND j.date <= %s
        """
        cur.execute(query, (treatment_id, started_at, finished_at))
        registros = cur.fetchall()
        
        if not registros:
            continue
            
        print(f"[{tenant_db}] Tratamiento {treatment_id}: Procesando {len(registros)} registros para el reporte semanal.")
        
        try:
            # Agrupar registros por pilar
            grupos = {}
            textos_totales = []
            
            for r in registros:
                pilar = r['pillar_type']
                texto = r['analyzed_content']
                if pilar not in grupos:
                    grupos[pilar] = []
                grupos[pilar].append(texto)
                textos_totales.append(f"- [{pilar}] {texto}")
                
            # FASE 1: Resumen General (LLM)
            contexto_semanal = "\n".join(textos_totales)
            prompt_resumen = f"""
Eres un sistema de generación de resúmenes semanales para seguimiento psicológico.

Tu tarea es redactar un resumen breve dirigido al psicólogo tratante a partir de los registros extraídos de los diarios del paciente durante la última semana.

OBJETIVOS:

- Sintetizar el estado general de la semana.
- Identificar los temas predominantes.
- Destacar patrones o dificultades recurrentes.
- Mencionar cambios o evolución únicamente cuando sean evidentes.
- Priorizar los temas más frecuentes, intensos o repetitivos.

REGLAS:

- Utiliza lenguaje profesional, formal y objetivo.
- Redacta de manera similar a una nota de seguimiento psicológico.
- Resume únicamente la información presente en los registros.
- No diagnostiques trastornos.
- No emitas juicios clínicos.
- No sugieras tratamientos.
- No realices recomendaciones.
- No interpretes información ausente.
- No exageres cambios o tendencias.
- Si no existe evidencia suficiente de evolución, no la menciones.

ESTILO:

- Prioriza la claridad y la utilidad clínica.
- Evita repeticiones.
- Evita enumerar todos los registros.
- Integra la información en una síntesis breve.
- Describe únicamente los hallazgos más relevantes.

FORMATO:

- Un único párrafo.
- Entre 400 y 600 caracteres.
- Máximo 700 caracteres.
- Sin títulos ni listas.
- Redacción fluida y profesional.

EJEMPLOS DE RESÚMENES VÁLIDOS:

"Durante la semana predominaron registros de ansiedad relacionados con el ámbito laboral, acompañados de dificultades de sueño y variaciones en el estado de ánimo. También se identificaron contenidos de autocrítica en situaciones sociales y un incremento de la motivación para realizar actividades recreativas hacia el final del período. Los registros sugieren una disminución parcial del malestar durante los últimos días de la semana."

"Durante la semana se registraron dificultades interpersonales en el entorno familiar, asociadas a mayores niveles de estrés. También se observaron pensamientos recurrentes durante la noche que afectaron el descanso. A pesar de estas dificultades, la paciente reportó la continuidad de algunas rutinas de autocuidado y experiencias positivas relacionadas con el establecimiento de límites personales."

REGISTROS DE LA SEMANA:

{contexto_semanal}

Genera únicamente el resumen.
"""
            res_resumen = openai_client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "system", "content": prompt_resumen}]
            )
            summary_text = res_resumen.choices[0].message.content.strip()
            
            # Insertar reporte semanal y obtener el ID
            cur.execute("""
                INSERT INTO weekly_reports (treatment_id, summary, started_at, finished_at)
                VALUES (%s, %s, %s, %s) RETURNING id
            """, (treatment_id, summary_text, started_at, finished_at))
            weekly_report_id = cur.fetchone()['id']
            
            # FASE 2: Clustering por Pilar
            for pilar, textos in grupos.items():
                if len(textos) == 0: continue
                
                # Generar Titulo del Cluster
                textos_cluster = "\n".join(f"- {t}" for t in textos)
                prompt_titulo = f"""
Analiza los siguientes registros pertenecientes al mismo grupo temático ({pilar}).

Tu tarea es generar un único título que describa el patrón común observado.

REGLAS:

- Debe representar el tema recurrente compartido.
- Debe ser general y reutilizable.
- Debe utilizar lenguaje profesional.
- Debe ser descriptivo y específico.
- No debe mencionar días, eventos o situaciones aisladas.
- No debe incluir diagnósticos.
- No debe incluir interpretaciones clínicas.

LONGITUD:

- Mínimo 20 caracteres.
- Máximo 80 caracteres.

FORMATO:

- Una sola frase.
- Sin punto final.
- Sin explicaciones.
- Responder únicamente el título.

Ejemplos válidos:

- Preocupación laboral persistente
- Disminución de la motivación cotidiana
- Dificultades recurrentes del sueño
- Conflictos interpersonales frecuentes
- Sensación persistente de agotamiento

REGISTROS:

{textos_cluster}
"""
                res_titulo = openai_client.chat.completions.create(
                    model="gpt-4o-mini",
                    messages=[{"role": "system", "content": prompt_titulo}]
                )
                titulo_cluster = res_titulo.choices[0].message.content.strip().strip('"')
                
                # Vectorizar el Titulo del Cluster
                emb_res = openai_client.embeddings.create(input=titulo_cluster, model="text-embedding-3-small")
                vector_cluster = emb_res.data[0].embedding
                
                # Insertar en weekly_cluster_reports
                cur.execute("""
                    INSERT INTO weekly_cluster_reports (weekly_report_id, title, type, repetitions, cluster_embedding)
                    VALUES (%s, %s, %s, %s, %s)
                """, (weekly_report_id, titulo_cluster, pilar, len(textos), vector_cluster))
                
            conn.commit()
            print(f"[{tenant_db}] Tratamiento {treatment_id}: Reporte semanal creado con {len(grupos)} clusteres.")
            
        except Exception as e:
            conn.rollback()
            print(f"[{tenant_db}] Error creando reporte para tratamiento {treatment_id}: {e}")
            continue
            
    conn.close()

def lambda_handler(event, context):
    print("Iniciando Lambda Generador de Reportes Semanales...")
    
    target_date = None
    if event and 'target_date' in event:
        target_date = event['target_date']
        print(f"SIMULACION ACTIVADA: Procesando para la semana terminada el -> {target_date}")
        
    tenants = obtener_tenants()
    for tenant in tenants:
        try:
            procesar_tenant(tenant, target_date)
        except Exception as e:
            print(f"Error critico en {tenant}: {e}")
            
    return {
        'statusCode': 200,
        'body': json.dumps(f'Reportes semanales procesados exitosamente.')
    }

if __name__ == "__main__":
    lambda_handler(None, None)
