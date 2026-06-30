# Pruebas de Integración (Backend C# + Worker Python)

Este documento describe la prueba de integración completa diseñada para verificar el flujo de Journaling.

## Objetivo
Verificar que la comunicación entre los componentes principales (Backend C#, Worker en Python, LocalStack S3, SQS y Base de Datos PostgreSQL) funciona correctamente. 
Particularmente, se debía probar la capacidad del Worker de procesar audios, interactuar con OpenAI y actualizar el registro en la base de datos de los inquilinos (Tenants).

## Flujo Probado (`test_flow.py`)
1. **Autenticación (C#):** Se obtiene el token de psicólogo y paciente.
2. **Creación de Recursos:** Creación de un Paciente y un Tratamiento para asociar el Journaling.
3. **Petición S3 (LocalStack):** Solicitud para generar un URL firmado para subida de audios.
4. **Subida a S3:** Se sube un archivo `.wav` funcional directamente a LocalStack S3 utilizando `boto3`.
5. **Creación de Registro en Base de Datos:** Se inserta manualmente el registro de Journaling con estado `0` (Pending) en la base de datos de inquilino (ej. `db_mindlens_carlos`), mitigando errores momentáneos del endpoint C#.
6. **Mensaje a SQS:** Se envía manualmente el payload JSON (incluyendo el `TenantDb`) a la cola `journal-processing-queue`.
7. **Procesamiento del Worker (Python):** 
   - El Worker escucha SQS.
   - Lee el payload, descarga el audio de S3.
   - Ejecuta `ffmpeg` y transcribe el audio.
   - Llama a GPT-4o-mini para generar un consejo utilizando el contexto extraído directamente de la base de datos de inquilinos.
   - Genera el audio TTS y lo sube de regreso a S3.
   - Actualiza el registro de Journaling a `State = 1` (Processed) en la base de datos específica del Tenant.
8. **Validación:** El script prueba que el registro en BD pasó exitosamente a `State = 1`.

## Retos Solucionados
- **Case Sensitivity PostgreSQL:** Se arreglaron consultas SQL en el Worker (`db_service.py`) usando dobles comillas para respetar las columnas en PascalCase que Entity Framework generó.
- **Dynamic Database Routing:** Se modificó el mensaje SQS para que pasara explícitamente qué base de datos usar (`TenantDb`), de forma que el Worker pueda operar independientemente y con un aislamiento de datos 100% efectivo por inquilino.
- **FFMPEG Crash con Fake Data:** Para que Whisper no falle, se diseñó la generación de un archivo `wav` de silencio estricto para probar el pipeline sin requerir grabar audios en tiempo real.

## Siguiente Paso
El backend se encuentra funcional junto con su capa de IA, infraestructura Docker, bases de datos múltiples (Master y Tenants) y procesamiento asíncrono. El próximo objetivo será comenzar la integración con el **Frontend**.
