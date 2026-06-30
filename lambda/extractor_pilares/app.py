import os
import json
import psycopg2
from psycopg2.extras import RealDictCursor
from openai import OpenAI

# Variables de entorno inyectadas por AWS Secrets Manager / Docker
DB_HOST = os.getenv("MASTER_DB_HOST", "db")
DB_USER = os.getenv("POSTGRES_USER", "journal_user")
DB_PASS = os.getenv("POSTGRES_PASSWORD", "journal_pass")
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "")

openai_client = OpenAI(api_key=OPENAI_API_KEY) if OPENAI_API_KEY else None

PILARES = [
    "Estado de Ánimo Principal",
    "Nivel de Estrés o Ansiedad",
    "Calidad del Sueño y Energía",
    "Relaciones Interpersonales",
    "Autoestima y Autoconcepto",
    "Preocupaciones Recurrentes",
    "Motivación y Metas",
    "Síntomas Físicos Reportados"
]

def obtener_tenants():
    # Conexión a la Master DB para saber qué bases de datos procesar
    conn = psycopg2.connect(host=DB_HOST, database="master_db", user=DB_USER, password=DB_PASS)
    cur = conn.cursor(cursor_factory=RealDictCursor)
    cur.execute("SELECT database_name FROM tenants WHERE state = 'ACTIVE'")
    tenants = [row['database_name'] for row in cur.fetchall()]
    conn.close()
    return tenants

def procesar_tenant(tenant_db, target_date=None):
    conn = psycopg2.connect(host=DB_HOST, database=tenant_db, user=DB_USER, password=DB_PASS)
    cur = conn.cursor(cursor_factory=RealDictCursor)
    
    # 1. Construir query con filtro opcional
    query = """
        SELECT j.id, j.transcription, 
               p.age_range, p.relationship_status, p.occupation_type, 
               p.living_situation, p.primary_goal, p.has_previous_therapy
        FROM journalings j
        JOIN treatments t ON j.treatment_id = t.id
        JOIN patients p ON t.patient_id = p.id
        WHERE j.status = 'completed' 
        AND j.id NOT IN (SELECT journaling_id FROM journaling_register)
    """
    params = []
    if target_date:
        query += " AND j.date = %s"
        params.append(target_date)
        
    cur.execute(query, tuple(params))
    journalings = cur.fetchall()
    
    for j in journalings:
        texto = j['transcription']
        if not texto or not openai_client: continue
        
        contexto_paciente = f"""
- Edad: {j['age_range']}
- Estado Civil: {j['relationship_status']}
- Ocupación: {j['occupation_type']}
- Situación de Vivienda: {j['living_situation']}
- Meta Principal de Terapia: {j['primary_goal']}
- Terapia Previa: {'Sí' if j['has_previous_therapy'] else 'No'}
"""
        
        try:
            # 2. Extracción con IA (Prompt Engineering del Usuario Mejorado)
            prompt = f"""Eres un sistema de extracción de información para seguimiento psicológico.

Tu única función es analizar la transcripción de un diario personal y extraer información relevante para el psicólogo tratante.

NO debes:
- diagnosticar trastornos;
- emitir juicios clínicos;
- recomendar tratamientos;
- realizar interpretaciones clínicas;
- inferir información no presente en el relato.

Extrae únicamente información explícita o claramente inferible.

CONTEXTO DEL PACIENTE:
{contexto_paciente}

PILARES BASE:
{chr(10).join(f"- {p}" for p in PILARES)}

REGLAS:
1. Analiza todos los pilares base.
2. Si no existe información relevante para un pilar, NO lo incluyas.
3. Puedes crear hasta DOS pilares adicionales si representan temas importantes para el seguimiento psicológico y no encajan adecuadamente en los pilares base.
4. Los pilares adicionales deben ser breves, generales y clínicamente útiles.
5. No generes pilares relacionados con eventos específicos o situaciones aisladas.

Ejemplos válidos:
- Duelo
- Regulación Emocional
- Consumo de Sustancias
- Aislamiento Social
- Ira o Impulsividad

Ejemplos inválidos:
- Problemas con el jefe
- Discusión del martes
- Examen universitario

ESCALA DE SEVERIDAD:
1 = ausencia de malestar o situación positiva.
2 = malestar leve u ocasional.
3 = malestar moderado.
4 = malestar significativo o frecuente.
5 = malestar severo o crítico.

CONFIANZA:
Valor entre 0.00 y 1.00 que representa qué tan seguro estás de la información extraída.

JUSTIFICACIÓN:
- Debe consistir en UNA sola oración.
- Máximo 150 caracteres.
- Debe ser objetiva y descriptiva.
- Utiliza lenguaje formal similar a notas de seguimiento psicológico.
- Resume el contenido relevante del paciente.
- Evita citas textuales, interpretaciones o diagnósticos.
- Redacta preferentemente en tercera persona.

TRANSCRIPCIÓN:
"{texto}"

Responde ÚNICAMENTE un JSON válido.
Ejemplo:
{{
  "Estado de Ánimo Principal": {{
    "justificacion": "Refiere tristeza persistente y disminución del interés en actividades habituales.",
    "severidad": 4,
    "confianza": 0.93
  }}
}}
"""
            response = openai_client.chat.completions.create(
                model="gpt-4o-mini",
                messages=[{"role": "system", "content": prompt}],
                response_format={ "type": "json_object" }
            )
            
            extraccion = json.loads(response.choices[0].message.content)
            
            # 3. Vectorización e Inserción
            for pilar, datos in extraccion.items():
                justificacion = datos.get("justificacion", "")
                severidad = datos.get("severidad", None)
                confianza = datos.get("confianza", None)
                
                if justificacion:
                    emb_res = openai_client.embeddings.create(input=justificacion, model="text-embedding-3-small")
                    vector = emb_res.data[0].embedding
                    
                    cur.execute("""
                        INSERT INTO journaling_register (journaling_id, pillar_type, analyzed_content, severity_score, confidence_score, embedding) 
                        VALUES (%s, %s, %s, %s, %s, %s)
                    """, (j['id'], pilar, justificacion, severidad, confianza, vector))
            
            # Commit por cada registro exitoso
            conn.commit()
            print(f"[{tenant_db}] Procesado exitosamente journaling ID {j['id']} con {len(extraccion)} pilares.")
            
        except Exception as e:
            conn.rollback() # Revertir si hay error en este journaling
            print(f"[{tenant_db}] Error procesando journaling ID {j['id']}: {e}")
            continue

    conn.close()

def lambda_handler(event, context):
    """Entry point para AWS Lambda (gatillado por EventBridge)"""
    print("Iniciando Lambda Extractor Nocturno...")
    
    target_date = None
    if event and 'target_date' in event:
        target_date = event['target_date']
        print(f"SIMULACIÓN ACTIVADA: Procesando fecha -> {target_date}")
        
    tenants = obtener_tenants()
    
    for tenant in tenants:
        try:
            procesar_tenant(tenant, target_date)
        except Exception as e:
            print(f"Error crítico en {tenant}: {e}")
            
    return {
        'statusCode': 200,
        'body': json.dumps(f'Procesados {len(tenants)} tenants exitosamente.')
    }

if __name__ == "__main__":
    # Para probar en local
    lambda_handler(None, None)
