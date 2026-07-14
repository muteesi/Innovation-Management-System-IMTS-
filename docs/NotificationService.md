# Notification Service

## Architecture

The Notification Service exposes REST endpoints for notification creation, retrieval, unread tracking, marking as read, and deletion. It uses ASP.NET Core Web API, Entity Framework Core, and SQL Server.

## Responsibilities

- Store notifications for staff users
- Support unread counts and filtering
- Support read-all and delete actions
- Provide extension points for future authentication and authorization

## Database Design

- Notifications
- NotificationTypes
- NotificationPreferences

## API Endpoints

- GET /api/notifications
- GET /api/notifications/unread-count
- POST /api/notifications
- PUT /api/notifications/{id}/read
- PUT /api/notifications/read-all
- DELETE /api/notifications/{id}

## Validation Rules

- Title and message are required
- RecipientUserId should be supplied for user-scoped notifications

## Security Considerations

- Authentication and role checks should be added later
- Soft delete is used for data safety
