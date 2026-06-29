* [ ] erdock

# Arquitectura y Diseño Detallado para Journal Psicológico

---

## 1. Diagrama Lógico: Flujo de la Aplicación Paso a Paso (Secuencia)

```mermaid
sequenceDiagram
    autonumber
    actor App as App Móvil
    participant NPM as Nginx / ALB
    participant Backend as Backend (.NET)
    participant S3 as Amazon S3 / LocalStack
    participant SQS as Amazon SQS / LocalStack
    participant Worker as Worker (Python)
    participant DB as RDS PostgreSQL
    participant IA as OpenAI APIs

    Note over App: 1. Inicio de grabación
    App->>NPM: GET /journal/upload-url
    NPM->>Backend: Ruteo
    Backend->>Backend: SDK de AWS S3 genera Presigned URL
    Backend-->>App: Retorna { uploadUrl, s3Key: "audios/paciente123.m4a" }

    Note over App: 2. Sube audio directo (Bypass del backend)
    App->>S3: HTTP PUT a uploadUrl (binario de 3MB aprox.)
    S3-->>App: 200 OK
  
    Note over App: 3. Notificación de fin y encolado
    App->>NPM: POST /journal/entry { s3Key: "audios/paciente123.m4a" }
    NPM->>Backend: Ruteo
    Backend->>DB: INSERT JournalEntry (status: 'processing')
    Backend->>SQS: SQS SendMessage { entryId: 101, s3Key: "..." }
    Backend-->>App: 202 Accepted { entryId: 101 }
  
    Note over Worker: 4. Procesamiento Asíncrono de IA (Workers)
    Worker->>SQS: SQS ReceiveMessage (Long Polling)
    SQS-->>Worker: JSON { entryId: 101, s3Key: "..." }
    Worker->>S3: Descargar archivo "audios/paciente123.m4a"
  
    Worker->>IA: OpenAI Whisper API (STT): POST /v1/audio/transcriptions
    IA-->>Worker: Transcripción de texto
  
    Worker->>DB: SELECT contexto_psicologo
    Worker->>IA: OpenAI GPT-4o-mini (LLM): Prompt(Contexto + Transcripción)
    IA-->>Worker: Texto respuesta del psicólogo
  
    Worker->>IA: OpenAI TTS-1 (TTS): POST /v1/audio/speech
    IA-->>Worker: Binario MP3 (Respuesta narrada)
  
    Worker->>S3: Subir MP3 a "respuestas/reply_101.mp3"
    Worker->>DB: UPDATE JournalEntry (status: 'completed', urls)
    Worker->>SQS: SQS DeleteMessage (Borrar tarea de la cola)
  
    Note over App: 5. Polling desde el Frontend
    loop Cada 3 segundos desde el paso 3
        App->>NPM: GET /journal/entry/101
        NPM->>Backend: Ruteo
        Backend->>DB: SELECT status, s3Key_reply
        Backend-->>App: Status 'completed' + Generate Presigned URL
    end
    App->>S3: HTTP GET a Presigned URL de Respuesta
    S3-->>App: Audio reproducido
```

    ---

## 2. Diagrama Físico: Entorno Local (Docker)

En local, simularemos todos los servicios usando contenedores. Aquí reintegramos **NPM (Nginx Proxy Manager)** como la puerta de entrada única para el Frontend.

```mermaid
graph TD
    AppLocal(("App Móvil (Local)"))
    IA(("Mocks / API OpenAI"))
  
    subgraph "Docker Desktop (Máquina Local)"
        NPM["NPM (Nginx Proxy Manager)"]
        API["Backend (.NET 8)"]
        Worker["Worker (Python)"]
        DB[("PostgreSQL")]
        LS["LocalStack (S3 + SQS)"]
    end
  
    AppLocal -->|HTTP puerto 80| NPM
    NPM -->|Ruta /api| API
    API -->|Lee/Escribe metadata| DB
    API -->|Apunta a LocalStack| LS
    Worker -->|Consulta tareas| LS
    Worker -->|Descarga / Sube Audios| LS
    Worker -->|Actualiza estado| DB
    Worker -->|Llamadas HTTP| IA
```

---

## 3. Diagrama Físico: Entorno AWS (Producción)

En AWS, **NPM es reemplazado nativamente por el ALB (Application Load Balancer)**, el cual rutea el tráfico hacia tus contenedores en ECS Fargate.

