import * as React from 'react';
import {
  ResponsiveContainer,
  Tooltip as RechartsTooltip,
  Legend as RechartsLegend,
} from 'recharts';
import type { Payload } from 'recharts/types/component/DefaultTooltipContent';

import { cn } from '@/lib/utils';

// Format: { THEME_NAME: CSS_SELECTOR }
const THEMES = { light: '', dark: '.dark' } as const;

export type ChartConfig = {
  [k in string]: {
    label?: React.ReactNode;
    icon?: React.ComponentType;
  } & (
    | { color?: string; theme?: never }
    | { color?: never; theme: Record<keyof typeof THEMES, string> }
  );
};

type ChartContextProps = {
  config: ChartConfig;
};

const ChartContext = React.createContext<ChartContextProps | null>(null);

function useChart() {
  const context = React.useContext(ChartContext);

  if (!context) {
    throw new Error('useChart must be used within a <ChartContainer />');
  }

  return context;
}

const ChartContainer = React.forwardRef<
  HTMLDivElement,
  React.ComponentProps<'div'> & {
    config: ChartConfig;
    children: React.ComponentProps<typeof ResponsiveContainer>['children'];
  }
>(({ id, className, children, config, ...props }, ref) => {
  const uniqueId = React.useId();
  const chartId = `chart-${id || uniqueId.replace(/:/g, '')}`;

  return (
    <ChartContext.Provider value={{ config }}>
      <div
        data-chart={chartId}
        ref={ref}
        className={cn(
          "flex items-center aspect-video justify-center mx-auto text-xs overflow-hidden [&_.recharts-cartesian-axis-tick_text]:fill-muted-foreground [&_.recharts-cartesian-grid_line[stroke='#ccc']]:stroke-border/50 [&_.recharts-curve.recharts-tooltip-cursor]:stroke-border [&_.recharts-dot[stroke='#fff']]:stroke-transparent [&_.recharts-layer]:outline-none [&_.recharts-polar-grid_[stroke='#ccc']]:stroke-border [&_.recharts-radial-bar-background-sector]:fill-muted [&_.recharts-rectangle.recharts-tooltip-cursor]:fill-muted [&_.recharts-reference-line_[stroke='#ccc']]:stroke-border [&_.recharts-sector[stroke='#fff']]:stroke-transparent [&_.recharts-sector]:outline-none [&_.recharts-surface]:outline-none",
          className
        )}
        style={{
          minHeight: 0,
          maxHeight: '100%',
          ...props.style,
        }}
        {...props}
      >
        <ChartStyle id={chartId} config={config} />
        <ResponsiveContainer width="100%" height="100%">
          {children}
        </ResponsiveContainer>
      </div>
    </ChartContext.Provider>
  );
});
ChartContainer.displayName = 'Chart';

const ChartStyle = ({ id, config }: { id: string; config: ChartConfig }) => {
  const colorConfig = React.useMemo(
    () => Object.entries(config).filter(([, cfg]) => cfg.theme || cfg.color),
    [config]
  );

  const css = React.useMemo(() => {
    if (!colorConfig.length) return '';
    return Object.entries(THEMES)
      .map(
        ([theme, prefix]) => `
${prefix} [data-chart=${id}] {
${colorConfig
  .map(([key, itemConfig]) => {
    const color =
      itemConfig.theme?.[theme as keyof typeof itemConfig.theme] ||
      itemConfig.color;
    return color ? `  --color-${key}: ${color};` : null;
  })
  .filter(Boolean)
  .join('\n')}
}
`
      )
      .join('\n');
  }, [colorConfig, id]);

  if (!css) return null;

  return <style dangerouslySetInnerHTML={{ __html: css }} />;
};

const ChartTooltip = RechartsTooltip;

