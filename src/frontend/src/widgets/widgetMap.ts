import { LoadingScreen } from "@/components/LoadingScreen";
import { ArticleWidget } from "@/widgets/article";
import { CardWidget } from "@/widgets/card";
import { BadgeWidget } from "@/widgets/badge";
import { DropDownMenuWidget } from "@/widgets/dropDownMenu";
import { ExpandableWidget } from "@/widgets/expandable";
import { ProgressWidget } from "@/widgets/progress";
import { SheetWidget } from "@/widgets/sheet";
import { SlotWidget } from "@/widgets/slot";
import { TooltipWidget } from "@/widgets/tooltip";
import { PaginationWidget } from "@/widgets/pagination";
import { ChatLoadingWidget, ChatMessageWidget, ChatStatusWidget } from "@/widgets/chat";
import { ToolbarWidget } from "@/widgets/toolbar";
import { BreadcrumbsWidget } from "@/widgets/breadcrumbs";
import { StackedProgressWidget } from "@/widgets/stackedProgress";
import { FileDialogWidget, SaveDialogWidget, FolderDialogWidget } from "@/widgets/filePicker";
import { BladeContainerWidget, BladeWidget } from "@/widgets/blades";
import { DetailsWidget, DetailWidget } from "@/widgets/details";
import { DialogWidget } from "@/widgets/dialogs/DialogWidget";
import { DialogHeaderWidget } from "@/widgets/dialogs/DialogHeaderWidget";
import { DialogBodyWidget } from "@/widgets/dialogs/DialogBodyWidget";
import { DialogFooterWidget } from "@/widgets/dialogs/DialogFooterWidget";
import { FormWidget } from "@/widgets/forms";
import { FieldWidget } from "@/widgets/inputs/FieldWidget";
import { TextInputWidget } from "@/widgets/inputs/TextInputWidget";
import { BoolInputWidget } from "@/widgets/inputs/BoolInputWidget";
import { NumberInputWidget } from "@/widgets/inputs/NumberInputWidget";
import { ReadOnlyInputWidget } from "@/widgets/inputs/ReadOnlyInputWidget";
import { StackLayoutWidget } from "@/widgets/layouts/StackLayoutWidget";
import { GridLayoutWidget } from "@/widgets/layouts/GridLayoutWidget";
import { HeaderLayoutWidget } from "@/widgets/layouts/HeaderLayoutWidget";
import { FooterLayoutWidget } from "@/widgets/layouts/FooterLayoutWidget";
import { SidebarLayoutWidget, SidebarMenuWidget } from "@/widgets/layouts/sidebar";
import {
  ResizablePanelGroupWidget,
  ResizablePanelWidget,
} from "@/widgets/layouts/ResizablePanelGroupWidget";
import { FloatingPanelWidget } from "@/widgets/layouts/FloatingPanelWidget";
import { ListItemWidget } from "@/widgets/lists";
import { TreeWidget } from "@/widgets/tree";
import { TextBlockWidget } from "@/widgets/primitives/TextBlockWidget";
import { HtmlWidget } from "@/widgets/primitives/HtmlWidget";
import { ErrorWidget } from "@/widgets/primitives/ErrorWidget";
import { SvgWidget } from "@/widgets/primitives/SvgWidget";
import { ImageWidget } from "@/widgets/primitives/ImageWidget";
import { IframeWidget } from "@/widgets/primitives/IframeWidget";
import { FragmentWidget } from "@/widgets/primitives/FragmentWidget";
import { SeparatorWidget } from "@/widgets/primitives/SeparatorWidget";
import { SkeletonWidget } from "@/widgets/primitives/SkeletonWidget";
import { IconWidget } from "@/widgets/primitives/IconWidget";
import { BoxWidget } from "@/widgets/primitives/BoxWidget";
import { CalloutWidget } from "@/widgets/primitives/CalloutWidget";
import { KbdWidget } from "@/widgets/primitives/KbdWidget";
import { EmptyWidget } from "@/widgets/primitives/EmptyWidget";
import { AvatarWidget } from "@/widgets/primitives/AvatarWidget";
import { IvyLogoWidget } from "@/widgets/primitives/IvyLogoWidget";
import { SpacerWidget } from "@/widgets/primitives/SpacerWidget";
import { LoadingWidget } from "@/widgets/primitives/LoadingWidget";
import { AppHostWidget } from "@/widgets/primitives/AppHostWidget";
import { AutoScrollWidget } from "@/widgets/primitives/AutoScrollWidget";
import { TableWidget, TableRowWidget, TableCellWidget } from "@/widgets/tables";
import { SmartSearch } from "@/docs-internal/SmartSearch";
import { lazyWithRetry } from "@/lib/lazyWithRetry";

