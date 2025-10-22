import React, { useEffect, useRef } from 'react';
import { useEventHandler } from '@/components/event-handler';
import { logger } from '@/lib/logger';
import { Sizes } from '@/types/sizes';
import { cn } from '@/lib/utils';

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

  const getSizeClasses = (size: Sizes) => {
    switch (size) {
      case Sizes.Small:
        return 'gap-1 text-xs [&_input:not([type="checkbox"])]:text-xs [&_input:not([type="checkbox"])]:h-8 [&_input:not([type="checkbox"])]:px-2 [&_input:not([type="checkbox"])]:py-1 [&_textarea]:text-xs [&_textarea]:min-h-[60px] [&_select]:text-xs [&_select]:h-8 [&_label]:text-xs [&_p]:text-xs [&_button:not([role="checkbox"])]:text-xs [&_button:not([role="checkbox"])]:h-8 [&_button:not([role="checkbox"])]:px-3 [&_.field]:gap-1 [&_.field]:mt-0.5 [&_.field]:mb-1 [&_button[role="checkbox"]]:!h-3 [&_button[role="checkbox"]]:!w-3';
      case Sizes.Large:
        return 'gap-4 text-base [&_input:not([type="checkbox"])]:text-base [&_input:not([type="checkbox"])]:h-12 [&_input:not([type="checkbox"])]:px-4 [&_input:not([type="checkbox"])]:py-3 [&_textarea]:text-base [&_textarea]:min-h-[120px] [&_select]:text-base [&_select]:h-12 [&_label]:text-base [&_p]:text-base [&_button:not([role="checkbox"])]:text-base [&_button:not([role="checkbox"])]:h-12 [&_button:not([role="checkbox"])]:px-6 [&_.field]:gap-3 [&_.field]:mt-3 [&_.field]:mb-1.5 [&_button[role="checkbox"]]:!h-5 [&_button[role="checkbox"]]:!w-5';
      default:
        return 'gap-3 text-sm [&_input:not([type="checkbox"])]:text-sm [&_input:not([type="checkbox"])]:h-10 [&_input:not([type="checkbox"])]:px-3 [&_input:not([type="checkbox"])]:py-2 [&_textarea]:text-sm [&_textarea]:min-h-[80px] [&_select]:text-sm [&_select]:h-10 [&_label]:text-sm [&_p]:text-sm [&_button:not([role="checkbox"])]:text-sm [&_button:not([role="checkbox"])]:h-10 [&_button:not([role="checkbox"])]:px-4 [&_.field]:gap-2 [&_.field]:mt-1 [&_.field]:mb-2 [&_button[role="checkbox"]]:!h-4 [&_button[role="checkbox"]]:!w-4';
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
    <div ref={formRef} className={cn('flex flex-col', getSizeClasses(size))}>
      {children}
    </div>
  );
};
