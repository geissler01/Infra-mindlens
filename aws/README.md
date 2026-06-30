# Scripts de Despliegue en AWS (Infraestructura como Código)

Esta carpeta contiene todos los scripts en PowerShell (`.ps1`) y sus archivos de configuración asociados (`.json`) para automatizar el despliegue de nuestra infraestructura en AWS utilizando ECS Fargate, ALB, SQS y RDS.

## ⚠️ Nota Importante sobre la Ejecución

La mayoría de estos scripts leen archivos de configuración locales pasándolos al AWS CLI mediante la sintaxis `file://...` (por ejemplo: `--cli-input-json file://backend_task.json`). 

Para evitar errores donde AWS CLI no encuentre estos archivos, **siempre debes ejecutar los scripts estando posicionado dentro de esta misma carpeta**.

**Forma Correcta de Ejecución:**
```powershell
cd aws
.\deploy_ecs.ps1
```

## 📈 Tareas Pendientes (Roadmap de Infraestructura)

### Autoescalado del Worker (Application Auto Scaling + CloudWatch)
Actualmente el clúster se despliega con un número fijo de contenedores (Desired Count = 1). 
Tenemos planificado implementar una estrategia de autoescalado para el contenedor del **Worker** a fin de gestionar mejor las cargas de procesamiento de audio e IA:

1. **CloudWatch Alarms:** Se configurarán alarmas para vigilar el rendimiento del servicio de los workers en ECS. Específicamente, monitorizar el consumo de Memoria RAM (y CPU) durante períodos sostenidos de 2 o 3 minutos.
2. **Target Tracking Scaling / Step Scaling:** Integrar `Application Auto Scaling` (el otro servicio de AWS para autoescalado) que estará enlazado a las alarmas de CloudWatch.
3. **Escalado Horizontal:** 
   - **Scale Out:** Si la alarma detecta alta saturación (ej. CPU o RAM superior al 75% por 3 minutos), se crearán (replicarán) nuevas tareas del worker automáticamente para procesar la cola de SQS más rápido.
   - **Scale In:** Cuando la carga disminuya y los workers estén ociosos, el servicio eliminará los workers excedentes, dejando solo 1 para optimizar los costos.

*(Esta configuración se añadirá en los scripts `.ps1` en las próximas iteraciones de infraestructura).*
