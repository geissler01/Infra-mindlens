import json
import os
import random
from datetime import datetime, timedelta
from openai import OpenAI
from dotenv import load_dotenv

# Configuración inicial
FACTORY_DIR = os.path.dirname(os.path.abspath(__file__))
env_path = os.path.abspath(os.path.join(FACTORY_DIR, "..", "..", "..", ".env"))
load_dotenv(dotenv_path=env_path)

api_key = os.getenv("OPENAI_API_KEY")
if not api_key:
    raise ValueError("OPENAI_API_KEY no encontrada en el .env")

client = OpenAI(api_key=api_key)

FACTORY_DIR = os.path.dirname(os.path.abspath(__file__))
SEEDS_DIR = os.path.abspath(os.path.join(FACTORY_DIR, "..", "seeds"))
PROFILES_FILE = os.path.join(FACTORY_DIR, "patients_profiles.json")

# Fechas para los diarios (los últimos 15 días)
today = datetime.now()
dates = [(today - timedelta(days=15 - i)).strftime("%Y-%m-%d") for i in range(15)]
# Elegimos 10 días aleatorios de esos 15 para simular que no escriben todos los días
dates.sort()

with open(PROFILES_FILE, 'r', encoding='utf-8') as f:
    data = json.load(f)

psychologists = data['psychologists']
patients = data['patients']

def generar_historias_llm(paciente):
    print(f"Generando historias para {paciente['first_name']} {paciente['last_names']}...")
    prompt = f"""
Eres un paciente de psicología con el siguiente perfil:
Nombre: {paciente['first_name']} {paciente['last_names']}
Edad: {paciente['age_range']}
Ocupación: {paciente['occupation_type']}
Estado Civil: {paciente['relationship_status']}
Situación: {paciente['living_situation']}
Meta principal de terapia: {paciente['primary_goal']}

Tu tarea es doble:
1. Redactar 10 entradas de diario cronológicas (una por día) que muestren tu evolución emocional a lo largo de las últimas dos semanas respecto a tu meta principal. 
   - Para simular audios largos (de 1 a 4 minutos), mantén cada entrada entre 150 y 400 palabras.
   - Cuenta historias específicas, relata eventos del día, describe tus síntomas físicos y tu diálogo interno.
   - HAZLO EXTREMADAMENTE REALISTA: Muestra resistencia a la terapia, días caóticos, retrocesos, y pequeñas victorias. Si es un audio, usa muletillas naturales ("eh...", "supongo que...", "no sé").

2. Actuar como la IA de apoyo emocional y generar la respuesta perfecta para cada entrada. Tu respuesta debe estar directamente conectada a lo que el paciente acaba de contar en ese diario en específico, validando su emoción exacta. Usa un tono cálido, empático y profesional (Máximo 2 párrafos cortos).

Responde ÚNICAMENTE en formato JSON con la siguiente estructura exacta:
{{
  "entradas": [
    {{
      "transcription": "Texto largo del día 1...",
      "ai_reply_text": "Respuesta empática de la IA..."
    }},
    ...hasta la 10
  ]
}}
"""
    try:
        response = client.chat.completions.create(
            model="gpt-4o-mini",
            messages=[{"role": "system", "content": prompt}],
            response_format={ "type": "json_object" }
        )
        content = json.loads(response.choices[0].message.content)
        return content.get("entradas", [])
    except Exception as e:
        print(f"Error con OpenAI para {paciente['first_name']}: {e}")
        # Retorno seguro en caso de error
        return [{"transcription": f"Fallo genérico {i}", "ai_reply_text": "Lo siento."} for i in range(10)]

def generar_respuesta_ia_basica(texto):
    # Genera una respuesta empática muy corta simulando al GPT-4o-mini del Worker
    # (Para no gastar demasiada API aquí, podríamos generarlas o usar un texto predefinido, pero
    # dado que es una demo, la generaremos de forma rápida)
    return "Gracias por compartir esto. Reconocer tus emociones es el primer paso. Estoy aquí para acompañarte."

