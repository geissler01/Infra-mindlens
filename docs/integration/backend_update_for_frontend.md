# Actualización de Backend (V2) - Guía para el Frontend

Este documento resume los cambios integrados recientemente en el Backend Dockerizado (`infra-docker-prueba`) a partir del repositorio original (`187-mindlens`). Está diseñado para que el equipo de Frontend (React/Flutter) sepa qué nuevos endpoints y estructuras de datos están disponibles y listos para consumirse de forma local (y próximamente en AWS).

## 1. Nuevo Módulo de Preguntas (`Questions`)

Se ha incorporado por completo el flujo de Preguntas, lo que permite asignar y consultar preguntas específicas para los tratamientos.

- **Nuevos Endpoints Disponibles:**
  Se ha habilitado el `QuestionController` con sus respectivas rutas para la creación, consulta y filtrado de preguntas (basadas en los `QuestionFilters`).
- **Nuevas Estructuras (DTOs):**
  - Todas las estructuras de envío y respuesta de preguntas están ahora operativas. 
  - *Nota para Frontend:* Si antes estabas mockeando esta parte, ahora puedes conectar los endpoints reales de `/api/questions` (o la ruta correspondiente en el controlador).

## 2. Mejoras en Diario (`Journaling`) y Tratamientos (`Treatment`)

Se ha actualizado la lógica interna para soportar interacciones complejas con IA y S3 (multimodalidad).

- **Nuevos DTOs de Respuesta para Audio:**
  Si el paciente sube o descarga un audio del diario, el backend ahora responde utilizando estructuras especializadas que contienen las URLs prefirmadas de AWS S3.
  - `JournalingUploadAudioResponse`
  - `JournalingDownloadAudioResponse`
  - *Nota para Frontend:* Asegúrate de interceptar estas respuestas para manejar las *Presigned URLs* que te permitirán hacer `PUT` directamente a S3 sin saturar nuestro backend, y `GET` para descargar la respuesta de voz de la IA.

- **Flujos Asíncronos SQS:**
  Al crear un Journaling (`CreateJournalingDto`) o una respuesta de Journaling (`CreateJournalingAnswerDto`), el backend inyectará la tarea en la cola SQS configurada localmente vía LocalStack. 
  - *Nota para Frontend:* Debes implementar un *Polling* cada ciertos segundos apuntando al registro recién creado para saber cuándo el estado pase de `processing` a `done` por parte del Worker en Python.

## 3. Consideraciones de Base de Datos y Seeders

- **Estructura Conservada:** Nuestro esquema DDL principal no cambió (`pillar_type` en PostgreSQL sigue siendo la fuente de la verdad para el tipo de registros). 
- **Datos de Prueba Iniciales:** Los Seeders locales siguen activos en `docker-compose up`. Podrás hacer login con cualquiera de los usuarios pre-generados (ej. `carlos@mindlens.com` con el password `Password123!`).

## 4. Configuración y JWT

- **Nueva Llave JWT:** El servidor localmente ahora utiliza la llave de producción/staging proporcionada: `u9B6vRXZWp8yKqM3tNfG2hJb4vCxD9zW1kLpQ6mF8sY=`.
- **AWS Local:** Todo el tráfico de S3 y SQS localmente está apuntando al contenedor de `localstack` interno en el puerto `4566`.

### Siguientes Pasos
El backend ya compila y levanta correctamente en Docker. El paso a seguir es conectar tus aplicaciones Frontend a `http://localhost:8080` (o el puerto configurado en el proxy) y validar el *End-to-End* completo.
