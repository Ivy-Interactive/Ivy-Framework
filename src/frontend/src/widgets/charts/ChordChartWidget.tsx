import React, { useMemo } from "react";
import { getHeight, getWidth } from "@/lib/styles";
import { useThemeWithMonitoring } from "@/components/theme-provider";
import ReactECharts from "echarts-for-react";
import {
  getColors,
  generateTextStyle,
  generateEChartToolbox,
  generateTooltip,
  generateEChartLegend,
} from "./sharedUtils";
import { ChordChartWidgetProps } from "./chartTypes";
import { getChartThemeColors } from "./styles";

const ChordChartWidget: React.FC<ChordChartWidgetProps> = ({
  data,
  width = "Full",
  height = "Full",
  colorScheme = "Default",
  sort = false,
  tooltip,
  legend,
  toolbox,
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

  const option = useMemo(() => {
    const nodes = data?.nodes || [];
    const links = data?.links || [];

    // Build node data with category indices for coloring
    const graphNodes = nodes.map((node, index) => ({
      name: node.name,
      symbolSize: 20,
      category: index,
      itemStyle: {
        color: chartColors[index % chartColors.length],
      },
    }));

    // Build links with source/target as names and value
    const graphLinks = links.map((link) => ({
      source: nodes[link.source]?.name ?? String(link.source),
      target: nodes[link.target]?.name ?? String(link.target),
      value: link.value,
      lineStyle: {
        width: Math.max(1, Math.sqrt(link.value) * 0.5),
        opacity: 0.4,
        curveness: 0.3,
      },
    }));

    // Sort nodes if requested
    if (sort) {
      graphNodes.sort((a, b) => {
        const aTotal = links
          .filter((l) => nodes[l.source]?.name === a.name || nodes[l.target]?.name === a.name)
          .reduce((sum, l) => sum + l.value, 0);
        const bTotal = links
          .filter((l) => nodes[l.source]?.name === b.name || nodes[l.target]?.name === b.name)
          .reduce((sum, l) => sum + l.value, 0);
        return bTotal - aTotal;
      });
    }

    // Build categories for legend
    const categories = nodes.map((node, index) => ({
      name: node.name,
      itemStyle: {
        color: chartColors[index % chartColors.length],
      },
    }));

    return {
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
      },
      legend: legend
        ? generateEChartLegend(legend, {
            foreground: themeColors.foreground,
            fontSans: themeColors.fontSans,
          })
        : { show: false },
      animationDurationUpdate: 1500,
      animationEasingUpdate: "quinticInOut",
      series: [
        {
          type: "graph",
          layout: "circular",
          circular: {
            rotateLabel: false,
          },
          data: graphNodes,
          links: graphLinks,
          categories: categories,
          roam: true,
          label: {
            show: true,
            position: "right",
            color: themeColors.foreground,
            fontFamily: themeColors.fontSans,
            fontSize: 12,
          },
          lineStyle: {
            color: "source",
            curveness: 0.3,
          },
          emphasis: {
            focus: "adjacency",
            lineStyle: {
              width: 4,
              opacity: 0.8,
            },
          },
        },
      ],
      toolbox: generateEChartToolbox(toolbox && { ...toolbox, magicType: false }),
    };
  }, [chartColors, data, sort, legend, themeColors, tooltip, toolbox]);

  return (
    <div style={styles}>
      <ReactECharts option={option} style={chartStyles} notMerge={true} lazyUpdate={true} />
    </div>
  );
};

export default ChordChartWidget;
