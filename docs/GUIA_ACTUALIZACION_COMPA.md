# Guía de Sincronización para Backend (Para el Desarrollador)

¡Hola! Mientras estabas trabajando en la lógica de negocio en el repo `187-mindlens`, nosotros estuvimos adaptando el backend (`infra-docker-prueba`) para que pueda desplegarse de manera robusta y automática en la infraestructura de la nube de AWS (Docker + Fargate + RDS PostgreSQL).

Hay **tres diferencias arquitectónicas clave** entre el código en el que estás trabajando y la infraestructura actual. Para que podamos desplegar tu nueva lógica sin romper los servidores, necesitas aplicar esta guía.

---

## 1. Diferencias Arquitectónicas (¿Qué cambió?)

1. **Separación de las Migraciones (`migrator/`)**
   - **Antes:** Cuando la API arrancaba, corría `app.ApplyMigrationsAsync()` para actualizar la base de datos.
   - **Ahora:** En la nube no podemos correr migraciones desde la API (causa bloqueos cuando escalamos a múltiples servidores). Hemos extraído todas las migraciones y el seeding de datos a un proyecto de consola nuevo llamado `MindLens.Migrator`. La API solo se encarga de servir peticiones.

2. **Resolución del Bug de Entity Framework (Usuarios Nulos)**
   - **El Problema:** Descubrimos que en tus modelos `Tenant.cs` y `PsychologistProfile.cs` tenías la propiedad `public User Psychologist { get; set; } = new User();`. Esto causaba que EF Core insertara silenciosamente **usuarios vacíos** en `AspNetUsers` cada vez que creabas un inquilino, rompiendo por completo las llaves foráneas.
   - **La Solución:** Lo cambiamos a `public User? Psychologist { get; set; }`.

3. **Seeding Multi-Tenant y Archivos `.sql` Limpios**
   - Extrajimos todos los datos de los psicólogos a una carpeta `backend/database/seeds`. Además, tuvimos que limpiar los archivos `.sql` generados (borramos los comandos `\restrict`) porque el motor de PostgreSQL crudo no los soporta.

---

## 2. Pasos para Integrar tu Lógica a la Infraestructura

Para ponerte al día y que podamos desplegar tus nuevos Controladores y Servicios, sigue estos pasos:

### Paso 1: Trae la Estructura de Infraestructura a tu Repo
Si estás trabajando en una rama o carpeta separada (`187-mindlens`), necesitas incorporar las siguientes carpetas de nuestro código de infraestructura:
- Copia la carpeta `migrator/` a la raíz de tu proyecto.
- Copia la carpeta `backend/database/` (que contiene todos los `.sql`).
- Asegúrate de usar el **Dockerfile multi-stage** actualizado (que compila tanto la API como el Migrator).

### Paso 2: Remueve la lógica de Migración del `Program.cs` de la API
Si en tu `Program.cs` tienes algo parecido a:
```csharp
using (var scope = app.Services.CreateScope()) {
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    await db.Database.MigrateAsync();
}
```
**¡Bórralo!** La API (`MindLens.Api`) ya no debe alterar la base de datos en su arranque.

### Paso 3: Corrige los Modelos de Inquilinos
Asegúrate de que tus archivos de modelos no instancien usuarios por defecto. 

En `backend/Models/Tenant.cs`:
```diff
- public User Psychologist { get; set; } = new User();
+ public User? Psychologist { get; set; }
```

En `backend/Models/PsychologistProfile.cs`:
```diff
- public User Psychologist { get; set; } = new User();
+ public User? Psychologist { get; set; }
```

### Paso 4: ¿Cómo trabajar de ahora en adelante con Migraciones?
A partir de hoy, si cambias un modelo y necesitas actualizar la base de datos, tu flujo de trabajo será el siguiente:

1. Creas la migración localmente como siempre: 
   `dotnet ef migrations add NombreDeTuMigracion --project backend`
2. Haces commit y push de tus cambios.
3. **No tienes que hacer nada más**. Cuando despleguemos a AWS, nuestro pipeline construirá el contenedor `poc-migrator`, lanzará una tarea efímera en Fargate, aplicará tu nueva migración usando los Contextos de base de datos actualizados y finalizará. Luego la API se actualizará automáticamente.

---

¡Eso es todo! Si sigues estos 4 pasos, tu nueva lógica de negocio se acoplará perfectamente con nuestra arquitectura escalable.
