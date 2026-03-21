import { LoadingScreen } from "@/components/LoadingScreen";
import {
  ArticleWidget,
  BadgeWidget,
  ButtonWidget,
  CardWidget,
  ChatLoadingWidget,
  ChatMessageWidget,
  ChatStatusWidget,
  ChatWidget,
  DropDownMenuWidget,
  ExpandableWidget,
  ProgressWidget,
  SheetWidget,
  SlotWidget,
  TooltipWidget,
  PaginationWidget,
} from "@/widgets";
import { BreadcrumbsWidget } from "@/widgets/breadcrumbs";
import { FileDialogWidget, SaveDialogWidget, FolderDialogWidget } from "@/widgets/filePicker";
import { BladeContainerWidget, BladeWidget } from "@/widgets/blades";
import { DetailsWidget, DetailWidget } from "@/widgets/details";
import {
  DialogWidget,
  DialogHeaderWidget,
  DialogBodyWidget,
  DialogFooterWidget,
} from "@/widgets/dialogs";
import { FormWidget } from "@/widgets/forms";
import {
  FieldWidget,
  TextInputWidget,
  BoolInputWidget,
  DateTimeInputWidget,
  NumberInputWidget,
  NumberRangeInputWidget,
  SelectInputWidget,
  ReadOnlyInputWidget,
  ColorInputWidget,
  IconInputWidget,
  FeedbackInputWidget,
  AsyncSelectInputWidget,
  DateRangeInputWidget,
  FileInputWidget,
  SignatureInputWidget,
} from "@/widgets/inputs";
import {
  StackLayoutWidget,
  GridLayoutWidget,
  HeaderLayoutWidget,
  FooterLayoutWidget,
  TabsLayoutWidget,
  TabWidget,
  SidebarLayoutWidget,
  SidebarMenuWidget,
  ResizablePanelGroupWidget,
  ResizablePanelWidget,
  FloatingPanelWidget,
} from "@/widgets/layouts";
import { ListWidget, ListItemWidget } from "@/widgets/lists";
import { TreeWidget } from "@/widgets/tree";
import {
  TextBlockWidget,
  HtmlWidget,
  ErrorWidget,
  SvgWidget,
  ImageWidget,
  IframeWidget,
  FragmentWidget,
  SeparatorWidget,
  SkeletonWidget,
  IconWidget,
  BoxWidget,
  CalloutWidget,
  KbdWidget,
  EmptyWidget,
  AvatarWidget,
  IvyLogoWidget,
  SpacerWidget,
  LoadingWidget,
  AppHostWidget,
  AudioPlayerWidget,
  VideoPlayerWidget,
  RichTextBlockWidget,
} from "@/widgets/primitives";
import { DataTable } from "@/widgets/dataTables";
import { TableWidget, TableRowWidget, TableCellWidget } from "@/widgets/tables";
import React from "react";
import { SmartSearch } from "@/docs-internal/SmartSearch";

