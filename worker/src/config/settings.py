import os
from pydantic import BaseModel

class Settings(BaseModel):
    # AWS & SQS
    AWS_REGION: str = os.getenv("AWS_REGION", "us-east-1")
    AWS_ENDPOINT_URL: str = os.getenv("AWS_ENDPOINT_URL", "http://localstack:4566")
    SQS_QUEUE_URL: str = os.getenv("SQS_QUEUE_URL", "http://localstack:4566/000000000000/journal-processing-queue")
    BUCKET_NAME: str = os.getenv("BUCKET_NAME", "journal-audios")
    
    # OpenAI
    OPENAI_API_KEY: str = os.getenv("OPENAI_API_KEY", "")

    # Tenant DB (Postgres PgVector)
    TENANT_DB_HOST: str = os.getenv("TENANT_DB_HOST", "db")
    TENANT_DB_NAME: str = os.getenv("TENANT_DB_NAME", "journal_db")
    TENANT_DB_USER: str = os.getenv("TENANT_DB_USER", "journal_user")
    TENANT_DB_PASS: str = os.getenv("TENANT_DB_PASS", "journal_pass")
    
    # Master DB (Global Routing)
    MASTER_DB_HOST: str = os.getenv("MASTER_DB_HOST", "db_master")
    MASTER_DB_NAME: str = os.getenv("MASTER_DB_NAME", "master_db")
    MASTER_DB_USER: str = os.getenv("MASTER_DB_USER", "master_user")
    MASTER_DB_PASS: str = os.getenv("MASTER_DB_PASS", "master_pass")

settings = Settings()
