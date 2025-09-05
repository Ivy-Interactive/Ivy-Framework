import React from 'react';
import {
  Pagination,
  PaginationContent,
  PaginationItem,
  PaginationPrevious,
  PaginationLink,
  PaginationEllipsis,
  PaginationNext,
} from '@/components/ui/pagination';
import { useEventHandler } from '@/components/event-handler';
import { cn } from '@/lib/utils';

interface PaginationWidgetProps {
  id: string;
  page?: number;
  numPages?: number;
  siblings?: number;
  boundaries?: number;
  disabled?: boolean;
}

export const PaginationWidget: React.FC<PaginationWidgetProps> = ({
  id,
  page,
  numPages = 0,
  siblings = 1,
  boundaries = 1,
  disabled = false,
}) => {
  const eventHandler = useEventHandler();

  const showPages: number[] = [];
  for (let p = 0; p < numPages; p++) {
    if (p < boundaries || p >= numPages - boundaries) {
      // This is a boundary
      showPages.push(p + 1);
    } else if (typeof page === 'number') {
      // The page "p" should be shown if Math.abs(p - (page - 1)) <= siblings. But to prevent the
      // component from shrinking when the page is near the start or the end, we need to clamp the
      // (page - 1) part.
      let leftClamp = boundaries + siblings;
      let rightClamp = numPages - 1 - boundaries - siblings;
      if (boundaries) {
        leftClamp += 1;
        rightClamp -= 1;
      }
      if (
        Math.abs(p - Math.min(Math.max(page - 1, leftClamp), rightClamp)) <=
        siblings
      ) {
        // This is a sibling
        showPages.push(p + 1);
      }
    }
  }

  return (
    <Pagination>
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious
            aria-disabled={disabled || !page || page === 1}
            className={cn(
              'select-none',
              disabled || !page || page === 1
                ? 'pointer-events-none opacity-50'
                : null
            )}
            onClick={
              !page || page === 1
                ? undefined
                : () => eventHandler('OnChange', id, [page - 1])
            }
          />
        </PaginationItem>
        {showPages.map((p, i) => {
          const isGap = i > 0 && p - showPages[i - 1] > 1;
          const isOneGap = i > 0 && p - showPages[i - 1] === 2;
          return (
            <React.Fragment key={p}>
              {isGap &&
                (isOneGap ? (
                  <PaginationItem>
                    <PaginationLink
                      aria-disabled={disabled}
                      className={
                        disabled ? 'pointer-events-none opacity-50' : undefined
                      }
                      onClick={
                        p - 1 === page
                          ? undefined
                          : () => eventHandler('OnChange', id, [p - 1])
                      }
                      isActive={p - 1 === page}
                    >
                      {p - 1}
                    </PaginationLink>
                  </PaginationItem>
                ) : (
                  <PaginationItem>
                    <PaginationEllipsis />
                  </PaginationItem>
                ))}
              <PaginationItem>
                <PaginationLink
                  aria-disabled={disabled}
                  className={
                    disabled ? 'pointer-events-none opacity-50' : undefined
                  }
                  onClick={
                    p === page
                      ? undefined
                      : () => eventHandler('OnChange', id, [p])
                  }
                  isActive={p === page}
                >
                  {p}
                </PaginationLink>
              </PaginationItem>
            </React.Fragment>
          );
        })}
        <PaginationItem>
          <PaginationNext
            aria-disabled={disabled || !page || page === numPages}
            className={cn(
              'select-none',
              disabled || !page || page === numPages
                ? 'pointer-events-none opacity-50'
                : null
            )}
            onClick={
              !page || page === numPages
                ? undefined
                : () => eventHandler('OnChange', id, [page + 1])
            }
          />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  );
};
