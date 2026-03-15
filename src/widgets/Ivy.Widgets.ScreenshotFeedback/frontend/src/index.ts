import { ScreenshotFeedback } from './ScreenshotFeedback';

if (typeof window !== 'undefined') {
  (window as unknown as Record<string, unknown>).Ivy_Widgets_ScreenshotFeedback = {
    ScreenshotFeedback,
  };
}

export { ScreenshotFeedback };
