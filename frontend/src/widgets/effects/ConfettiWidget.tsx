import confetti from "canvas-confetti";
import React, { useEffect, useRef } from "react";

interface ConfettiWidgetProps {
  children: React.ReactNode;
  trigger?: "Auto" | "Click" | "Hover";
}

const ConfettiWidget: React.FC<ConfettiWidgetProps> = ({
  children,
  trigger = "Click",
}) => {
  const elementRef = useRef<HTMLDivElement>(null);

  const triggerConfetti = (x: number, y: number) => {
    confetti({
      particleCount: 100,
      spread: 70,
      origin: { x, y },
      colors: ["#004734", "#006d4c", "#009464", "#00b97d", "#00df97"],
    });
  };

  const handleClick = (e: React.MouseEvent) => {
    if (trigger !== "Click") return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = (rect.left + rect.width / 2) / window.innerWidth;
    const y = (rect.top + rect.height / 2) / window.innerHeight;
    triggerConfetti(x, y);
  };

  const handleMouseEnter = (e: React.MouseEvent) => {
    if (trigger !== "Hover") return;
    const rect = e.currentTarget.getBoundingClientRect();
    const x = (rect.left + rect.width / 2) / window.innerWidth;
    const y = (rect.top + rect.height / 2) / window.innerHeight;
    triggerConfetti(x, y);
  };

  useEffect(() => {
    if (trigger === "Auto" && elementRef.current) {
      const rect = elementRef.current.getBoundingClientRect();
      const x = (rect.left + rect.width / 2) / window.innerWidth;
      const y = (rect.top + rect.height / 2) / window.innerHeight;
      triggerConfetti(x, y);
    }
  }, [trigger]);

  return (
    <div ref={elementRef} onClick={handleClick} onMouseEnter={handleMouseEnter}>
      {children}
    </div>
  );
};

export default ConfettiWidget;
