import React, { useState, useCallback } from 'react';
import { getColor, getOverflow, Overflow } from '@/lib/styles';
import { cn } from '@/lib/utils';
import { typography } from '@/lib/styles';
import { useStream } from '@/components/stream-handler/hooks';
import { useEventHandler } from '@/components/event-handler/hooks';
import { Scales } from '@/types/scale';
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
  stream?: string;
  textAlignment?: TextAlignment;
  noWrap?: boolean;
  overflow?: Overflow;
  scale?: Scales;
  events?: string[];
}

const scaleClasses: Record<string, string> = {
  [Scales.Small]: typography.small,
  [Scales.Large]: typography.large,
};

export const RichTextBlockWidget: React.FC<RichTextBlockWidgetProps> = ({
  id,
  runs = [],
  stream,
  textAlignment,
  noWrap,
  overflow,
  scale,
  events = [],
}) => {
  const [streamedRuns, setStreamedRuns] = useState<TextRun[]>([]);
  const eventHandler = useEventHandler();

  const onData = useCallback((run: TextRun) => {
    setStreamedRuns(prev => [...prev, run]);
  }, []);

  useStream<TextRun>(stream, onData);

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
        scale && scaleClasses[scale]
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
          return (
            <a
              key={index}
              href={run.link}
              target={isBlank ? '_blank' : '_self'}
              rel={isBlank ? 'noopener noreferrer' : undefined}
              className={cn(className, 'underline')}
              style={runStyles}
              onClick={e => {
                if (events.includes('OnLinkClick')) {
                  e.preventDefault();
                  eventHandler('OnLinkClick', id, [run.link]);
                }
              }}
            >
              {content}
            </a>
          );
        }

        return (
          <span key={index} className={className} style={runStyles}>
            {content}
          </span>
        );
      })}
    </span>
  );
};
