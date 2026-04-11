import React, { useCallback, useMemo, useRef } from "react";
import { getHeight, getWidth } from "@/lib/styles";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import ReactECharts from "echarts-for-react";
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
  generateTooltip,
} from "./sharedUtils";
import { ChartType, FunnelChartWidgetProps } from "./chartTypes";
import { generateDataProps } from "./sharedUtils";
import { getChartThemeColors } from "./styles";
import { FUNNEL_DEFAULTS, FUNNEL_LEGEND_DEFAULTS, applyDefaults } from "./chartDefaults";
import { Densities } from "@/types/density";

import { EMPTY_ARRAY } from "@/lib/constants";

const FunnelChartWidget: React.FC<FunnelChartWidgetProps> = ({
  data = EMPTY_ARRAY,
  width = "Full",
  height = "Full",
  funnels = EMPTY_ARRAY,
  tooltip,
  toolbox,
  legend,
  colorScheme = "Default",
  sort = "Descending",
  orientation = "Vertical",
  gap = 0,
  density: _density = Densities.Medium,
}) => {
  const { colors, isDark } = useThemeWithMonitoring({
    monitorDOM: false,
    monitorSystem: true,
  });

  const themeColors = useMemo(() => getChartThemeColors(colors, isDark), [colors, isDark]);

  const heightStyle = height ? getHeight(height) : {};
  const isFull = height?.toLowerCase().startsWith("full");

  const styles: React.CSSProperties = {
    ...getWidth(width),
    position: "relative",
    ...(isFull ? { display: "flex", flexDirection: "column", height: "100%" } : {}),
  };

  const chartStyles: React.CSSProperties = {
    ...(isFull ? { flex: 1, minHeight: "200px" } : { ...heightStyle, minHeight: "200px" }),
    width: "100%",
  };

  const { valueKeys } = generateDataProps(data);

  const chartColors = useMemo(() => getColors(colorScheme, colors), [colorScheme, colors]);

  const funnelData = useMemo(() => {
    // Determine the value and name keys from the first funnel config,
    // falling back to the standard PieChartData property names
    const firstFunnel = funnels?.[0];
    const valKey = firstFunnel?.dataKey
      ? firstFunnel.dataKey.charAt(0).toLowerCase() + firstFunnel.dataKey.slice(1)
      : "measure";
    const nameKey = firstFunnel?.nameKey
      ? firstFunnel.nameKey.charAt(0).toLowerCase() + firstFunnel.nameKey.slice(1)
      : "dimension";

    return data.map((d) => {
      const record = d as Record<string, unknown>;
      return {
        value: record[valKey] ?? d.measure,
        name: (record[nameKey] ?? d.dimension) as string,
      };
    });
  }, [data, funnels]);

  const echartsSort = useMemo(() => {
    switch (sort?.toLowerCase()) {
      case "ascending":
        return "ascending";
      case "none":
        return "none";
      case "descending":
      default:
        return "descending";
    }
  }, [sort]);

  const echartsOrient = useMemo(() => {
    return orientation?.toLowerCase() === "horizontal" ? "horizontal" : "vertical";
  }, [orientation]);

  const series = useMemo(
    () =>
      valueKeys.map((key) => {
        const rawFunnelConfig = funnels?.find((f) => f.dataKey.toLowerCase() === key);
        const funnelConfig = rawFunnelConfig
          ? applyDefaults(rawFunnelConfig, FUNNEL_DEFAULTS)
          : FUNNEL_DEFAULTS;

        return {
          name: key,
          type: ChartType.Funnel,
          sort: echartsSort,
          orient: echartsOrient,
          gap: gap ?? 0,
          left: "10%",
          right: "10%",
          top: "10%",
          bottom: "10%",
          min: 0,
          max: Math.max(...funnelData.map((d) => d.value as number), 100),
          minSize: funnelConfig.minSize ?? "0%",
          maxSize: funnelConfig.maxSize ?? "100%",
          animation: funnelConfig.animated ?? FUNNEL_DEFAULTS.animated,
          label: {
            show: true,
            position: "inside" as const,
            color: themeColors.foreground,
            fontFamily: themeColors.fontSans,
          },
          labelLine: {
            show: false,
          },
          itemStyle: {
            color: funnelConfig.fill ?? undefined,
            opacity: funnelConfig.fillOpacity ?? undefined,
            borderColor: funnelConfig.stroke ?? undefined,
            borderWidth: funnelConfig.strokeWidth ?? FUNNEL_DEFAULTS.strokeWidth,
          },
          emphasis: {
            label: {
              fontSize: 20,
            },
          },
          data: funnelData,
        };
      }),
    [valueKeys, funnels, funnelData, echartsSort, echartsOrient, gap, themeColors],
  );

  const option = useMemo(() => {
    const leg = legend ? applyDefaults(legend, FUNNEL_LEGEND_DEFAULTS) : null;

    return {
      color: chartColors,
      ...(leg && {
        legend: {
          orient: leg.layout?.toLowerCase() === "vertical" ? "vertical" : "horizontal",
          left:
            leg.align?.toLowerCase() === "left"
              ? "left"
              : leg.align?.toLowerCase() === "right"
                ? "right"
                : "center",
          top:
            leg.verticalAlign?.toLowerCase() === "top"
              ? "top"
              : leg.verticalAlign?.toLowerCase() === "middle"
                ? "middle"
                : "bottom",
          icon: leg.iconType ?? "circle",
          itemWidth: leg.iconSize ?? FUNNEL_LEGEND_DEFAULTS.iconSize,
          itemHeight: leg.iconSize ?? FUNNEL_LEGEND_DEFAULTS.iconSize,
          type: "scroll",
          textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
        },
      }),
      textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
      tooltip: {
        ...generateTooltip(tooltip, undefined, {
          foreground: themeColors.foreground,
          fontSans: themeColors.fontSans,
          background: themeColors.background,
          mutedForeground: themeColors.mutedForeground,
        }),
        trigger: "item" as const,
        formatter: (params: { name: string; value: number; percent: number; marker: string }) => {
          return `${params.marker} ${params.name}<br/><strong>${params.value.toLocaleString()}</strong> (${params.percent}%)`;
        },
      },
      series: series,
      toolbox: generateEChartToolbox(toolbox && { ...toolbox, magicType: false }),
    };
  }, [chartColors, legend, themeColors, tooltip, series, toolbox]);

  const containerRef = useRef<HTMLDivElement>(null);
  const handleChartReady = useCallback(() => {
    containerRef.current?.setAttribute("data-chart-rendered", "true");
  }, []);

  return (
    <div ref={containerRef} style={styles}>
      <ReactECharts
        option={option}
        style={chartStyles}
        notMerge={true}
        lazyUpdate={true}
        onChartReady={handleChartReady}
      />
    </div>
  );
};

export default FunnelChartWidget;
