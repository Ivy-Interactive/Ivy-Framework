import React from "react";
import type { VariantProps } from "class-variance-authority";

import { cn } from "@/lib/utils";
import {
  detailLabelSizeVariant,
  detailValueSizeVariant,
  detailValueMultiLinePaddingVariant,
  detailsSizeVariant,
} from "./detail/detail-variant";
import { DetailProvider } from "./detail/DetailContext";
import { useDetailDensity } from "./detail/useDetailDensity";
import { Densities } from "@/types/density";

export interface DetailsProps
  extends
    Omit<React.HTMLAttributes<HTMLDivElement>, "size">,
    VariantProps<typeof detailsSizeVariant> {}

const Details = React.forwardRef<HTMLDivElement, DetailsProps>(
  ({ className, density: propDensity, children, ...props }, ref) => {
    const contextDensity = useDetailDensity();
    const density = propDensity ?? contextDensity ?? Densities.Medium;

    return (
      <DetailProvider density={density as Densities}>
        <div ref={ref} className={cn("w-full [&>:last-child]:border-0", className)} {...props}>
          {children}
        </div>
      </DetailProvider>
    );
  },
);
Details.displayName = "Details";

export interface DetailItemProps extends Omit<React.HTMLAttributes<HTMLDivElement>, "size"> {
  label: string;
  multiline?: boolean;
  density?: Densities;
}

const DetailItem = React.forwardRef<HTMLDivElement, DetailItemProps>(
  ({ className, label, multiline, density: propDensity, children, ...props }, ref) => {
    const contextDensity = useDetailDensity();
    const density = propDensity ?? contextDensity;

    return (
      <div
        ref={ref}
        className={cn(
          "border-b flex",
          multiline && "flex-col",
          !multiline && "items-center",
          className,
        )}
        {...props}
      >
        <div className={cn(detailLabelSizeVariant({ density }))}>{label}</div>
        <div
          className={cn(
            detailValueSizeVariant({ density }),
            multiline
              ? cn(
                  "whitespace-normal break-words text-left",
                  detailValueMultiLinePaddingVariant({ density }),
                )
              : "truncate text-right ml-auto",
          )}
        >
          {children}
        </div>
      </div>
    );
  },
);
DetailItem.displayName = "DetailItem";

export { Details, DetailItem };
