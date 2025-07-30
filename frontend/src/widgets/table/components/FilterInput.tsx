import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { X } from 'lucide-react';
import { useState } from 'react';
import { useGlideDataGrid } from '../hooks/useGlideDataGrid';
import { combineFiltersWithAnd } from '../utils/filterUtils';

export const FilterInput = () => {
  const [value, setValue] = useState('');
  const [appliedFilters, setAppliedFilters] = useState<string[]>([]);
  const { data, refetchWithFilters } = useGlideDataGrid();

  const handleFiltersChange = (filters: string[]) => {
    if (!data?.columns) return;

    const columnNames = data.columns.map(col => col.name);
    const filter = combineFiltersWithAnd(filters, columnNames);
    refetchWithFilters(filter);
  };

  const handleKeyPress = (e: React.KeyboardEvent<HTMLInputElement>) => {
    if (
      e.key === 'Enter' &&
      value.trim() &&
      !appliedFilters.includes(value.trim())
    ) {
      const newFilters = [...appliedFilters, value.trim()];
      setAppliedFilters(newFilters);
      handleFiltersChange(newFilters);
      setValue('');
    }
  };

  const handleRemoveFilter = (filterToRemove: string) => {
    const newFilters = appliedFilters.filter(
      filter => filter !== filterToRemove
    );
    setAppliedFilters(newFilters);
    handleFiltersChange(newFilters);
  };

  return (
    <div className="flex items-center gap-2">
      {appliedFilters.map(filter => (
        <Badge
          key={filter}
          variant="secondary"
          className="cursor-pointer flex items-center gap-1"
        >
          {filter}
          <X size={12} onClick={() => handleRemoveFilter(filter)} />
        </Badge>
      ))}
      <Input
        placeholder="Filter..."
        value={value}
        onChange={e => setValue(e.target.value)}
        onKeyDown={handleKeyPress}
        className="w-48"
      />
    </div>
  );
};
