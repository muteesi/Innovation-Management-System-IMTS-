# Email Service

## Architecture

The Email Service centralizes outbound email handling through a queue-backed design. It accepts send requests, stores them in an email queue, and exposes templates for standard Bank of Uganda communications.

## Responsibilities

- Queue outbound emails
- Store delivery logs
- Support template-based messaging
- Provide extension points for SMTP and future providers

## Database Design

- EmailQueueItems
- EmailLogs
- EmailTemplates

## API Endpoints

- POST /api/email/send
- GET /api/email/queue
- POST /api/email/templates/seed

## Validation Rules

- To address and subject are required.
- Template keys should exist when templates are used.

## Security Considerations

- SMTP credentials should be moved to configuration and secrets management.
- The current implementation is ready for provider abstraction.
