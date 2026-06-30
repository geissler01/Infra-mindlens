# Generador de Reportes Semanales (Cold Pipeline)

Esta funcion Lambda representa la segunda fase de Inteligencia Artificial asincrona de la plataforma. Su proposito es ejecutarse semanalmente para consolidar los insights diarios de un paciente, agruparlos semanticamente mediante IA y generar reportes clinicos de alto nivel para el psicologo.

## Arquitectura y Flujo de Datos

A continuacion se presenta el diagrama de arquitectura y flujo de ejecucion de la Lambda en el ecosistema AWS.

```mermaid
sequenceDiagram
    autonumber
    actor AWS EventBridge
    participant Lambda Generador
    participant Postgres (RDS)
    participant OpenAI API

    Note over AWS EventBridge, Lambda Generador: Se ejecuta cada Domingo a la medianoche (Cron)
    
    AWS EventBridge->>Lambda Generador: Dispara Evento (Target Date)
    activate Lambda Generador
    
    Lambda Generador->>Postgres (RDS): Consulta Tenants activos
    Postgres (RDS)-->>Lambda Generador: Retorna lista de bases de datos
    
    loop Por cada Tenant y Tratamiento Activo
        Lambda Generador->>Postgres (RDS): Extrae journaling_register (Ultimos 7 dias)
        Postgres (RDS)-->>Lambda Generador: Retorna vectores e insights (analyzed_content)
        
        Note over Lambda Generador, OpenAI API: Fase 1: Resumen Clinico General
        Lambda Generador->>OpenAI API: Envia todos los insights semanales (Prompt)
        OpenAI API-->>Lambda Generador: Retorna Resumen Semanal Consolidado
        Lambda Generador->>Postgres (RDS): Inserta en weekly_reports
        
        Note over Lambda Generador, OpenAI API: Fase 2: Clustering (Agrupacion Semantica)
        Lambda Generador->>Lambda Generador: Agrupa localmente por pillar_type
        
        loop Por cada Grupo (Cluster)
            Lambda Generador->>OpenAI API: Solicita Titulo Clinico (LLM) para el Cluster
            OpenAI API-->>Lambda Generador: Retorna Titulo del Cluster
            
            Lambda Generador->>OpenAI API: Solicita Embedding (Vector) del Titulo
            OpenAI API-->>Lambda Generador: Retorna Vector 1536d
            
            Lambda Generador->>Postgres (RDS): Inserta en weekly_cluster_reports
        end
    end
    
    deactivate Lambda Generador
```

## Descripcion de los Pasos (Diagrama)

1. **Disparo Programado:** AWS EventBridge (mediante una regla Cron) despierta la Lambda todos los domingos a la medianoche. Inyecta la fecha actual como `target_date`.
2. **Descubrimiento de Tenants:** La Lambda se conecta a la base de datos `master_db` para obtener la lista de bases de datos inquilinas activas.
3. **Consulta de Rango Semanal:** Por cada tratamiento activo, la Lambda calcula el rango de fechas (target_date - 7 dias) y extrae todos los registros de `journaling_register`.
4. **Resumen General (IA Generativa):** Todos los textos de `analyzed_content` extraidos en los 7 dias se concatenan y se envian a GPT-4o-mini para redactar un resumen clinico cohesivo de la evolucion del paciente. El resultado se guarda en `weekly_reports`.
5. **Agrupacion Local:** La Lambda toma los registros y los agrupa en memoria segun su `pillar_type` (Ej. Todo lo de "Estres" junto, todo lo de "Autoestima" junto).
6. **Sintesis de Cluester (IA Generativa):** Por cada grupo, se envian sus textos a GPT-4o-mini para que redacte un "Titulo" unico que resuma el comportamiento especifico de ese pilar durante la semana.
7. **Vectorizacion (IA Embeddings):** Ese nuevo Titulo generado es enviado a la API de embeddings (`text-embedding-3-small`) para obtener su representacion vectorial ultra-precisa.
8. **Persistencia Final:** El vector, el titulo y la cantidad de repeticiones se insertan en la tabla `weekly_cluster_reports`. Esto permite que el motor de busqueda por similitud del Dashboard del psicologo opere sobre conceptos semanales consolidados.

## Estructura del Codigo

- `app.py`: Logica principal, consultas SQL y orquestacion de las llamadas a OpenAI.
- `simulate_weekly.py`: Script para viajar en el tiempo y procesar reportes semanales historicos pasandole parametros de fecha especificos.
- `requirements.txt`: Dependencias de empaquetado para AWS.
