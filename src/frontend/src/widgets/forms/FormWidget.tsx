import React, { useEffect, useRef } from 'react';
import { useEventHandler } from '@/components/event-handler';

interface FormWidgetProps {
  id: string;
  children?: React.ReactNode;
}

export const FormWidget: React.FC<FormWidgetProps> = ({ id, children }) => {
  const formRef = useRef<HTMLDivElement>(null);
  const eventHandler = useEventHandler();

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

  return <div ref={formRef}>{children}</div>;
};
