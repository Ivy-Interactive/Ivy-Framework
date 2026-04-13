import { cva } from "class-variance-authority";
import { densityHeight } from "../density-scale";

export const expandableTriggerVariant = cva(
  "w-full flex justify-between items-center cursor-pointer hover:bg-accent/50 rounded-box transition-colors disabled:cursor-not-allowed disabled:hover:bg-transparent overflow-hidden box-border shrink-0",
  {
    variants: {
      density: {
        Small: `${densityHeight.Small} px-2 py-1 gap-2`,
        Medium: `${densityHeight.Medium} px-3 py-2 gap-3`,
        Large: `${densityHeight.Large} px-4 py-3 gap-4`,
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const expandableHeaderVariant = cva(
  'flex-1 min-w-0 pointer-events-none [&_button]:pointer-events-auto [&_input]:pointer-events-auto [&_select]:pointer-events-auto [&_[role="button"]]:pointer-events-auto [&_[role="switch"]]:pointer-events-auto [&_[role="checkbox"]]:pointer-events-auto [&_a[href]]:pointer-events-auto [&_button]:cursor-pointer [&_input]:cursor-default [&_[role="switch"]]:cursor-pointer [&_[role="checkbox"]]:cursor-pointer [&_button]:text-foreground [&_[role="switch"]]:text-foreground [&_input]:text-foreground',
  {
    variants: {
      density: {
        Small: "ml-1 pr-6 [&_*]:text-xs",
        Medium: "pr-8 [&_*]:text-sm",
        Large: "-ml-1 pr-10 [&_*]:text-base",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const expandableChevronContainerVariant = cva(
  "absolute top-0 bottom-0 right-0 flex items-center justify-center pointer-events-none shrink-0 z-10",
  {
    variants: {
      density: {
        Small: "w-6",
        Medium: "w-8",
        Large: "w-10",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const expandableChevronVariant = cva(
  "opacity-50 shrink-0 transition-transform duration-200 ease-in-out",
  {
    variants: {
      density: {
        Small: "h-3 w-3",
        Medium: "h-4 w-4",
        Large: "h-5 w-5",
      },
    },
    defaultVariants: {
      density: "Medium",
    },
  },
);

export const expandableContentVariant = cva("overflow-hidden transition-all", {
  variants: {
    density: {
      Small: "pl-3 pr-2 py-1.5 space-y-1.5 [&_*]:text-xs",
      Medium: "pl-3 pr-3 py-2 space-y-2 [&_*]:text-sm",
      Large: "pl-3 pr-4 py-3 space-y-3 [&_*]:text-base",
    },
  },
  defaultVariants: {
    density: "Medium",
  },
});
