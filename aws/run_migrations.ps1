$env:AWS_PAGER=""
Write-Output "Registrando nueva definición de tarea para Migrator..."
aws ecs register-task-definition --cli-input-json file://migrator_task.json > $null

Write-Output "Lanzando tarea independiente (One-Off Task) para migraciones..."

$taskOutput = aws ecs run-task `
  --cluster journal-cluster `
  --task-definition poc-migrator-task `
  --launch-type FARGATE `
  --network-configuration "awsvpcConfiguration={subnets=[subnet-01b94f155d091cb3b,subnet-03fd9c85b6ab5aaa1],securityGroups=[sg-043aada5e888dee01],assignPublicIp=DISABLED}"


Write-Output $taskOutput
Write-Output "Tarea lanzada correctamente. Monitorea los logs en CloudWatch bajo el Log Group /ecs/poc-migrator"
