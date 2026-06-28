import os

SQS_QUEUE_URL = os.getenv('SQS_QUEUE_URL', '')
AWS_ENDPOINT_URL = os.getenv('AWS_ENDPOINT_URL', '')
AWS_REGION = os.getenv('AWS_REGION', 'us-east-1')

DB_HOST = os.getenv('DB_HOST', 'localhost')
DB_PASS = os.getenv('DB_PASS', 'postgres')
DB_USER = os.getenv('DB_USER', 'journal_user')
DB_NAME = os.getenv('DB_NAME', 'journal_db')

OPENAI_API_KEY = os.getenv('OPENAI_API_KEY', '')

BUCKET_NAME = "journal-audios-bucket"
