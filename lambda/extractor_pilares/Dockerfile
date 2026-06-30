# Usamos la imagen oficial de AWS Lambda para Python 3.11
FROM public.ecr.aws/lambda/python:3.11

# Copiar requirements.txt
COPY requirements.txt ${LAMBDA_TASK_ROOT}

# Instalar dependencias
RUN pip install -r requirements.txt

# Copiar el código de la función
COPY app.py ${LAMBDA_TASK_ROOT}

# Comando por defecto que ejecuta AWS Lambda
CMD [ "app.lambda_handler" ]
