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

// Test ID utilities
export interface TestIdProps {
  variant?: "Default" | "Slider";
  formatStyle?: "Decimal" | "Currency" | "Percent";
  currency?: string;
  disabled?: boolean;
  invalid?: string;
  nullable?: boolean;
  min?: number;
  max?: number;
  step?: number;
  precision?: number;
  placeholder?: string;
}

/**
 * Generates a descriptive test ID based on widget properties
 * @param props - Widget properties to generate test ID from
 * @returns A descriptive test ID string
 */
export function generateTestId(props: TestIdProps): string {
  const parts: string[] = [];
  
  // Add variant
  parts.push(props.variant === "Slider" ? "slider" : "number");
  
  // Add format style
  if (props.formatStyle) {
    parts.push(props.formatStyle.toLowerCase());
  }
  
  // Add currency if present
  if (props.currency) {
    parts.push(props.currency.toLowerCase());
  }
  
  // Add state indicators
  if (props.disabled) {
    parts.push("disabled");
  }
  
  if (props.invalid) {
    parts.push("invalid");
  }
  
  if (props.nullable) {
    parts.push("nullable");
  }
  
  // Add constraints
  if (props.min !== undefined) {
    parts.push(`min${props.min}`);
  }
  
  if (props.max !== undefined) {
    parts.push(`max${props.max}`);
  }
  
  if (props.step !== undefined && props.step !== 1) {
    parts.push(`step${props.step}`);
  }
  
  if (props.precision !== undefined && props.precision !== 2) {
    parts.push(`precision${props.precision}`);
  }
  
  // Add placeholder indicator
  if (props.placeholder) {
    parts.push("with-placeholder");
  }
  
  return parts.join("-");
}

/**
 * Returns the test ID only in development mode
 * @param testId - The test ID to conditionally render
 * @returns The test ID in development, undefined in production
 */
export function getTestId(testId: string): string | undefined {
  // In Vite, import.meta.env.DEV is true in development
  return import.meta.env.DEV ? testId : undefined;
}


