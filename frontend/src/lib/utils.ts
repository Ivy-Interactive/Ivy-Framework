import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

export function getAppId():string|null { 
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('appId');
}

export function getAppArgs():string|null { 
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('appArgs');
}

export function getParentId():string|null { 
  const urlParams = new URLSearchParams(window.location.search);
  return urlParams.get('parentId');
}

function generateUUID(): string {
  if (typeof crypto.randomUUID === 'function') {
    return crypto.randomUUID();
  }
  return '10000000-1000-4000-8000-100000000000'.replace(/[018]/g, (c: string) =>
    (Number(c) ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> Number(c) / 4).toString(16)
  );
}

export function getMachineId(): string {
  let id = localStorage.getItem("machineId");
  if (!id) {
    id = generateUUID();
    localStorage.setItem("machineId", id);
  }
  return id;
}

export function getIvyHost():string {
  const urlParams = new URLSearchParams(window.location.search);
  const ivyHost = urlParams.get('ivyHost');
  if (ivyHost) return ivyHost;

  const metaHost = document.querySelector('meta[name="ivy-host"]')?.getAttribute('content');
  if (metaHost) return metaHost;
  
  return window.location.origin;
}

export function camelCase(titleCase: unknown): unknown {
  if (typeof titleCase !== 'string') {
    return titleCase;
  }
  return titleCase.charAt(0).toLowerCase() + titleCase.slice(1);
}