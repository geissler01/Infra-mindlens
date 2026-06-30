import boto3
from src.config.settings import settings

boto3_session = boto3.Session()
if settings.AWS_ENDPOINT_URL:
    sqs = boto3_session.client('sqs', endpoint_url=settings.AWS_ENDPOINT_URL, region_name=settings.AWS_REGION)
    s3 = boto3_session.client('s3', endpoint_url=settings.AWS_ENDPOINT_URL, region_name=settings.AWS_REGION)
else:
    sqs = boto3_session.client('sqs', region_name=settings.AWS_REGION)
    s3 = boto3_session.client('s3', region_name=settings.AWS_REGION)

def receive_messages():
    response = sqs.receive_message(
        QueueUrl=settings.SQS_QUEUE_URL,
        MaxNumberOfMessages=1,
        WaitTimeSeconds=20
    )
    return response.get('Messages', [])

def delete_message(receipt_handle):
    sqs.delete_message(
        QueueUrl=settings.SQS_QUEUE_URL,
        ReceiptHandle=receipt_handle
    )

def download_audio(s3_key, local_path):
    s3.download_file(settings.BUCKET_NAME, s3_key, local_path)

def upload_audio(local_path, s3_key):
    s3.upload_file(local_path, settings.BUCKET_NAME, s3_key)
