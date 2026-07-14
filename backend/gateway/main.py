"""
API Gateway - Central entry point for all microservices
Routes requests to appropriate backend services
Port: 8000
"""

import sys
import os

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

from typing import Optional

import httpx
from fastapi import FastAPI, Request, Depends, HTTPException, status
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from pydantic import BaseModel

from backend.shared.auth_utils import get_current_user, create_access_token, verify_password
from backend.shared.config import (
    API_GATEWAY_PORT,
    AUTH_SERVICE_URL,
    IDEAS_SERVICE_URL,
    WORKFLOW_SERVICE_URL,
    RESOURCES_SERVICE_URL,
    REPORTING_SERVICE_URL,
    NOTIFICATION_SERVICE_URL,
)

app = FastAPI(
    title="IMTS API Gateway",
    description="Central API Gateway for Innovation Management System",
    version="1.0.0",
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ================================================================
# SERVICE ROUTING
# ================================================================
SERVICE_ROUTES = {
    "/api/auth": AUTH_SERVICE_URL,
    "/api/ideas": IDEAS_SERVICE_URL,
    "/api/workflow": WORKFLOW_SERVICE_URL,
    "/api/resources": RESOURCES_SERVICE_URL,
    "/api/reporting": REPORTING_SERVICE_URL,
    "/api/notifications": NOTIFICATION_SERVICE_URL,
}


async def proxy_request(service_base_url: str, path: str, request: Request):
    """Proxy a request to the appropriate microservice"""
    target_url = f"{service_base_url}{path}"

    # Get query parameters
    params = dict(request.query_params)

    # Get request body
    body = await request.body()

    # Get headers (excluding host)
    headers = dict(request.headers)
    headers.pop("host", None)

    timeout = httpx.Timeout(60.0)

    try:
        async with httpx.AsyncClient(timeout=timeout, verify=False) as client:
            response = await client.request(
                method=request.method,
                url=target_url,
                headers=headers,
                params=params,
                content=body,
            )

            # Try to return JSON, otherwise return raw content
            try:
                return JSONResponse(
                    content=response.json(),
                    status_code=response.status_code,
                    headers=dict(response.headers),
                )
            except Exception:
                return JSONResponse(
                    content={"detail": response.text},
                    status_code=response.status_code,
                )

    except httpx.ConnectError:
        return JSONResponse(
            content={
                "detail": f"Service unavailable: {service_base_url}",
                "service": service_base_url.split("//")[1].split(":")[0],
            },
            status_code=503,
        )
    except Exception as e:
        return JSONResponse(
            content={"detail": f"Proxy error: {str(e)}"},
            status_code=500,
        )


@app.api_route("/api/{service_type}/{path:path}", methods=["GET", "POST", "PUT", "DELETE", "PATCH"])
async def gateway_handler(service_type: str, path: str, request: Request):
    """Route requests to the appropriate microservice"""
    service_map = {
        "auth": AUTH_SERVICE_URL,
        "ideas": IDEAS_SERVICE_URL,
        "workflow": WORKFLOW_SERVICE_URL,
        "resources": RESOURCES_SERVICE_URL,
        "reporting": REPORTING_SERVICE_URL,
        "notifications": NOTIFICATION_SERVICE_URL,
    }

    service_base_url = service_map.get(service_type)
    if not service_base_url:
        return JSONResponse(
            content={"detail": f"Unknown service: {service_type}"},
            status_code=404,
        )

    full_path = f"/api/{service_type}/{path}" if path else f"/api/{service_type}"
    return await proxy_request(service_base_url, full_path, request)


# ================================================================
# HEALTH CHECK
# ================================================================
@app.get("/health")
async def health_check():
    """Check health of all microservices"""
    services = {
        "auth": AUTH_SERVICE_URL,
        "ideas": IDEAS_SERVICE_URL,
        "workflow": WORKFLOW_SERVICE_URL,
        "resources": RESOURCES_SERVICE_URL,
        "reporting": REPORTING_SERVICE_URL,
        "notifications": NOTIFICATION_SERVICE_URL,
    }

    statuses = {}
    all_healthy = True

    for name, url in services.items():
        try:
            async with httpx.AsyncClient(timeout=5.0) as client:
                response = await client.get(f"{url}/health", timeout=5.0)
                statuses[name] = "healthy" if response.status_code == 200 else "unhealthy"
                if response.status_code != 200:
                    all_healthy = False
        except Exception:
            statuses[name] = "unreachable"
            all_healthy = False

    return {
        "gateway": "healthy",
        "services": statuses,
        "all_healthy": all_healthy,
    }


@app.get("/")
async def root():
    """Gateway root endpoint"""
    return {
        "service": "IMTS API Gateway",
        "version": "1.0.0",
        "endpoints": {
            "auth": "/api/auth",
            "ideas": "/api/ideas",
            "workflow": "/api/workflow",
            "resources": "/api/resources",
            "reporting": "/api/reporting",
            "notifications": "/api/notifications",
        },
        "health": "/health",
        "docs": "/docs",
    }


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=API_GATEWAY_PORT)