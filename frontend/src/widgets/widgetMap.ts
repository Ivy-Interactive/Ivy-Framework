import { WidgetMap } from '@/types/widgets';
import * as Widgets from '@/widgets';
import * as Layouts from '@/widgets/layouts';
import * as Inputs from '@/widgets/inputs';
import * as Forms from '@/widgets/forms';
import * as Dialogs from '@/widgets/dialogs';
import * as Tables from '@/widgets/tables';
import * as Blades from '@/widgets/blades';
import * as Lists from '@/widgets/lists';
import * as Details from '@/widgets/details';
import * as Primitives from '@/widgets/primitives';
import React from 'react';
import { LoadingScreen } from '@/components/LoadingScreen';

export const widgetMap: WidgetMap = {
  $loading: LoadingScreen,

  // Primitives
  'Ivy.TextBlock': Primitives.TextBlockWidget,
  'Ivy.Markdown': React.lazy(
    () => import('@/widgets/primitives/MarkdownWidget')
  ),
  'Ivy.Json': React.lazy(() => import('@/widgets/primitives/JsonWidget')),
  'Ivy.Html': Primitives.HtmlWidget,
  'Ivy.Xml': React.lazy(() => import('@/widgets/primitives/XmlWidget')),
  'Ivy.Error': Primitives.ErrorWidget,
  'Ivy.Svg': Primitives.SvgWidget,
  'Ivy.Image': Primitives.ImageWidget,
  'Ivy.Iframe': Primitives.IframeWidget,
  'Ivy.Code': React.lazy(() => import('@/widgets/primitives/CodeWidget')),
  'Ivy.Fragment': Primitives.FragmentWidget,
  'Ivy.Separator': Primitives.SeparatorWidget,
  'Ivy.Skeleton': Primitives.SkeletonWidget,
  'Ivy.Icon': Primitives.IconWidget,
  'Ivy.Box': Primitives.BoxWidget,
  'Ivy.Embed': React.lazy(() => import('@/widgets/primitives/EmbedWidget')),
  'Ivy.Callout': Primitives.CalloutWidget,
  'Ivy.Article': Primitives.ArticleWidget,
  'Ivy.Kbd': Primitives.KbdWidget,
  'Ivy.Empty': Primitives.EmptyWidget,
  'Ivy.Avatar': Primitives.AvatarWidget,
  'Ivy.IvyLogo': Primitives.IvyLogoWidget,
  'Ivy.Spacer': Primitives.SpacerWidget,
  'Ivy.Loading': Primitives.LoadingWidget,

  // Widgets
  'Ivy.Button': Widgets.ButtonWidget,
  'Ivy.Progress': Widgets.ProgressWidget,
  'Ivy.Tooltip': Widgets.TooltipWidget,
  'Ivy.Slot': Widgets.SlotWidget,
  'Ivy.Card': Widgets.CardWidget,
  'Ivy.Sheet': Widgets.SheetWidget,
  'Ivy.Badge': Widgets.BadgeWidget,
  'Ivy.Expandable': Widgets.ExpandableWidget,
  'Ivy.Chat': Widgets.ChatWidget,
  'Ivy.ChatMessage': Widgets.ChatMessageWidget,
  'Ivy.ChatLoading': Widgets.ChatLoadingWidget,
  'Ivy.ChatStatus': Widgets.ChatStatusWidget,
  'Ivy.DropDownMenu': Widgets.DropDownMenuWidget,

  // Layouts
  'Ivy.StackLayout': Layouts.StackLayoutWidget,
  'Ivy.WrapLayout': Layouts.WrapLayoutWidget,
  'Ivy.GridLayout': Layouts.GridLayoutWidget,
  'Ivy.HeaderLayout': Layouts.HeaderLayoutWidget,
  'Ivy.FooterLayout': Layouts.FooterLayoutWidget,
  'Ivy.TabsLayout': Layouts.TabsLayoutWidget,
  'Ivy.Tab': Layouts.TabWidget,
  'Ivy.SidebarLayout': Layouts.SidebarLayoutWidget,
  'Ivy.SidebarMenu': Layouts.SidebarMenuWidget,
  'Ivy.ResizeablePanelGroup': Layouts.ResizeablePanelGroupWidget,
  'Ivy.ResizeablePanel': Layouts.ResizeablePanelWidget,
  'Ivy.FloatingPanel': Layouts.FloatingPanelWidget,

  // Inputs
  'Ivy.TextInput': Inputs.TextInputWidget,
  'Ivy.BoolInput': Inputs.BoolInputWidget,
  'Ivy.DateTimeInput': Inputs.DateTimeInputWidget,
  'Ivy.NumberInput': Inputs.NumberInputWidget,
  'Ivy.SelectInput': Inputs.SelectInputWidget,
  'Ivy.ReadOnlyInput': Inputs.ReadOnlyInputWidget,
  'Ivy.ColorInput': Inputs.ColorInputWidget,
  'Ivy.FeedbackInput': Inputs.FeedbackInputWidget,
  'Ivy.AsyncSelectInput': Inputs.AsyncSelectInputWidget,
  'Ivy.DateRangeInput': Inputs.DateRangeInputWidget,
  'Ivy.FileInput': Inputs.FileInputWidget,
  'Ivy.CodeInput': React.lazy(() => import('@/widgets/inputs/CodeInputWidget')),

  // Forms
  'Ivy.Form': Forms.FormWidget,
  'Ivy.FormField': Forms.FormFieldWidget,

  // Dialogs
  'Ivy.Dialog': Dialogs.DialogWidget,
  'Ivy.DialogHeader': Dialogs.DialogHeaderWidget,
  'Ivy.DialogBody': Dialogs.DialogBodyWidget,
  'Ivy.DialogFooter': Dialogs.DialogFooterWidget,

  // Blades
  'Ivy.BladeContainer': Blades.BladeContainerWidget,
  'Ivy.Blade': Blades.BladeWidget,

  // Tables
  'Ivy.Table': Tables.TableWidget,
  'Ivy.TableRow': Tables.TableRowWidget,
  'Ivy.TableCell': Tables.TableCellWidget,

  // Lists
  'Ivy.List': Lists.ListWidget,
  'Ivy.ListItem': Lists.ListItemWidget,

  // Details
  'Ivy.Details': Details.DetailsWidget,
  'Ivy.Detail': Details.DetailWidget,

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
