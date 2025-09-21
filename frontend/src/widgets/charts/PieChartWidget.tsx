import React from 'react';
import { LegendProps, Pie, PieChart, Cell, LabelList, Label } from 'recharts';
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
} from '@/components/ui/chart';
import {
  ColorScheme,
  ExtendedPieProps,
  ExtendedTooltipProps,
  generatePieProps,
  getColorGenerator,
  generateLegendProps,
  generateLabelListProps,
} from './shared';
import { getHeight, getWidth } from '@/lib/styles';

interface PieChartTotalProps {
  formattedValue: string;
  label: string;
}

interface PieChartData {
  [key: string]: string | number;
}

interface PieChartWidgetProps {
  id: string;
  data: PieChartData[];
  width?: string;
  height?: string;
  pies?: ExtendedPieProps[];
  tooltip?: ExtendedTooltipProps;
  legend?: LegendProps;
  colorScheme: ColorScheme;
  total?: PieChartTotalProps;
}

const PieChartWidget: React.FC<PieChartWidgetProps> = ({
  data,
  width,
  height,
  pies,
  tooltip,
  legend,
  colorScheme,
  total,
}) => {
  const styles: React.CSSProperties = {
    ...getWidth(width),
    ...getHeight(height),
  };

  const chartConfig = {} satisfies ChartConfig;
  const [colorGenerator] = getColorGenerator(colorScheme);

  return (
    <ChartContainer
      config={chartConfig}
      style={styles}
      className="aspect-square w-full"
    >
      <PieChart
        margin={{ top: 0, right: 0, bottom: 0, left: 0 }}
        accessibilityLayer
      >
        {legend && (
          <ChartLegend
            {...generateLegendProps(legend)}
            verticalAlign="bottom"
            align="center"
            layout="horizontal"
            content={<ChartLegendContent splitThreshold={6} />}
          />
        )}

          {tooltip && (
            <ChartTooltip
              cursor={false}
              isAnimationActive={tooltip?.animated}
              content={<ChartTooltipContent />}
            />
          )}

        {pies?.map((props, pieIndex) => {
          const isDonut = !!props.innerRadius;
          const manySlices = Array.isArray(data) && data.length > 6;
          const basePieProps = generatePieProps(props);
          const pieOverrides =
            isDonut && manySlices
              ? { labelLine: false, minAngle: 8, cx: '50%', cy: '50%' }
              : { cx: '50%', cy: '50%' };
          return (
            <Pie
              data={data}
              key={`pie${pieIndex}`}
              {...basePieProps}
              {...pieOverrides}
            >
              {data.map((_, dataIndex) => (
                <Cell
                  key={`cell-${dataIndex}`}
                  fill={colorGenerator(dataIndex)}
                />
              ))}

              {props.labelLists
                ?.filter(ll => {
                  // For donuts, drop inside labels; keep outside only.
                  const pos = (ll.position as unknown as string) || '';
                  return !isDonut || pos.toString().toLowerCase() !== 'inside';
                })
                .map((labelList, labelListIndex) => (
                  <LabelList
                    key={`labelList-${labelListIndex}`}
                    {...generateLabelListProps(labelList)}
                  />
                ))}

              {pieIndex === 0 && total && (
                <Label
                  position="center"
                  content={({ viewBox }) => {
                    if (viewBox && 'cx' in viewBox && 'cy' in viewBox) {
                      const vb = viewBox as Record<string, unknown>;
                      const asNumber = (v: unknown): number => {
                        if (typeof v === 'number') return v;
                        if (typeof v === 'string') {
                          const n = Number(v);
                          return Number.isFinite(n) ? n : 0;
                        }
                        return 0;
                      };
                      const cx = asNumber(vb.cx) || 0;
                      const cy = asNumber(vb.cy) || 0;
                      let innerR = asNumber(vb.innerRadius) || 0;
                      let outerR = asNumber(vb.outerRadius) || 0;

                      // Resolve radii if they were provided as percentages in props
                      const toNumberFromProp = (
                        v: unknown,
                        ref: number
                      ): number => {
                        if (typeof v === 'number') return v;
                        if (typeof v === 'string') {
                          const m = v.trim().match(/^([0-9]+(?:\.[0-9]+)?)%$/);
                          if (m) return (parseFloat(m[1]) / 100) * (ref || 1);
                          const n = Number(v);
                          if (Number.isFinite(n)) return n;
                        }
                        return 0;
                      };

                      if (!outerR && basePieProps.outerRadius) {
                        // Fallback: assume outerRadius is a % of chart size; use viewBox.radius if present
                        const vbR =
                          asNumber(
                            (viewBox as Record<string, unknown>).radius
                          ) ||
                          asNumber((viewBox as Record<string, unknown>).r) ||
                          0;
                        outerR = toNumberFromProp(
                          basePieProps.outerRadius as unknown,
                          vbR || 100
                        );
                      }
                      if (!innerR && basePieProps.innerRadius) {
                        innerR = toNumberFromProp(
                          basePieProps.innerRadius as unknown,
                          outerR
                        );
                      }

                      // If still missing, estimate innerR as 60% of outerR for donuts
                      const ring =
                        innerR > 0 ? innerR : outerR > 0 ? outerR * 0.6 : 80;

                      // Available width inside donut hole (diameter of inner circle)
                      const available = Math.max(
                        0,
                        innerR > 0 ? innerR * 2 : outerR * 1.2
                      );

                      // Estimate text width roughly as 0.6 * fontSize * charCount
                      const rawText = String(total.formattedValue ?? '');
                      const toNumber = (s: string): number | null => {
                        const clean = s.replace(/[^0-9.-]/g, '');
                        const num = Number(clean);
                        return Number.isFinite(num) ? num : null;
                      };
                      const abbreviate = (n: number): string => {
                        const abs = Math.abs(n);
                        if (abs >= 1_000_000_000)
                          return `${(n / 1_000_000_000).toFixed(2)}B`;
                        if (abs >= 1_000_000)
                          return `${(n / 1_000_000).toFixed(2)}M`;
                        if (abs >= 1_000) return `${(n / 1_000).toFixed(1)}K`;
                        return n.toLocaleString();
                      };

                      let text = rawText;

                      // Start with a generous font tied to the ring, then shrink only if it doesn't fit
                      let valueFont = Math.max(
                        16,
                        Math.min(36, Math.floor(ring * 0.5))
                      );
                      const minFont = 12;

                      const estimateWidth = (str: string, font: number) =>
                        str.length * font * 0.6;
                      while (
                        valueFont > minFont &&
                        estimateWidth(text, valueFont) > available * 0.9
                      ) {
                        valueFont -= 1;
                      }

                      // If still too wide at min font, try abbreviating numbers
                      if (estimateWidth(text, valueFont) > available * 0.95) {
                        const num = toNumber(rawText);
                        if (num !== null) {
                          const abbr = abbreviate(num);
                          text = abbr;
                          // Re-fit after abbreviation
                          while (
                            valueFont > minFont &&
                            estimateWidth(text, valueFont) > available * 0.9
                          ) {
                            valueFont -= 1;
                          }
                        }
                      }

                      const labelFont = Math.max(
                        11,
                        Math.min(16, Math.floor(valueFont * 0.55))
                      );
                      const gap = Math.max(12, Math.floor(valueFont * 0.85));

                      return (
                        <g pointerEvents="none">
                          <text
                            x={cx}
                            y={cy}
                            textAnchor="middle"
                            dominantBaseline="central"
                            alignmentBaseline="middle"
                          >
                            {/* Value exactly at center */}
                            <tspan
                              x={cx}
                              y={cy}
                              style={{
                                fontSize: `${valueFont}px`,
                                lineHeight: 1,
                              }}
                              className="fill-foreground font-bold"
                            >
                              {text}
                            </tspan>
                            {/* Label below the value */}
                            <tspan
                              x={cx}
                              y={cy + gap}
                              style={{
                                fontSize: `${labelFont}px`,
                                lineHeight: 1,
                              }}
                              className="fill-muted-foreground"
                            >
                              {total.label}
                            </tspan>
                          </text>
                        </g>
                      );
                    }
                  }}
                />
              )}
            </Pie>
          );
        })}
      </PieChart>
    </ChartContainer>
  );
};

export default PieChartWidget;
