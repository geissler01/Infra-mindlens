#!/bin/bash
# Este script se ejecuta automáticamente cuando LocalStack inicia

echo "🚀 Iniciando configuración de LocalStack para el MVP del Journal Psicológico..."

# 1. Crear Bucket de S3 para guardar los audios de los pacientes y respuestas de la IA
echo "📦 Creando bucket de S3: journal-audios-bucket..."
awslocal s3 mb s3://journal-audios-bucket

# 2. Crear Cola de SQS para comunicación Backend -> Worker
echo "📬 Creando cola SQS: journal-processing-queue..."
awslocal sqs create-queue --queue-name journal-processing-queue

echo "✅ Configuración de AWS Local completada exitosamente."
