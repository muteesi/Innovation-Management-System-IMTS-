# Database Design

## Database Name

InnovationManagementDB

## Core Principles

- SQL Server + Entity Framework Core
- Code-first migrations
- Soft delete patterns via Deleted and DeletedDate
- Shared audit columns on every entity

## Main Tables

- Notifications
- NotificationTypes
- NotificationPreferences
- Resources
- ResourceCategories
- ResourceVersions
- DownloadLogs
- EmailQueueItems
- EmailLogs
- EmailTemplates

## Relationships

- Resources belong to categories
- ResourceVersions belong to a resource
- DownloadLogs belong to a resource
- Notifications are addressed to a recipient user
- Email queue items may reference templates
