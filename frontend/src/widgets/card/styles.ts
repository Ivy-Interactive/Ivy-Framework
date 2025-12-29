import { Scales } from '@/types/scale';

interface CardSizeClasses {
  header: string;
  content: string;
  footer: string;
  title: string;
  description: string;
  icon: string;
}

const baseHeaderClasses =
  'items-center [&>*]:w-full [&_:is(p,span)]:!pt-1 [&_:is(h1,h2,h3,h4,h5,h6)]:!my-0 [&_:is(h1,h2,h3,h4,h5,h6)]:!font-normal [&_:is(h1,h2,h3,h4,h5,h6)]:!min-h-9 [&_:is(h1,h2,h3,h4,h5,h6)]:!flex [&_:is(h1,h2,h3,h4,h5,h6)]:!flex-1 [&_:is(h1,h2,h3,h4,h5,h6)]:!items-center';

const baseContentClasses = 'break-words whitespace-normal';

export const cardStyles = {
  container: 'flex flex-col overflow-hidden gap-2',

  header: {
    base: 'flex-none',
  },

  content: {
    base: 'flex-1',
    noHeader: 'pt-6',
  },

  footer: {
    base: 'flex-none',
  },

  hover: {
    none: null,
    pointer: 'cursor-pointer',
    pointerAndTranslate:
      'cursor-pointer transform hover:-translate-x-[4px] hover:-translate-y-[4px] active:translate-x-[-2px] active:translate-y-[-2px] transition',
  },
} as const;

export const getSizeClasses = (scale?: Scales): CardSizeClasses => {
  switch (scale) {
    case Scales.Small:
      return {
        header: `px-3 pt-1 pb-0 ${baseHeaderClasses} [&_:is(h1,h2,h3,h4,h5,h6)]:!text-sm`,
        content: `p-3 pt-0 [&_*]:!text-sm ${baseContentClasses}`,
        footer: 'p-3 pt-0',
        title: 'text-sm',
        description: 'text-xs mt-1',
        icon: 'h-4 w-4',
      };
    case Scales.Large:
      return {
        header: `px-8 pt-5 pb-4 ${baseHeaderClasses} [&_:is(h1,h2,h3,h4,h5,h6)]:!text-lg`,
        content: `p-8 pt-0 ${baseContentClasses}`,
        footer: 'p-8 pt-0',
        title: 'text-lg',
        description: 'text-base mt-3',
        icon: 'h-6 w-6',
      };
    default:
      return {
        header: `px-6 pt-4 pb-2 ${baseHeaderClasses} [&_:is(h1,h2,h3,h4,h5,h6,p)]:!text-base`,
        content: `p-6 pt-0 ${baseContentClasses}`,
        footer: 'p-6 pt-0',
        title: 'text-base',
        description: 'text-sm',
        icon: 'h-5 w-5',
      };
  }
};
