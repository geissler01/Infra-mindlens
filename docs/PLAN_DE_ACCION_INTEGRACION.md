# 🗺️ Plan de Acción: Integración y Despliegue (Fase 3)

Basado en la arquitectura definida y los últimos acuerdos, este es el plan de acción estratégico ajustado para integrar Frontends (Flutter y React), Backend (.NET) y AWS.

## Fase 1: Integración Local (End-to-End)

Antes de ir a la nube, debemos asegurar que las piezas hablen el mismo idioma en el entorno local.

### 1. Conexión Frontend (Flutter/React) ↔ Backend (.NET)
- **Autenticación (JWT):** Las apps deben implementar el flujo de login conectándose a la `master_db` a través del Backend para obtener el token JWT y el `tenant_id`.
- **Manejo de Archivos (S3 Local/LocalStack):**
  - Solicitar las **Presigned URLs** (PUT) al Backend antes de subir notas de voz.
  - Se debe definir una **nomenclatura estricta** para S3: `tenant-{id}/{patient_id}/{YYYY-MM-DD}_{uuid}.mp3` para mantener el bucket ordenado.
- **Polling y Descarga de IA (El Camino de Vuelta):**
  - Implementar un **Polling cada 3 segundos** hacia el Backend (`GET /api/journalings/{id}`) hasta que el estado cambie a `done`.
  - Si es audio, el Frontend solicitará la Presigned URL (GET). **Crucial:** Debido a que la URL expira, la app (Flutter) debe **descargar** el archivo de audio localmente al dispositivo de inmediato en segundo plano, para garantizar que el paciente pueda escucharlo sin preocuparse por la caducidad del permiso de S3.

### 2. Conexión Backend ↔ Worker IA (Python)
- **Validación de Versiones:** Asegurar que el Driver de Postgres del Backend (.NET) y el Worker usen las versiones adecuadas compatibles con la versión productiva de PostgreSQL y la extensión `pgvector`, garantizando que la cola de mensajes fluya sin problemas de compatibilidad.
- Confirmar que el Worker consuma la cola de SQS, llame a OpenAI y actualice la base de datos sin fricciones.

---

## Fase 2: Despliegue en AWS (Producción)

### 1. Infraestructura Base (Validación)
- **VPC y Subredes:** Aprovecharemos la infraestructura de VPC, Subredes Públicas/Privadas y el NAT Gateway que **ya existen y están activas** en nuestra cuenta de AWS.
- **Bases de Datos (RDS):** Desplegar PostgreSQL. *Validación técnica requerida:* Garantizar que la versión de RDS soporte `pgvector` nativamente (RDS para PostgreSQL lo soporta de fábrica desde v15.2+).
- **Almacenamiento (S3):** Crear el bucket productivo aplicando la nomenclatura definida (`tenant/paciente/fecha_uuid`) y configurando políticas de CORS para permitir subidas/descargas desde el dominio web y la app móvil.

### 2. Cómputo y Auto-Scaling (ECS Fargate)
- **Backend (.NET):** Desplegar la API detrás de un Application Load Balancer (ALB).
- **Worker (Python):** Desplegar como un servicio leyendo la cola SQS de forma perpetua.
- **Auto-Scaling (¡Sí, es fácil!):** Configurar **Service Auto Scaling** en ECS es un estándar muy amigable. Crearemos "Políticas de seguimiento de objetivos" (Target Tracking). Por ejemplo: si el consumo promedio de RAM o CPU de los Workers supera el 70% durante 3 minutos, CloudWatch disparará una alarma que automáticamente levantará más contenedores, y los irá destruyendo poco a poco cuando la demanda baje. Es nativo y muy fácil de implementar.

### 3. Ecosistema Asíncrono (AWS Lambda)
- Desplegar las imágenes Docker en ECR.
- Configurar EventBridge con los cron jobs acordados:
  - Extractor Diario: **3:00 AM**.
  - Generador Semanal: **Domingos 1:00 AM**.
  - Pre-Sesión: **5:00 AM** (días de cita).

### 4. Estrategia de Despliegue Frontend (React & Flutter)
Al tener clientes separados, la estrategia de alojamiento cambia:
- **App Móvil (Flutter):** Se compila y publica en las tiendas (App Store / Google Play). Sus llamadas a la API apuntan directamente al DNS del Load Balancer (ALB) de AWS.
- **Dashboard Web (React - Psicólogos/Admins):** 
  - **Recomendación (S3 + CloudFront):** Alojar un frontend en React (Single Page Application) dentro de un contenedor ECS es costoso, lento y una mala práctica.
  - **La Solución Ideal:** Lo subiremos a **AWS S3** y le pondremos **CloudFront** (CDN) por delante. Es un proceso donde simplemente corremos `npm run build`, tomamos los archivos estáticos generados (HTML/CSS/JS) y los soltamos en el bucket de S3 configurado como "Static Website Hosting". Es súper barato (céntimos al mes), infinitamente más rápido a nivel global, y muy fácil de mantener.

---

## Fase 3: Pruebas y Certificación
1. **Prueba de Humo (Smoke Test):** Hacer login desde Flutter y React, grabar una nota de voz, esperar la descarga y validarlo en el dashboard.
2. **Cierre de Brechas de Seguridad:** Revisar Security Groups para garantizar que el RDS siga 100% blindado y solo acepte tráfico de ECS/Lambdas.
3. **Validación de Auto-Scaling:** Simularemos un pico de tráfico enviando muchos audios al mismo tiempo para ver en vivo cómo CloudWatch activa la alarma y levanta nuevos contenedores del Worker.
