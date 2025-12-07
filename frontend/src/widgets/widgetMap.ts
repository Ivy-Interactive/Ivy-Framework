import { LoadingScreen } from '@/components/LoadingScreen';
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
} from '@/widgets';
import { BladeContainerWidget, BladeWidget } from '@/widgets/blades';
import { DetailsWidget, DetailWidget } from '@/widgets/details';
import {
  DialogWidget,
  DialogHeaderWidget,
  DialogBodyWidget,
  DialogFooterWidget,
} from '@/widgets/dialogs';
import { FormWidget } from '@/widgets/forms';
import {
  FieldWidget,
  TextInputWidget,
  BoolInputWidget,
  DateTimeInputWidget,
  NumberInputWidget,
  SelectInputWidget,
  ReadOnlyInputWidget,
  ColorInputWidget,
  FeedbackInputWidget,
  AsyncSelectInputWidget,
  DateRangeInputWidget,
  FileInputWidget,
} from '@/widgets/inputs';
import {
  StackLayoutWidget,
  WrapLayoutWidget,
  GridLayoutWidget,
  HeaderLayoutWidget,
  FooterLayoutWidget,
  TabsLayoutWidget,
  TabWidget,
  SidebarLayoutWidget,
  SidebarMenuWidget,
  ResizeablePanelGroupWidget,
  ResizeablePanelWidget,
  FloatingPanelWidget,
} from '@/widgets/layouts';
import { ListWidget, ListItemWidget } from '@/widgets/lists';
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
} from '@/widgets/primitives';
import { DataTable } from '@/widgets/dataTables';
import { TableWidget, TableRowWidget, TableCellWidget } from '@/widgets/tables';
import React from 'react';

