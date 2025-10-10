import React, { useState, useEffect } from 'react';
import { useTable } from './TableContext';
import { tableStyles } from './styles/style';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { QueryEditor, QueryEditorChangeEvent } from 'filter-query-editor';
import { Filter } from '@/services/grpcTableService';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from '@/components/ui/sheet';
import { Menu } from 'lucide-react';

export const TableOptions: React.FC = () => {
  const {
    visibleRows,
    columns,
    editable,
    hasMore,
    activeFilter,
    setActiveFilter,
  } = useTable();
  const [query, setQuery] = useState<string>('');
  const [pendingFilter, setPendingFilter] = useState<Filter | null>(null);
  const [isSmallScreen, setIsSmallScreen] = useState(false);

  // Check screen size
  useEffect(() => {
    const checkScreenSize = () => {
      setIsSmallScreen(window.innerWidth < 900);
    };

    checkScreenSize();
    window.addEventListener('resize', checkScreenSize);

    return () => window.removeEventListener('resize', checkScreenSize);
  }, []);

  if (columns.length === 0) {
    return null;
  }

  const handleQueryChange = (event: QueryEditorChangeEvent) => {
    setQuery(event.text);

    if (event.isValid && event.filters) {
      setPendingFilter({ group: event.filters });
    } else {
      setPendingFilter(null);
    }
  };

  const handleKeyDown = (event: React.KeyboardEvent) => {
    if (
      event.key === 'Enter' &&
      (event.metaKey || event.ctrlKey || !event.shiftKey)
    ) {
      event.preventDefault();
      if (pendingFilter) {
        setActiveFilter(pendingFilter);
      } else {
        setActiveFilter(null);
      }
    }
  };

  const queryEditorContent = (
    <div className="flex gap-2 flex-col sm:flex-row">
      <div
        className="w-full sm:w-[600px] query-editor-wrapper"
        onKeyDown={handleKeyDown}
      >
        <QueryEditor
          value={query}
          columns={columns}
          onChange={handleQueryChange}
          placeholder='e.g., name = "John" AND age > 18 '
          height={40}
          className="font-mono rounded-lg border shadow-sm [&:focus-within]:ring-2 [&:focus-within]:ring-ring [&:focus-within]:ring-offset-2"
        />
        {/* TODO: don't force styling */}
        <style>{`
          .query-editor-wrapper .cm-editor.cm-focused {
            outline: none !important;
          }
          .query-editor-wrapper .cm-content {
            padding: 8px 16px;
            min-height: auto;
          }
        `}</style>
      </div>
      {activeFilter && (
        <Button variant="outline" onClick={() => setActiveFilter(null)}>
          Clear
        </Button>
      )}
    </div>
  );

  const metadataContent = (
    <div className="flex gap-4 flex-wrap">
      <Label className="text-sm text-muted-foreground">
        {columns.length} {columns.length === 1 ? 'column' : 'columns'}
      </Label>
      <Label className="text-sm text-muted-foreground">
        {visibleRows} {visibleRows === 1 ? 'row' : 'rows'}
      </Label>
      {editable && (
        <Label className="text-sm text-muted-foreground">Editable</Label>
      )}
      {!hasMore && visibleRows > 0 && (
        <Label className="text-sm text-muted-foreground">All data loaded</Label>
      )}
    </div>
  );

  if (isSmallScreen) {
    return (
      <div style={tableStyles.tableOptions.container}>
        <div className={tableStyles.tableOptions.inner}>
          {queryEditorContent}
          <Sheet>
            <SheetTrigger asChild>
              <Button variant="outline" size="sm">
                <Menu className="h-4 w-4 mr-2" />
                Table Info
              </Button>
            </SheetTrigger>
            <SheetContent side="right">
              <SheetHeader>
                <SheetTitle>Table Information</SheetTitle>
              </SheetHeader>
              <div className="mt-4 space-y-4">{metadataContent}</div>
            </SheetContent>
          </Sheet>
        </div>
      </div>
    );
  }

  return (
    <div style={tableStyles.tableOptions.container}>
      <div className={tableStyles.tableOptions.inner}>
        {queryEditorContent}
        {metadataContent}
      </div>
    </div>
  );
};
