# 🚀 Próximos Pasos: Despliegue en AWS (Nube)

Todo el ecosistema de IA ha sido probado localmente y validado. Nuestro código, la lógica vectorial y el prompt-engineering son correctos.

El próximo hito fundamental es llevar estos tres motores (Lambdas) al entorno productivo de **AWS**, lo cual haremos paso a paso **manualmente desde la consola de AWS**.

## 1. Empaquetado y Despliegue de las Lambdas
Como las tres funciones dependen de librerías pesadas (`psycopg2-binary` y `openai`), usaremos contenedores Docker:
1. **Creación de Repositorios en ECR:** Iremos a la consola de Elastic Container Registry (ECR) y crearemos tres repositorios (ej. `lambda-diaria`, `lambda-semanal`, `lambda-presesion`).
2. **Push de las Imágenes:** Construiremos las imágenes de Docker localmente y las subiremos a ECR. (Los scripts `.ps1` en la raíz como `build_push.ps1` nos servirán de guía conceptual).
3. **Creación en AWS Lambda:** Desde la consola de Lambda, crearemos tres nuevas funciones seleccionando la opción "Imagen de contenedor" (Container Image) y apuntando a las URIs de ECR recién subidas.

## 2. Redes y Seguridad (VPC)
Para que las Lambdas puedan ver la base de datos RDS, necesitamos configurar la red desde la consola:
1. En la configuración de cada Lambda, la asignaremos a nuestra **VPC** y **Subredes Privadas** (siguiendo la estructura que muestran los archivos como `infra_setup.ps1`).
2. Les asignaremos el **Security Group** correspondiente para que el RDS permita su tráfico (puerto 5432).
3. **Importante:** La VPC debe tener un NAT Gateway para que las Lambdas tengan salida a internet y puedan comunicarse con la API de OpenAI.

## 3. Gestión de Credenciales
Por seguridad, **no** subiremos el archivo `.env` ni quemaremos contraseñas en el código. 
Toda credencial confidencial (`OPENAI_API_KEY`, usuario y contraseña de la base de datos) se colocará directamente en AWS. Lo haremos desde la consola:
- Configurando las **Variables de Entorno** en la pestaña de configuración de cada función Lambda.
- O (más seguro) guardándolas en **AWS Systems Manager (Parameter Store)** y dándole permisos al Rol de la Lambda para leerlas.

## 4. Orquestación del Tiempo (AWS EventBridge)
Nuestras Lambdas necesitan despertar automáticamente. Iremos a la consola de **Amazon EventBridge** y crearemos reglas (Reglas de tipo *Schedule*) para detonar cada Lambda en sus horarios exactos:

| Motor (Lambda) | Frecuencia | Hora de Ejecución (Local) | Propósito |
| :--- | :--- | :--- | :--- |
| **Extractor de Pilares (Diaria)** | Todos los días | **3:00 AM** | Extraer pilares y vectores mientras el sistema tiene poco tráfico. |
| **Generador Reportes (Semanal)** | Semanal (Domingos) | **1:00 AM** | Agrupar los pilares de la semana y generar el resumen consolidado dominical. |
| **Reporte Pre-Sesión** | Días de sesión | **5:00 AM** | Preparar el Flash Briefing horas antes de que el psicólogo comience sus consultas del día. |

## 5. Pruebas Finales en Producción
Una vez configurado todo en la consola:
1. Forzaremos ejecuciones de prueba desde la pestaña "Test" de AWS Lambda.
2. Revisaremos los logs en **CloudWatch** para confirmar que la base de datos responde y OpenAI procesa todo rápidamente.
3. Calibraremos la memoria (RAM) asignada y el Timeout de las Lambdas para optimizar los costos.
