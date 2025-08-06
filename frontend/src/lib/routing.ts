import { getAppId, getAppArgs, getParentId } from './utils';

export interface RouteInfo {
  appId: string | null;
  appArgs: string | null;
  parentId: string | null;
  isMainApp: boolean;
}

export function parseRouteFromPath(pathname: string): RouteInfo {
  // Remove leading slash and split by '/'
  const path = pathname.replace(/^\/+/, '').split('/');

  // If no path or just empty, let the backend decide the default app
  if (path.length === 0 || (path.length === 1 && path[0] === '')) {
    return {
      appId: null, // Let backend determine default app
      appArgs: null,
      parentId: null,
      isMainApp: true,
    };
  }

  // For all routes, always load the Chrome app (which contains the sidebar)
  // The Chrome app will handle loading the specific app in its content area
  return {
    appId: '$chrome', // Always Chrome app
    appArgs: path.join('/'), // Pass the target app as appArgs
    parentId: null,
    isMainApp: true,
  };
}

export function getRouteFromUrl(): RouteInfo {
  return parseRouteFromPath(window.location.pathname);
}

export function updateRoute(appId: string | null, appArgs?: string) {
  if (!appId) {
    // Navigate to main app
    window.history.pushState(null, '', '/');
    return;
  }

  // Create clean URL for main app routes
  const cleanUrl = `/${appArgs || appId}`;
  window.history.pushState(null, '', cleanUrl);
}

export function getCurrentRoute(): RouteInfo {
  // Check if we have query parameters (embedded mode)
  const queryAppId = getAppId();
  if (queryAppId) {
    return {
      appId: queryAppId,
      appArgs: getAppArgs(),
      parentId: getParentId(),
      isMainApp: false,
    };
  }

  // Use path-based routing (main app mode)
  const route = getRouteFromUrl();

  if (route.appId === null) {
    return {
      appId: null,
      appArgs: null,
      parentId: null,
      isMainApp: true,
    };
  }

  return route;
}
