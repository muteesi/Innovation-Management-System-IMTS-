# API Reference

## Base URLs

- Notification Service: <http://localhost:5242>
- Resource Service: <http://localhost:5201>
- Email Service: <http://localhost:5211>
- Gateway: <http://localhost:5177>

## Notification Endpoints

- GET /api/notifications
- GET /api/notifications/unread-count
- POST /api/notifications
- PUT /api/notifications/{id}/read
- PUT /api/notifications/read-all
- DELETE /api/notifications/{id}

## Resource Endpoints

- GET /api/resources
- GET /api/resources/stats
- POST /api/resources
- PUT /api/resources/{id}
- DELETE /api/resources/{id}
- GET /api/resources/download/{id}

## Email Endpoints

- POST /api/email/send
- GET /api/email/queue
- POST /api/email/templates/seed
