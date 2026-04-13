import React, { useCallback, useMemo, useRef } from "react";
import { getHeight, getWidth } from "@/lib/styles";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import ReactECharts from "echarts-for-react";
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
  generateTooltip,
  generateEChartLegend,
  formatTooltipValue,
} from "./sharedUtils";
import { SankeyChartWidgetProps } from "./chartTypes";
import { getChartThemeColors } from "./styles";
import { Densities } from "@/types/density";

const SankeyChartWidget: React.FC<SankeyChartWidgetProps> = ({
  data,
  width = "Full",
  height = "Full",
  colorScheme = "Default",
  nodeWidth = 20,
  nodeGap = 8,
  curvature = 0.5,
  layoutIterations = 32,
  nodeAlign = "Justify",
  tooltip,
  legend,
  toolbox,
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

  const chartColors = useMemo(() => getColors(colorScheme, colors), [colorScheme, colors]);

  const option = useMemo(
    () => ({
      color: chartColors,
      textStyle: generateTextStyle(themeColors.foreground, themeColors.fontSans),
      tooltip: {
        ...generateTooltip(tooltip, undefined, {
          foreground: themeColors.foreground,
          fontSans: themeColors.fontSans,
          background: themeColors.background,
          mutedForeground: themeColors.mutedForeground,
        }),
        trigger: "item",
        formatter: (params: any) => {
          if (params.dataType === "edge") {
            const sourceName = params.data.source;
            const targetName = params.data.target;
            const value = formatTooltipValue(params.data.value, tooltip);
            return `${sourceName} → ${targetName}<br/>${value}`;
          }
          if (params.value != null) {
            const formattedValue = formatTooltipValue(params.value, tooltip);
            return `${params.name}<br/><strong>${formattedValue}</strong>`;
          }
          return params.name;
        },
      },
      legend: generateEChartLegend(legend, {
        foreground: themeColors.foreground,
        fontSans: themeColors.fontSans,
      }),
      series: [
        {
          type: "sankey",
          data: data?.nodes || [],
          links: (data?.links || []).map((link) => ({
            ...link,
            source: data?.nodes?.[link.source]?.name ?? link.source,
            target: data?.nodes?.[link.target]?.name ?? link.target,
          })),
          nodeWidth: nodeWidth,
          nodeGap: nodeGap,
          layoutIterations: layoutIterations,
          orient: "horizontal",
          nodeAlign: nodeAlign.toLowerCase(),
          emphasis: {
            focus: "adjacency",
          },
          lineStyle: {
            color: "gradient",
            curveness: curvature,
          },
          label: {
            color: themeColors.foreground,
            fontFamily: themeColors.fontSans,
          },
        },
      ],
      toolbox: generateEChartToolbox(toolbox && { ...toolbox, magicType: false }),
    }),
    [
      chartColors,
      data,
      nodeWidth,
      nodeGap,
      curvature,
      layoutIterations,
      nodeAlign,
      legend,
      themeColors,
      tooltip,
      toolbox,
    ],
  );

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

export default SankeyChartWidget;
