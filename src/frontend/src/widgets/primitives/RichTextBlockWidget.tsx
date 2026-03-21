import React, { useState, useCallback } from 'react';
import { getColor, getOverflow, Overflow } from '@/lib/styles';
import { cn } from '@/lib/utils';
import { typography } from '@/lib/styles';
import { useStream } from '@/components/stream-handler/hooks';
import { useEventHandler } from '@/components/event-handler/hooks';
import { Densities } from '@/types/density';
import { TextAlignment } from '@/types/textAlignment';

interface TextRun {
  content: string;
  bold?: boolean;
  italic?: boolean;
  strikeThrough?: boolean;
  color?: string;
  highlightColor?: string;
  word?: boolean;
  link?: string;
  linkTarget?: 'Self' | 'Blank';
}

interface RichTextBlockWidgetProps {
  id: string;
  runs?: TextRun[];
  stream?: { id: string };
  textAlignment?: TextAlignment;
  noWrap?: boolean;
  overflow?: Overflow;
  density?: Densities;
  events?: string[];
}

const scaleClasses: Record<string, string> = {
  [Densities.Small]: typography.small,
  [Densities.Large]: typography.large,
};

const EMPTY_RUNS: TextRun[] = [];
const EMPTY_EVENTS: string[] = [];

export const RichTextBlockWidget: React.FC<RichTextBlockWidgetProps> = ({
  id,
  runs = EMPTY_RUNS,
  stream,
  textAlignment,
  noWrap,
  overflow,
  density,
  events = EMPTY_EVENTS,
}) => {
  const [streamedRuns, setStreamedRuns] = useState<TextRun[]>([]);
  const eventHandler = useEventHandler();

  const onData = useCallback((run: TextRun) => {
    setStreamedRuns(prev => [...prev, run]);
  }, []);

  useStream<TextRun>(stream?.id, onData);

  const allRuns = [...runs, ...streamedRuns];

  const styles: React.CSSProperties = {
    ...getOverflow(overflow),
    wordBreak: 'normal',
    overflowWrap: 'break-word',
    ...(textAlignment && {
      textAlign:
        textAlignment.toLowerCase() as React.CSSProperties['textAlign'],
    }),
  };

  return (
    <span
      style={styles}
      className={cn(
        noWrap && 'whitespace-nowrap',
        density && scaleClasses[density]
      )}
    >
      {allRuns.map((run, index) => {
        const runStyles: React.CSSProperties = {
          ...getColor(run.color, 'color', 'background'),
          ...getColor(run.highlightColor, 'backgroundColor', 'background'),
        };

        const className = cn(
          run.bold && 'font-semibold',
          run.italic && 'italic',
          run.strikeThrough && 'line-through'
        );

        const content = (
          <>
            {run.word && index > 0 ? ' ' : ''}
            {run.content}
          </>
        );

        if (run.link) {
          const isBlank = run.linkTarget === 'Blank';
          if (events.includes('OnLinkClick')) {
            return (
              <button
                key={run.link + (run.content || '')}
                type="button"
                className={cn(className, 'underline cursor-pointer text-left')}
                style={runStyles}
                onClick={() => {
                  eventHandler('OnLinkClick', id, [run.link]);
                }}
              >
                {content}
              </button>
            );
          }

          return (
            <a
              key={run.link + (run.content || '')}
              href={run.link}
              target={isBlank ? '_blank' : '_self'}
              rel={isBlank ? 'noopener noreferrer' : undefined}
              className={cn(className, 'underline')}
              style={runStyles}
            >
              {content}
            </a>
          );
        }

        return (
          <span
            key={run.content || 'empty-text'}
            className={className}
            style={runStyles}
          >
            {content}
          </span>
        );
      })}
    </span>
  );
};