const ChartTooltipContent = React.forwardRef<
  HTMLDivElement,
  React.ComponentProps<typeof RechartsTooltip> &
    React.ComponentProps<'div'> & {
      active?: boolean;
      payload?: Payload<number, string>[];
      label?: unknown;
      hideLabel?: boolean;
      hideIndicator?: boolean;
      indicator?: 'line' | 'dot' | 'dashed';
      nameKey?: string;
      labelKey?: string;
    }
>(
  (
    {
      active,
      payload,
      className,
      indicator = 'dot',
      hideLabel = false,
      hideIndicator = false,
      label,
      labelFormatter,
      labelClassName,
      formatter,
      color,
      nameKey,
      labelKey,
    },
    ref
  ) => {
    const { config } = useChart();

    const tooltipLabel = React.useMemo(() => {
      if (hideLabel || !payload?.length) {
        return null;
      }

      const [item] = payload;
      const key = `${labelKey || item.dataKey || item.name || 'value'}`;
      const itemConfig = getPayloadConfigFromPayload(config, item, key);
      const value =
        !labelKey && typeof label === 'string'
          ? config[label as keyof typeof config]?.label || label
          : itemConfig?.label;

      if (labelFormatter) {
        return (
          <div className={cn('font-medium', labelClassName)}>
            {labelFormatter(value, payload)}
          </div>
        );
      }

      if (!value) {
        return null;
      }

      return <div className={cn('font-medium', labelClassName)}>{value}</div>;
    }, [
      label,
      labelFormatter,
      payload,
      hideLabel,
      labelClassName,
      config,
      labelKey,
    ]);

    if (!active || !payload?.length) {
      return null;
    }

    const nestLabel = payload.length === 1 && indicator !== 'dot';

    return (
      <div
        ref={ref}
        className={cn(
          'grid min-w-[8rem] items-start gap-1.5 rounded-lg border border-border/50 bg-background px-2.5 py-1.5 text-xs shadow-xl',
          className
        )}
      >
        {!nestLabel ? tooltipLabel : null}
        <div className="grid gap-1.5">
          {payload.map((item, index) => {
            const key = `${nameKey || item.name || item.dataKey || 'value'}`;
            const itemConfig = getPayloadConfigFromPayload(config, item, key);
            const indicatorColor =
              item.color ||
              item.payload?.fill ||
              item.payload?.stroke ||
              color ||
              `var(--color-${key})`;

            return (
              <div
                key={String(item.dataKey)}
                className={cn(
                  'flex w-full flex-wrap items-stretch gap-2 [&>svg]:h-2.5 [&>svg]:w-2.5 [&>svg]:text-muted-foreground',
                  indicator === 'dot' && 'items-center'
                )}
              >
                {formatter && item?.value !== undefined && item.name ? (
                  formatter(item.value, item.name, item, index, item.payload)
                ) : (
                  <>
                    {itemConfig?.icon ? (
                      <itemConfig.icon />
                    ) : (
                      !hideIndicator && (
                        <div
                          className={cn('shrink-0 rounded-[2px] border', {
                            'h-2.5 w-2.5': indicator === 'dot',
                            'w-1': indicator === 'line',
                            'w-0 border-[1.5px] border-dashed bg-transparent':
                              indicator === 'dashed',
                            'my-0.5': nestLabel && indicator === 'dashed',
                          })}
                          style={{
                            backgroundColor:
                              indicator === 'dashed'
                                ? 'transparent'
                                : indicatorColor,
                            borderColor: indicatorColor,
                          }}
                        />
                      )
                    )}
                    <div
                      className={cn(
                        'flex flex-1 justify-between leading-none',
                        nestLabel ? 'items-end' : 'items-center'
                      )}
                    >
                      <div className="grid gap-1.5">
                        {nestLabel ? tooltipLabel : null}
                        <span className="text-muted-foreground">
                          {itemConfig?.label || item.name}
                        </span>
                      </div>
                      {item.value && (
                        <span className="font-mono font-medium tabular-nums text-foreground">
                          {item.value.toLocaleString()}
                        </span>
                      )}
                    </div>
                  </>
                )}
              </div>
            );
          })}
        </div>
      </div>
    );
  }
);
ChartTooltipContent.displayName = 'ChartTooltip';

