aws ecr get-login-password --region us-east-2 | docker login --username AWS --password-stdin 016963912697.dkr.ecr.us-east-2.amazonaws.com
Write-Output "Building Backend..."
docker build -t poc-backend backend/
docker tag poc-backend:latest 016963912697.dkr.ecr.us-east-2.amazonaws.com/poc-backend:latest
Write-Output "Pushing Backend..."
docker push 016963912697.dkr.ecr.us-east-2.amazonaws.com/poc-backend:latest

Write-Output "Building Worker..."
docker build -t poc-worker worker/
docker tag poc-worker:latest 016963912697.dkr.ecr.us-east-2.amazonaws.com/poc-worker:latest
Write-Output "Pushing Worker..."
docker push 016963912697.dkr.ecr.us-east-2.amazonaws.com/poc-worker:latest
