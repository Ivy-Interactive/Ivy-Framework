import React, { useEffect, useRef } from 'react';

interface FormWidgetProps {
  id: string;
  children?: React.ReactNode;
}

export const FormWidget: React.FC<FormWidgetProps> = ({ children }) => {
  const formRef = useRef<HTMLFormElement>(null);

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

        // Focus next input if exists
        if (nextInput) {
          nextInput.focus();
        }
      }
    };

    form.addEventListener('keydown', handleKeyDown);
    return () => form.removeEventListener('keydown', handleKeyDown);
  }, []);

  return <form ref={formRef}>{children}</form>;
};
