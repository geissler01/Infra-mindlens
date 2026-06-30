# Objetivo: Integracion del Backend Avanzado (Documentacion de Migracion)

El objetivo de este plan es fusionar el entorno de nuestro proyecto (`infra-docker-prueba`) con el backend avanzado (desarrollado por tu compañero en `187-mindlens`). **La premisa principal es mantener íntegro el código y la arquitectura de tu compañero**, ya que es la base para la integración con el frontend. Nosotros únicamente ajustaremos las piezas precisas (AWS, base de datos) para conectar nuestro despliegue en Localstack (S3, SQS) sin romper sus estándares.

---

## Acuerdos y Decisiones de Integracion

> [!NOTE]
> **1. Respeto Total a la Arquitectura del Compañero:**
> Solo tocaremos las capas estrictamente necesarias para la persistencia de archivos, encolamiento de mensajes y consistencia de nombres en la base de datos. Mantendremos Identity, JWT, Multi-tenancy, y todas sus reglas de negocio intactas.
>
> **2. Nomenclatura Estricta de S3 Keys:**
> Para asegurar el orden y la facilidad de mantenimiento en el bucket S3, estandarizaremos la generación de las llaves (`VoiceRecordKey` y `AiReplyKey`). Toda llave generada tendrá la siguiente estructura jerárquica orgánica:
> `audios/tenant_{tenantId}/patient_{patientId}/treatment_{treatmentId}/{uuid}.m4a`
> (Ejemplo: `audios/tenant_a3f4/patient_9b2c/treatment_88cd/123e4567.m4a`).
> 
> **3. Expiración de URLs (Flujo Frontend):**
> Para mantener la seguridad sin afectar la experiencia del usuario, el frontend siempre solicitará un link de descarga temporal llamando a `GetById()`. Este link tendrá una **expiración de 15 minutos**. Dado que el frontend inicia la reproducción o descarga del audio inmediatamente al recibir el link (y los audios duran máx. 5 minutos), 15 minutos es el balance perfecto para evitar cortes y garantizar la seguridad.
> 
> **4. Formato de Mensaje SQS:**
> Mantendremos el ID original del compañero, que es un identificador único global (`Guid`). El formato JSON exacto que enviaremos a la cola SQS será:
> ```json
> {
>   "JournalingId": "a3f4-5b6c-...", 
>   "EntryType": "voice" o "text",
>   "S3Key": "audios/tenant_{id}/patient_{id}/treatment_{id}/{uuid}.m4a"
> }
> ```
> 
> **5. Ajustes DDL y Base de datos (`type` a `pillar_type`):**
> Haremos que el código de tu compañero utilice `PillarType` (o lo mapearemos a la columna `pillar_type` en SQL) en todos los modelos donde aplique (`Question`, `JournalingRegister`, `WeeklyClusterReport`). Esto garantiza que los *tenants* se sigan creando de forma automática por código (tal cual lo ideó tu compañero) pero respetando estrictamente la estructura de tus DDLs actuales.

---

## Plan de Ejecucion (Paso a Paso)

### Fase 1: Migracion del Codigo del Backend
1. **Respaldo:** Renombrar la carpeta `infra-docker-prueba/backend` a `infra-docker-prueba/backend_poc_backup`. (Mantiene nuestro avance como backup).
2. **Copia limpia:** Crear una nueva carpeta `infra-docker-prueba/backend` y copiar todo el contenido de `187-mindlens/backend/MindLens.Api` allí.
3. **Dockerfile:** Crear un nuevo `Dockerfile` en la nueva carpeta que apunte a compilar `MindLens.Api.csproj`, manteniendo las directivas necesarias para correr bajo `docker-compose`.

### Fase 2: Ajuste de Modelos (Entity Framework) y DDL
1. **Modificar la clase `Journaling.cs`:**
   - Renombrar `VoiceRecordUrl` a `VoiceRecordKey`.
   - Renombrar `AiReplyUrl` a `AiReplyKey`.
2. **Modificar las clases `Question.cs`, `JournalingRegister.cs` y `WeeklyClusterReport.cs`:**
   - Cambiar la propiedad `Type` a `PillarType` (y decorarla con `[Column("pillar_type")]` según la sintaxis de Entity Framework) para unificar la base de datos con los scripts DDL locales.
3. **Contextos y Migraciones:** Validar la inyección del `TenantContext` y `ApplicationContext` para asegurar que las tablas se construyan respetando esta nomenclatura.

