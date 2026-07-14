# Resource Service

## Architecture

The Resource Service manages innovation resources and tracks downloads. Files are stored locally via a configurable storage path and only metadata is persisted in the database.

## Responsibilities

- Manage resource metadata and categories
- Support search, filtering, and pagination
- Track download counts
- Support CRUD operations for resource administrators

## Database Design

- Resources
- ResourceCategories
- ResourceVersions
- DownloadLogs

## API Endpoints

- GET /api/resources
- GET /api/resources/stats
- POST /api/resources
- PUT /api/resources/{id}
- DELETE /api/resources/{id}
- GET /api/resources/download/{id}

## Validation Rules

- Title, description, category, and status should be supplied.
- File metadata should be persisted even when a file is not uploaded.

## Security Considerations

- File storage path should be restricted and protected.
- Upload endpoints should be protected once authentication is enabled.
