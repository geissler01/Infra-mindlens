# Alineación de Endpoints (Frontend vs Backend)

Este documento contiene un análisis exhaustivo del estado actual de las aplicaciones Frontend (Web en React y Mobile en Flutter) versus el contrato real del Backend (.NET). Su objetivo es brindar a los desarrolladores de Frontend la **lista de rutas definitivas** y los **ajustes exactos** que deben realizar en sus repositorios para una integración exitosa, de cara a nuestro próximo despliegue en AWS.

---

## 1. Configuración de Rutas Base (URLs Definitivas)

Actualmente, ambos Frontends apuntan a rutas de prueba. Deben actualizarse para apuntar a la ruta que expone nuestro proxy/backend.

- **Entorno Local (Docker):** `http://localhost:85` (o `http://10.0.2.2:85` para emuladores Android).
- **Entorno Producción (AWS):** `https://api.mindlens.com` (o el dominio definitivo asignado al Application Load Balancer).
- **Prefijo API:** Todo llamado debe llevar el prefijo `/api` (ej. `/api/auth/login`), no `/v1`.

### Tareas para el Frontend:
- **Mobile (`187-mindlens-mobile-frontend`):** Cambiar en los repositorios (`api_journal_repository.dart`, `api_auth_repository.dart`, etc.) la variable `_baseUrl` de `https://api.mindlens.com/v1/...` a la ruta base local con el prefijo `/api`.
- **Web (`187-mindlens-web-frontend`):** Validar que `src/shared/config/api.js` utilice la variable de entorno `.env` adecuada para apuntar a `http://localhost:85/api`.

---

## 2. Mapa de Alineación de Endpoints y Discrepancias

Hemos detectado que los frontends esperan algunos endpoints que el backend maneja de otra forma. A continuación, el mapeo exacto de cómo deben realizarse las peticiones.

### A. Autenticación (Auth)
- **Frontend (Mobile) esperaba:** `POST /v1/auth/login` y `POST /v1/auth/logout`.
- **Backend (Definitivo):** 
  - Login: `POST /api/auth/login`
  - Registro: `POST /api/auth/register`
- **Discrepancia a corregir:** El backend **no cuenta con un endpoint de `/logout`**. Dado que usamos JWT (Tokens Sin Estado), el "logout" debe realizarse **exclusivamente en el Frontend**, eliminando el token del almacenamiento local (`sessionStorage` o `SharedPreferences`).

### B. Gestión de Pacientes y Psicólogos (Web)
- **Frontend (Web) esperaba:** `GET /patients`, `GET /psychologists`, `POST /auth/forgot-password`.
- **Backend (Definitivo):**
  - Pacientes: `GET /api/patients`, `GET /api/patients/{id}`, `POST /api/patients`, `PUT /api/patients/{id}`. (Se requiere prefijo `/api/`).
  - Psicólogos: El backend **no tiene un controlador separado** para psicólogos. Para listar psicólogos debes utilizar `GET /api/users` y enviar por *Query Parameters* el filtro del rol (`?Role=Psychologist`).
  - Recuperación de contraseña: El backend actualmente **no tiene implementado `/api/auth/forgot-password`**. Esta es una deuda técnica que el Frontend deberá mockear temporalmente o solicitar al Backend.

### C. Diario (Journaling) y Multimodalidad
- **Frontend (Mobile) esperaba:** `GET /v1/journal/entries`, `POST /v1/journal/request-callback`.
- **Backend (Definitivo):**
  - Listar Diario: `GET /api/journalings` (con `JournalingFilters`).
  - Listar Respuestas a Tareas: `GET /api/journalings/answers`.
  - Crear Diario (Texto): `POST /api/journalings`.
  - Crear Respuesta (Texto): `POST /api/journalings/answers`.
- **Discrepancia a corregir:** El `request-callback` no existe en el backend. Toda creación de diario va a `/api/journalings`.

### D. Flujo de Audio (S3 Presigned URLs)
Actualmente, el Frontend Móvil sube archivos asumiendo multipart o base64 en los mocks. **Debe implementarse el siguiente flujo para subir audios:**
1. **Frontend** llama a `GET /api/journalings/s3-key`.
2. **Backend** devuelve un `JournalingUploadAudioResponse` con una URL prefirmada (`PresignedUrl`).
3. **Frontend** hace un `PUT` (usando `package:http`) **directamente a esa URL** con los bytes del archivo `.mp3`.
4. **Frontend** llama a `POST /api/journalings` enviando el `S3Key` generado para que el backend asocie el audio al registro y active el Worker de IA por SQS.

Para descargar/escuchar audios del backend o de la IA:
1. **Frontend** llama a `GET /api/journalings/download-audio/{s3key}`.
2. **Backend** devuelve la URL prefirmada de descarga.
3. El reproductor del frontend usa esa URL temporal para hacer *streaming* del audio.

### E. Tareas y Evaluaciones (Assessment / Questions)
- **Frontend (Mobile) esperaba:** `GET /v1/assessment`.
- **Backend (Definitivo):** Las tareas están divididas en "Questions".
  - Listar Preguntas (Librería): `GET /api/questions`.
  - Ver preguntas asignadas a un tratamiento: `GET /api/treatments/{id}/questions`.
  - Responder a una pregunta asignada: `POST /api/journalings/answers` enviando el `QuestionId`.

---

## 3. Consideraciones de JWT (Seguridad)

- Toda petición a cualquier ruta bajo `/api` (excepto `/api/auth/login` y `/register`) exige que el Frontend envíe el Header:
  `Authorization: Bearer <TU_TOKEN_JWT>`
- En el código web, asegúrense que el `httpClient.js` interceptor realmente inyecte el token leído del `localStorage/sessionStorage`.
- En el código móvil, modifiquen las llamadas de `package:http` en los Repositories para inyectar este Header.

> **Nota para Backend:** Se detectó y corrigió un error en `JournalingController.cs` donde algunas rutas iniciaban con `/` (ej. `[HttpGet("/answers")]`), lo que sobreescribía la ruta principal `api/journalings`. Ahora todos los endpoints funcionan uniformemente anidados bajo `api/journalings/...`.
