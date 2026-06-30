# 🧠 Ecosistema de Inteligencia Artificial (Lambdas)

Este directorio contiene el "cerebro asíncrono" de nuestra plataforma. Está dividido en tres motores (funciones AWS Lambda) impulsados por **OpenAI** y **Bases de Datos Vectoriales (PgVector)**. 

Estas tres piezas de software trabajan en conjunto para transformar los registros diarios y caóticos de los pacientes, en herramientas de altísimo valor clínico para los psicólogos.

---

## 1. 🌙 Extractor de Pilares Clínicos (Diaria)
**Carpeta:** `extractor_pilares` | **Ejecución:** Cada noche (Ej. 11:59 PM)

### ¿En qué consiste?
Es el motor fundacional. Lee de forma masiva los audios y textos (journalings) que los pacientes enviaron durante el día, los procesa a través de la IA, extrae "pilares" o temas clave (ansiedad, autoestima, etc.), los califica por severidad, y los convierte en **vectores semánticos**.

### Valor para el Psicólogo y el Paciente
- **Para el paciente:** Siente la libertad de hablar de cualquier tema, sabiendo que la plataforma lo organizará sin que tenga que llenar aburridos formularios de estrés.
- **Para el psicólogo:** Estandariza la subjetividad de los diarios, entregándole métricas cuantificables (niveles de severidad y confianza) sobre cómo se sintió el paciente ese día específico, todo categorizado limpiamente.

---

## 2. 📊 Generador de Reportes (Semanal)
**Carpeta:** `generador_reportes` | **Ejecución:** Fin de semana o día de corte

### ¿En qué consiste?
Es el motor analítico. Toma todos los "pilares" y "vectores" que la Lambda Diaria extrajo durante los últimos 7 días y utiliza algoritmos de agrupamiento espacial (K-Means) en la base de datos para detectar clusters (temas recurrentes). Luego, envía estos clusters a la IA para redactar un resumen clínico semanal.

### Valor para el Psicólogo y el Paciente
- **Para el paciente:** Recibe un seguimiento estructurado. El progreso (o retroceso) de su semana es evidenciado con datos duros en lugar de la memoria frágil de "tuve una mala semana".
- **Para el psicólogo:** Se ahorra horas de leer diarios extensos y escuchar notas de voz. Obtiene una vista de pájaro de la semana del paciente: qué temas fueron los más recurrentes, qué detona sus emociones y un resumen analítico listo para usar en sus notas de evolución clínicas.

---

## 3. ⚡ Reporte Pre-Sesión / Flash Briefing (Día de la Cita)
**Carpeta:** `reporte_pre_sesion` | **Ejecución:** Madrugada del día de la cita con el psicólogo

### ¿En qué consiste?
Es el motor táctico. Cuando el paciente tiene sesión ese día, la Lambda cruza los registros de la última semana junto con las **respuestas a las consignas** que el psicólogo le dejó como tarea. La IA sintetiza todo en un informe ultra corto de viñetas (4 a 6 puntos máximos) que el sistema mostrará al terapeuta justo antes de abrir la puerta.

### Valor para el Psicólogo y el Paciente
- **Para el paciente:** Garantiza que el terapeuta está 100% al día con su situación actual y con las tareas asignadas, maximizando el tiempo de la sesión (los temidos "50 minutos") en terapia real en lugar de "ponerse al día".
- **Para el psicólogo:** Es su salvavidas clínico. En solo 30 segundos de lectura escaneable, recuerda exactamente los puntos críticos, crisis recientes y el cumplimiento de tareas del paciente que está por entrar, reduciendo la fricción mental de cambiar de un paciente a otro.
