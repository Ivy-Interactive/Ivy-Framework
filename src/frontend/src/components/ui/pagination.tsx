import * as React from "react";
import { ChevronLeft, ChevronRight, MoreHorizontal } from "lucide-react";

import { cn } from "@/lib/utils";
import { ButtonProps } from "@/components/ui/button/button";
import { buttonVariant } from "@/components/ui/button";
import { Densities } from "@/types/density";

const Pagination = ({ className, ...props }: React.ComponentProps<"nav">) => (
  <nav aria-label="pagination" className={cn("flex w-fit justify-center", className)} {...props} />
);
Pagination.displayName = "Pagination";

const PaginationContent = React.forwardRef<HTMLUListElement, React.ComponentProps<"ul">>(
  ({ className, ...props }, ref) => (
    <ul ref={ref} className={cn("flex flex-row items-center gap-1", className)} {...props} />
  ),
);
PaginationContent.displayName = "PaginationContent";

const PaginationItem = React.forwardRef<HTMLLIElement, React.ComponentProps<"li">>(
  ({ className, ...props }, ref) => <li ref={ref} className={cn("", className)} {...props} />,
);
PaginationItem.displayName = "PaginationItem";

type PaginationLinkProps = {
  isActive?: boolean;
  density?: Densities;
} & Pick<ButtonProps, "size"> &
  React.ComponentProps<"a">;

function getLinkSize(
  density: Densities | undefined,
  isIconOnly: boolean,
): ButtonProps["size"] {
  if (density === Densities.Small) return isIconOnly ? "icon-sm" : "sm";
  if (density === Densities.Large) return "lg";
  return isIconOnly ? "icon" : "default";
}

function getIconClass(density: Densities | undefined): string {
  if (density === Densities.Small) return "h-3 w-3";
  if (density === Densities.Large) return "h-5 w-5";
  return "h-4 w-4";
}

const PaginationLink = ({
  className,
  isActive,
  density,
  size,
  ...props
}: PaginationLinkProps) => (
  <a
    aria-current={isActive ? "page" : undefined}
    className={cn(
      buttonVariant({
        variant: isActive ? "outline" : "ghost",
        size: size ?? getLinkSize(density, true),
      }),
      className,
    )}
    {...props}
  />
);
PaginationLink.displayName = "PaginationLink";

type PaginationNavProps = {
  density?: Densities;
} & React.ComponentProps<typeof PaginationLink>;

const PaginationPrevious = ({ className, density, ...props }: PaginationNavProps) => {
  const gapClass = density === Densities.Small ? "gap-0.5" : density === Densities.Large ? "gap-1.5" : "gap-1";
  const paddingClass = density === Densities.Small ? "pl-1.5" : density === Densities.Large ? "pl-3" : "pl-2.5";
  return (
    <PaginationLink
      aria-label="Go to previous page"
      size={getLinkSize(density, false)}
      density={density}
      className={cn(gapClass, paddingClass, className)}
      {...props}
    >
      <ChevronLeft className={getIconClass(density)} />
      <span>Previous</span>
    </PaginationLink>
  );
};
PaginationPrevious.displayName = "PaginationPrevious";

const PaginationNext = ({ className, density, ...props }: PaginationNavProps) => {
  const gapClass = density === Densities.Small ? "gap-0.5" : density === Densities.Large ? "gap-1.5" : "gap-1";
  const paddingClass = density === Densities.Small ? "pr-1.5" : density === Densities.Large ? "pr-3" : "pr-2.5";
  return (
    <PaginationLink
      aria-label="Go to next page"
      size={getLinkSize(density, false)}
      density={density}
      className={cn(gapClass, paddingClass, className)}
      {...props}
    >
      <span>Next</span>
      <ChevronRight className={getIconClass(density)} />
    </PaginationLink>
  );
};
PaginationNext.displayName = "PaginationNext";

type PaginationEllipsisProps = {
  density?: Densities;
} & React.ComponentProps<"span">;

const PaginationEllipsis = ({ className, density, ...props }: PaginationEllipsisProps) => {
  const containerClass = density === Densities.Small ? "h-6 w-6" : density === Densities.Large ? "h-11 w-11" : "h-9 w-9";
  return (
    <span
      aria-hidden
      className={cn("flex items-center justify-center", containerClass, className)}
      {...props}
    >
      <MoreHorizontal className={getIconClass(density)} />
      <span className="sr-only">More pages</span>
    </span>
  );
};
PaginationEllipsis.displayName = "PaginationEllipsis";

export {
  Pagination,
  PaginationContent,
  PaginationLink,
  PaginationItem,
  PaginationPrevious,
  PaginationNext,
  PaginationEllipsis,
};
