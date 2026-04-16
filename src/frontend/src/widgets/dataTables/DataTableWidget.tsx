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
import { TableProps } from "./types/types";
import { getWidth, getHeight } from "@/lib/styles";
import { applyConfigDefaults, applyColumnsDefaults } from "./DataTableDefaults";

interface TableLayoutProps {
  children?: React.ReactNode;
  emptyView?: React.ReactNode[];
}

const TableLayout: React.FC<TableLayoutProps> = ({ children, emptyView }) => {
  const { error, columns, visibleRows, isLoading } = useTable();
  const showTableEditor = columns.length > 0;

  if (error) {
    return <ErrorDisplay title="Table Error" message={error} />;
  }

  // Show empty view when data has loaded, there are no rows, and an empty view slot was provided
  if (showTableEditor && !isLoading && visibleRows === 0 && emptyView && emptyView.length > 0) {
    return <div style={tableStyles.table.container}>{emptyView}</div>;
  }

  return <div style={tableStyles.table.container}>{showTableEditor ? children : <Loading />}</div>;
};

interface DataTableWidgetProps extends TableProps {
  events?: string[];
  density?: Densities;
  slots?: {
    EmptyView?: React.ReactNode[];
    HeaderLeft?: React.ReactNode[];
    HeaderRight?: React.ReactNode[];
  };
}

const EMPTY_EVENTS: string[] = [];

export const DataTable: React.FC<DataTableWidgetProps> = ({
  id,
  columns,
  connection,
  config = {},
  editable = false,
  width = "Full",
  height = "Full",
  density,
  events = EMPTY_EVENTS,
  rowActions,
  updateStream,
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

  // If height is Full, use flex-based sizing instead of height: 100%.
  // In unconstrained parents (e.g. Layout.Vertical() with no explicit height),
  // height: 100% resolves to 0 because the parent has no definite height.
  // flexGrow fills available space in flex parents, while minHeight ensures
  // at least ~5 rows are visible in unconstrained parents.
  if (height === "Full") {
    delete containerStyle.height;
    containerStyle.display = "flex";
    containerStyle.flexDirection = "column";
    containerStyle.flexGrow = 1;
    containerStyle.minHeight = "200px";
  }

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
        <TableLayout emptyView={slots?.EmptyView}>
          <DataTableHeader>
            <div className="flex items-center gap-2 w-full">
              <div className="flex items-center gap-1">
                {finalConfig.allowFiltering && (
                  <DataTableOption
                    icon={FilterIcon}
                    label="Filter"
                    tooltip="Filter table data"
                    displayMode="inline"
                    inlineDirection="right"
                    showLabel={false}
                  >
                    <DataTableFilterOption allowLlmFiltering={finalConfig.allowLlmFiltering} />
                  </DataTableOption>
                )}
                {slots?.HeaderLeft}
              </div>
              <div className="flex-1" />
              <div className="flex items-center gap-1">{slots?.HeaderRight}</div>
            </div>
          </DataTableHeader>

          <DataTableEditor
            widgetId={id}
            events={events}
            hasOptions={finalConfig.allowFiltering}
            rowActions={rowActions}
            showAggregateFooter={hasFooter}
          />
        </TableLayout>
      </TableProvider>
    </div>
  );
};

export default DataTable;
