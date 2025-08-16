/**
 * Logger utility for frontend debugging
 * Provides formatted timestamp logs with different log levels
 */

// Local storage key for developer options
const DEVELOPER_OPTIONS_KEY = 'ivy-developer-options';

// Global developer options state (cached from localStorage)
let developerOptions = {
  showDetailedLogging: false,
};

// Function to get developer options from localStorage
const getDeveloperOptionsFromStorage = () => {
  try {
    const stored = localStorage.getItem(DEVELOPER_OPTIONS_KEY);
    if (stored) {
      return JSON.parse(stored);
    }
  } catch (error) {
    console.warn('Failed to parse developer options from localStorage:', error);
  }
  return { showDetailedLogging: false };
};

// Initialize developer options from localStorage
const initializeDeveloperOptions = () => {
  developerOptions = getDeveloperOptionsFromStorage();
};

// Function to update developer options in localStorage and update cached state
export const setDeveloperOptions = (options: {
  showDetailedLogging: boolean;
}) => {
  try {
    const currentOptions = getDeveloperOptionsFromStorage();
    const newOptions = { ...currentOptions, ...options };
    localStorage.setItem(DEVELOPER_OPTIONS_KEY, JSON.stringify(newOptions));

    // Update cached state
    developerOptions = newOptions;

    console.info('Developer options updated:', newOptions);
  } catch (error) {
    console.warn('Failed to save developer options to localStorage:', error);
  }
};

// Function to get current developer options (from cache)
export const getCurrentDeveloperOptions = () => {
  return developerOptions;
};

// Global function to toggle developer options (accessible from console)
export const toggleDeveloperLogging = () => {
  const newValue = !developerOptions.showDetailedLogging;
  setDeveloperOptions({ showDetailedLogging: newValue });
  console.info(`Developer logging ${newValue ? 'enabled' : 'disabled'}`);
  return newValue;
};

// Make it available globally for console access
if (typeof window !== 'undefined') {
  // Initialize on first load
  initializeDeveloperOptions();

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  (window as any).toggleDeveloperLogging = toggleDeveloperLogging;
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  (window as any).getDeveloperOptions = getCurrentDeveloperOptions;
}

class Logger {
  /**
   * Format current time as HH:mm:ss:milliseconds
   */
  private formatTime(): string {
    const now = new Date();
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');
    const milliseconds = String(now.getMilliseconds()).padStart(3, '0');

    return `${hours}:${minutes}:${seconds}:${milliseconds}`;
  }

  /**
   * Log message with timestamp at debug level
   */
  debug(...args: unknown[]): void {
    if (developerOptions.showDetailedLogging) {
      console.info(`[${this.formatTime()}]`, ...args);
    }
  }

  /**
   * Log message with timestamp at info level
   */
  info(...args: unknown[]): void {
    console.info(`[${this.formatTime()}]`, ...args);
  }

  /**
   * Log message with timestamp at warn level
   */
  warn(...args: unknown[]): void {
    console.warn(`[${this.formatTime()}]`, ...args);
  }

  /**
   * Log message with timestamp at error level
   */
  error(...args: unknown[]): void {
    console.error(`[${this.formatTime()}]`, ...args);
  }
}

// Export singleton instance
export const logger = new Logger();