```mermaid
graph TD
    subgraph "AWS Cloud"
        subgraph "VPC (Virtual Private Cloud)"
            subgraph "Subred Pública (Internet)"
                ALB["Application Load Balancer
                (Reemplaza a NPM)"]
            end
      
            subgraph "Subred Privada (Protegida)"
                ECS_API["Backend .NET (ECS Fargate)"]
                ECS_Worker["Worker Python (ECS Fargate)"]
            end
      
            subgraph "Subred Aislada (Segura)"
                RDS[("Amazon RDS PostgreSQL")]
            end
        end
  
        S3[("Amazon S3")]
        SQS[["Amazon SQS"]]
        IA(("APIs OpenAI"))
  
        AppProd((App Móvil)) --> ALB
        ALB -->|Tráfico API| ECS_API
        ECS_API --> RDS
        ECS_API --> SQS
        ECS_API --> S3
  
        SQS --> ECS_Worker
        ECS_Worker --> S3
        ECS_Worker --> RDS
        ECS_Worker --> IA
    end
```

---

## 4. Resolviendo deudas técnicas (Respuestas y Conceptos)

### A. LocalStack NO es Terraform y NO toca el AWS real

* **Terraform** es código (Infraestructura como Código) que se conecta a internet y *crea* cosas en tu cuenta real de AWS (que cuestan dinero).
* **LocalStack** es simplemente un simulador offline (un emulador). Es como un videojuego de AWS. Se ejecuta 100% dentro del Docker local.
* Para usar LocalStack, al Backend de .NET simplemente se le dice en el archivo `.env` que la URL de S3 no es `aws.com`, sino `http://localstack:4566`. El código de C# seguirá usando el SDK oficial de AWS sin darse cuenta de que está siendo "engañado" y guardando los archivos en el disco duro local.

### B. Nginx Proxy Manager (NPM)

NPM actúa como la puerta de entrada (puerto 80) y redirige todo el tráfico que empiece con `/api/` al contenedor de Backend .NET. Esto evita problemas de CORS en desarrollo para el equipo de Frontend. En AWS, NPM simplemente se reemplaza con el Application Load Balancer de AWS.

---

## 5. Guía Técnica por Servicio (Para el equipo de desarrollo)

Detalle de exactamente qué librerías y técnicas debe usar cada integrante del equipo para lograr el MVP:

### Backend (.NET 8 C#)

* **Librerías a usar:**
  * `AWSSDK.S3` (NuGet) para generar las Presigned URLs.
  * `AWSSDK.SQS` (NuGet) para enviar mensajes a la cola.
  * `Npgsql.EntityFrameworkCore.PostgreSQL` o `Dapper` para conectarse a la Base de Datos.
* **Técnica Clave:** En el `appsettings.Development.json`, deben sobrescribir el `ServiceURL` de AWS para que apunte a `http://localstack:4566`. En producción, simplemente eliminan esa variable y el SDK se conectará al AWS real automáticamente.

### Worker (Python 3.11+)

* **Librerías a usar:**
  * `boto3` (El SDK oficial de AWS para Python). Lo usarán para conectarse a SQS y S3.
  * `psycopg2-binary` o `SQLAlchemy` para consultar la base de datos PostgreSQL.
  * `openai` (Librería oficial para usar Whisper, GPT-4o-mini y TTS).
* **Técnica Clave (Long Polling en SQS):** El script de Python debe hacer un loop infinito (`while True:`). Usarán el método `sqs.receive_message(QueueUrl, WaitTimeSeconds=20)`. Esto hace que Python se quede "dormido" sin consumir CPU hasta que llegue un mensaje. Cuando termine de procesar los audios con OpenAI y guardar en DB, **deben usar `sqs.delete_message`** para decirle a SQS que la tarea se completó y no vuelva a enviarla.

### Frontend (React / React Native / Flutter)

* **Librerías a usar:** Ninguna librería especial de AWS. Solo `axios` o `fetch` nativo.
* **Técnica Clave:** Para subir el archivo a S3, deben usar el método `PUT` hacia la Presigned URL generada por el Backend, enviando el archivo de audio directamente en el `Body` (o usando `multipart/form-data`).
* **Técnica de Polling:** Una vez que reciban el `202 Accepted` del backend, deben crear un `setInterval` que llame al endpoint `GET /journal/entry/{id}` cada 3 o 4 segundos, hasta que el backend responda con el status `completed` y la URL del audio del psicólogo.

### Base de Datos (PostgreSQL)

* **Técnica Clave:** El equipo de Base de Datos debe mantener el índice (`INDEX`) sobre el campo `status` de la tabla de Journals, porque el Frontend va a estar haciendo muchas consultas (Polling) preguntando `WHERE status = 'completed'`. Esto mantendrá la base de datos extremadamente rápida.
