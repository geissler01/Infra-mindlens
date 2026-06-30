# Pruebas de Integración y Mocks (Backend & Worker)

Esta carpeta contiene los scripts utilizados para probar la infraestructura y la integración entre los servicios de la aplicación (API C#, PostgreSQL Multi-Tenant, LocalStack S3/SQS, y el Worker en Python) durante la fase inicial de desarrollo, cuando el frontend aún no estaba disponible.

## Archivos
- `test_flow.py`: Script principal de integración End-to-End (E2E). Simula el flujo completo de subida de un diario (Journaling) por parte de un paciente.
- `describe.py`: Script auxiliar que se utilizó para explorar y extraer los esquemas y metadatos de las tablas en PostgreSQL, confirmando las convenciones de nombres impuestas por EF Core.

## Consideraciones Críticas para el Frontend
Al desarrollar y probar el frontend contra la API real, los desarrolladores deben tener en cuenta las siguientes limitaciones o *workarounds* (soluciones temporales) que se utilizaron en estos scripts porque ciertos endpoints presentaban fallos:

### 1. Endpoint de Creación de Tratamientos (`POST /api/treatments`)
- **Fallo:** La API de C# presentaba caídas (segfaults/crashes) provocadas por problemas con `Include()` de Entity Framework Core al intentar relacionar entidades con la extensión de base de datos vectorizada.
- **Workaround del Test:** En el script `test_flow.py`, la creación del paciente y del tratamiento se hizo insertando registros directamente en la base de datos de inquilino (ej. `db_mindlens_carlos`) usando `psycopg2`.
- **Acción para el Frontend:** Asegurarse de que el endpoint de creación de tratamientos haya sido parcheado antes de integrarlo. Si la API sigue crasheando, el frontend recibirá errores `500` o la conexión se cerrará abruptamente.

### 2. Endpoint de Generación de Presigned URLs (`GET /s3-key`)
- **Fallo:** Estaba configurado con una ruta absoluta `[HttpGet("/s3-key")]` en el controlador, lo que hacía que no colgara del prefijo `/api` y causara errores de ruteo (`404 Not Found`).
- **Workaround del Test:** El script calcula las rutas S3 de manera manual (`boto3.client`) simulando ser el backend para generar el archivo y subirlo.
- **Acción para el Frontend:** Comprobar con la colección de Postman/Swagger si la ruta quedó como `/api/journalings/s3-key` o en la raíz `/s3-key` al momento de implementarlo.

### 3. Validación de Audios y Transcripción (OpenAI & FFMPEG)
- **Fallo/Comportamiento:** Para que el worker procese el audio, este descarga el archivo desde LocalStack S3 y lo pasa por `ffmpeg`. Si se envían archivos corruptos o texto plano simulando ser un audio (ej. `b"fake audio data"`), `ffmpeg` crasheará y el registro de base de datos nunca será marcado como procesado.
- **Workaround del Test:** El script genera en memoria un archivo `.wav` funcional de 1 segundo de silencio total y lo sube al Bucket.
- **Acción para el Frontend:** Cuando desarrollen la función de grabación de micrófono en el frontend, asegúrense de exportar un archivo de audio real válido (como `.webm`, `.mp3`, o `.wav`). Si suben archivos incompletos, el procesamiento asíncrono fallará silenciosamente (marcará un error en los logs del worker) y el frontend se quedará atascado esperando el resultado.

### 4. Gatillador de SQS
- **Workaround del Test:** El script inyecta mensajes directamente a SQS, incluyendo la propiedad `"TenantDb"` en formato explícito (`"TenantDb": "db_mindlens_carlos"`), permitiendo que el worker sepa a qué base de datos multi-tenant conectarse.
- **Acción para el Frontend:** El frontend **solo debe llamar** a `POST /api/journalings`. La responsabilidad de inyectar este mensaje en SQS es exclusiva del backend en C#. Si esto falla durante pruebas del frontend, se debe revisar la integración del backend con LocalStack SQS.

## Ejecución de los scripts de prueba
Para volver a ejecutar el test en el estado actual de los contenedores (idealmente luego de levantar el entorno desde cero), puedes correr:

```bash
docker cp tests/test_flow.py worker-python:/app/test_flow.py
docker exec worker-python python /app/test_flow.py
```
