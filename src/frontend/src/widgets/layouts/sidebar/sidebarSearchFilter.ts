import type { MenuItem } from '@/types/widgets';

/** Normalize string for search: strip whitespace/dash/underscore, lowercase */
function normalizeForSearch(s: string): string {
  if (!s) return '';
  return s.replace(/[\s\-_]+/g, '').toLowerCase();
}

function isWordMatch(tag: string, searchString: string): boolean {
  const words = tag.split(/[-_\s]+/);
  const lower = searchString.toLowerCase();
  return words.some(word => word.toLowerCase().startsWith(lower));
}

function labelContainsAsWord(label: string, searchString: string): boolean {
  if (!label || !searchString) return false;
  const escaped = searchString.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
  const pattern = new RegExp(`\\b${escaped}\\b`, 'i');
  return pattern.test(label);
}

/**
 * Score how well a menu item matches the search string (mirrors C# ChromeUtils.ItemMatchScore).
 */
export function itemMatchScore(item: MenuItem, searchString: string): number {
  const label = item.label ?? '';
  const normalizedLabel = normalizeForSearch(label);
  const normalizedSearch = normalizeForSearch(searchString);

  if (label.toLowerCase() === searchString.toLowerCase()) return 5;
  if (normalizedSearch.length > 0 && normalizedLabel === normalizedSearch)
    return 5;
  if (label.toLowerCase().startsWith(searchString.toLowerCase())) return 4;
  if (
    normalizedSearch.length > 0 &&
    normalizedLabel.startsWith(normalizedSearch)
  )
    return 4;
  if (labelContainsAsWord(label, searchString)) return 3;
  if (label.toLowerCase().includes(searchString.toLowerCase())) return 2;
  if (normalizedSearch.length > 0 && normalizedLabel.includes(normalizedSearch))
    return 2;
  const hints = (item as MenuItem & { searchHints?: string[] }).searchHints;
  if (
    hints?.some(
      tag =>
        isWordMatch(tag, searchString) ||
        (normalizedSearch.length > 0 &&
          normalizeForSearch(tag).includes(normalizedSearch))
    )
  )
    return 1;
  return 0;
}

/** Flatten menu tree to leaf items with path (mirrors C# FlattenWithPath). */
export function flattenWithPath(
  menuItems: MenuItem[],
  parentPath = ''
): { item: MenuItem; path: string }[] {
  const result: { item: MenuItem; path: string }[] = [];
  for (const item of menuItems) {
    const currentPath = !parentPath
      ? (item.label ?? '')
      : `${parentPath} / ${item.label ?? ''}`;

    if (item.children && item.children.length > 0) {
      result.push(...flattenWithPath(item.children, currentPath));
    } else {
      result.push({ item, path: parentPath });
    }
  }
  return result;
}

/**
 * Filter full menu tree by query and return items in "Search Results" shape
 * (single group with matching leaves + path), and flat list for list component.
 */
export function filterMenuItemsForSearch(
  items: MenuItem[],
  query: string
): { searchResultsItems: MenuItem[]; flatItems: MenuItem[] } {
  const q = query?.trim() ?? '';
  if (!q) {
    return { searchResultsItems: [], flatItems: [] };
  }

  const flattened = flattenWithPath(items);
  const scored = flattened
    .map(({ item, path }) => ({
      item: { ...item, path: path || undefined },
      score: itemMatchScore(item, q),
    }))
    .filter(x => x.score > 0)
    .sort(
      (a, b) =>
        b.score - a.score ||
        (a.item.label ?? '').localeCompare(b.item.label ?? '')
    );

  const flatItems = scored.map(x => x.item);
  const searchResultsItems: MenuItem[] =
    flatItems.length > 0
      ? [
          {
            label: 'Search Results',
            variant: 'Default',
            checked: false,
            disabled: false,
            expanded: true,
            children: flatItems,
          } as MenuItem,
        ]
      : [];

  return { searchResultsItems, flatItems };
}
