export interface WidgetNode {
  type: string;
  id: string;
  props: {
    [key: string]: any;
  };
  children?: WidgetNode[];
  events: string[];
}

export interface WidgetMap {
  [key: string]:
    | React.ComponentType<any>
    | React.LazyExoticComponent<React.ComponentType<any>>;
}

export type WidgetEventHandlerType = (
  eventName: string,
  widgetId: string,
  args: any[]
) => void;

export interface MenuItem {
  label: string;
  icon?: string;
  tag?: string;
  children?: MenuItem[];
  variant: "Default" | "Separator" | "Checkbox" | "Radio" | "Group";
  checked: boolean;
  disabled: boolean;
  shortcut?: string;
  expanded: boolean;
}

export interface InternalLink {
  title: string;
  appId: string;
}
