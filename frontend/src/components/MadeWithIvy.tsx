'use client';
import { useState, useEffect } from 'react';
import IvyLogo from './IvyLogo';

export default function MadeWithIvy() {
  const [isHovered, setIsHovered] = useState(false);
  const [shouldShow, setShouldShow] = useState(false);

  useEffect(() => {
    const checkWindowSize = () => {
      setShouldShow(window.innerWidth >= 600 && window.innerHeight >= 600);
    };

    checkWindowSize();
    window.addEventListener('resize', checkWindowSize);

    return () => window.removeEventListener('resize', checkWindowSize);
  }, []);

  if (!shouldShow) return null;

  return (
    <div
      className="bg-primary fixed bottom-0 right-0 z-100 overflow-hidden rounded-tl-full "
      onMouseEnter={() => setIsHovered(true)}
      onMouseLeave={() => setIsHovered(false)}
    >
      <div
        className={`
          rounded-tl-full 
          flex 
          items-end 
          justify-end
          transition-all 
          duration-300 
          ease-in-out 
          origin-bottom-right
          cursor-pointer
          ${isHovered ? 'w-48 h-48' : 'w-16 h-16'}
          ${isHovered ? 'bg-primary-foreground' : 'bg-primary'}
        `}
        onClick={() => {
          window.open(
            'https://github.com/Ivy-Interactive/Ivy-Framework',
            '_blank'
          );
        }}
      >
        <div
          style={{ color: 'var(--primary)' }}
          className={`
            flex 
            flex-col
            items-right 
            gap-1.5
            transition-opacity 
            duration-300
            m-4
            ${isHovered ? 'opacity-100' : 'opacity-0'}
          `}
        >
          <span className="font-mono font-bold text-gray-400">MADE WITH</span>
          <IvyLogo className="w-24" />
        </div>
      </div>
    </div>
  );
}
