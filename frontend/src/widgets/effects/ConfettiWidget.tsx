import React, { useEffect, useRef, useCallback, useMemo } from 'react';
import confetti from 'canvas-confetti';

interface ConfettiWidgetProps {
  children: React.ReactNode;
  trigger?: 'Auto' | 'Click' | 'Hover';
}

const ConfettiWidget: React.FC<ConfettiWidgetProps> = ({
  children,
  trigger = 'Click',
}) => {
  const elementRef = useRef<HTMLDivElement>(null);

  const quadrant = useMemo(
    () =>
      confetti.shapeFromPath({
        path: 'M47 0H0V47.0222C25.9234 47.0222 47 25.9801 47 0Z',
      }),
    []
  );

  const confettiConfig = useMemo(
    () => ({
      particleCount: 100,
      spread: 70,
      shapes: [quadrant],
      colors: ['#00CC92', '#0D4A2F'],
    }),
    [quadrant]
  );

  const triggerConfetti = useCallback(
    (x: number, y: number) => {
      confetti({
        ...confettiConfig,
        origin: { x, y },
      });
    },
    [confettiConfig]
  );

  const handleClick = (e: React.MouseEvent) => {
    if (trigger !== 'Click') return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = (rect.left + rect.width / 2) / window.innerWidth;
    const y = (rect.top + rect.height / 2) / window.innerHeight;
    triggerConfetti(x, y);
  };

  const handleMouseEnter = (e: React.MouseEvent) => {
    if (trigger !== 'Hover') return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = (rect.left + rect.width / 2) / window.innerWidth;
    const y = (rect.top + rect.height / 2) / window.innerHeight;
    triggerConfetti(x, y);
  };

  useEffect(() => {
    if (trigger === 'Auto' && elementRef.current) {
      const rect = elementRef.current.getBoundingClientRect();
      const x = (rect.left + rect.width / 2) / window.innerWidth;
      const y = (rect.top + rect.height / 2) / window.innerHeight;
      confetti({
        ...confettiConfig,
        origin: { x, y },
      });
    }
  }, [trigger, confettiConfig]);

  return (
    <div ref={elementRef} onClick={handleClick} onMouseEnter={handleMouseEnter}>
      {children}
    </div>
  );
};

export default ConfettiWidget;
