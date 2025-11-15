import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { TabsTrigger } from '@/components/ui/tabs';
import { X } from 'lucide-react';
import { cn } from '@/lib/utils';
import type {
  SortableTabTriggerProps,
  SortableDropdownMenuItemProps,
} from '../types';

/**
 * A draggable tab trigger component that integrates with dnd-kit
 * Handles drag-and-drop reordering of tabs
 */
export function SortableTabTrigger({
  id,
  value,
  onClick,
  onMouseDown,
  className,
  children,
  useRadix = false,
  ...props
}: SortableTabTriggerProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id });

  return (
    <TabsTrigger
      ref={setNodeRef}
      style={{
        transform: transform
          ? `translate3d(${transform.x}px, ${transform.y}px, 0)`
          : undefined,
        transition: isDragging ? 'none' : transition,
        opacity: isDragging ? 0.7 : 1,
        zIndex: isDragging ? 100 : undefined,
        cursor: isDragging ? 'grabbing' : 'grab',
      }}
      value={value}
      onClick={onClick}
      onMouseDown={onMouseDown}
      className={className}
      useRadix={useRadix}
      {...attributes}
      {...listeners}
      {...props}
      role="tab"
    >
      {children}
    </TabsTrigger>
  );
}

/**
 * A draggable dropdown menu item for tabs that overflow
 * Handles drag-and-drop reordering within the dropdown menu
 */
export function SortableDropdownMenuItem({
  id,
  children,
  onClick,
  isActive,
  showClose,
}: SortableDropdownMenuItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id });

  return (
    <div
      ref={setNodeRef}
      style={{
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
        zIndex: isDragging ? 100 : undefined,
      }}
      {...attributes}
      {...listeners}
      onClick={onClick}
      className={cn(
        'group w-full flex items-center p-1 text-sm cursor-pointer select-none rounded-sm transition-colors hover:bg-accent',
        isActive && 'bg-accent text-accent-foreground'
      )}
    >
      <span className="truncate text-left">{children}</span>
      {showClose && (
        <button
          type="button"
          className="ml-auto opacity-60 p-1 hover:opacity-100 invisible group-hover:visible cursor-pointer"
          onClick={e => {
            e.stopPropagation();
            window.dispatchEvent(
              new CustomEvent('tab-close', { detail: { id } })
            );
          }}
        >
          <X className="w-3 h-3" />
        </button>
      )}
    </div>
  );
}
