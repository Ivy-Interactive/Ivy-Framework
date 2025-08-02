import { Theme } from '@glideapps/glide-data-grid';

// TODO: Update theme to match design system
// Currently can't use css variables due to @glideapps/glide-data-grid limitations
// We could create design system provider that injects these styles
export const tableTheme: Partial<Theme> = {
  bgCell: '#fff',
  bgHeader: '#fff',
  bgHeaderHasFocus: '#f3f4f6',
  bgHeaderHovered: '#e5e7eb',
  textDark: '#111827',
  textMedium: '#6b7280',
  textLight: '#9ca3af',
  borderColor: '#e5e7eb',
  linkColor: '#e5e7eb',
  cellHorizontalPadding: 16,
  cellVerticalPadding: 8,
};
