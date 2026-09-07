import { useCallback } from "react";
import { Theme } from "@glideapps/glide-data-grid";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import { getCSSVariable, ThemeColors } from "@/lib/theme";
import { Densities } from "@/types/density";
import { DENSITY_CONFIG } from "../dataTableEditor/constants";

interface UseTableThemeProps {
  showVerticalBorders: boolean | undefined;
  enableRowHover: boolean | undefined;
  visibleRows: number;
  hoverRow: number | undefined;
  density?: Densities;
  /** When true, header hover uses the same color as the default header (no click flash). */
  suppressHeaderHoverHighlight?: boolean;
}

/**
 * Hook to generate and manage table theme
 */
export const useTableTheme = ({
  showVerticalBorders,
  enableRowHover,
  visibleRows,
  hoverRow,
  density = Densities.Medium,
  suppressHeaderHoverHighlight = false,
}: UseTableThemeProps) => {
  const densityConfig = DENSITY_CONFIG[density];
  const {
    customTheme: tableTheme,
    colors: themeColors,
    isDark,
  } = useThemeWithMonitoring<Partial<Theme>>({
    monitorDOM: true,
    monitorSystem: true,
    customThemeGenerator: (colors: ThemeColors, isDark: boolean): Partial<Theme> => {
      const bgHeader = colors.background || (isDark ? "#1a1a1f" : "#f9fafb");
      const rawFontSans = getCSSVariable("--font-sans") || "Geist";
      const fontSans = rawFontSans.replace(/^["']|["']$/g, "") || "Geist";
      const resolvedFontFamily = `${fontSans}, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif`;
      return {
        fontFamily: resolvedFontFamily,
        headerFontStyle: "600 13px",
        baseFontStyle: "13px",
        editorFontSize: "13px",
        bgCell: colors.background || (isDark ? "#000000" : "#ffffff"),
        bgHeader,
        bgHeaderHasFocus: colors.muted || (isDark ? "#26262b" : "#f3f4f6"),
        bgHeaderHovered: suppressHeaderHoverHighlight
          ? bgHeader
          : colors.accent || (isDark ? "#26262b" : "#e5e7eb"),
        textHeader: colors.foreground || (isDark ? "#f8f8f8" : "#111827"),
        // textHeaderSelected contrasts with accentColor (selected column headers in column-select mode)
        textHeaderSelected: colors.primaryForeground || (isDark ? "#f8f8f8" : "#ffffff"),
        textDark: colors.foreground || (isDark ? "#f8f8f8" : "#111827"),
        textMedium: colors.mutedForeground || (isDark ? "#a1a1aa" : "#6b7280"),
        textLight: colors.mutedForeground || (isDark ? "#71717a" : "#9ca3af"),
        // bgIconHeader is the background color for icon areas, should be subtle
        bgIconHeader: colors.muted || (isDark ? "#26262b" : "#f3f4f6"),
        // accentColor is used for the selected cell focus ring, selected column header bg,
        // fill handle, and highlighted checkbox — use primary so selection reads as deliberate
        accentColor: colors.primary || (isDark ? "#3b82f6" : "#2563eb"),
        // accentFg is the foreground/text color used on top of accentColor backgrounds
        accentFg: colors.primaryForeground || (isDark ? "#f8f8f8" : "#ffffff"),
        // column focus bg color
        accentLight: colors.muted || (isDark ? "#27272a" : "#e4e4e7"),
        horizontalBorderColor: colors.border || (isDark ? "#404045" : "#d1d5db"),
        linkColor: colors.primary || colors.accent || (isDark ? "#3b82f6" : "#2563eb"),
        // Control vertical borders by setting borderColor to transparent when disabled
        borderColor:
          (showVerticalBorders ?? false)
            ? colors.border || (isDark ? "#404045" : "#d1d5db")
            : "transparent",
        bgSearchResult: isDark ? "#3f3520" : "#fff9e3",
        cellHorizontalPadding: densityConfig.cellHorizontalPadding,
        cellVerticalPadding: densityConfig.cellVerticalPadding,
        headerIconSize: densityConfig.headerIconSize,
        // Add proper text colors for group headers and icons
        textGroupHeader: colors.mutedForeground || (isDark ? "#a1a1aa" : "#6b7280"),
        // Icon foreground color
        fgIconHeader: colors.mutedForeground || (isDark ? "#9ca3af" : "#6b7280"),
      };
    },
  });

  // Get row theme override for hover effect and empty filler rows
  const getRowThemeOverride = useCallback(
    (row: number) => {
      // If this is an empty filler row, remove all borders and styling
      if (row >= visibleRows) {
        return {
          bgCell: themeColors.background || (isDark ? "#000000" : "#ffffff"),
          bgCellMedium: themeColors.background || (isDark ? "#000000" : "#ffffff"),
          borderColor: "transparent",
          horizontalBorderColor: "transparent",
        };
      }
      // Handle hover effect for data rows
      if (!(enableRowHover ?? false) || row !== hoverRow) return undefined;
      // Use theme-aware colors for hover effect
      return {
        bgCell: themeColors.muted || (isDark ? "#26262b" : "#f7f7f7"),
        bgCellMedium: themeColors.background || (isDark ? "#1f1f23" : "#f0f0f0"),
      };
    },
    [hoverRow, enableRowHover, themeColors, isDark, visibleRows],
  );

  return {
    tableTheme,
    themeColors,
    isDark,
    getRowThemeOverride,
  };
};
