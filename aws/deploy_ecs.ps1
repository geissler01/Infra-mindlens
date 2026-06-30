$env:AWS_PAGER=""
Write-Output "1. Configurando IAM Role para ECS..."
try {
    aws iam create-role --role-name ecsTaskExecutionRolePoc --assume-role-policy-document file://trust_policy.json 2>$null
} catch {}
aws iam attach-role-policy --role-name ecsTaskExecutionRolePoc --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
aws iam attach-role-policy --role-name ecsTaskExecutionRolePoc --policy-arn arn:aws:iam::aws:policy/AmazonSQSFullAccess
aws iam attach-role-policy --role-name ecsTaskExecutionRolePoc --policy-arn arn:aws:iam::aws:policy/AmazonSSMReadOnlyAccess
Start-Sleep -Seconds 10 # Esperar a que IAM propague

# 2. Log Groups
try { aws logs create-log-group --log-group-name /ecs/poc-backend 2>$null } catch {}
try { aws logs create-log-group --log-group-name /ecs/poc-worker 2>$null } catch {}
try { aws logs create-log-group --log-group-name /ecs/poc-adminer 2>$null } catch {}

# 3. Registrar Task Definitions
aws ecs register-task-definition --cli-input-json file://backend_task.json
aws ecs register-task-definition --cli-input-json file://worker_task.json
aws ecs register-task-definition --cli-input-json file://adminer_task.json

# 4. Crear Servicios en ECS
$tgArn = (aws elbv2 describe-target-groups --names tg-poc-backend --query 'TargetGroups[0].TargetGroupArn' --output text)
$tgAdminerArn = (aws elbv2 describe-target-groups --names tg-poc-adminer --query 'TargetGroups[0].TargetGroupArn' --output text)

Write-Output "Actualizando/Creando servicio Backend..."
aws ecs update-service --cluster journal-cluster --service-name backend-service --task-definition poc-backend-task --force-new-deployment > $null 2>&1
aws ecs create-service --cluster journal-cluster --service-name backend-service --task-definition poc-backend-task --desired-count 1 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[subnet-01b94f155d091cb3b,subnet-03fd9c85b6ab5aaa1],securityGroups=[sg-043aada5e888dee01],assignPublicIp=DISABLED}" --load-balancers "targetGroupArn=$tgArn,containerName=backend,containerPort=8080" > $null 2>&1

Write-Output "Actualizando/Creando servicio Worker..."
aws ecs update-service --cluster journal-cluster --service-name worker-service --task-definition poc-worker-task --force-new-deployment > $null 2>&1
aws ecs create-service --cluster journal-cluster --service-name worker-service --task-definition poc-worker-task --desired-count 1 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[subnet-01b94f155d091cb3b,subnet-03fd9c85b6ab5aaa1],securityGroups=[sg-043aada5e888dee01],assignPublicIp=DISABLED}" > $null 2>&1

Write-Output "Actualizando/Creando servicio Adminer..."
aws ecs update-service --cluster journal-cluster --service-name adminer-service --task-definition poc-adminer-task --force-new-deployment > $null 2>&1
aws ecs create-service --cluster journal-cluster --service-name adminer-service --task-definition poc-adminer-task --desired-count 1 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[subnet-01b94f155d091cb3b,subnet-03fd9c85b6ab5aaa1],securityGroups=[sg-043aada5e888dee01],assignPublicIp=DISABLED}" --load-balancers "targetGroupArn=$tgAdminerArn,containerName=adminer,containerPort=8080" > $null 2>&1

Write-Output "5. Configurando Auto Scaling para Worker..."
aws application-autoscaling register-scalable-target --service-namespace ecs --resource-id service/journal-cluster/worker-service --scalable-dimension ecs:service:DesiredCount --min-capacity 1 --max-capacity 5
Set-Content -Path autoscaling.json -Value '{"TargetValue": 70.0, "PredefinedMetricSpecification": {"PredefinedMetricType": "ECSServiceAverageCPUUtilization"}, "ScaleOutCooldown": 60, "ScaleInCooldown": 60}'
aws application-autoscaling put-scaling-policy --service-namespace ecs --resource-id service/journal-cluster/worker-service --scalable-dimension ecs:service:DesiredCount --policy-name cpu-scaling --policy-type TargetTrackingScaling --target-tracking-scaling-policy-configuration file://autoscaling.json > $null
Remove-Item autoscaling.json

Write-Output "SERVICIOS ECS LANZADOS CON EXITO!"
