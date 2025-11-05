import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';
import { textBlockClassMap } from './textBlockClassMap';
import routingConstants from '../routing-constants.json' assert { type: 'json' };

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

export function getAppId(): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  const appIdFromParams = urlParams.get('appId');
  if (appIdFromParams) {
    return appIdFromParams;
  }

  // If no appId parameter, try to parse from path
  const path = window.location.pathname.toLowerCase();
  const originalPath = window.location.pathname;

  // Skip if path is empty or just "/"
  if (!path || path === '/') {
    return null;
  }

  // Skip if path starts with any excluded pattern (must be exact segment match)
  if (
    routingConstants.excludedPaths.some(
      excluded => path === excluded || path.startsWith(excluded + '/')
    )
  ) {
    return null;
  }

  // Skip if path has a static file extension
  if (routingConstants.staticFileExtensions.some(ext => path.endsWith(ext))) {
    return null;
  }

  // Convert path to appId
  // Remove leading slash and use the rest as appId
  const appId = originalPath.replace(/^\/+/, '');

  // Only convert if the path looks like an app ID (contains at least one segment and no dots)
  if (appId && !appId.includes('.')) {
    return appId;
  }

  return null;
}

export function getAppArgs(): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('appArgs');
}

export function getParentId(): string | null {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('parentId');
}

export function getChromeParam(): boolean {
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('chrome')?.toLowerCase() !== 'false';
}

function generateUUID(): string {
  if (typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return '10000000-1000-4000-8000-100000000000'.replace(/[018]/g, (c: string) =>
    (
      Number(c) ^
      (crypto.getRandomValues(new Uint8Array(1))[0] & (15 >> (Number(c) / 4)))
    ).toString(16)
  );
}

export function getMachineId(): string {
  let id = localStorage.getItem('machineId');
  if (!id) {
    id = generateUUID();
    localStorage.setItem('machineId', id);
  }
  return id;
}

export function getIvyHost(): string {
  const urlParams = new URLSearchParams(window.location.search);
  const ivyHost = urlParams.get('ivyHost');
  if (ivyHost) return ivyHost;

  const metaHost = document
    .querySelector('meta[name="ivy-host"]')
    ?.getAttribute('content');
  if (metaHost) return metaHost;

  return window.location.origin;
}

export function camelCase(titleCase: unknown): unknown {
  if (typeof titleCase !== 'string') {
    return titleCase;
  }
  return titleCase.charAt(0).toLowerCase() + titleCase.slice(1);
}

// Shared Ivy tag-to-class map for headings, paragraphs, lists, tables, etc.
export const ivyTagClassMap = textBlockClassMap;
