export interface WidgetNode {
  type: string;
  id: string;
  props: {
    [key: string]: unknown;
  };
  children?: WidgetNode[];
  events: string[];
}

export type WidgetEventHandlerType = (
  eventName: string,
  widgetId: string,
  args: unknown[]
) => void;

export interface MenuItem {
  label: string;
  icon?: string;
  tag?: string;
  tooltip?: string;
  children?: MenuItem[];
  variant: 'Default' | 'Separator' | 'Checkbox' | 'Radio' | 'Group';
  checked: boolean;
  disabled: boolean;
  shortcut?: string;
  expanded: boolean;
  path?: string;
}

export interface InternalLink {
  title: string;
  appId: string;
}
