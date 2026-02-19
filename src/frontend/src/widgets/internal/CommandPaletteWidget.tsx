import { useEffect, useState, useMemo, useRef } from 'react';
import {
  CommandDialog,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from '@/components/ui/command';
import { MenuItem } from '@/types/widgets';
import Icon from '@/components/Icon';
import { useEventHandler } from '@/components/event-handler';

interface CommandPaletteWidgetProps {
  id: string;
  items: MenuItem[];
}

export const CommandPaletteWidget = ({
  id,
  items = [],
}: CommandPaletteWidgetProps) => {
  const [isOpen, setIsOpen] = useState(false);
  const [search, setSearch] = useState('');
  const listRef = useRef<HTMLDivElement>(null);
  const eventHandler = useEventHandler();

  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      // Check for Shift + 7
      if (event.shiftKey && event.code === 'Digit7') {
        event.preventDefault();
        setIsOpen(prev => !prev);
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, []);

  // Reset scroll when search changes
  useEffect(() => {
    if (listRef.current) {
      listRef.current.scrollTop = 0;
    }
  }, [search]);

  // Reset search when dialog closes
  useEffect(() => {
    if (!isOpen) {
      setSearch('');
    }
  }, [isOpen]);

  const flatItems = useMemo(() => {
    return items.filter(item => item.tag && item.label);
  }, [items]);

  const handleSelect = (tag: string) => {
    eventHandler('OnSelect', id, [tag]);
    setIsOpen(false);
  };

  return (
    <CommandDialog open={isOpen} onOpenChange={setIsOpen}>
      <CommandInput
        placeholder="Search widgets, categories, or keywords"
        value={search}
        onValueChange={setSearch}
      />
      <CommandList ref={listRef}>
        <CommandEmpty>No results found.</CommandEmpty>
        <CommandGroup heading="Navigation">
          {flatItems.map(item => (
            <CommandItem
              key={item.tag}
              value={`${item.label} ${item.path || ''}`}
              onSelect={() => handleSelect(item.tag!)}
            >
              <Icon name={item.icon} className="mr-2 h-4 w-4" />
              <div className="flex flex-col">
                <span>{item.label}</span>
                {item.path && (
                  <span className="text-xs text-muted-foreground">
                    {item.path}
                  </span>
                )}
              </div>
            </CommandItem>
          ))}
        </CommandGroup>
      </CommandList>
    </CommandDialog>
  );
};
