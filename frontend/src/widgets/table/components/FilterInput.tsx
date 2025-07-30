import { Input } from '@/components/ui/input';
import { useState } from 'react';
import { useGlideDataGrid } from '../hooks/useGlideDataGrid';
import { DataTableData } from '../types';

interface FilterInputProps {
  onFilter: (value: string, data: DataTableData) => DataTableData;
}

export const FilterInput = ({ onFilter }: FilterInputProps) => {
  const [value, setValue] = useState('');
  const { data, setData } = useGlideDataGrid();

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (e.key === 'Enter') {
      setData(onFilter(value, data!));
    }
  };

  return (
    <Input
      placeholder="Filter..."
      value={value}
      onChange={e => setValue(e.target.value)}
      onKeyDown={handleKeyPress}
    />
  );
};
