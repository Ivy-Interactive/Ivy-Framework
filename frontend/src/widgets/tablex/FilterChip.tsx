import React from 'react';
import { X } from 'lucide-react';
import { Filter, FilterGroup, Condition } from '@/services/grpcTableService';

interface FilterChipProps {
  filter: Filter;
  onRemove: () => void;
}

const formatCondition = (condition: Condition): string => {
  const { column, function: func, args } = condition;

  switch (func) {
    case 'equals':
      return `${column} = ${args[0]}`;
    case 'notEquals':
      return `${column} ≠ ${args[0]}`;
    case 'greaterThan':
      return `${column} > ${args[0]}`;
    case 'greaterThanOrEqual':
      return `${column} ≥ ${args[0]}`;
    case 'lessThan':
      return `${column} < ${args[0]}`;
    case 'lessThanOrEqual':
      return `${column} ≤ ${args[0]}`;
    case 'contains':
      return `${column} contains "${args[0]}"`;
    case 'notContains':
      return `${column} not contains "${args[0]}"`;
    case 'startsWith':
      return `${column} starts with "${args[0]}"`;
    case 'endsWith':
      return `${column} ends with "${args[0]}"`;
    case 'inRange':
      return `${column} between ${args[0]} and ${args[1]}`;
    case 'blank':
      return `${column} is blank`;
    case 'notBlank':
      return `${column} is not blank`;
    case 'true':
      return `${column} is true`;
    case 'false':
      return `${column} is false`;
    default:
      return `${column} ${func} ${args.join(', ')}`;
  }
};

const formatFilterGroup = (group: FilterGroup, depth = 0): string => {
  if (group.filters.length === 0) return '';

  const parts = group.filters
    .map(filter => {
      if (filter.condition) {
        return formatCondition(filter.condition);
      } else if (filter.group) {
        return depth < 2
          ? `(${formatFilterGroup(filter.group, depth + 1)})`
          : '';
      }
      return '';
    })
    .filter(Boolean);

  return parts.join(` ${group.op} `);
};

export const FilterChip: React.FC<FilterChipProps> = ({ filter, onRemove }) => {
  let displayText = '';

  if (filter.condition) {
    displayText = formatCondition(filter.condition);
  } else if (filter.group) {
    displayText = formatFilterGroup(filter.group);
  }

  if (!displayText) return null;

  return (
    <div className="inline-flex items-center gap-1 px-3 py-1 bg-blue-100 text-blue-800 rounded-full text-sm">
      <span className="max-w-[200px] truncate" title={displayText}>
        {displayText}
      </span>
      <button
        onClick={onRemove}
        className="p-0.5 hover:bg-blue-200 rounded-full"
      >
        <X size={14} />
      </button>
    </div>
  );
};
