# Resolución de Bugs Críticos en el Proceso de Migración

Este documento detalla el diagnóstico y resolución de dos bugs severos que impedían la correcta inicialización y poblado (seeding) de las bases de datos de los inquilinos (Tenants) en nuestro sistema Multi-Tenant con EF Core y PostgreSQL.

## 1. Generación de Usuarios Vacíos ("NULL") y Omisión del Seeding de Tenants

### Síntoma
- Al revisar la base de datos `master_db` usando Adminer, notamos que en la tabla `AspNetUsers` se estaban generando registros adicionales donde todos los campos (FirstName, LastName, Email, etc.) estaban vacíos o en `NULL`, excepto el `Id` y el `Role`.
- Al mismo tiempo, los archivos SQL quemados para poblar a los Tenants (ej. `02-carlos_seed.sql`) no se estaban ejecutando. El migrator logueaba que los pacientes se insertaban en la master, pero no lanzaba ni un solo insert a los Tenants, saltándolos de forma completamente silenciosa.

### Diagnóstico (El "Bug Silencioso" de Entity Framework)
Tras una revisión arquitectónica a fondo, descubrimos que la raíz del problema yacía en la instanciación de clases en C#. En los modelos de dominio, teníamos:

```csharp
// En backend/Models/Tenant.cs
public User Psychologist { get; set; } = new User();

// En backend/Models/PsychologistProfile.cs
public User Psychologist { get; set; } = new User();
```

Durante el proceso de MasterDataSeeder, cuando creábamos el psicólogo y luego instanciábamos `new Tenant {...}`, Entity Framework (EF Core) trackeaba no solo el nuevo Tenant, sino que al detectar `= new User()` en las propiedades de navegación, asumía que debía persistir dos nuevos usuarios totalmente vacíos en la base de datos.
Peor aún, **EF Core sobreescribió silenciosamente el `PsychologistId`** del Tenant para que apuntara a ese "Usuario Vacío" recién creado, en lugar de apuntar al psicólogo real (ej. Carlos).

Esto desencadenó el segundo síntoma: cuando el `RawSqlSeeder` intentaba ubicar el Tenant de Carlos (`WHERE PsychologistId = [Id_Real_De_Carlos]`), la consulta devolvía nulo. Por ende, el código hacía un `continue` y **nunca intentaba ejecutar los archivos SQL**, dejando las bases de datos vacías.

### Solución
Se removió la instanciación por defecto en ambos modelos, marcando la propiedad de navegación como opcional (`?`), lo cual es la convención correcta en EF Core:

```csharp
public Guid PsychologistId { get; set; }
public User? Psychologist { get; set; } // Sin el '= new User()'
```

Esta modificación, que solo tomó un par de clics, eliminó los usuarios fantasma y restauró los enlaces foráneos (Foreign Keys) de los Tenants, permitiendo al Seeder encontrarlos.

---

## 2. Fallo de Ejecución de Vectores (Syntax Error)

### Síntoma
Una vez que el Migrator logró encontrar los Tenants y ejecutar los archivos base, abortó abruptamente en el momento en que intentó inyectar los embeddings. En los logs de CloudWatch en AWS Fargate arrojó:
> `Unhandled exception. Npgsql.PostgresException (0x80004005): 42601: syntax error at or near "\"`

### Diagnóstico (Comandos Específicos de psql)
Al examinar los archivos pesados exportados (ej. `07-vectors_carlos.sql`), identificamos el siguiente código al inicio y al final de los archivos:

```sql
\restrict siwNto6oa3G2psAKhqvcs5n2VoHQznfxNxutKXyo6TxPKY9bRXdrY6AMkpv9UgO
...
\unrestrict siwNto6oa3G2psAKhqvcs5n2VoHQznfxNxutKXyo6TxPKY9bRXdrY6AMkpv9UgO
```

Estos comandos que inician con barra invertida (`\`) son **meta-comandos exclusivos de la interfaz de consola `psql`** o plugins de clientes como Adminer. 
Sin embargo, en C# utilizamos `tenantDb.Database.ExecuteSqlRawAsync(sql)`. Este método se conecta directamente al **motor** de PostgreSQL, el cual no sabe interpretar comandos de la consola psql, arrojando el error de sintaxis y abortando el proceso.

### Solución
Utilizamos un script en PowerShell para limpiar y remover todas las sentencias que iniciaban con `\` en todos los archivos de extensión `.sql` de manera automática, saneando los archivos para el motor crudo de PostgreSQL.

---

## Conclusión

Tras implementar estas dos correcciones, reconstruimos la imagen de Docker y desplegamos la tarea de migración en nuestro cluster Fargate (`poc-migrator`). El resultado de los logs confirmó el éxito total:

```text
[RawSqlSeeder] Seeding tenant db_mindlens_carlos...
[RawSqlSeeder] Executing /app/database/seeds/02-carlos_seed.sql
[RawSqlSeeder] Executing /app/database/seeds/07-vectors_carlos.sql
...
MIGRATIONS AND SEEDING COMPLETED SUCCESSFULLY!
```

Ambos errores estaban profundamente interconectados a nivel de infraestructura y ORM, pero su resolución reafirma que la arquitectura en C# es completamente capaz de manejar las migraciones sin necesidad de separar el proceso a un servicio independiente en Python.