### Fase 3: Integracion de Servicios AWS (S3 y SQS)
1. Traer `AwsHelper.cs` del respaldo hacia `backend/Services/`.
2. Crear `IAwsHelper.cs` y registrarlo en `Program.cs` junto a `IAmazonS3` e `IAmazonSQS` (pasando las credenciales de Localstack y `AWS_ENDPOINT_URL`).
3. **Modificar `JournalingService.cs`:**
   - En **`GetS3Key()`**: Armar la estructura jerárquica de S3 acordada (`tenant/patient/treatment/uuid.m4a`) y retornar el resultado de `AwsHelper.GenerateUploadPresignedUrl()`.
   - En **`Create()`**: Después del grabado exitoso, llamar a `AwsHelper.SendProcessingMessageAsync()` enviando el JSON de 3 campos.
   - En **`GetById()`**: Al retornar el registro, usar los `Key` de S3 para generar los links temporales mediante `AwsHelper.GenerateDownloadPresignedUrl()` con 15 minutos de expiración.

### Fase 4: Ajustes Finales de Infraestructura (Docker y Worker)
1. **`docker-compose.yml`**: Suministrar al contenedor de la nueva API las variables de entorno de conexión que espera el código de tu compañero (`ConnectionStrings__SharedDB`, parámetros de JWT, AWS).
2. **Worker IA (Python)**: Adaptar el script principal en `worker/main.py` para:
   - Leer el JSON y parsear el `JournalingId` como GUID.
   - Si `EntryType` es "voice", usar la `S3Key` para descargar el audio.
   - Procesar y guardar el resultado en la base de datos correcta (Tenant DB).

---

## Plan de Verificacion
1. Ejecutar `docker-compose up -d --build`.
2. Con Swagger/Postman, crear/verificar un Tenant y un Paciente.
3. Solicitar URL de S3, subir audio, y registrar un Journaling (se enviará el GUID a la cola SQS).
4. Ver logs del Worker para verificar que recibe el GUID, descarga de S3 (usando la ruta estructurada) y guarda en la base de datos que usa el esquema con `pillar_type`.
5. Ejecutar `GetById()` desde el backend y validar que devuelve una URL prefirmada fresca (con expires=900 para 15 minutos).

---
---

# Integracion de Backend Avanzado Completada (Walkthrough)

La migración y fusión del backend avanzado desarrollado por tu compañero con la infraestructura de AWS (S3, SQS y PostgreSQL localstack) ha concluido con éxito.

## Que se implemento?

### 1. Conservacion y Migracion del Codigo
- **Respaldo:** Tu backend original de la PoC fue respaldado en `backend_poc_backup` para no perder la historia.
- **Backend de tu Compañero:** El código íntegro de `187-mindlens/backend/MindLens.Api` fue traído a la carpeta principal `backend/`. No se afectó nada de la lógica de Identity, Multi-tenant, Middlewares o Controladores originales, garantizando que el Frontend no sufrirá quiebres.
- Se implementó un nuevo Dockerfile para que `docker-compose` pueda correr el proyecto completo (usando `.NET 10.0 SDK`).

### 2. Estandarizacion de Base de Datos
- **Mapeo `pillar_type`:** Para que la migración automática de tu compañero encaje perfecto con tus semillas DDL, configuramos el TenantContext usando Entity Framework Core. Ahora las propiedades `Type` de los modelos `Question`, `JournalingRegister` y `WeeklyClusterReport` se mapean internamente a la columna **`pillar_type`**.
- Se cambiaron explícitamente los campos del Journaling a **`VoiceRecordKey`** y **`AiReplyKey`** asegurando la semántica correcta del dato guardado en DB.

### 3. AWS Helpers Inyectados (S3 y SQS)
- Creamos el servicio nativo AwsHelper.cs bajo Inyección de Dependencias (`IAwsHelper`).
- El servicio JournalingService ahora es capaz de:
  - Generar automáticamente **S3 Keys hiper-estructuradas** (`audios/tenant_{id}/patient_{id}/treatment_{id}/{uuid}.m4a`) devolviendo el upload link.
  - Generar links dinámicos seguros de solo **15 minutos de expiración** a la hora de reproducir las respuestas (llamadas al `GetById`).
  - Enviar el JSON correcto a SQS (`JournalingId`, `EntryType`, `S3Key`) cada vez que se crea un nuevo Journaling.

### 4. Ajustes Finales (Worker y Docker)
- El docker-compose.yml ahora provee al contenedor `backend` todas las llaves necesarias (Ej. `ConnectionStrings__SharedDB` y variables `Jwt__*`).
- El Worker de Python fue ajustado para entender el nuevo formato con GUIDs del compañero `data.get("JournalingId")`.

## Siguientes Pasos
Te sugiero reconstruir los contenedores para levantar el nuevo servidor e inicializar la base de datos:
1. `docker-compose down -v` (Si quieres borrar la DB antigua e iniciar fresco).
2. `docker-compose up -d --build`.
3. Validar con Postman/Swagger enviando un Journaling y verificando el Log del Worker en Python.