export const widgetMap = {
  $loading: LoadingScreen,

  // Primitives
  "Ivy.TextBlock": TextBlockWidget,
  "Ivy.RichTextBlock": lazyWithRetry(() =>
    import("@/widgets/primitives/RichTextBlockWidget").then((m) => ({
      default: m.RichTextBlockWidget,
    })),
  ),
  "Ivy.Markdown": lazyWithRetry(() => import("@/widgets/primitives/MarkdownWidget")),
  "Ivy.Json": lazyWithRetry(() => import("@/widgets/primitives/JsonWidget")),
  "Ivy.Html": HtmlWidget,
  "Ivy.Xml": lazyWithRetry(() => import("@/widgets/primitives/XmlWidget")),
  "Ivy.Error": ErrorWidget,
  "Ivy.Svg": SvgWidget,
  "Ivy.Image": ImageWidget,
  "Ivy.Iframe": IframeWidget,
  "Ivy.CodeBlock": lazyWithRetry(() => import("@/widgets/primitives/CodeBlockWidget")),
  "Ivy.Fragment": FragmentWidget,
  "Ivy.Separator": SeparatorWidget,
  "Ivy.Skeleton": SkeletonWidget,
  "Ivy.Icon": IconWidget,
  "Ivy.Box": BoxWidget,
  "Ivy.Embed": lazyWithRetry(() => import("@/widgets/primitives/EmbedWidget")),
  "Ivy.Script": lazyWithRetry(() => import("@/widgets/primitives/ScriptWidget")),
  "Ivy.Callout": CalloutWidget,
  "Ivy.Kbd": KbdWidget,
  "Ivy.Empty": EmptyWidget,
  "Ivy.Avatar": AvatarWidget,
  "Ivy.IvyLogo": IvyLogoWidget,
  "Ivy.Spacer": SpacerWidget,
  "Ivy.Loading": LoadingWidget,
  "Ivy.AppHost": AppHostWidget,
  "Ivy.AutoScroll": AutoScrollWidget,
  "Ivy.AudioPlayer": lazyWithRetry(() =>
    import("@/widgets/primitives/AudioPlayerWidget").then((m) => ({
      default: m.AudioPlayerWidget,
    })),
  ),
  "Ivy.VideoPlayer": lazyWithRetry(() =>
    import("@/widgets/primitives/VideoPlayerWidget").then((m) => ({
      default: m.VideoPlayerWidget,
    })),
  ),
  "Ivy.Stepper": lazyWithRetry(() => import("@/widgets/primitives/StepperWidget")),
  "Ivy.Terminal": lazyWithRetry(() => import("@/widgets/primitives/TerminalWidget")),

  // Widgets
  "Ivy.Article": ArticleWidget,
  "Ivy.Button": lazyWithRetry(() =>
    import("@/widgets/button/ButtonWidget").then((m) => ({
      default: m.ButtonWidget,
    })),
  ),
  "Ivy.Progress": ProgressWidget,
  "Ivy.StackedProgress": StackedProgressWidget,
  "Ivy.Tooltip": TooltipWidget,
  "Ivy.Toolbar": ToolbarWidget,
  "Ivy.Slot": SlotWidget,
  "Ivy.Card": CardWidget,
  "Ivy.Sheet": SheetWidget,
  "Ivy.Badge": BadgeWidget,
  "Ivy.Breadcrumbs": BreadcrumbsWidget,
  "Ivy.Expandable": ExpandableWidget,
  "Ivy.Chat": lazyWithRetry(() =>
    import("@/widgets/chat/ChatWidget").then((m) => ({
      default: m.ChatWidget,
    })),
  ),
  "Ivy.ChatMessage": ChatMessageWidget,
  "Ivy.ChatLoading": ChatLoadingWidget,
  "Ivy.ChatStatus": ChatStatusWidget,
  "Ivy.DropDownMenu": DropDownMenuWidget,
  "Ivy.Pagination": PaginationWidget,
  "Ivy.Kanban": lazyWithRetry(() =>
    import("@/widgets/kanban/KanbanWidget").then((m) => ({
      default: m.KanbanWidget,
    })),
  ),
  "Ivy.KanbanCard": lazyWithRetry(() =>
    import("@/widgets/kanban/KanbanCardWidget").then((m) => ({
      default: m.KanbanCardWidget,
    })),
  ),
  "Ivy.Calendar": lazyWithRetry(() =>
    import("@/widgets/calendar/CalendarWidget").then((m) => ({
      default: m.CalendarWidget,
    })),
  ),
  "Ivy.CalendarEvent": lazyWithRetry(() =>
    import("@/widgets/calendar/CalendarEventWidget").then((m) => ({
      default: m.CalendarEventWidget,
    })),
  ),

  // Layouts
  "Ivy.StackLayout": StackLayoutWidget,
  "Ivy.GridLayout": GridLayoutWidget,
  "Ivy.HeaderLayout": HeaderLayoutWidget,
  "Ivy.FooterLayout": FooterLayoutWidget,
  "Ivy.TabsLayout": lazyWithRetry(() =>
    import("@/widgets/layouts/tabs/TabsLayoutWidget").then((m) => ({
      default: m.TabsLayoutWidget,
    })),
  ),
  "Ivy.Tab": lazyWithRetry(() =>
    import("@/widgets/layouts/tabs/TabWidget").then((m) => ({
      default: m.TabWidget,
    })),
  ),
  "Ivy.SidebarLayout": SidebarLayoutWidget,
  "Ivy.SidebarMenu": SidebarMenuWidget,
  "Ivy.ResizablePanelGroup": ResizablePanelGroupWidget,
  "Ivy.ResizablePanel": ResizablePanelWidget,
  "Ivy.FloatingPanel": FloatingPanelWidget,

  // Inputs
  "Ivy.Field": FieldWidget,
  "Ivy.TextInput": TextInputWidget,
  "Ivy.BoolInput": BoolInputWidget,
  "Ivy.DateTimeInput": lazyWithRetry(() =>
    import("@/widgets/inputs/DateTimeInputWidget").then((m) => ({
      default: m.DateTimeInputWidget,
    })),
  ),
  "Ivy.NumberInput": NumberInputWidget,
  "Ivy.NumberRangeInput": lazyWithRetry(() =>
    import("@/widgets/inputs/NumberRangeInputWidget").then((m) => ({
      default: m.NumberRangeInputWidget,
    })),
  ),
  "Ivy.SelectInput": lazyWithRetry(() =>
    import("@/widgets/inputs/SelectInputWidget").then((m) => ({
      default: m.SelectInputWidget,
    })),
  ),
  "Ivy.ReadOnlyInput": ReadOnlyInputWidget,
  "Ivy.ColorInput": lazyWithRetry(() =>
    import("@/widgets/inputs/ColorInputWidget").then((m) => ({
      default: m.ColorInputWidget,
    })),
  ),
  "Ivy.IconInput": lazyWithRetry(() =>
    import("@/widgets/inputs/IconInputWidget").then((m) => ({
      default: m.IconInputWidget,
    })),
  ),
  "Ivy.FeedbackInput": lazyWithRetry(() =>
    import("@/widgets/inputs/FeedbackInputWidget").then((m) => ({
      default: m.FeedbackInputWidget,
    })),
  ),
  "Ivy.AsyncSelectInput": lazyWithRetry(() =>
    import("@/widgets/inputs/AsyncSelectInputWidget").then((m) => ({
      default: m.AsyncSelectInputWidget,
    })),
  ),
  "Ivy.DateRangeInput": lazyWithRetry(() =>
    import("@/widgets/inputs/DateRangeInputWidget").then((m) => ({
      default: m.DateRangeInputWidget,
    })),
  ),
  "Ivy.FileInput": lazyWithRetry(() =>
    import("@/widgets/inputs/FileInputWidget").then((m) => ({
      default: m.FileInputWidget,
    })),
  ),
  "Ivy.ContentInput": lazyWithRetry(() =>
    import("@/widgets/inputs/ContentInputWidget").then((m) => ({
      default: m.ContentInputWidget,
    })),
  ),
  "Ivy.SignatureInput": lazyWithRetry(() =>
    import("@/widgets/inputs/SignatureInputWidget").then((m) => ({
      default: m.SignatureInputWidget,
    })),
  ),

  "Ivy.CodeInput": lazyWithRetry(() => import("@/widgets/inputs/code/CodeInputWidget")),
  "Ivy.AudioInput": lazyWithRetry(() => import("@/widgets/inputs/AudioInputWidget")),
  "Ivy.CameraInput": lazyWithRetry(() => import("@/widgets/cameraInput/CameraInputWidget")),

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
  "Ivy.DataTable": lazyWithRetry(() => import("@/widgets/dataTables/DataTableWidget")),

  // Lists
  "Ivy.List": lazyWithRetry(() =>
    import("@/widgets/lists/ListWidget").then((m) => ({
      default: m.ListWidget,
    })),
  ),
  "Ivy.ListItem": ListItemWidget,

  // Tree
  "Ivy.Tree": TreeWidget,

  // Details
  "Ivy.Details": DetailsWidget,
  "Ivy.Detail": DetailWidget,

  // Charts
  "Ivy.LineChart": lazyWithRetry(() => import("@/widgets/charts/LineChartWidget")),
  "Ivy.PieChart": lazyWithRetry(() => import("@/widgets/charts/PieChartWidget")),
  "Ivy.AreaChart": lazyWithRetry(() => import("@/widgets/charts/AreaChartWidget")),
  "Ivy.BarChart": lazyWithRetry(() => import("@/widgets/charts/BarChartWidget")),
  "Ivy.ScatterChart": lazyWithRetry(() => import("@/widgets/charts/ScatterChartWidget")),
  "Ivy.RadarChart": lazyWithRetry(() => import("@/widgets/charts/RadarChartWidget")),
  "Ivy.SankeyChart": lazyWithRetry(() => import("@/widgets/charts/SankeyChartWidget")),
  "Ivy.ChordChart": lazyWithRetry(() => import("@/widgets/charts/ChordChartWidget")),
  "Ivy.FunnelChart": lazyWithRetry(() => import("@/widgets/charts/FunnelChartWidget")),
  "Ivy.GaugeChart": lazyWithRetry(() => import("@/widgets/charts/GaugeChartWidget")),

  // Effects
  "Ivy.Confetti": lazyWithRetry(() => import("@/widgets/effects/ConfettiWidget")),
  "Ivy.Animation": lazyWithRetry(() => import("@/widgets/effects/AnimationWidget")),

  // Internal
  "Ivy.Docs.Shared.Internal.SmartSearch": SmartSearch,
  "Ivy.Widgets.Internal.SidebarNews": lazyWithRetry(
    () => import("@/widgets/internal/SidebarNewsWidget"),
  ),
  "Ivy.Widgets.Internal.ThemeColorPicker": lazyWithRetry(() =>
    import("@/widgets/internal/ThemeColorPickerWidget").then((m) => ({
      default: m.ThemeColorPickerWidget,
    })),
  ),
};
