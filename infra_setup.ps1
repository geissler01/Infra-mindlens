$vpcId = "vpc-0c319e153cc4ecfb5"
$rtPublic = "rtb-06aa631d11ea8578a"
$rtPrivate = "rtb-0f59069ae123224a6"

Write-Output "Creando Subred Publica 2..."
$subPub2 = (aws ec2 create-subnet --vpc-id $vpcId --cidr-block 71.0.3.0/24 --availability-zone us-east-2b --query 'Subnet.SubnetId' --output text)
aws ec2 create-tags --resources $subPub2 --tags Key=Name,Value=S-Public-2
aws ec2 associate-route-table --subnet-id $subPub2 --route-table-id $rtPublic

Write-Output "Creando Subred Privada 2..."
$subPriv2 = (aws ec2 create-subnet --vpc-id $vpcId --cidr-block 71.0.4.0/24 --availability-zone us-east-2b --query 'Subnet.SubnetId' --output text)
aws ec2 create-tags --resources $subPriv2 --tags Key=Name,Value=S-Private-2
aws ec2 associate-route-table --subnet-id $subPriv2 --route-table-id $rtPrivate

Write-Output "Creando Security Groups..."
$sgAlb = (aws ec2 create-security-group --group-name alb-poc-sg --description "ALB POC" --vpc-id $vpcId --query 'GroupId' --output text)
$sgEcs = (aws ec2 create-security-group --group-name ecs-poc-sg --description "ECS POC" --vpc-id $vpcId --query 'GroupId' --output text)
$sgRds = (aws ec2 create-security-group --group-name rds-poc-sg --description "RDS POC" --vpc-id $vpcId --query 'GroupId' --output text)

Write-Output "Configurando Reglas de Security Groups..."
aws ec2 authorize-security-group-ingress --group-id $sgAlb --protocol tcp --port 80 --cidr 0.0.0.0/0
aws ec2 authorize-security-group-ingress --group-id $sgEcs --protocol tcp --port 80 --source-group $sgAlb
aws ec2 authorize-security-group-ingress --group-id $sgRds --protocol tcp --port 5432 --source-group $sgEcs

Write-Output "RESULTADOS:"
Write-Output "Public2: $subPub2"
Write-Output "Private2: $subPriv2"
Write-Output "ALB_SG: $sgAlb"
Write-Output "ECS_SG: $sgEcs"
Write-Output "RDS_SG: $sgRds"
