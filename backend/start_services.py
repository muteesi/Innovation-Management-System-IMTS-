"""
IMTS Microservices Startup Script
Starts all microservices in separate processes
"""

import subprocess
import sys
import os
import time
import signal

# Add backend to path
sys.path.insert(0, os.path.dirname(__file__))

from shared.config import (
    AUTH_SERVICE_PORT,
    IDEAS_SERVICE_PORT,
    WORKFLOW_SERVICE_PORT,
    RESOURCES_SERVICE_PORT,
    REPORTING_SERVICE_PORT,
    NOTIFICATION_SERVICE_PORT,
    API_GATEWAY_PORT,
)

SERVICES = [
    ("Auth Service", "services.auth_service.main", AUTH_SERVICE_PORT),
    ("Ideas Service", "services.ideas_service.main", IDEAS_SERVICE_PORT),
    ("Workflow Service", "services.workflow_service.main", WORKFLOW_SERVICE_PORT),
    ("Resources Service", "services.resources_service.main", RESOURCES_SERVICE_PORT),
    ("Reporting Service", "services.reporting_service.main", REPORTING_SERVICE_PORT),
    ("Notification Service", "services.notification_service.main", NOTIFICATION_SERVICE_PORT),
    ("API Gateway", "gateway.main", API_GATEWAY_PORT),
]

processes = []


def start_service(name: str, module: str, port: int):
    """Start a service in a subprocess"""
    cmd = [
        sys.executable,
        "-m", "uvicorn",
        f"{module}:app",
        "--host", "0.0.0.0",
        "--port", str(port),
        "--reload",
    ]
    print(f"🚀 Starting {name} on port {port}...")
    proc = subprocess.Popen(
        cmd,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        universal_newlines=True,
        bufsize=1,
    )
    processes.append(proc)
    return proc


def stop_all():
    """Stop all running services"""
    print("\n🛑 Stopping all services...")
    for proc in processes:
        proc.terminate()
    for proc in processes:
        proc.wait(timeout=5)
    print("✅ All services stopped.")


def main():
    print("=" * 60)
    print("🏦 IMTS - Innovation Management System")
    print("Starting all microservices...")
    print("=" * 60)

    # Start all services
    procs = []
    for name, module, port in SERVICES:
        proc = start_service(name, module, port)
        procs.append((name, proc))

    # Wait for services to start
    print("\n⏳ Waiting for services to initialize...")
    time.sleep(3)

    print("\n" + "=" * 60)
    print("✅ All services started!")
    print("=" * 60)
    for name, _, port in SERVICES:
        print(f"   {name}: http://localhost:{port}")
    print(f"\n🌐 API Gateway: http://localhost:{API_GATEWAY_PORT}")
    print(f"📖 API Docs: http://localhost:{API_GATEWAY_PORT}/docs")
    print("=" * 60)
    print("Press Ctrl+C to stop all services.\n")

    try:
        # Wait for all processes
        for name, proc in procs:
            proc.wait()
    except KeyboardInterrupt:
        pass
    finally:
        stop_all()


if __name__ == "__main__":
    main()