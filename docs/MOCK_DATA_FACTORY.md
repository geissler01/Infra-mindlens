# Generación de Datos de Prueba (Seed Factory)

Este documento explica cómo funciona la arquitectura de generación de datos masivos para demostraciones y entornos de prueba.

## Concepto Core: "Compilación Estática"
En lugar de generar datos sobre la marcha en cada inicio de la aplicación o gastar saldo en la API de OpenAI repetidamente, utilizamos el concepto de "Compilación de Única Vez".

Tenemos un script en Python (`generate_stories.py`) que actúa como motor de creación de datos. Este script solo se ejecuta **una vez** (o cuando se necesiten datos nuevos) y su único trabajo es escribir código SQL estático en la carpeta `backend/database/seeds/`.

Cuando Docker Compose levanta la base de datos de Postgres, simplemente monta estos archivos SQL y los ejecuta. Para Docker, son instrucciones SQL normales escritas a mano, lo que garantiza tiempos de inicialización de milisegundos y **0% de costos de API**.

## Estructura de Archivos

```text
backend/database/seed_factory/
├── patients_profiles.json      # (Fuente de la Verdad) Datos a mano de pacientes y psicólogos
└── generate_stories.py         # El motor creador de SQL
```

## 1. Control Manual (`patients_profiles.json`)
La ventaja de usar un JSON como fuente de la verdad (en lugar de archivos SQL) es que Python puede iterar sobre JSON de manera nativa para construir los diccionarios y enviarlos como contexto a OpenAI. 

El archivo JSON contendrá:
- La configuración de los 5 psicólogos (nombre, especialidad, tenant_db).
- La configuración de los 12 pacientes (edad, estado civil, ocupación, meta principal).

El script leerá este JSON y él mismo escribirá las consultas `INSERT INTO patients...` en los archivos SQL, ahorrándonos el trabajo de mantener las tablas sincronizadas manualmente.

## 2. Generación de Historias (OpenAI)
Por cada paciente en el JSON, el script `generate_stories.py` usará OpenAI (GPT-4) con el siguiente contexto:
- *"Eres un paciente de terapia con el siguiente perfil clínico: [Datos del JSON]"*
- *"Redacta 10 entradas de diario (algunas de audio y otras de texto) a lo largo de 15 días, mostrando congruencia y evolución en tu tratamiento."*

## 3. Manejo en el Worker (Atajo de Texto)
Nuestra Seed Factory intercalará la propiedad `entry_type` (`audio` o `text`) en las historias generadas. Esto es vital para probar nuestro Worker real:

- **Audio (`entry_type = 'audio'`):** Simula una carga de archivo real a S3. El Worker tendrá que descargar el audio, procesarlo con FFmpeg, pasarlo por Whisper para obtener el texto y finalmente enviarlo a GPT para la respuesta empática.
- **Texto (`entry_type = 'text'`):** El paciente solo escribió en la app. El SQS le avisará al Worker. El Worker leerá que es texto, saltará la conexión a S3 y a Whisper, y pasará directamente el texto a GPT para responder.

## 4. Dogfooding (Pipeline Testing)
La Seed Factory **NO** calculará los Vectores Clínicos (Pilares) ni los Reportes Semanales. Dejará los diarios en `status = 'completed'` para que sean nuestras **Lambdas en Producción** las que se encarguen de hacer el procesamiento masivo. De esta forma, cada vez que generemos semillas, estaremos estresando y probando nuestro propio código de infraestructura real.

## 5. Cómo Ejecutar
Para generar o regenerar los archivos `.sql` sin contaminar tu entorno local con librerías de Python, puedes usar Docker.

Ejecuta el siguiente comando desde la **raíz de tu proyecto** (donde está el archivo `.env`). Este comando inyectará tu API Key de OpenAI al contenedor, instalará las dependencias en memoria, creará los `.sql` y se autodestruirá:

```bash
docker run --rm --env-file .env -v "${PWD}:/app" -w /app/backend/database/seed_factory python:3.11-slim bash -c "pip install openai python-dotenv && python 01_generate_stories.py"
```

Una vez finalice, revisa la carpeta `backend/database/seeds/`. Encontrarás los archivos `01-master_seed.sql` hasta `06-tenant_e_seed.sql` listos para inicializarse cuando levantes tus bases de datos con `docker-compose up`.
