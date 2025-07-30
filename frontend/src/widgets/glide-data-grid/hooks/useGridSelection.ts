import { CompactSelection, GridSelection } from '@glideapps/glide-data-grid';
import { useState } from 'react';

export function useGridSelection() {
  const [gridSelection, setGridSelection] = useState<GridSelection>({
    columns: CompactSelection.empty(),
    rows: CompactSelection.empty(),
    current: undefined,
  });

  return { gridSelection, setGridSelection };
}
