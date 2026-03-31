# Shorten AppShell Query Parameter

## Summary

The query parameter for controlling AppShell visibility has been shortened from `?appshell=false` to `?shell=false` for better usability and conciseness.

## What Changed

### Frontend (TypeScript)
- Query parameter `?appshell=true|false` renamed to `?shell=true|false`
- Utility function `getAppShellParam()` renamed to `getShellParam()`

### Backend (C#)
- URL generation updated to use `shell=false` instead of `appshell=false`
- Query parameter parsing updated to read `shell` instead of `appshell`

## Before

```typescript
const isAppShell = getAppShellParam();
// URL: http://localhost:5000?appshell=false
```

## After

```typescript
const isShell = getShellParam();
// URL: http://localhost:5000?shell=false
```

## Migration Path

1. **Update URL parameters**: Replace `?appshell=false` with `?shell=false` in bookmarks, tests, and documentation
2. **Update code**: Replace `getAppShellParam()` calls with `getShellParam()` in custom frontend code
