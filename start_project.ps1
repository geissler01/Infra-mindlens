# start_project.ps1
# Este script enciende los recursos para cuando vayas a presentar tu proyecto.

Write-Output "1. Encendiendo Base de Datos RDS (Esto tomará unos minutos)..."
aws rds start-db-instance --db-instance-identifier journal-db-poc --no-cli-pager

Write-Output "2. Encendiendo Tareas de ECS (Llevando Backend y Worker a 1)..."
aws ecs update-service --cluster journal-cluster --service backend-service --desired-count 1 --no-cli-pager
aws ecs update-service --cluster journal-cluster --service worker-service --desired-count 1 --no-cli-pager

Write-Output "¡Proyecto Encendido! Dale un par de minutos a la DB y a los contenedores para arrancar antes de probar Postman."
