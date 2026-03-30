import { cva } from "class-variance-authority";

export const badgeVariant = cva(
  "inline-flex items-center rounded-selector border font-normal leading-none transition-colors focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2",
  {
    variants: {
      variant: {
        primary: "border-transparent bg-primary text-primary-foreground shadow",
        secondary: "border-transparent bg-secondary text-secondary-foreground",
        destructive: "border-transparent bg-destructive text-destructive-foreground shadow",
        outline: "text-foreground",
        success: "border-transparent bg-success text-success-foreground shadow",
        warning: "border-transparent bg-warning text-warning-foreground shadow",
        info: "border-transparent bg-info text-info-foreground shadow",
      },
      density: {
        medium: "px-2 py-0.5 text-xs",
        small: "px-1 py-0 text-[10px]",
        large: "px-3 py-1 text-sm",
      },
    },
    defaultVariants: {
      variant: "primary",
      density: "medium",
    },
  },
);
