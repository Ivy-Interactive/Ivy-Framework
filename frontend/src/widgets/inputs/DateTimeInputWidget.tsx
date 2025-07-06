import * as React from 'react';
import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from '@/components/ui/popover';
import { format } from 'date-fns';
import { Calendar as CalendarIcon } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useEventHandler } from '@/components/EventHandlerContext';

interface DateTimeInputWidgetProps {
  id: string;
  value?: string;
  placeholder?: string;
  disabled: boolean;
}

export const DateTimeInputWidget: React.FC<DateTimeInputWidgetProps> = ({
  id,
  value,
  placeholder,
  disabled,
}) => {
  const eventHandler = useEventHandler();
  const date = value ? new Date(value) : undefined;
  const [open, setOpen] = useState(false);

  const handleSelect = (selectedDate: Date | undefined) => {
    eventHandler('OnChange', id, [selectedDate?.toISOString()]);
    setOpen(false);
  };

  return (
    <Popover key={id} open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          disabled={disabled}
          variant={'outline'}
          className={cn(
            'w-full justify-start text-left font-normal',
            !date && 'text-muted-foreground'
          )}
        >
          <CalendarIcon className="mr-2 h-4 w-4" />
          {date ? (
            format(date, 'yyyy-MM-dd')
          ) : (
            <span>{placeholder || 'Pick a date'}</span>
          )}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0">
        <Calendar
          mode="single"
          selected={date}
          onSelect={handleSelect}
          initialFocus
        />
      </PopoverContent>
    </Popover>
  );
};