export const widgetMap = {
  $loading: LoadingScreen,

  // Primitives
  "Ivy.TextBlock": TextBlockWidget,
  "Ivy.RichTextBlock": RichTextBlockWidget,
  "Ivy.Markdown": React.lazy(() => import("@/widgets/primitives/MarkdownWidget")),
  "Ivy.Json": React.lazy(() => import("@/widgets/primitives/JsonWidget")),
  "Ivy.Html": HtmlWidget,
  "Ivy.Xml": React.lazy(() => import("@/widgets/primitives/XmlWidget")),
  "Ivy.Error": ErrorWidget,
  "Ivy.Svg": SvgWidget,
  "Ivy.Image": ImageWidget,
  "Ivy.Iframe": IframeWidget,
  "Ivy.CodeBlock": React.lazy(() => import("@/widgets/primitives/CodeBlockWidget")),
  "Ivy.Fragment": FragmentWidget,
  "Ivy.Separator": SeparatorWidget,
  "Ivy.Skeleton": SkeletonWidget,
  "Ivy.Icon": IconWidget,
  "Ivy.Box": BoxWidget,
  "Ivy.Embed": React.lazy(() => import("@/widgets/primitives/EmbedWidget")),
  "Ivy.Script": React.lazy(() => import("@/widgets/primitives/ScriptWidget")),
  "Ivy.Callout": CalloutWidget,
  "Ivy.Kbd": KbdWidget,
  "Ivy.Empty": EmptyWidget,
  "Ivy.Avatar": AvatarWidget,
  "Ivy.IvyLogo": IvyLogoWidget,
  "Ivy.Spacer": SpacerWidget,
  "Ivy.Loading": LoadingWidget,
  "Ivy.AppHost": AppHostWidget,
  "Ivy.AudioPlayer": AudioPlayerWidget,
  "Ivy.VideoPlayer": VideoPlayerWidget,
  "Ivy.Stepper": React.lazy(() => import("@/widgets/primitives/StepperWidget")),
  "Ivy.Terminal": React.lazy(() => import("@/widgets/primitives/TerminalWidget")),

  // Widgets
  "Ivy.Article": ArticleWidget,
  "Ivy.Button": ButtonWidget,
  "Ivy.Progress": ProgressWidget,
  "Ivy.Tooltip": TooltipWidget,
  "Ivy.Slot": SlotWidget,
  "Ivy.Card": CardWidget,
  "Ivy.Sheet": SheetWidget,
  "Ivy.Badge": BadgeWidget,
  "Ivy.Breadcrumbs": BreadcrumbsWidget,
  "Ivy.Expandable": ExpandableWidget,
  "Ivy.Chat": ChatWidget,
  "Ivy.ChatMessage": ChatMessageWidget,
  "Ivy.ChatLoading": ChatLoadingWidget,
  "Ivy.ChatStatus": ChatStatusWidget,
  "Ivy.DropDownMenu": DropDownMenuWidget,
  "Ivy.Pagination": PaginationWidget,
  "Ivy.Kanban": React.lazy(() =>
    import("@/widgets/kanban/KanbanWidget").then((m) => ({
      default: m.KanbanWidget,
    })),
  ),
  "Ivy.KanbanCard": React.lazy(() =>
    import("@/widgets/kanban/KanbanCardWidget").then((m) => ({
      default: m.KanbanCardWidget,
    })),
  ),
  "Ivy.Calendar": React.lazy(() =>
    import("@/widgets/calendar/CalendarWidget").then((m) => ({
      default: m.CalendarWidget,
    })),
  ),
  "Ivy.CalendarEvent": React.lazy(() =>
    import("@/widgets/calendar/CalendarEventWidget").then((m) => ({
      default: m.CalendarEventWidget,
    })),
  ),

  // Layouts
  "Ivy.StackLayout": StackLayoutWidget,
  "Ivy.GridLayout": GridLayoutWidget,
  "Ivy.HeaderLayout": HeaderLayoutWidget,
  "Ivy.FooterLayout": FooterLayoutWidget,
  "Ivy.TabsLayout": TabsLayoutWidget,
  "Ivy.Tab": TabWidget,
  "Ivy.SidebarLayout": SidebarLayoutWidget,
  "Ivy.SidebarMenu": SidebarMenuWidget,
  "Ivy.ResizablePanelGroup": ResizablePanelGroupWidget,
  "Ivy.ResizablePanel": ResizablePanelWidget,
  "Ivy.FloatingPanel": FloatingPanelWidget,

  // Inputs
  "Ivy.Field": FieldWidget,
  "Ivy.TextInput": TextInputWidget,
  "Ivy.BoolInput": BoolInputWidget,
  "Ivy.DateTimeInput": DateTimeInputWidget,
  "Ivy.NumberInput": NumberInputWidget,
  "Ivy.NumberRangeInput": NumberRangeInputWidget,
  "Ivy.SelectInput": SelectInputWidget,
  "Ivy.ReadOnlyInput": ReadOnlyInputWidget,
  "Ivy.ColorInput": ColorInputWidget,
  "Ivy.IconInput": IconInputWidget,
  "Ivy.FeedbackInput": FeedbackInputWidget,
  "Ivy.AsyncSelectInput": AsyncSelectInputWidget,
  "Ivy.DateRangeInput": DateRangeInputWidget,
  "Ivy.FileInput": FileInputWidget,
  "Ivy.SignatureInput": SignatureInputWidget,
  "Ivy.CodeInput": React.lazy(() => import("@/widgets/inputs/code/CodeInputWidget")),
  "Ivy.AudioInput": React.lazy(() => import("@/widgets/inputs/AudioInputWidget")),
  "Ivy.CameraInput": React.lazy(() => import("@/widgets/cameraInput/CameraInputWidget")),

  // Forms
  "Ivy.Form": FormWidget,

  // File Pickers
  "Ivy.FileDialog": FileDialogWidget,
  "Ivy.SaveDialog": SaveDialogWidget,
  "Ivy.FolderDialog": FolderDialogWidget,

  // Dialogs
  "Ivy.Dialog": DialogWidget,
  "Ivy.DialogHeader": DialogHeaderWidget,
  "Ivy.DialogBody": DialogBodyWidget,
  "Ivy.DialogFooter": DialogFooterWidget,

  // Blades
  "Ivy.BladeContainer": BladeContainerWidget,
  "Ivy.Blade": BladeWidget,

  // Tables
  "Ivy.Table": TableWidget,
  "Ivy.TableRow": TableRowWidget,
  "Ivy.TableCell": TableCellWidget,

  // DataTables
  "Ivy.DataTable": DataTable,

  // Lists
  "Ivy.List": ListWidget,
  "Ivy.ListItem": ListItemWidget,

  // Tree
  "Ivy.Tree": TreeWidget,

  // Details
  "Ivy.Details": DetailsWidget,
  "Ivy.Detail": DetailWidget,

  // Charts
  "Ivy.LineChart": React.lazy(() => import("@/widgets/charts/LineChartWidget")),
  "Ivy.PieChart": React.lazy(() => import("@/widgets/charts/PieChartWidget")),
  "Ivy.AreaChart": React.lazy(() => import("@/widgets/charts/AreaChartWidget")),
  "Ivy.BarChart": React.lazy(() => import("@/widgets/charts/BarChartWidget")),
  "Ivy.ScatterChart": React.lazy(() => import("@/widgets/charts/ScatterChartWidget")),
  "Ivy.RadarChart": React.lazy(() => import("@/widgets/charts/RadarChartWidget")),
  "Ivy.SankeyChart": React.lazy(() => import("@/widgets/charts/SankeyChartWidget")),
  "Ivy.ChordChart": React.lazy(() => import("@/widgets/charts/ChordChartWidget")),
  "Ivy.FunnelChart": React.lazy(() => import("@/widgets/charts/FunnelChartWidget")),
  "Ivy.GaugeChart": React.lazy(() => import("@/widgets/charts/GaugeChartWidget")),

  // Effects
  "Ivy.Confetti": React.lazy(() => import("@/widgets/effects/ConfettiWidget")),
  "Ivy.Animation": React.lazy(() => import("@/widgets/effects/AnimationWidget")),

  // Internal
  "Ivy.Docs.Shared.Internal.SmartSearch": SmartSearch,
  "Ivy.Widgets.Internal.SidebarNews": React.lazy(
    () => import("@/widgets/internal/SidebarNewsWidget"),
  ),
  "Ivy.Widgets.Internal.ThemeColorPicker": React.lazy(() =>
    import("@/widgets/internal/ThemeColorPickerWidget").then((m) => ({
      default: m.ThemeColorPickerWidget,
    })),
  ),
};
