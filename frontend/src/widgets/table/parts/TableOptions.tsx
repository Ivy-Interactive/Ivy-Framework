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
        <DialogContent className="bg-white p-4 rounded-lg max-w-[512px] flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <h2>Filter</h2>
            <Button
              variant="ghost"
              size="sm"
              onClick={() => setIsFilterOpen(false)}
            >
              <svg
                className="w-[9.251px] h-[9.251px]"
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
          <div className="flex items-center justify-between">
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
