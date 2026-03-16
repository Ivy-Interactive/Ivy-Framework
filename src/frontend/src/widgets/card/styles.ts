import { Densities } from '@/types/density';

interface CardSizeClasses {
  header: string;
  content: string;
  contentNoHeader: string;
  footer: string;
  title: string;
  description: string;
  icon: string;
}

const baseHeaderClasses =
  'items-center [&>*]:w-full [&>*]:min-w-0 [&_:is(p,span)]:!pt-1 [&_:is(h1,h2,h3,h4,h5,h6)]:!my-0 [&_:is(h1,h2,h3,h4,h5,h6)]:!font-normal [&_:is(h1,h2,h3,h4,h5,h6)]:!min-h-9 [&_:is(h1,h2,h3,h4,h5,h6)]:!leading-9 [&_:is(h1,h2,h3,h4,h5,h6)]:!items-center [&_:is(h1,h2,h3,h4,h5,h6)]:!min-w-0 [&_:is(h1,h2,h3,h4,h5,h6)]:!truncate';

const iconSizeClasses = {
  small: '[&_svg]:!h-4 [&_svg]:!w-4 [&_svg]:!min-h-4 [&_svg]:!min-w-4',
  medium: '[&_svg]:!h-5 [&_svg]:!w-5 [&_svg]:!min-h-5 [&_svg]:!min-w-5',
  large: '[&_svg]:!h-6 [&_svg]:!w-6 [&_svg]:!min-h-6 [&_svg]:!min-w-6',
};

const baseContentClasses = 'break-words whitespace-normal';

export const cardStyles = {
  container: 'flex flex-col overflow-hidden gap-2',

  header: {
    base: 'flex-none',
  },

  content: {
    base: 'flex-1',
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

export const getSizeClasses = (density?: Densities): CardSizeClasses => {
  switch (density) {
    case Densities.Small:
      return {
        header: `px-3 pt-2 pb-1 ${baseHeaderClasses} ${iconSizeClasses.small} [&_:is(h1,h2,h3,h4,h5,h6)]:!text-sm [&_:is(h1,h2,h3,h4,h5,h6)]:!min-h-6 [&_:is(h1,h2,h3,h4,h5,h6)]:!leading-6`,
        content: `p-3 pt-0 [&_*]:!text-sm ${baseContentClasses}`,
        contentNoHeader: 'pt-3',
        footer: 'p-3 pt-0',
        title: 'text-sm',
        description: 'text-xs mt-1',
        icon: 'h-4 w-4',
      };
    case Densities.Large:
      return {
        header: `px-8 pt-6 pb-4 ${baseHeaderClasses} ${iconSizeClasses.large} [&_:is(h1,h2,h3,h4,h5,h6)]:!text-lg`,
        content: `p-8 pt-0 ${baseContentClasses}`,
        contentNoHeader: 'pt-8',
        footer: 'p-8 pt-0',
        title: 'text-lg',
        description: 'text-base mt-3',
        icon: 'h-6 w-6',
      };
    default:
      return {
        header: `px-6 pt-4 pb-2 ${baseHeaderClasses} ${iconSizeClasses.medium} [&_:is(h1,h2,h3,h4,h5,h6,p)]:!text-base`,
        content: `p-6 pt-0 ${baseContentClasses}`,
        contentNoHeader: 'pt-6',
        footer: 'p-6 pt-0',
        title: 'text-base',
        description: 'text-sm',
        icon: 'h-5 w-5',
      };
  }
};
