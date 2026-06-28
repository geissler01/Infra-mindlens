# Guía de Pruebas: Simulando el Flujo de Journaling en Postman

Esta guía te ayudará a probar todo el flujo de Alta Velocidad (Hot Pipeline) desde Postman, simulando ser la Aplicación Móvil. Verás cómo interactuar con S3, encolar el trabajo y escuchar la respuesta de la Inteligencia Artificial.

## Requisitos Previos

1. Asegúrate de que todos los contenedores estén corriendo (`docker-compose ps`).
2. Ten a mano un archivo de audio corto `.m4a` o `.mp3` en tu computadora (ej: grabarte a ti mismo diciendo "Me siento un poco estresado por el trabajo hoy").

---

## Paso 1: Pedir permiso para subir el audio (Presigned URL)

La App Móvil no puede subir archivos a S3 sin permiso. Primero le pide al Backend una URL temporal (Presigned URL).

- **Método:** `GET`
- **URL:** `http://localhost:85/api/journal/upload-url`
- **Acción:** Dale al botón **Send**.

**Respuesta Esperada (JSON):**

```json
{
    "uploadUrl": "http://localhost:4566/journal-audios-bucket/audios/treatment_1/uuid-largo.m4a?AWSAccessKeyId=...",
    "s3Key": "audios/treatment_1/uuid-largo.m4a"
}
```

*⚠️ **Copia ambos valores**. Los vas a necesitar en los siguientes pasos.*

---

## Paso 2: Subir el archivo de audio directamente a S3

Ahora eres el Frontend. Vas a subir el archivo de audio pesando cero en la memoria del Backend, enviándolo directamente a S3 (LocalStack).

- **Método:** `PUT` *(¡Muy importante, debe ser PUT!)*
- **URL:** Pega aquí la `uploadUrl` exacta que copiaste en el Paso 1.
  *(Nota: La URL ya incluye 'localhost' en lugar de 'localstack' gracias a la configuración del entorno local).*
- **Body:** Ve a la pestaña **Body**, selecciona la opción **`binary`**.
- **Archivo:** Aparecerá un botón "Select File". Busca y selecciona tu archivo `.mp3` o `.m4a`.
- **Acción:** Dale al botón **Send**.

**Respuesta Esperada:**
Debe devolverte un estatus **`200 OK`** vacío. Eso significa que S3 recibió el archivo exitosamente.

---

## Paso 3: Avisar al Backend y encolar el trabajo

El archivo ya está en S3, pero el Backend no lo sabe. Vamos a avisarle para que guarde el registro en la Base de Datos y mande la tarea al SQS para despertar al Worker.

- **Método:** `POST`
- **URL:** `http://localhost:85/api/journal/entry`
- **Headers:** Asegúrate de tener `Content-Type: application/json`.
- **Body (raw JSON):**

```json
{
    "treatmentId": 1,
    "s3Key": "AQUÍ_PEGA_EL_S3KEY_DEL_PASO_1"
}
```

- **Acción:** Dale al botón **Send**.

**Respuesta Esperada (JSON):**

```json
{
    "entryId": 1
}
```

Estatus **`202 Accepted`**. ¡El Worker de Python acaba de ser notificado por SQS y está trabajando en segundo plano!

---

## Paso 4: Hacer Polling (Preguntar "¿Ya está listo?")

La App móvil le preguntará al Backend cada pocos segundos si la IA ya terminó. Vamos a simularlo.

- **Método:** `GET`
- **URL:** `http://localhost:85/api/journal/entry/1` (Reemplaza el `1` por el `entryId` que te devolvió el Paso 3).
- **Acción:** Dale al botón **Send**.

**Respuesta Temprana (Si eres rápido):**

```json
{
    "id": 1,
    "status": "processing"
}
```

*(Si ves esto, espera 3-4 segundos y vuelve a darle a **Send**).*

**Respuesta Final (¡Éxito!):**

```json
{
    "id": 1,
    "status": "completed",
    "ai_reply_url": "http://localstack:4566/journal-audios-bucket/respuestas/reply_1.mp3?AWSAccessKey...",
    "ai_reply_text": "Juan, respira profundo. Entiendo que la oficina esté caótica...",
    "is_emergency": false
}
```

## Paso 5: Escuchar la respuesta

1. Copia la `ai_reply_url` que te dio el paso anterior.
2. Pega la URL en tu navegador (Chrome, Edge, etc.) y presiona Enter.
3. **¡Escucharás al asistente de la IA hablando con voz empática dándote el consejo!**
