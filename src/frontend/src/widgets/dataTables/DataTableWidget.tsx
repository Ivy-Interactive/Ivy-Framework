import "@glideapps/glide-data-grid/dist/index.css";
import "./styles/checkbox.css";
import React, { useMemo } from "react";
import { TableProvider } from "./dataTableContext";
import { useTable } from "./dataTableContext";
import { ErrorDisplay } from "@/components/ErrorDisplay";
import { Loading } from "@/components/Loading";
import { DataTableEditor } from "./dataTableEditor";
import { DataTableHeader } from "./DataTableHeader";
import { DataTableOption } from "./DataTableOption";
import { DataTableFilterOption } from "./options/DataTableFilterOption";
import { Filter as FilterIcon } from "lucide-react";
import { tableStyles } from "./styles/style";
import { Densities } from "@/types/density";
import { TableProps, DataTableConfig } from "./types/types";
import { getWidth, getHeight } from "@/lib/styles";
import { applyConfigDefaults, applyColumnsDefaults } from "./DataTableDefaults";
import { getDataTableMinHeight } from "./dataTableLayout";
import type { SpriteMap } from "@glideapps/glide-data-grid";

interface TableLayoutProps {
  children?: React.ReactNode;
}

const TableLayout: React.FC<TableLayoutProps> = ({ children }) => {
  const { error, columns } = useTable();
  const showTableEditor = columns.length > 0;

  if (error) {
    return <ErrorDisplay title="Table Error" message={error} />;
  }

  if (!showTableEditor) {
    return (
      <div style={tableStyles.table.container}>
        <Loading />
      </div>
    );
  }

  return <div style={{ ...tableStyles.table.container }}>{children}</div>;
};

interface DataTableWidgetProps extends TableProps {
  events?: string[];
  density?: Densities;
  headerIcons?: SpriteMap;
  slots?: {
    EmptyView?: React.ReactNode[];
    HeaderLeft?: React.ReactNode[];
    HeaderRight?: React.ReactNode[];
  };
}

const EMPTY_EVENTS: string[] = [];
const EMPTY_CONFIG: DataTableConfig = {};

export const DataTable: React.FC<DataTableWidgetProps> = ({
  id,
  columns,
  connection,
  config = EMPTY_CONFIG,
  editable = false,
  width = "Full",
  height = "Full",
  density,
  events = EMPTY_EVENTS,
  rowActions,
  perRowActions,
  updateStream,
  headerIcons,
  slots,
  "data-testid": dataTestId,
}) => {
  const finalConfig = useMemo(
    () => ({
      ...applyConfigDefaults(config),
      // Frontend-only config options (not in backend)
      filterType: config.filterType,
      enableRowHover: config.enableRowHover ?? true,
    }),
    [config],
  );

  const finalColumns = useMemo(() => applyColumnsDefaults(columns), [columns]);

  const hasFooter = useMemo(
    () => finalColumns.some((col) => col.footer && col.footer.length > 0),
    [finalColumns],
  );

  // Create styles object with width and height if provided
  const containerStyle: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  // Shrink with the viewport down to a usable minimum, then let the page scroll.
  const minHeight = getDataTableMinHeight({
    density: density ?? Densities.Medium,
    hasFilter: finalConfig.allowFiltering,
    hasGroups: finalConfig.showGroups,
    hasAggregateFooter: hasFooter,
  });

  if (height === "Full") {
    delete containerStyle.height;
    containerStyle.display = "flex";
    containerStyle.flexDirection = "column";
    containerStyle.flexGrow = 1;
    containerStyle.flexShrink = 1;
    containerStyle.minHeight = minHeight;
    // no maxHeight — flexGrow fills the flex parent; a percentage max-height
    // collapses the table inside the scrolling app host (issue #1695 / PR #4485).
  } else {
    containerStyle.display = "flex";
    containerStyle.flexDirection = "column";
    containerStyle.flexShrink = 1;
    containerStyle.minHeight = minHeight;
    if (containerStyle.height) {
      containerStyle.flexBasis = containerStyle.height;
      delete containerStyle.height;
    }
  }

  const densityMode = density ?? Densities.Medium;
  const spacing = {
    [Densities.Small]: { mb: "mb-1", gapOuter: "gap-1", gapInner: "gap-1" },
    [Densities.Medium]: { mb: "mb-2", gapOuter: "gap-2", gapInner: "gap-2" },
    [Densities.Large]: { mb: "mb-4", gapOuter: "gap-4", gapInner: "gap-3" },
  }[densityMode];

  return (
    <div style={containerStyle} data-testid={dataTestId}>
      <TableProvider
        columns={finalColumns}
        connection={connection}
        config={finalConfig}
        editable={editable}
        density={density}
        updateStream={updateStream}
      >
        <TableLayout>
          <DataTableHeader className={spacing.mb}>
            <div className={`flex flex-wrap items-center w-full ${spacing.gapOuter}`}>
              <div className={`flex min-w-0 flex-1 items-center ${spacing.gapInner}`}>
                {finalConfig.allowFiltering && (
                  <DataTableOption
                    icon={FilterIcon}
                    label="Filter"
                    tooltip="Filter table data"
                    displayMode="inline"
                    inlineDirection="right"
                    showLabel={false}
                    density={densityMode}
                  >
                    <DataTableFilterOption allowLlmFiltering={finalConfig.allowLlmFiltering} />
                  </DataTableOption>
                )}
                {slots?.HeaderLeft}
              </div>
              <div className={`flex shrink-0 items-center ${spacing.gapInner}`}>
                {slots?.HeaderRight}
              </div>
            </div>
          </DataTableHeader>

          <DataTableEditor
            widgetId={id}
            events={events}
            hasOptions={finalConfig.allowFiltering}
            rowActions={rowActions}
            perRowActions={perRowActions}
            showAggregateFooter={hasFooter}
            headerIcons={headerIcons}
          />
        </TableLayout>
      </TableProvider>
    </div>
  );
};

export default DataTable;
