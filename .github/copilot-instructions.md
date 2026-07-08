# Copilot Instructions

## Project Guidelines
- In Oqtane modules, localization should use full 5-character culture tags (for example, it-IT, en-US, es-ES) because Oqtane language packs are tracked/installed using specific culture codes.
- For module localization using a Resources folder, register localization with `ResourcesPath` set to "Resources" (for example, `AddLocalization(options => options.ResourcesPath = "Resources")`).
- Do not modify any files under the _Oqtane/oqtane.framework folder; only change files inside the user's module/workspace project.
- For the Oqtane DataRoom module, all destructive and scoped folder operations (delete, create, extract) must use server-side API endpoints that explicitly validate module authorization via `IsAuthorizedEntityId(EntityNames.Module, moduleId)` before executing. Direct client-side calls to framework services will bypass DataRoom permission checks and may allow unauthorized users to modify content if they have Edit Module permission. Secure endpoints should gate the operation first, then call the service.

## Notification Management
- For the Subscription feature in GIBS.Module.DataRoom, batch notifications into an hourly scheduled job and send one consolidated email per subscriber for new files added in that hour to avoid excessive emails.