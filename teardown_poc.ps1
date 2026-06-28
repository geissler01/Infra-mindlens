# IMPORTANTE: Este script "apaga" las cosas que cuestan dinero por hora.

Write-Output "1. Apagando los contenedores en ECS (Fargate)..."
aws ecs update-service --cluster journal-cluster --service backend-service --desired-count 0
aws ecs update-service --cluster journal-cluster --service worker-service --desired-count 0

Write-Output "2. Borrando el Load Balancer (ALB)..."
$albArn = aws elbv2 describe-load-balancers --names alb-poc --query "LoadBalancers[0].LoadBalancerArn" --output text
if ($albArn -ne $null -and $albArn -ne "") {
    aws elbv2 delete-load-balancer --load-balancer-arn $albArn
    Start-Sleep -Seconds 5
}

Write-Output "3. Borrando el Target Group..."
$tgArn = aws elbv2 describe-target-groups --names tg-poc-backend --query "TargetGroups[0].TargetGroupArn" --output text
if ($tgArn -ne $null -and $tgArn -ne "") {
    aws elbv2 delete-target-group --target-group-arn $tgArn
}

Write-Output "¡Listo! Los contenedores y el Load Balancer están apagados/borrados."
Write-Output "NOTA: La Base de Datos (RDS) y los repositorios (ECR) siguen vivos. Si deseas borrar la DB, hazlo manualmente o dímelo para agregar el comando."
