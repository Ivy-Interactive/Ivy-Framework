import React from 'react';

interface CalendarEventWidgetProps {
  id: string;
  eventId?: string;
  title?: string;
  start?: string;
  end?: string;
  color?: string;
  allDay?: boolean;
  children?: React.ReactNode;
}

export const CalendarEventWidget: React.FC<CalendarEventWidgetProps> = ({
  children,
}) => {
  // CalendarEventWidget is a data node consumed by the parent CalendarWidget.
  // It renders its children (slot content) as-is if rendered standalone.
  return React.createElement(React.Fragment, null, children);
};
