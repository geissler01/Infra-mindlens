# stop_project.ps1
# Este script apaga los recursos que cobran dinero por hora para que no pagues durante el fin de semana.

Write-Output "1. Apagando Tareas de ECS (Llevando Backend y Worker a 0)..."
aws ecs update-service --cluster journal-cluster --service backend-service --desired-count 0 --no-cli-pager
aws ecs update-service --cluster journal-cluster --service worker-service --desired-count 0 --no-cli-pager

Write-Output "2. Deteniendo (Pausando) la Base de Datos RDS..."
aws rds stop-db-instance --db-instance-identifier journal-db-poc --no-cli-pager

Write-Output "¡Proyecto Pausado! (Tu Load Balancer sigue vivo para que no cambie la URL, su costo es mínimo)."
