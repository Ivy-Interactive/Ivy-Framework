import React, { ReactNode } from 'react';
import { cn } from '@/lib/utils';
import { tableStyles } from './styles/style';
import { DataColumn } from './types/types';

/**
 * Footer component that overlaps the bottom of the DataTableEditor
 * Horizontal scrollbars from the editor will appear on top of this footer
 */
export interface DataTableFooterProps {
  children?: ReactNode;
  className?: string;
}

export const DataTableFooter: React.FC<DataTableFooterProps> = ({
  children,
  className,
}) => {
  return (
    <div className={cn(className)} style={tableStyles.tableEditor.footer}>
      {children}
    </div>
  );
};

/**
 * Renders aggregate footer values from column footer data
 */
export interface AggregateFooterProps {
  columns: DataColumn[];
}

export const AggregateFooter: React.FC<AggregateFooterProps> = ({
  columns,
}) => {
  const hasFooter = columns.some(
    (col) => col.footer && col.footer.length > 0
  );
  if (!hasFooter) return null;

  return (
    <DataTableFooter>
      <div style={footerStyles.row}>
        {columns.map((col) => {
          const footerValues = col.footer;
          if (!footerValues || footerValues.length === 0) {
            return (
              <div key={col.name} style={footerStyles.cell}>
                &nbsp;
              </div>
            );
          }
          return (
            <div
              key={col.name}
              style={{
                ...footerStyles.cell,
                textAlign: col.align === 'Right' ? 'right' : col.align === 'Center' ? 'center' : 'left',
              }}
            >
              {footerValues.map((value, i) => (
                <div key={i} style={footerStyles.value}>
                  {value}
                </div>
              ))}
            </div>
          );
        })}
      </div>
    </DataTableFooter>
  );
};

const footerStyles = {
  row: {
    display: 'flex',
    gap: '8px',
    alignItems: 'flex-start',
  } as React.CSSProperties,
  cell: {
    flex: 1,
    minWidth: 0,
    fontSize: '12px',
    fontWeight: 600,
    color: 'var(--foreground)',
  } as React.CSSProperties,
  value: {
    lineHeight: '1.4',
    whiteSpace: 'nowrap',
    overflow: 'hidden',
    textOverflow: 'ellipsis',
  } as React.CSSProperties,
};
