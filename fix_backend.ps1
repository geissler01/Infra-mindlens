Write-Output "Agregando paquete faltante a C#..."
cd backend
dotnet add package AWSSDK.Extensions.NETCore.Setup
cd ..

Write-Output "Buildeando Backend..."
docker build -t poc-backend backend/
docker tag poc-backend:latest 016963912697.dkr.ecr.us-east-2.amazonaws.com/poc-backend:latest

Write-Output "Pusheando Backend..."
docker push 016963912697.dkr.ecr.us-east-2.amazonaws.com/poc-backend:latest

Write-Output "Actualizando ECS..."
aws ecs update-service --cluster journal-cluster --service backend-service --force-new-deployment
Write-Output "LISTO"
