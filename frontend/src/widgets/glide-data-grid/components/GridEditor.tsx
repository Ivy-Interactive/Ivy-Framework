import { DataEditor } from '@glideapps/glide-data-grid';
import '@glideapps/glide-data-grid/dist/index.css';
import { useMemo } from 'react';
import { useGlideDataGrid } from '../hooks/useGlideDataGrid';
import { defaultGlideTheme } from '../styles';
import { createGlideColumns } from '../utils/glideGridColumns';
import {
  calculateGridDimensions,
  parsePixelValue,
} from '../utils/gridDimensions';

export function GridEditor() {
  const {
    props,
    data,
    gridSelection,
    setGridSelection,
    columnWidths,
    getCellContent,
    handleColumnResize,
    handleCellEdited,
  } = useGlideDataGrid();

  const {
    width,
    height,
    resizableColumns = true,
    editable = true,
    smoothScrollX = true,
    smoothScrollY = true,
    rowMarkers = 'both',
  } = props;

  const columns = useMemo(() => {
    if (!data?.columns) return [];
    return createGlideColumns(data.columns, columnWidths, resizableColumns);
  }, [data?.columns, columnWidths, resizableColumns]);

  const dimensions = useMemo(() => {
    const widthPx = parsePixelValue(width || '800px', 800);
    const heightPx = parsePixelValue(height || '600px', 600);
    return calculateGridDimensions(widthPx, heightPx);
  }, [width, height]);

  if (!data) {
    return null;
  }

  return (
    <DataEditor
      getCellContent={getCellContent}
      columns={columns}
      rows={data.rows.length}
      onColumnResize={handleColumnResize}
      onCellEdited={editable ? handleCellEdited : undefined}
      gridSelection={gridSelection}
      onGridSelectionChange={setGridSelection}
      theme={defaultGlideTheme}
      smoothScrollX={smoothScrollX}
      smoothScrollY={smoothScrollY}
      rowMarkers={rowMarkers}
      width={dimensions.width}
      height={dimensions.height}
      isDraggable={false}
      rangeSelect="rect"
      columnSelect="none"
      rowSelect="none"
    />
  );
}
