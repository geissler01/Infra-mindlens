$env:AWS_PAGER=""
Write-Output "Lanzando tarea independiente (One-Off Task) para migraciones..."

Set-Content -Path overrides.json -Value '{"containerOverrides": [{"name": "backend", "environment": [{"name": "MIGRATE_AND_SEED", "value": "true"}]}]}'

$taskOutput = aws ecs run-task `
  --cluster journal-cluster `
  --task-definition poc-backend-task `
  --launch-type FARGATE `
  --network-configuration "awsvpcConfiguration={subnets=[subnet-01b94f155d091cb3b,subnet-03fd9c85b6ab5aaa1],securityGroups=[sg-043aada5e888dee01],assignPublicIp=DISABLED}" `
  --overrides file://overrides.json

Remove-Item overrides.json

Write-Output $taskOutput
Write-Output "Tarea lanzada correctamente. Monitorea los logs en CloudWatch bajo el Log Group /ecs/poc-backend"
