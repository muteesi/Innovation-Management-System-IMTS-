"""
Shared configuration for all microservices
"""

import os
from dotenv import load_dotenv

load_dotenv()

# JWT Configuration
JWT_SECRET = os.getenv("JWT_SECRET", "imts-jwt-secret-key-change-in-production-2024")
JWT_ALGORITHM = os.getenv("JWT_ALGORITHM", "HS256")
JWT_EXPIRATION_MINUTES = int(os.getenv("JWT_EXPIRATION_MINUTES", "60"))

# Service Ports
AUTH_SERVICE_PORT = int(os.getenv("AUTH_SERVICE_PORT", "8001"))
IDEAS_SERVICE_PORT = int(os.getenv("IDEAS_SERVICE_PORT", "8002"))
WORKFLOW_SERVICE_PORT = int(os.getenv("WORKFLOW_SERVICE_PORT", "8003"))
RESOURCES_SERVICE_PORT = int(os.getenv("RESOURCES_SERVICE_PORT", "8004"))
REPORTING_SERVICE_PORT = int(os.getenv("REPORTING_SERVICE_PORT", "8005"))
NOTIFICATION_SERVICE_PORT = int(os.getenv("NOTIFICATION_SERVICE_PORT", "8006"))
API_GATEWAY_PORT = int(os.getenv("API_GATEWAY_PORT", "8000"))

# Service URLs (for inter-service communication)
AUTH_SERVICE_URL = f"http://localhost:{AUTH_SERVICE_PORT}"
IDEAS_SERVICE_URL = f"http://localhost:{IDEAS_SERVICE_PORT}"
WORKFLOW_SERVICE_URL = f"http://localhost:{WORKFLOW_SERVICE_PORT}"
RESOURCES_SERVICE_URL = f"http://localhost:{RESOURCES_SERVICE_PORT}"
REPORTING_SERVICE_URL = f"http://localhost:{REPORTING_SERVICE_PORT}"
NOTIFICATION_SERVICE_URL = f"http://localhost:{NOTIFICATION_SERVICE_PORT}"

# Email
SMTP_SERVER = os.getenv("SMTP_SERVER", "smtp.gmail.com")
SMTP_PORT = int(os.getenv("SMTP_PORT", "587"))
SMTP_USERNAME = os.getenv("SMTP_USERNAME", "")
SMTP_PASSWORD = os.getenv("SMTP_PASSWORD", "")
NOTIFICATION_EMAIL_FROM = os.getenv("NOTIFICATION_EMAIL_FROM", "imts@bankofuganda.org")