def write_master_seed():
    filepath = os.path.join(SEEDS_DIR, "01-master_seed.sql")
    with open(filepath, "w", encoding="utf-8") as f:
        f.write("-- 1. Insert Users (Psicólogos)\n")
        for psy in psychologists:
            # Hash falso para la demo
            f.write(f"INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('u_{psy['id']}', '{psy['name'].split()[0]}', '{' '.join(psy['name'].split()[1:])}', '{psy['email']}', 'PSYCHOLOGIST', 'hashed_pass_123') ON CONFLICT DO NOTHING;\n")
        
        f.write("\n-- 2. Insert Perfiles de Psicólogos\n")
        for psy in psychologists:
            f.write(f"INSERT INTO psychologists_profile (id, user_id, specialty, years_experience) VALUES ('{psy['id']}', 'u_{psy['id']}', '{psy['specialty']}', {psy['years_experience']}) ON CONFLICT DO NOTHING;\n")

        f.write("\n-- 3. Insert Tenants (Las bases de datos)\n")
        for psy in psychologists:
            f.write(f"INSERT INTO tenants (id, psychologist_id, domain, database_name, state) VALUES ('{psy['tenant_db']}_id', '{psy['id']}', 'app.com/{psy['tenant_db']}', '{psy['tenant_db']}', 'ACTIVE') ON CONFLICT DO NOTHING;\n")
            
        f.write("\n-- 4. Insert Users (Pacientes)\n")
        for pat in patients:
            f.write(f"INSERT INTO users (id, first_name, last_names, email, role, password) VALUES ('{pat['global_user_id']}', '{pat['first_name']}', '{pat['last_names']}', '{pat['email']}', 'PATIENT', 'hashed_pass_123') ON CONFLICT DO NOTHING;\n")
            
        f.write("\n-- 5. Insert Treatments Registry (Master)\n")
        for pat in patients:
            f.write(f"INSERT INTO treatments_registry (id, psychologist_id, patient_id) VALUES ('treat_{pat['global_user_id']}', '{pat['psychologist_id']}', '{pat['global_user_id']}') ON CONFLICT DO NOTHING;\n")

def write_tenant_seeds():
    # Agrupar pacientes por tenant
    tenants_map = {}
    for pat in patients:
        tdb = pat['tenant_db']
        if tdb not in tenants_map:
            tenants_map[tdb] = []
        tenants_map[tdb].append(pat)
        
    tenant_files = {
        "tenant_a_db": "02-tenant_a_seed.sql",
        "tenant_b_db": "03-tenant_b_seed.sql",
        "tenant_c_db": "04-tenant_c_seed.sql",
        "tenant_d_db": "05-tenant_d_seed.sql",
        "tenant_e_db": "06-tenant_e_seed.sql"
    }

    for tenant_db, pats in tenants_map.items():
        filepath = os.path.join(SEEDS_DIR, tenant_files[tenant_db])
        with open(filepath, "w", encoding="utf-8") as f:
            f.write("-- Población de pacientes\n")
            for pat in pats:
                f.write(f"INSERT INTO patients (id, global_user_id, phone, emergency_phone, address, age_range, relationship_status, occupation_type, living_situation, primary_goal, has_previous_therapy) VALUES ({pat['id']}, '{pat['global_user_id']}', '{pat['phone']}', '{pat['emergency_phone']}', '{pat['address']}', '{pat['age_range']}', '{pat['relationship_status']}', '{pat['occupation_type']}', '{pat['living_situation']}', '{pat['primary_goal']}', {'TRUE' if pat['has_previous_therapy'] else 'FALSE'});\n")
            
            f.write("\n-- Tratamientos\n")
            for pat in pats:
                f.write(f"INSERT INTO treatments (id, patient_id, state) VALUES ({pat['id']}, {pat['id']}, 'ACTIVE');\n")
                
            f.write("\n-- Journalings (Generados por GPT-4)\n")
            for pat in pats:
                historias = generar_historias_llm(pat)
                
                # Seleccionar 10 fechas
                fechas_paciente = random.sample(dates, 10)
                fechas_paciente.sort()
                
                for i, historia in enumerate(historias[:10]):
                    entry_type = random.choice(["audio", "text"])
                    # Escapar comillas simples para SQL
                    
                    if isinstance(historia, dict):
                        safe_text = historia.get("transcription", "").replace("'", "''")
                        safe_reply = historia.get("ai_reply_text", "").replace("'", "''")
                    else:
                        safe_text = str(historia).replace("'", "''")
                        safe_reply = "Gracias por compartir."
                        
                    f.write(f"INSERT INTO journalings (treatment_id, date, entry_type, transcription, ai_reply_text, status) VALUES ({pat['id']}, '{fechas_paciente[i]}', '{entry_type}', '{safe_text}', '{safe_reply}', 'completed');\n")
                    
            f.write("\n-- Actualizar secuencias\n")
            f.write("SELECT setval('patients_id_seq', (SELECT MAX(id) FROM patients));\n")
            f.write("SELECT setval('treatments_id_seq', (SELECT MAX(id) FROM treatments));\n")
            f.write("SELECT setval('journalings_id_seq', (SELECT MAX(id) FROM journalings));\n")

if __name__ == "__main__":
    print("Iniciando Seed Factory...")
    write_master_seed()
    write_tenant_seeds()
    print(f"Proceso completado. Archivos guardados en {SEEDS_DIR}")
