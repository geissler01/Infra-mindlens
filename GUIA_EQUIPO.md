# Guía para el Equipo de Desarrollo (MVP Journal Psicológico)

¡Hola equipo! Esta infraestructura Docker está configurada para simular el entorno real de AWS en sus propias computadoras. No necesitan instalar Postgres ni configurar AWS en la nube; todo corre aquí adentro.

## 🚀 Cómo arrancar el proyecto

1. Abran una terminal en esta carpeta (`infra-docker/`).
2. Configuren su API Key de OpenAI en el archivo `.env`:
   ```bash
   OPENAI_API_KEY=sk-tu-api-key
   ```
3. Ejecuten el comando:
   ```bash
   docker-compose up -d
   ```
4. Para ver que todo está corriendo, prueben entrar a [http://localhost/](http://localhost/). Deberían ver un mensaje del API Gateway.

---

## 💻 Instrucciones para cada Rol

### 1. Equipo Backend (C# .NET)
* **Su código va en:** `/backend`
* Su trabajo es exponer los endpoints RESTful en el puerto 80 (usualmente configurado así en ASP.NET en Docker).
* Todo el tráfico que entra por `http://localhost/api/...` será redirigido a ustedes automáticamente por Nginx.
* **Librerías a instalar (NuGet):** `AWSSDK.S3` y `AWSSDK.SQS`.
* **El Truco de LocalStack:** Las variables de entorno ya están en el `docker-compose.yml`. Configuren su cliente de AWS SDK para que use la variable `AWS_SERVICE_URL=http://localstack:4566`. ¡Con esto su código guardará los archivos S3 en la computadora y no en la nube!
* **Base de Datos:** Se pueden conectar usando `Host=db;Port=5432;Database=journal_db;Username=journal_user;Password=journal_pass`.

### 2. Equipo IA (Worker Python)
* **Su código va en:** `/worker`
* Su trabajo es escribir un script que nunca termine (un `while True:`) usando "Long Polling" sobre SQS.
* **Librerías a instalar (pip):** `boto3`, `psycopg2` (o `sqlalchemy`) y `openai`.
* Cuando lean de SQS, el Endpoint será `http://localstack:4566`. Usen `boto3.client('sqs', endpoint_url='http://localstack:4566')`.
* Recuerden borrar el mensaje de SQS solo cuando todo el proceso (STT -> GPT -> TTS -> S3) haya terminado exitosamente.

### 3. Equipo Frontend (App Móvil)
* Su punto de entrada al API SIEMPRE es `http://localhost/api/`. No llamen directamente a los puertos del backend ni de Nginx.
* **Flujo de subida:**
  1. Soliciten la Presigned URL haciendo GET a `/api/journal/upload-url`.
  2. Hagan un **HTTP PUT** directo a esa URL larga con el archivo de audio. ¡Cero librerías especiales!
  3. Avisen al backend que terminaron llamando a `/api/journal/entry`.
  4. Creen un temporizador que llame a `/api/journal/entry/{id}` cada 3 segundos hasta que les devuelvan el status `completed`.

¡Éxitos con el MVP! Todo está configurado para que la transición a producción en AWS sea cambiar un par de IPs.
