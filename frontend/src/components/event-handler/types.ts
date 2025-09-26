import { WidgetEventHandlerType } from '@/types/widgets';

export interface EventHandlerContextProps {
  eventHandler: WidgetEventHandlerType;
}

export type EventHandler = (event: string, id: string, args: unknown[]) => void;
