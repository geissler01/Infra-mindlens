# 1. Crear Role (Fix)
aws iam create-role --role-name ecsTaskExecutionRolePoc --assume-role-policy-document file://trust_policy.json
aws iam attach-role-policy --role-name ecsTaskExecutionRolePoc --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
aws iam attach-role-policy --role-name ecsTaskExecutionRolePoc --policy-arn arn:aws:iam::aws:policy/AmazonSQSFullAccess
Start-Sleep -Seconds 10 # Esperar a que IAM propague

# 2. Log Groups
aws logs create-log-group --log-group-name /ecs/poc-backend
aws logs create-log-group --log-group-name /ecs/poc-worker

# 3. Registrar Task Definitions
aws ecs register-task-definition --cli-input-json file://backend_task.json
aws ecs register-task-definition --cli-input-json file://worker_task.json

# 4. Crear Servicios en ECS
$tgArn = "arn:aws:elasticloadbalancing:us-east-2:016963912697:targetgroup/tg-poc-backend/89e55fd0c8388bd9"
aws ecs create-service --cluster journal-cluster --service-name backend-service --task-definition poc-backend-task --desired-count 1 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[subnet-01b94f155d091cb3b,subnet-03fd9c85b6ab5aaa1],securityGroups=[sg-043aada5e888dee01],assignPublicIp=DISABLED}" --load-balancers "targetGroupArn=$tgArn,containerName=backend,containerPort=8080"
aws ecs create-service --cluster journal-cluster --service-name worker-service --task-definition poc-worker-task --desired-count 1 --launch-type FARGATE --network-configuration "awsvpcConfiguration={subnets=[subnet-01b94f155d091cb3b,subnet-03fd9c85b6ab5aaa1],securityGroups=[sg-043aada5e888dee01],assignPublicIp=DISABLED}"

Write-Output "SERVICIOS ECS LANZADOS CON EXITO!"
