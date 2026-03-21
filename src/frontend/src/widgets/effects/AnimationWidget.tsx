import React from 'react';
import {
  m,
  LazyMotion,
  domAnimation,
  AnimatePresence,
  Variants,
  Transition,
} from 'framer-motion';

interface AnimationWidgetProps {
  children?: React.ReactNode;
  type:
    | 'Rotate'
    | 'SlideIn'
    | 'FadeIn'
    | 'ZoomIn'
    | 'SlideOut'
    | 'FadeOut'
    | 'ZoomOut'
    | 'Bounce'
    | 'Shake'
    | 'Flip'
    | 'Stagger'
    | 'Wave'
    | 'Pulse'
    | 'Spring'
    | 'Hover';
  duration?: number;
  delay?: number;
  direction?: 'Left' | 'Right' | 'Up' | 'Down';
  distance?: number;
  easing?:
    | 'EaseIn'
    | 'EaseOut'
    | 'EaseInOut'
    | 'Linear'
    | 'CircIn'
    | 'CircOut'
    | 'CircInOut'
    | 'BackIn'
    | 'BackOut'
    | 'BackInOut'
    | 'Anticipate'
    | 'AnticipateOut'
    | 'BounceIn'
    | 'BounceOut'
    | 'BounceInOut'
    | 'ElasticIn'
    | 'ElasticOut'
    | 'ElasticInOut';
  repeat?: number | null;
  repeatDelay?: number;
  visible?: boolean;
  intensity?: number;
  trigger?: 'Auto' | 'Click' | 'Hover';
}

const getEasing = (easing?: string) => {
  switch (easing) {
    // Basic easings
    case 'EaseIn':
      return 'easeIn';
    case 'EaseOut':
      return 'easeOut';
    case 'EaseInOut':
      return 'easeInOut';
    case 'Linear':
      return 'linear';

    // Circular easings
    case 'CircIn':
      return 'circIn';
    case 'CircOut':
      return 'circOut';
    case 'CircInOut':
      return 'circInOut';

    // Back easings (with slight overshoot)
    case 'BackIn':
      return 'backIn';
    case 'BackOut':
      return 'backOut';
    case 'BackInOut':
      return 'backInOut';

    // Anticipate easings (with slight anticipation)
    case 'Anticipate':
    case 'AnticipateOut':
      return 'anticipate';

    // Bounce easings - map to valid easings
    case 'BounceIn':
    case 'BounceOut':
    case 'BounceInOut':
      return 'easeInOut';

    // Elastic easings - map to valid easings
    case 'ElasticIn':
    case 'ElasticOut':
    case 'ElasticInOut':
      return 'easeInOut';

    default:
      return 'easeInOut';
  }
};

const getDirectionOffset = (direction?: string, distance: number = 100) => {
  switch (direction) {
    case 'Left':
      return { x: -distance };
    case 'Right':
      return { x: distance };
    case 'Up':
      return { y: -distance };
    case 'Down':
      return { y: distance };
    default:
      return { x: 0, y: 0 };
  }
};

