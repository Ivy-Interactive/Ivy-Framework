import React, { useState } from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';
import { Header } from './TableHeader';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';

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
                className="w-4 h-4 mr-2"
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
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Filter options</DialogTitle>
            <DialogDescription>
              Configure filters for the current table.{` `}
              This feature is coming soon.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="secondary" onClick={() => setIsFilterOpen(false)}>
              Close
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* TableFilter would go here when filter functionality is implemented */}
    </>
  );
};