const ChartLegend = RechartsLegend;

const ChartLegendContent = React.forwardRef<
  HTMLDivElement,
  React.ComponentProps<'div'> & {
    payload?: Payload<number, string>[];
    verticalAlign?: string;
    hideIcon?: boolean;
    nameKey?: string;
    splitThreshold?: number;
  }
>(
  (
    {
      className,
      hideIcon = false,
      payload,
      verticalAlign = 'bottom',
      nameKey,
      splitThreshold = 6,
    },
    ref
  ) => {
    const { config } = useChart();

    if (!payload?.length) {
      return null;
    }

    const items = payload;
    const many = items.length > splitThreshold;
    const half = many ? Math.ceil(items.length / 2) : items.length;

    const renderItem = (item: Payload<number, string>) => {
      const key = `${nameKey || item.dataKey || 'value'}`;
      const itemConfig = getPayloadConfigFromPayload(config, item, key);
      const labelText =
        (typeof itemConfig?.label === 'string'
          ? itemConfig.label
          : undefined) ||
        (typeof item.value === 'string' ? item.value : undefined) ||
        (item.dataKey ? String(item.dataKey) : 'value');

      const elementKey = `${String(item.dataKey)}-${String(item.value)}`;

      return (
        <div
          key={elementKey}
          className={cn(
            'flex items-center gap-1.5 py-1 px-1 flex-none [&>svg]:h-3 [&>svg]:w-3 [&>svg]:text-muted-foreground'
          )}
        >
          {itemConfig?.icon && !hideIcon ? (
            <itemConfig.icon />
          ) : (
            <div
              className="h-2 w-2 shrink-0 rounded-[2px]"
              style={{
                backgroundColor: item.color || `var(--color-${key})`,
              }}
            />
          )}
          <span className="text-muted-foreground whitespace-nowrap leading-normal">
            {labelText}
          </span>
        </div>
      );
    };

    return (
      <div
        ref={ref}
        className={cn(
          'w-full flex flex-col',
          verticalAlign === 'top' ? 'pb-3' : 'pt-3',
          className
        )}
      >
        {!many ? (
          <div className="flex w-full items-center justify-center gap-x-4 gap-y-2 flex-wrap">
            {items.map(renderItem)}
          </div>
        ) : (
          <div className="w-full overflow-x-auto">
            <div className="flex w-max flex-col gap-2">
              <div className="flex items-center justify-start gap-x-4 flex-nowrap">
                {items.slice(0, half).map(renderItem)}
              </div>
              <div className="flex items-center justify-start gap-x-4 flex-nowrap">
                {items.slice(half).map(renderItem)}
              </div>
            </div>
          </div>
        )}
      </div>
    );
  }
);
ChartLegendContent.displayName = 'ChartLegend';

// Helper to extract item config from a payload.
function getPayloadConfigFromPayload(
  config: ChartConfig,
  payload: unknown,
  key: string
) {
  if (typeof payload !== 'object' || payload === null) {
    return undefined;
  }

  const payloadPayload =
    'payload' in payload &&
    typeof payload.payload === 'object' &&
    payload.payload !== null
      ? payload.payload
      : undefined;

  let configLabelKey: string = key;

  if (
    key in payload &&
    typeof payload[key as keyof typeof payload] === 'string'
  ) {
    configLabelKey = payload[key as keyof typeof payload] as string;
  } else if (
    payloadPayload &&
    key in payloadPayload &&
    typeof payloadPayload[key as keyof typeof payloadPayload] === 'string'
  ) {
    configLabelKey = payloadPayload[
      key as keyof typeof payloadPayload
    ] as string;
  }

  return configLabelKey in config
    ? config[configLabelKey]
    : config[key as keyof typeof config];
}

export {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  ChartLegend,
  ChartLegendContent,
  ChartStyle,
};