const getAnimationVariant = (props: AnimationWidgetProps): Variants => {
  const {
    type,
    duration = 0.5,
    delay = 0,
    direction,
    distance,
    easing,
    repeat = null,
    repeatDelay = 0,
    intensity = 1,
  } = props;

  const baseTransition: Transition = {
    duration,
    delay,
    ease: getEasing(easing),
    repeat: repeat === null || repeat === undefined ? Infinity : repeat,
    repeatDelay,
  };

  const directionOffset = getDirectionOffset(direction, distance);

  const variants: Variants = {
    initial: {},
    animate: {},
    exit: {},
  };

  switch (type) {
    case 'Rotate':
      variants.initial = { rotate: 0 };
      variants.animate = { rotate: 360, transition: baseTransition };
      variants.exit = { rotate: 0, transition: baseTransition };
      break;

    case 'SlideIn':
      variants.initial = { ...directionOffset, opacity: 0 };
      variants.animate = { x: 0, y: 0, opacity: 1, transition: baseTransition };
      variants.exit = {
        ...directionOffset,
        opacity: 0,
        transition: baseTransition,
      };
      break;

    case 'FadeIn':
      variants.initial = { opacity: 0 };
      variants.animate = { opacity: 1, transition: baseTransition };
      variants.exit = { opacity: 0, transition: baseTransition };
      break;

    case 'ZoomIn':
      variants.initial = { scale: 0, opacity: 0 };
      variants.animate = { scale: 1, opacity: 1, transition: baseTransition };
      variants.exit = { scale: 0, opacity: 0, transition: baseTransition };
      break;

    case 'SlideOut':
      variants.initial = { x: 0, y: 0, opacity: 1 };
      variants.animate = {
        ...directionOffset,
        opacity: 0,
        transition: baseTransition,
      };
      variants.exit = { x: 0, y: 0, opacity: 1, transition: baseTransition };
      break;

    case 'FadeOut':
      variants.initial = { opacity: 1 };
      variants.animate = { opacity: 0, transition: baseTransition };
      variants.exit = { opacity: 1, transition: baseTransition };
      break;

    case 'ZoomOut':
      variants.initial = { scale: 1, opacity: 1 };
      variants.animate = { scale: 0, opacity: 0, transition: baseTransition };
      variants.exit = { scale: 1, opacity: 1, transition: baseTransition };
      break;

    case 'Bounce':
      variants.initial = { y: 0 };
      variants.animate = {
        y: [0, -50 * intensity, 0],
        transition: {
          ...baseTransition,
          duration: duration * 2,
        },
      };
      variants.exit = { y: 0, transition: baseTransition };
      break;

    case 'Shake':
      variants.initial = { x: 0 };
      variants.animate = {
        x: [
          0,
          10 * intensity,
          -10 * intensity,
          10 * intensity,
          -10 * intensity,
          0,
        ],
        transition: {
          ...baseTransition,
          duration: duration * 2,
        },
      };
      variants.exit = { x: 0, transition: baseTransition };
      break;

    case 'Flip':
      variants.initial = { rotateX: 0 };
      variants.animate = { rotateX: 360, transition: baseTransition };
      variants.exit = { rotateX: 0, transition: baseTransition };
      break;

    case 'Stagger':
      variants.initial = { opacity: 0, y: 20 };
      variants.animate = {
        opacity: 1,
        y: 0,
        transition: {
          ...baseTransition,
          staggerChildren: 0.1,
        },
      };
      variants.exit = { opacity: 0, y: 20, transition: baseTransition };
      break;

    case 'Wave':
      variants.initial = { rotate: 0 };
      variants.animate = {
        rotate: [
          0,
          10 * intensity,
          -10 * intensity,
          10 * intensity,
          -10 * intensity,
          0,
        ],
        transition: {
          ...baseTransition,
          duration: duration * 2,
        },
      };
      variants.exit = { rotate: 0, transition: baseTransition };
      break;

    case 'Pulse':
      variants.initial = { scale: 1 };
      variants.animate = {
        scale: [1, 1.1 * intensity, 1],
        transition: {
          ...baseTransition,
          duration: duration * 2,
        },
      };
      variants.exit = { scale: 1, transition: baseTransition };
      break;

    case 'Spring':
      variants.initial = { scale: 0.8, opacity: 0 };
      variants.animate = {
        scale: 1,
        opacity: 1,
        transition: {
          type: 'spring',
          stiffness: 100 * intensity,
          damping: 10,
          mass: 1,
        },
      };
      variants.exit = { scale: 0.8, opacity: 0, transition: baseTransition };
      break;

    case 'Hover':
      variants.initial = { scale: 1 };
      variants.animate = {
        scale: 1.05 * intensity,
        transition: {
          type: 'spring',
          stiffness: 400,
          damping: 10,
        },
      };
      variants.exit = { scale: 1, transition: baseTransition };
      break;

    default:
      variants.initial = { opacity: 0 };
      variants.animate = { opacity: 1, transition: baseTransition };
      variants.exit = { opacity: 0, transition: baseTransition };
  }

  return variants;
};

const AnimationWidget: React.FC<AnimationWidgetProps> = props => {
  const defaults: AnimationWidgetProps = {
    trigger: 'Auto',
    visible: true,
    duration: 0.5,
    delay: 0,
    distance: 100,
    intensity: 1,
    easing: 'Linear',
    type: 'Rotate',
  };

  const merged = { ...defaults, ...props };

  const { children, visible, trigger } = merged;

  const [isAnimating, setIsAnimating] = React.useState(trigger === 'Auto');
  const variants = getAnimationVariant(merged);

  const handleClick = () => {
    if (trigger === 'Click') {
      setIsAnimating(true);
    }
  };

  const handleHoverStart = () => {
    if (trigger === 'Hover') {
      setIsAnimating(true);
    }
  };

  const handleHoverEnd = () => {
    if (trigger === 'Hover') {
      setIsAnimating(false);
    }
  };

  const handleAnimationComplete = () => {
    if (trigger === 'Click') {
      setIsAnimating(false);
    }
  };

  return (
    <AnimatePresence>
      {visible && (
        <LazyMotion features={domAnimation}>
          <m.div
            initial="initial"
            animate={isAnimating ? 'animate' : 'initial'}
            exit="exit"
            variants={variants}
            onClick={handleClick}
            onHoverStart={handleHoverStart}
            onHoverEnd={handleHoverEnd}
            onAnimationComplete={handleAnimationComplete}
            style={{
              cursor: trigger === 'Click' ? 'pointer' : 'default',
              display: 'inline-block',
              transformOrigin: 'center center',
            }}
          >
            {children}
          </m.div>
        </LazyMotion>
      )}
    </AnimatePresence>
  );
};

export default AnimationWidget;
