#!/bin/bash
set -e

# Asegurar que se lea el .env
if [ -f ".env" ]; then
    source .env
fi

echo "=========================================================="
echo " RESCATANDO VECTORES Y EXTRACCIONES DE LA IA (PG_DUMP)"
echo "=========================================================="

# Array con los tenants
TENANTS=("a" "b" "c" "d" "e")

for T in "${TENANTS[@]}"; do
    DB_NAME="tenant_${T}_db"
    FILE_NAME="backend/database/seeds/07-vectors_tenant_${T}.sql"
    
    echo "💾 Exportando tabla journaling_register de ${DB_NAME}..."
    
    # Ejecutamos pg_dump dentro del contenedor y guardamos en host
    docker exec postgres-db pg_dump -U "$POSTGRES_USER" -d "$DB_NAME" -t journaling_register -t weekly_reports -t weekly_cluster_reports -t questions -t treatments_questions -t journaling_answers -t pre_session_reports --data-only --inserts > "$FILE_NAME"
    
    echo "✅ Guardado en ${FILE_NAME}"
done

echo "=========================================================="
echo " ¡Vectores asegurados! Ahora tu base de datos nacerá"
echo " con inteligencia artificial pre-cargada."
echo "=========================================================="
