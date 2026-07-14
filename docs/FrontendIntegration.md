# Frontend Integration

## Integration Notes

The UI remains visually unchanged. The frontend now relies on gateway endpoints that return the same field names used by the existing pages.

## Expected Payload Shapes

- Notifications: { id, title, message, type, date, read, actionUrl }
- Resources: { id, title, description, category, type, size, icon, downloads, status, fileUrl, previewMetadata }
- Download: { success, fileUrl }
- Email: { id, status }

## Current Compatibility

The resource pages expect category values such as policy, templates, training, research, and tools. The notification page expects notification.type values like success, info, comment, and system.
