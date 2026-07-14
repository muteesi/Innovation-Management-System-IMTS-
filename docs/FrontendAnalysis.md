# Frontend Analysis

## Overview

This document captures the frontend contract that the new backend must support without changing the UI design.

## Pages and API Contract

### 1. Staff Notifications

- Page: staffuser/notifications.html
- Purpose: Display, filter, and manage notifications.
- Current Data Source: Hardcoded array plus localStorage persistence.
- Future API Endpoint: GET /api/notifications, GET /api/notifications/unread-count, PUT /api/notifications/{id}/read, PUT /api/notifications/read-all, DELETE /api/notifications/{id}
- Backend Service Responsible: Notification Service
- Required Database Table: Notifications
- Expected Request: GET /api/notifications?userId=staff-001&filter=all&page=1&pageSize=20
- Expected Response: { items: [{ id, title, message, type, date, read, actionUrl }], totalCount, page, pageSize }

### 2. Staff Resources

- Page: staffuser/resources.html
- Purpose: Browse and download innovation resources.
- Current Data Source: Hardcoded resources array.
- Future API Endpoint: GET /api/resources, GET /api/resources/download/{id}
- Backend Service Responsible: Resource Service
- Required Database Table: Resources
- Expected Request: GET /api/resources?category=templates&search=policy&page=1&pageSize=12
- Expected Response: { items: [{ id, title, description, category, type, size, icon, downloads, status, fileUrl }], totalCount, page, pageSize }

### 3. Innovation Team Resource Management

- Page: innovationteam/manageresources.html
- Purpose: Create, edit, delete, and summarize resources.
- Current Data Source: Hardcoded array and prompt-based editing.
- Future API Endpoint: GET /api/resources, POST /api/resources, PUT /api/resources/{id}, DELETE /api/resources/{id}, GET /api/resources/stats
- Backend Service Responsible: Resource Service
- Required Database Table: Resources
- Expected Request: POST /api/resources { title, description, category, fileType, fileName, fileUrl, status, version }
- Expected Response: { id }

### 4. Email Hooks

- Page: Various pages that trigger notifications and approvals.
- Purpose: Trigger emails for notifications and resource actions.
- Current Data Source: None; the frontend currently does not call email endpoints directly.
- Future API Endpoint: POST /api/email/send
- Backend Service Responsible: Email Service
- Required Database Table: EmailQueueItems
- Expected Request: POST /api/email/send { toAddress, subject, bodyHtml, templateKey }
- Expected Response: { id, status }
