Write-Output "1. Creando IAM Role para ECS..."
aws iam create-role --role-name ecsTaskExecutionRolePoc --assume-role-policy-document '{"Version":"2012-10-17","Statement":[{"Effect":"Allow","Principal":{"Service":"ecs-tasks.amazonaws.com"},"Action":"sts:AssumeRole"}]}'
aws iam attach-role-policy --role-name ecsTaskExecutionRolePoc --policy-arn arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy
aws iam attach-role-policy --role-name ecsTaskExecutionRolePoc --policy-arn arn:aws:iam::aws:policy/AmazonSQSFullAccess

Write-Output "2. Creando ALB y Target Group..."
$subPub1 = "subnet-0a132011555a07bbf"
$subPub2 = "subnet-00202a95fb6350bb6"
$vpcId = "vpc-0c319e153cc4ecfb5"
$sgAlb = "sg-04b1dfe78c01df6c1"

$albArn = (aws elbv2 create-load-balancer --name alb-poc --subnets $subPub1 $subPub2 --security-groups $sgAlb --query 'LoadBalancers[0].LoadBalancerArn' --output text)

Write-Output "Creando Target Group para Backend..."
$tgArn = (aws elbv2 create-target-group --name tg-poc-backend --protocol HTTP --port 8080 --vpc-id $vpcId --target-type ip --health-check-path /api/journalings --query 'TargetGroups[0].TargetGroupArn' --output text)
aws elbv2 create-listener --load-balancer-arn $albArn --protocol HTTP --port 80 --default-actions Type=forward,TargetGroupArn=$tgArn

Write-Output "Creando Target Group para Adminer..."
$tgAdminerArn = (aws elbv2 create-target-group --name tg-poc-adminer --protocol HTTP --port 8080 --vpc-id $vpcId --target-type ip --health-check-path / --query 'TargetGroups[0].TargetGroupArn' --output text)
aws elbv2 create-listener --load-balancer-arn $albArn --protocol HTTP --port 8080 --default-actions Type=forward,TargetGroupArn=$tgAdminerArn

Write-Output "Ajustando Security Groups para puerto 8080..."
# Permitir trafico al ALB en el puerto 8080 (Adminer)
aws ec2 authorize-security-group-ingress --group-id $sgAlb --protocol tcp --port 8080 --cidr 0.0.0.0/0
# Permitir trafico a ECS desde ALB en el puerto 8080
$sgEcs = "sg-043aada5e888dee01"
aws ec2 authorize-security-group-ingress --group-id $sgEcs --protocol tcp --port 8080 --source-group $sgAlb

Write-Output "3. Creando ECS Cluster..."
aws ecs create-cluster --cluster-name journal-cluster

Write-Output "ALB_ARN: $albArn"
Write-Output "TG_ARN: $tgArn"
