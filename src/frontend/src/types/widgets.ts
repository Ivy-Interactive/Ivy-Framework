export interface CallSite {
  path?: string;
  filePath?: string;
  lineNumber?: number;
  memberName?: string;
  declaringType?: string;
}

export interface WidgetNode {
  type: string;
  id: string;
  props: {
    [key: string]: unknown;
  };
  children?: WidgetNode[];
  events: string[];
  callSite?: CallSite;
}

export type WidgetEventHandlerType = (eventName: string, widgetId: string, args: unknown[]) => void;

export interface MenuItem {
  label: string;
  icon?: string;
  tag?: string;
  tooltip?: string;
  children?: MenuItem[];
  variant: "Default" | "Separator" | "Checkbox" | "Radio" | "Group";
  checked: boolean;
  disabled: boolean;
  color?: "Default" | "Destructive" | "Primary" | "Secondary" | "Success" | "Warning" | "Info";
  shortcut?: string;
  badge?: string;
  expanded: boolean;
  path?: string;
  onSelect?: (item: MenuItem) => void;
}

export interface InternalLink {
  title: string;
  appId: string;
}
