import { useEffect, useRef } from 'react';
import { setAutofillStyling } from '@/lib/utils';

// Global observers to avoid creating multiple observers per input
let globalResizeObserver: ResizeObserver | null = null;
let globalMutationObserver: MutationObserver | null = null;
const observedInputs = new WeakMap<
  HTMLElement,
  Set<HTMLInputElement | HTMLTextAreaElement>
>();

// Performance monitoring (can be removed in production)
let observerCreationCount = 0;
let inputRegistrationCount = 0;

// Simple logger that can be easily disabled
const logger = {
  info: (message: string) => {
    // Logging disabled by default - uncomment next line to enable during development
    // console.log(message);
    void message; // Suppress unused parameter warning
  },
};

function getGlobalResizeObserver(): ResizeObserver {
  if (!globalResizeObserver) {
    observerCreationCount++;
    logger.info(
      `🔍 Created global ResizeObserver (${observerCreationCount} total observers)`
    );
    globalResizeObserver = new ResizeObserver(entries => {
      for (const entry of entries) {
        const container = entry.target as HTMLElement;
        const inputs = observedInputs.get(container);
        if (inputs) {
          inputs.forEach(input => {
            setAutofillStyling(container, input);
          });
        }
      }
    });
  }
  return globalResizeObserver;
}

function getGlobalMutationObserver(): MutationObserver {
  if (!globalMutationObserver) {
    observerCreationCount++;
    logger.info(
      `🔍 Created global MutationObserver (${observerCreationCount} total observers)`
    );
    globalMutationObserver = new MutationObserver(mutations => {
      for (const mutation of mutations) {
        if (
          mutation.type === 'attributes' &&
          (mutation.attributeName === 'class' ||
            mutation.attributeName === 'style')
        ) {
          const container = mutation.target as HTMLElement;
          const inputs = observedInputs.get(container);
          if (inputs) {
            inputs.forEach(input => {
              setAutofillStyling(container, input);
            });
          }
        }
      }
    });
  }
  return globalMutationObserver;
}

/**
 * Hook that automatically sets up autofill styling for input elements.
 * This ensures autofill text is visible in dark themes by adapting to the
 * background color of the container element.
 *
 * Uses global observers for better performance with many inputs.
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

    const containerElement = container as HTMLElement;

    // Set initial styling
    setAutofillStyling(containerElement, input);

    // Add input to the global observer system
    if (!observedInputs.has(containerElement)) {
      observedInputs.set(containerElement, new Set());

      // Start observing this container with global observers
      getGlobalResizeObserver().observe(containerElement);
      getGlobalMutationObserver().observe(containerElement, {
        attributes: true,
        attributeFilter: ['class', 'style'],
      });
    }

    observedInputs.get(containerElement)!.add(input);
    inputRegistrationCount++;
    logger.info(
      `📝 Registered input ${inputRegistrationCount} (${observedInputs.get(containerElement)!.size} inputs in this container)`
    );

    return () => {
      const inputs = observedInputs.get(containerElement);
      if (inputs) {
        inputs.delete(input);

        // If no more inputs are observing this container, stop observing it
        if (inputs.size === 0) {
          observedInputs.delete(containerElement);
          getGlobalResizeObserver().unobserve(containerElement);
          // Note: MutationObserver doesn't have unobserve, but it's fine to leave it
          // as it will just not trigger callbacks for elements not in our WeakMap
        }
      }
    };
  }, [containerRef]);

  return inputRef;
}
