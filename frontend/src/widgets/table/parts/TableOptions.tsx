import React, { useState } from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';
import { Header } from './TableHeader';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent } from '@/components/ui/dialog';
import { Separator } from '@/components/ui/separator';
import { Input } from '@/components/ui/input';

export const TableOptions: React.FC = () => {
  const { columns } = useTable();
  const [isFilterOpen, setIsFilterOpen] = useState(false);

  if (columns.length === 0) {
    return null;
  }

  return (
    <>
      <div style={tableStyles.tableOptions.container}>
        <div className={tableStyles.tableOptions.inner}>
          <div className={tableStyles.tableOptions.leftSection}>
            <Header />
          </div>

          <div className={tableStyles.tableOptions.rightSection}>
            <Button variant="default" onClick={() => setIsFilterOpen(true)}>
              <svg
                className={tableStyles.tableOptions.dialog.filterIcon}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M3 4a1 1 0 011-1h16a1 1 0 011 1v2.586a1 1 0 01-.293.707l-6.414 6.414a1 1 0 00-.293.707V17l-4 4v-6.586a1 1 0 00-.293-.707L3.293 7.707A1 1 0 013 7V4z"
                />
              </svg>
              Filter
            </Button>
          </div>
        </div>
      </div>
      <Dialog open={isFilterOpen} onOpenChange={setIsFilterOpen}>
        <DialogContent className={tableStyles.tableOptions.dialog.content}>
          <div className={tableStyles.tableOptions.dialog.header}>
            <h2>Filter</h2>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setIsFilterOpen(false)}
            >
              <svg
                className={tableStyles.tableOptions.dialog.closeIcon}
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M6 18L18 6M6 6l12 12"
                />
              </svg>
            </Button>
          </div>
          <Separator />
          <Input />
          <Separator />
          <div className={tableStyles.tableOptions.dialog.footer}>
            <Button variant="outline" onClick={() => setIsFilterOpen(false)}>
              Cancel
            </Button>
            <Button onClick={() => setIsFilterOpen(false)}>Apply filter</Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
};
