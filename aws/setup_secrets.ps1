Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "    CONFIGURADOR DE SECRETOS AWS (SSM)        " -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "Por favor, ingresa las contraseñas reales que usaremos en producción."
Write-Host "Tus datos se enviarán cifrados a AWS y no se guardarán en tu PC.`n"

# Pedir la contraseña de Base de Datos
$DbPassword = Read-Host "Ingresa la contraseña de la Base de Datos (RDS)"

# Pedir la llave de OpenAI
$OpenAiKey = Read-Host "Ingresa tu OPENAI_API_KEY (Ej: sk-...)"

# Llave JWT por defecto (la que definimos en la arquitectura)
$JwtKey = "u9B6vRXZWp8yKqM3tNfG2hJb4vCxD9zW1kLpQ6mF8sY="

Write-Host "`nSubiendo parámetros a AWS SSM Parameter Store..." -ForegroundColor Yellow

# 1. JWT Key
aws ssm put-parameter --name "/mindlens/prod/jwt_key" --value $JwtKey --type SecureString --overwrite | Out-Null

# 2. Connection String (Backend .NET)
$dbHost = "journal-db-poc.cxeacy8aglmv.us-east-2.rds.amazonaws.com"
$dbUser = "postgres"
$connString = "Host=$dbHost;Port=5432;Database=master_db;Username=$dbUser;Password=$DbPassword"
aws ssm put-parameter --name "/mindlens/prod/db_connection_string" --value $connString --type SecureString --overwrite | Out-Null

# 3. DB Pass (Worker Python)
aws ssm put-parameter --name "/mindlens/prod/db_pass" --value $DbPassword --type SecureString --overwrite | Out-Null

# 4. OpenAI Key (Worker Python)
aws ssm put-parameter --name "/mindlens/prod/openai_key" --value $OpenAiKey --type SecureString --overwrite | Out-Null

Write-Host "¡Secretos registrados exitosamente en AWS de forma segura!" -ForegroundColor Green