export const widgetMap = {
  $loading: LoadingScreen,

  // Primitives
  'Ivy.TextBlock': TextBlockWidget,
  'Ivy.Markdown': React.lazy(
    () => import('@/widgets/primitives/MarkdownWidget')
  ),
  'Ivy.Json': React.lazy(() => import('@/widgets/primitives/JsonWidget')),
  'Ivy.Html': HtmlWidget,
  'Ivy.Xml': React.lazy(() => import('@/widgets/primitives/XmlWidget')),
  'Ivy.Error': ErrorWidget,
  'Ivy.Svg': SvgWidget,
  'Ivy.Image': ImageWidget,
  'Ivy.Iframe': IframeWidget,
  'Ivy.Code': React.lazy(() => import('@/widgets/primitives/CodeWidget')),
  'Ivy.Fragment': FragmentWidget,
  'Ivy.Separator': SeparatorWidget,
  'Ivy.Skeleton': SkeletonWidget,
  'Ivy.Icon': IconWidget,
  'Ivy.Box': BoxWidget,
  'Ivy.Embed': React.lazy(() => import('@/widgets/primitives/EmbedWidget')),
  'Ivy.Callout': CalloutWidget,
  'Ivy.Kbd': KbdWidget,
  'Ivy.Empty': EmptyWidget,
  'Ivy.Avatar': AvatarWidget,
  'Ivy.IvyLogo': IvyLogoWidget,
  'Ivy.Spacer': SpacerWidget,
  'Ivy.Loading': LoadingWidget,
  'Ivy.AppHost': AppHostWidget,
  'Ivy.Audio': AudioPlayerWidget,
  'Ivy.VideoPlayer': VideoPlayerWidget,
  'Ivy.Stepper': React.lazy(() => import('@/widgets/primitives/StepperWidget')),

  // Widgets
  'Ivy.Article': ArticleWidget,
  'Ivy.Button': ButtonWidget,
  'Ivy.Progress': ProgressWidget,
  'Ivy.Tooltip': TooltipWidget,
  'Ivy.Slot': SlotWidget,
  'Ivy.Card': CardWidget,
  'Ivy.Sheet': SheetWidget,
  'Ivy.Badge': BadgeWidget,
  'Ivy.Expandable': ExpandableWidget,
  'Ivy.Chat': ChatWidget,
  'Ivy.ChatMessage': ChatMessageWidget,
  'Ivy.ChatLoading': ChatLoadingWidget,
  'Ivy.ChatStatus': ChatStatusWidget,
  'Ivy.DropDownMenu': DropDownMenuWidget,
  'Ivy.Pagination': PaginationWidget,
  'Ivy.Kanban': React.lazy(() =>
    import('@/widgets/kanban/KanbanWidget').then(m => ({
      default: m.KanbanWidget,
    }))
  ),
  'Ivy.KanbanCard': React.lazy(() =>
    import('@/widgets/kanban/KanbanCardWidget').then(m => ({
      default: m.KanbanCardWidget,
    }))
  ),

  // Layouts
  'Ivy.StackLayout': StackLayoutWidget,
  'Ivy.WrapLayout': WrapLayoutWidget,
  'Ivy.GridLayout': GridLayoutWidget,
  'Ivy.HeaderLayout': HeaderLayoutWidget,
  'Ivy.FooterLayout': FooterLayoutWidget,
  'Ivy.TabsLayout': TabsLayoutWidget,
  'Ivy.Tab': TabWidget,
  'Ivy.SidebarLayout': SidebarLayoutWidget,
  'Ivy.SidebarMenu': SidebarMenuWidget,
  'Ivy.ResizeablePanelGroup': ResizeablePanelGroupWidget,
  'Ivy.ResizeablePanel': ResizeablePanelWidget,
  'Ivy.FloatingPanel': FloatingPanelWidget,

  // Inputs
  'Ivy.Field': FieldWidget,
  'Ivy.TextInput': TextInputWidget,
  'Ivy.BoolInput': BoolInputWidget,
  'Ivy.DateTimeInput': DateTimeInputWidget,
  'Ivy.NumberInput': NumberInputWidget,
  'Ivy.SelectInput': SelectInputWidget,
  'Ivy.ReadOnlyInput': ReadOnlyInputWidget,
  'Ivy.ColorInput': ColorInputWidget,
  'Ivy.FeedbackInput': FeedbackInputWidget,
  'Ivy.AsyncSelectInput': AsyncSelectInputWidget,
  'Ivy.DateRangeInput': DateRangeInputWidget,
  'Ivy.FileInput': FileInputWidget,
  'Ivy.CodeInput': React.lazy(
    () => import('@/widgets/inputs/code/CodeInputWidget')
  ),
  'Ivy.AudioRecorder': React.lazy(
    () => import('@/widgets/inputs/AudioRecorderWidget')
  ),

  // Forms
  'Ivy.Form': FormWidget,

  // Dialogs
  'Ivy.Dialog': DialogWidget,
  'Ivy.DialogHeader': DialogHeaderWidget,
  'Ivy.DialogBody': DialogBodyWidget,
  'Ivy.DialogFooter': DialogFooterWidget,

  // Blades
  'Ivy.BladeContainer': BladeContainerWidget,
  'Ivy.Blade': BladeWidget,

  // Tables
  'Ivy.Table': TableWidget,
  'Ivy.TableRow': TableRowWidget,
  'Ivy.TableCell': TableCellWidget,

  // DataTables
  'Ivy.DataTable': DataTable,

  // Lists
  'Ivy.List': ListWidget,
  'Ivy.ListItem': ListItemWidget,

  // Details
  'Ivy.Details': DetailsWidget,
  'Ivy.Detail': DetailWidget,

  // Charts
  'Ivy.LineChart': React.lazy(() => import('@/widgets/charts/LineChartWidget')),
  'Ivy.PieChart': React.lazy(() => import('@/widgets/charts/PieChartWidget')),
  'Ivy.AreaChart': React.lazy(() => import('@/widgets/charts/AreaChartWidget')),
  'Ivy.BarChart': React.lazy(() => import('@/widgets/charts/BarChartWidget')),

  // Effects
  'Ivy.Confetti': React.lazy(() => import('@/widgets/effects/ConfettiWidget')),
  'Ivy.Animation': React.lazy(
    () => import('@/widgets/effects/AnimationWidget')
  ),

  // Internal

  'Ivy.Widgets.Internal.DbmlCanvas': React.lazy(
    () => import('@/widgets/internal/DbmlCanvasWidget')
  ),
  'Ivy.Widgets.Internal.Terminal': React.lazy(
    () => import('@/widgets/internal/TerminalWidget')
  ),
  'Ivy.Widgets.Internal.SidebarNews': React.lazy(
    () => import('@/widgets/internal/SidebarNewsWidget')
  ),
};
