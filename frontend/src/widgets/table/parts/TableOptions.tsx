import React from 'react';
import { useTable } from '../context/TableContext';
import { tableStyles } from '../styles/style';
import { Header } from './TableHeader';
import { Button } from '@/components/ui/button';

export const TableOptions: React.FC = () => {
  const { columns } = useTable();

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
            <Button variant="default">
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

      {/* TableFilter would go here when filter functionality is implemented */}
    </>
  );
};
