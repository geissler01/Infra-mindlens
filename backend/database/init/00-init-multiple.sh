#!/bin/bash
set -e

echo "=== Creando bases de datos adicionales ==="

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE tenant_a_db;
    CREATE DATABASE tenant_b_db;
    CREATE DATABASE tenant_c_db;
    CREATE DATABASE tenant_d_db;
    CREATE DATABASE tenant_e_db;
EOSQL

echo "=== Aplicando Esquema y Semilla a MASTER DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "master_db" -f /docker-entrypoint-initdb.d/schema/01-master_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "master_db" -f /docker-entrypoint-initdb.d/seeds/01-master_seed.sql

echo "=== Aplicando Esquema y Semilla a TENANT A DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_a_db" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_a_db" -f /docker-entrypoint-initdb.d/seeds/02-tenant_a_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_a_db" -f /docker-entrypoint-initdb.d/seeds/07-vectors_tenant_a.sql

echo "=== Aplicando Esquema y Semilla a TENANT B DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_b_db" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_b_db" -f /docker-entrypoint-initdb.d/seeds/03-tenant_b_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_b_db" -f /docker-entrypoint-initdb.d/seeds/07-vectors_tenant_b.sql

echo "=== Aplicando Esquema y Semilla a TENANT C DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_c_db" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_c_db" -f /docker-entrypoint-initdb.d/seeds/04-tenant_c_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_c_db" -f /docker-entrypoint-initdb.d/seeds/07-vectors_tenant_c.sql

echo "=== Aplicando Esquema y Semilla a TENANT D DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_d_db" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_d_db" -f /docker-entrypoint-initdb.d/seeds/05-tenant_d_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_d_db" -f /docker-entrypoint-initdb.d/seeds/07-vectors_tenant_d.sql

echo "=== Aplicando Esquema y Semilla a TENANT E DB ==="
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_e_db" -f /docker-entrypoint-initdb.d/schema/02-tenant_schema.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_e_db" -f /docker-entrypoint-initdb.d/seeds/06-tenant_e_seed.sql
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "tenant_e_db" -f /docker-entrypoint-initdb.d/seeds/07-vectors_tenant_e.sql

echo "=== Inicialización Completada ==="
