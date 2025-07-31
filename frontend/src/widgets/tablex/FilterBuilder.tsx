import React, { useState } from 'react';
import { X, Trash2 } from 'lucide-react';
import { Filter, FilterGroup, Condition } from '@/services/grpcTableService';

interface FilterBuilderProps {
  columns: Array<{ name: string; type: string }>;
  onApply: (filter: Filter | null) => void;
  onClose: () => void;
  existingFilter?: Filter | null;
}

type ConditionOperator = {
  value: string;
  label: string;
  needsValue: boolean;
  multiValue?: boolean;
};

const TEXT_OPERATORS: ConditionOperator[] = [
  { value: 'equals', label: 'Equals', needsValue: true },
  { value: 'notEquals', label: 'Not equals', needsValue: true },
  { value: 'contains', label: 'Contains', needsValue: true },
  { value: 'notContains', label: 'Not contains', needsValue: true },
  { value: 'startsWith', label: 'Starts with', needsValue: true },
  { value: 'endsWith', label: 'Ends with', needsValue: true },
  { value: 'blank', label: 'Blank', needsValue: false },
  { value: 'notBlank', label: 'Not blank', needsValue: false },
];

const NUMBER_OPERATORS: ConditionOperator[] = [
  { value: 'equals', label: '=', needsValue: true },
  { value: 'notEquals', label: '≠', needsValue: true },
  { value: 'greaterThan', label: '>', needsValue: true },
  { value: 'greaterThanOrEqual', label: '≥', needsValue: true },
  { value: 'lessThan', label: '<', needsValue: true },
  { value: 'lessThanOrEqual', label: '≤', needsValue: true },
  { value: 'inRange', label: 'In range', needsValue: true, multiValue: true },
  { value: 'blank', label: 'Blank', needsValue: false },
  { value: 'notBlank', label: 'Not blank', needsValue: false },
];

const BOOLEAN_OPERATORS: ConditionOperator[] = [
  { value: 'true', label: 'True', needsValue: false },
  { value: 'false', label: 'False', needsValue: false },
];

interface ConditionRowProps {
  condition: Condition;
  columns: Array<{ name: string; type: string }>;
  onChange: (condition: Condition) => void;
  onRemove: () => void;
}

const ConditionRow: React.FC<ConditionRowProps> = ({
  condition,
  columns,
  onChange,
  onRemove,
}) => {
  const selectedColumn = columns.find(col => col.name === condition.column);
  const columnType = selectedColumn?.type || 'string';

  const getOperators = () => {
    if (columnType.includes('bool')) return BOOLEAN_OPERATORS;
    if (
      columnType.includes('int') ||
      columnType.includes('float') ||
      columnType.includes('double')
    ) {
      return NUMBER_OPERATORS;
    }
    return TEXT_OPERATORS;
  };

  const operators = getOperators();
  const selectedOperator = operators.find(
    op => op.value === condition.function
  );

  return (
    <div className="flex gap-2 items-center p-2 bg-gray-50 rounded">
      <select
        className="px-3 py-1 border rounded"
        value={condition.column}
        onChange={e =>
          onChange({
            ...condition,
            column: e.target.value,
            function: '',
            args: [],
          })
        }
      >
        <option value="">Select column</option>
        {columns.map(col => (
          <option key={col.name} value={col.name}>
            {col.name}
          </option>
        ))}
      </select>

      <select
        className="px-3 py-1 border rounded"
        value={condition.function}
        onChange={e =>
          onChange({ ...condition, function: e.target.value, args: [] })
        }
      >
        <option value="">Select operator</option>
        {operators.map(op => (
          <option key={op.value} value={op.value}>
            {op.label}
          </option>
        ))}
      </select>

      {selectedOperator?.needsValue && (
        <>
          {selectedOperator.multiValue ? (
            <div className="flex gap-1">
              <input
                type={columnType.includes('int') ? 'number' : 'text'}
                className="px-3 py-1 border rounded w-24"
                placeholder="From"
                value={condition.args[0] || ''}
                onChange={e =>
                  onChange({
                    ...condition,
                    args: [e.target.value, condition.args[1] || ''],
                  })
                }
              />
              <input
                type={columnType.includes('int') ? 'number' : 'text'}
                className="px-3 py-1 border rounded w-24"
                placeholder="To"
                value={condition.args[1] || ''}
                onChange={e =>
                  onChange({
                    ...condition,
                    args: [condition.args[0] || '', e.target.value],
                  })
                }
              />
            </div>
          ) : (
            <input
              type={columnType.includes('int') ? 'number' : 'text'}
              className="px-3 py-1 border rounded"
              placeholder="Value"
              value={condition.args[0] || ''}
              onChange={e => onChange({ ...condition, args: [e.target.value] })}
            />
          )}
        </>
      )}

      <button
        onClick={onRemove}
        className="p-1 text-red-500 hover:bg-red-50 rounded"
      >
        <Trash2 size={16} />
      </button>
    </div>
  );
};

