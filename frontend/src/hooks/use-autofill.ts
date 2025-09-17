import { useEffect, useRef } from 'react';
import { setAutofillStyling } from '@/lib/utils';

/**
 * Hook that automatically sets up autofill styling for input elements.
 * This ensures autofill text is visible in dark themes by adapting to the
 * background color of the container element.
 *
 * @param containerRef - Ref to the container element (optional, defaults to input's parent)
 * @returns Ref to attach to the input element
 */
export function useAutofillStyling(
  containerRef?: React.RefObject<HTMLElement>
) {
  const inputRef = useRef<HTMLInputElement | HTMLTextAreaElement>(null);

  useEffect(() => {
    const input = inputRef.current;
    if (!input) return;

    const container =
      containerRef?.current ||
      input.closest('[class*="bg-"]') ||
      input.parentElement;
    if (!container) return;

    // Set initial styling
    setAutofillStyling(container as HTMLElement, input);

    // Set up ResizeObserver to handle dynamic background changes
    const resizeObserver = new ResizeObserver(() => {
      setAutofillStyling(container as HTMLElement, input);
    });

    resizeObserver.observe(container);

    // Set up MutationObserver to handle class changes
    const mutationObserver = new MutationObserver(() => {
      setAutofillStyling(container as HTMLElement, input);
    });

    mutationObserver.observe(container, {
      attributes: true,
      attributeFilter: ['class', 'style'],
    });

    return () => {
      resizeObserver.disconnect();
      mutationObserver.disconnect();
    };
  }, [containerRef]);

  return inputRef;
}
