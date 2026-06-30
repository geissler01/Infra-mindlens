# Scripts de Despliegue en AWS (Infraestructura como Código)

Esta carpeta contiene todos los scripts en PowerShell (`.ps1`) y sus archivos de configuración asociados (`.json`) para automatizar el despliegue de nuestra infraestructura de producción en AWS utilizando **ECS Fargate**, **ALB**, **SSM Parameter Store** y **RDS**.

## 🏗️ Arquitectura Desplegada

Nuestra infraestructura contempla 3 servicios corriendo en ECS Fargate:
1. **Backend (.NET):** Expuesto públicamente a través del ALB en el puerto `80` (ruta base `/api`).
2. **Adminer (Gestor de BD):** Expuesto públicamente a través del mismo ALB pero en el puerto `8080`, conectado internamente a nuestra base de datos RDS para tareas de mantenimiento y administración.
3. **Worker (Python):** Sin exposición a internet. Procesa tareas de fondo consumiendo desde SQS y se conecta a OpenAI.

## 🔐 Gestión de Secretos (Parameter Store)

**NO QUEMAMOS CONTRASEÑAS EN EL CÓDIGO.**
Las plantillas (`backend_task.json` y `worker_task.json`) extraen los valores confidenciales dinámicamente desde AWS Systems Manager (SSM).
Antes de desplegar cualquier contenedor, **debes registrar tus secretos** ejecutando:
```powershell
.\setup_secrets.ps1 -DbPassword "tu_password_rds" -OpenAiKey "sk-tu_llave_openai"
```

## 🚀 Cómo Desplegar (Paso a Paso)

Para evitar errores de rutas con el AWS CLI, **siempre debes ejecutar los scripts estando posicionado dentro de la carpeta `aws/`**.

1. **Registrar Secretos:** `.\setup_secrets.ps1 -DbPassword "..." -OpenAiKey "..."`
2. **Crear Load Balancer y Target Groups:** `.\create_iam_alb.ps1`
3. **Construir y Subir Imágenes (ECR):** `.\build_push.ps1`
4. **Desplegar Servicios y Autoescalado:** `.\deploy_ecs.ps1`

## 📈 Autoescalado del Worker
El servicio `worker-service` cuenta con **Application Auto Scaling** integrado. Monitoriza la carga de CPU mediante CloudWatch y escalará automáticamente desde 1 hasta un máximo de 5 contenedores si el promedio de CPU excede el 70% por más de 3 minutos, optimizando costos y rendimiento.
