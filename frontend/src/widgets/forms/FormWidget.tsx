import React, { useEffect, useRef, createContext, useContext } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { logger } from '@/lib/logger';
import { Sizes } from '@/types/sizes';
import { cn } from '@/lib/utils';

// Create a context to pass form size to child components
const FormSizeContext = createContext<Sizes>(Sizes.Medium);

// Hook to use form size in child components
export const useFormSize = () => useContext(FormSizeContext);

interface FormWidgetProps {
  id: string;
  children?: React.ReactNode;
  size?: Sizes;
}

export const FormWidget: React.FC<FormWidgetProps> = ({
  id,
  children,
  size = Sizes.Medium,
}) => {
  const formRef = useRef<HTMLDivElement>(null);
  const eventHandler = useEventHandler();

  // Determine font size and spacing based on size
  const getSizeClasses = (size: Sizes) => {
    switch (size) {
      case Sizes.Small:
        return 'text-xs gap-2 [&_input]:text-xs [&_input]:h-8 [&_input]:px-2 [&_input]:py-1 [&_textarea]:text-xs [&_textarea]:min-h-[60px] [&_select]:text-xs [&_select]:h-8 [&_label]:text-xs [&_p]:text-xs [&_button]:text-xs [&_button]:h-8 [&_button]:px-3';
      case Sizes.Large:
        return 'text-base gap-12 [&_input]:text-base [&_input]:h-12 [&_input]:px-4 [&_input]:py-3 [&_textarea]:text-base [&_textarea]:min-h-[120px] [&_select]:text-base [&_select]:h-12 [&_label]:text-base [&_p]:text-base [&_button]:text-base [&_button]:h-12 [&_button]:px-6';
      default:
        return 'text-sm gap-6 [&_input]:text-sm [&_input]:h-10 [&_input]:px-3 [&_input]:py-2 [&_textarea]:text-sm [&_textarea]:min-h-[80px] [&_select]:text-sm [&_select]:h-10 [&_label]:text-sm [&_p]:text-sm [&_button]:text-sm [&_button]:h-10 [&_button]:px-4';
    }
  };

  useEffect(() => {
    const form = formRef.current;
    if (!form) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      const target = e.target;
      if (
        e.key === 'Enter' &&
        target instanceof HTMLElement &&
        (target.tagName === 'INPUT' || target.tagName === 'SELECT')
      ) {
        e.preventDefault();

        // Find all inputs in the form
        const inputs = Array.from(
          form.querySelectorAll('input:not([type="hidden"]), textarea, select')
        ).filter(
          el =>
            !el.hasAttribute('disabled') &&
            (el as HTMLElement).offsetParent !== null
        ) as HTMLElement[];

        const currentIndex = inputs.indexOf(target);
        const nextInput = inputs[currentIndex + 1];

        // Blur current (triggers backend validation via OnBlur)
        target.blur();

        // If there's a next input, focus it
        if (nextInput) {
          nextInput.focus();
        } else {
          // We're on the last field - check for invalid fields
          const invalidInputs = inputs.filter(input => {
            const parent = input.closest('[class*="flex-col"]');
            return (
              parent?.querySelector('[class*="text-destructive"]') !== null
            );
          });

          if (invalidInputs.length > 0) {
            // Navigate to first invalid field instead of submitting
            invalidInputs[0].focus();
          } else {
            // All fields valid - submit the form
            logger.info(`Form submit triggered via Enter key on last field`, {
              formId: id,
            });
            eventHandler('OnSubmit', id, []);
          }
        }
      }
    };

    form.addEventListener('keydown', handleKeyDown);
    return () => {
      form.removeEventListener('keydown', handleKeyDown);
    };
  }, [id, eventHandler]);

  return (
    <FormSizeContext.Provider value={size}>
      <div ref={formRef} className={cn('flex flex-col', getSizeClasses(size))}>
        {children}
      </div>
    </FormSizeContext.Provider>
  );
};
