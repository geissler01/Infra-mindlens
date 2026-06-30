#!/bin/bash
set -e

echo "=== Creando bases de datos adicionales ==="

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE db_mindlens_carlos;
    CREATE DATABASE db_mindlens_ana;
    CREATE DATABASE db_mindlens_luis;
    CREATE DATABASE db_mindlens_marta;
    CREATE DATABASE db_mindlens_pedro;
EOSQL

echo "=== Aplicando Esquema a MASTER DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "master_db" -f /docker-entrypoint-initdb.d/schema/01-master_schema.sql

echo "=== Aplicando Esquema y Semilla a TENANT CARLOS DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_carlos" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_carlos" -f /docker-entrypoint-initdb.d/seeds/02-carlos_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_carlos" -f /docker-entrypoint-initdb.d/seeds/07-vectors_carlos.sql

echo "=== Aplicando Esquema y Semilla a TENANT ANA DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_ana" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_ana" -f /docker-entrypoint-initdb.d/seeds/03-ana_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_ana" -f /docker-entrypoint-initdb.d/seeds/07-vectors_ana.sql

echo "=== Aplicando Esquema y Semilla a TENANT LUIS DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_luis" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_luis" -f /docker-entrypoint-initdb.d/seeds/04-luis_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_luis" -f /docker-entrypoint-initdb.d/seeds/07-vectors_luis.sql

echo "=== Aplicando Esquema y Semilla a TENANT MARTA DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_marta" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_marta" -f /docker-entrypoint-initdb.d/seeds/05-marta_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_marta" -f /docker-entrypoint-initdb.d/seeds/07-vectors_marta.sql

echo "=== Aplicando Esquema y Semilla a TENANT PEDRO DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_pedro" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_pedro" -f /docker-entrypoint-initdb.d/seeds/06-pedro_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "db_mindlens_pedro" -f /docker-entrypoint-initdb.d/seeds/07-vectors_pedro.sql

echo "=== Inicialización Completada ==="