interface FilterGroupBuilderProps {
  group: FilterGroup;
  columns: Array<{ name: string; type: string }>;
  onChange: (group: FilterGroup) => void;
  depth?: number;
}

const FilterGroupBuilder: React.FC<FilterGroupBuilderProps> = ({
  group,
  columns,
  onChange,
  depth = 0,
}) => {
  const addCondition = () => {
    onChange({
      ...group,
      filters: [
        ...group.filters,
        { condition: { column: '', function: '', args: [] } },
      ],
    });
  };

  const addGroup = () => {
    onChange({
      ...group,
      filters: [...group.filters, { group: { op: 'AND', filters: [] } }],
    });
  };

  const updateFilter = (index: number, filter: Filter) => {
    const newFilters = [...group.filters];
    newFilters[index] = filter;
    onChange({ ...group, filters: newFilters });
  };

  const removeFilter = (index: number) => {
    onChange({
      ...group,
      filters: group.filters.filter((_, i) => i !== index),
    });
  };

  return (
    <div
      className={`border rounded p-3 ${
        depth > 0 ? 'ml-4 border-gray-300' : 'border-gray-400'
      }`}
    >
      <div className="flex justify-between items-center mb-3">
        <div className="flex gap-2">
          <label className="font-medium">Join conditions with:</label>
          <select
            className="px-2 py-1 border rounded font-bold"
            value={group.op}
            onChange={e =>
              onChange({ ...group, op: e.target.value as 'AND' | 'OR' })
            }
          >
            <option value="AND">AND</option>
            <option value="OR">OR</option>
          </select>
        </div>
        <div className="flex gap-2">
          <button
            onClick={addCondition}
            className="px-3 py-1 bg-blue-500 text-white rounded hover:bg-blue-600 text-sm"
          >
            + Add Condition
          </button>
          {depth < 2 && (
            <button
              onClick={addGroup}
              className="px-3 py-1 bg-gray-500 text-white rounded hover:bg-gray-600 text-sm"
            >
              + Add Group
            </button>
          )}
        </div>
      </div>

      <div className="space-y-2">
        {group.filters.map((filter, index) => (
          <div key={index}>
            {filter.condition ? (
              <ConditionRow
                condition={filter.condition}
                columns={columns}
                onChange={condition =>
                  updateFilter(index, { ...filter, condition })
                }
                onRemove={() => removeFilter(index)}
              />
            ) : filter.group ? (
              <FilterGroupBuilder
                group={filter.group}
                columns={columns}
                onChange={newGroup => updateFilter(index, { group: newGroup })}
                depth={depth + 1}
              />
            ) : null}
          </div>
        ))}
      </div>

      {group.filters.length === 0 && (
        <div className="text-gray-500 text-center py-4">
          No conditions added. Click "Add Condition" to start.
        </div>
      )}
    </div>
  );
};

export const FilterBuilder: React.FC<FilterBuilderProps> = ({
  columns,
  onApply,
  onClose,
  existingFilter,
}) => {
  const [rootGroup, setRootGroup] = useState<FilterGroup>(
    existingFilter?.group || { op: 'AND', filters: [] }
  );

  const handleApply = () => {
    if (rootGroup.filters.length === 0) {
      onApply(null);
    } else {
      onApply({ group: rootGroup });
    }
    onClose();
  };

  const handleClear = () => {
    setRootGroup({ op: 'AND', filters: [] });
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg w-[800px] max-h-[80vh] overflow-hidden flex flex-col">
        <div className="flex justify-between items-center p-4 border-b">
          <h2 className="text-xl font-bold">Advanced Filter</h2>
          <button onClick={onClose} className="p-1 hover:bg-gray-100 rounded">
            <X size={20} />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-4">
          <FilterGroupBuilder
            group={rootGroup}
            columns={columns}
            onChange={setRootGroup}
          />
        </div>

        <div className="flex justify-between p-4 border-t">
          <button
            onClick={handleClear}
            className="px-4 py-2 border rounded hover:bg-gray-50"
          >
            Clear All
          </button>
          <div className="flex gap-2">
            <button
              onClick={onClose}
              className="px-4 py-2 border rounded hover:bg-gray-50"
            >
              Cancel
            </button>
            <button
              onClick={handleApply}
              className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
            >
              Apply Filter
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
