# Extractor de Pilares Clinicos (Cold Pipeline)

Esta funcion Lambda es el primer motor de Inteligencia Artificial asincrono de la plataforma. Su objetivo es ejecutarse en segundo plano (idealmente cada noche mediante AWS EventBridge) para procesar todos los diarios (`journalings`) que los pacientes completaron durante el dia.

## Que hace exactamente?

1. **Escaneo de Base de Datos:** Busca todos los `journalings` cuyo `status = 'completed'` pero que aun no tienen registros asociados en la tabla `journaling_register`.
2. **Inyeccion de Contexto:** Extrae el perfil demografico y clinico del paciente (edad, ocupacion, meta de terapia) haciendo un JOIN con las tablas `treatments` y `patients`.
3. **Prompt Engineering:** Envia la transcripcion del diario junto con el contexto del paciente al modelo `gpt-4o-mini`.
4. **Extraccion y Vectorizacion:**
   - La IA devuelve un JSON con insights clasificados en "Pilares" (ej. "Nivel de Estres", "Autoestima", o pilares dinamicos si la situacion lo amerita).
   - Por cada pilar, redacta una **justificacion** (un resumen clinico de maximo 150 caracteres).
   - Genera un **nivel de severidad** (1 al 5) y un **nivel de confianza** (0.00 al 1.00).
   - **Paso Critico:** Toma la `justificacion` (el resumen limpio y clinico, *no la transcripcion cruda*) y la envia a `text-embedding-3-small` para convertirla en un vector de 1536 dimensiones.
5. **Persistencia:** Guarda todo este paquete (Pilar, Justificacion, Severidad, Confianza, Vector) en la tabla `journaling_register` para ser consumido posteriormente por la Lambda de Reportes Semanales.

## Estructura del Codigo

- `app.py`: Contiene toda la logica principal de la Lambda y la conexion con Postgres y OpenAI.
- `simulate_time.py`: Un script de pruebas local. Permite simular el paso de los dias de forma historica inyectando un `target_date` en el evento de la Lambda.
- `requirements.txt`: Dependencias del entorno (usualmente se construye en un contenedor Docker o AWS SAM).

## Como ejecutar y simular localmente

Para probar o reconstruir los vectores de forma local sin depender de EventBridge, puedes usar el script `simulate_time.py` dentro de un contenedor Docker que tenga acceso a tu red de bases de datos.

Desde la raiz de tu proyecto (donde esta el archivo `.env`), ejecuta:

```bash
docker run --rm --env-file .env -v "${PWD}:/app" -w /app/lambda/extractor_pilares --network infra-docker-prueba_journal-net python:3.11-slim bash -c "pip install openai psycopg2-binary python-dotenv && python simulate_time.py"
```

> **Nota:** La Lambda posee manejo de errores por cada registro. Si OpenAI alucina un JSON invalido, la Lambda hace un `rollback()` local de ese paciente y continua con el resto del lote para no romper la ejecucion masiva.

## Esquema de Base de Datos (Tabla Destino)

Los datos se guardan en `journaling_register` con la siguiente estructura:

- `pillar_type`: El nombre del pilar extraido (Ej. "Estado de Animo Principal").
- `analyzed_content`: La justificacion clinica en 3ra persona (Ej. "El paciente refiere ansiedad aguda.").
- `severity_score`: Entero del 1 al 5.
- `confidence_score`: Nivel de certeza de la IA (Numeric 3,2).
- `embedding`: Vector matematico (PgVector) que representa el `analyzed_content`.
