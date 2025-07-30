import { cn } from '@/lib/utils';
import { useEffect, useState } from 'react';

import * as React from 'react';
import { Card } from '@/components/ui/card';

export interface SidebarNewsWidgetProps {
  feedUrl: string;
}

const BASE_URL = 'https://ivy.app/news/';

const SidebarNewsWidget = ({ feedUrl }: SidebarNewsWidgetProps) => {
  const [articles, setArticles] = useState<NewsArticle[] | null>(null);

  useEffect(() => {
    const fetchArticles = async () => {
      try {
        const response = await fetch(BASE_URL + 'news.json');
        const data = await response.json();
        if (Array.isArray(data) && data.length > 0) {
          setArticles(data);
        } else {
          setArticles([]);
        }
      } catch (error) {
        console.error('Failed to fetch news:', error);
        setArticles([]);
      }
    };
    fetchArticles();
  }, [feedUrl]);

  if (!articles || articles.length === 0) return null;

  return <News articles={articles} />;
};

export default SidebarNewsWidget;

////////////////////////////////////////////////////////////////////////////////////////////////////////////

interface NewsArticle {
  id: string;
  href: string;
  title: string;
  summary: string;
  image: string;
}

const OFFSET_FACTOR = 4;
const SCALE_FACTOR = 0.03;
const OPACITY_FACTOR = 0.1;
const STORAGE_KEY = 'dismissed-news';

function News({ articles }: { articles: NewsArticle[] }) {
  const [dismissedNews, setDismissedNews] = React.useState<string[] | null>(
    null
  );
  const cleanupDoneRef = React.useRef(false);

  useEffect(() => {
    try {
      const stored = localStorage.getItem(STORAGE_KEY);
      if (stored) {
        setDismissedNews(JSON.parse(stored));
      } else {
        setDismissedNews([]);
      }
    } catch {
      setDismissedNews([]);
    }
  }, []);

  useEffect(() => {
    if (dismissedNews !== null && !cleanupDoneRef.current) {
      const validIds = new Set(articles.map(a => a.id));
      const filtered = dismissedNews.filter(id => validIds.has(id));
      if (filtered.length !== dismissedNews.length) {
        setDismissedNews(filtered);
        localStorage.setItem(STORAGE_KEY, JSON.stringify(filtered));
      }
      cleanupDoneRef.current = true;
    }
  }, [articles, dismissedNews]);

  const [hasDismissedNews, setHasDismissedNews] = React.useState(false);
  const [showCompleted, setShowCompleted] = React.useState(false);

  const cards =
    dismissedNews === null
      ? []
      : articles.filter(({ id }) => !dismissedNews.includes(id));
  const cardCount = cards.length;

  React.useEffect(() => {
    if (dismissedNews !== null && cardCount > 0) {
      setShowCompleted(true);
    }
  }, [dismissedNews, cardCount]);

  React.useEffect(() => {
    let timeout: NodeJS.Timeout | undefined = undefined;
    if (cardCount === 0)
      timeout = setTimeout(() => setShowCompleted(false), 2700);
    return () => clearTimeout(timeout);
  }, [cardCount]);

  if (dismissedNews === null) return null;
  if (cards.length === 0 && !hasDismissedNews) return null;
  if (cards.length === 0 && showCompleted) return null;

  return cards.length > 0 ? (
    <div className="group overflow-hidden px-2" data-active={cardCount !== 0}>
      <div className="relative size-full">
        {[...cards]
          .reverse()
          .map(({ id, href, title, summary, image }, idx) => (
            <div
              key={id}
              className={cn(
                'absolute left-0 top-0 size-full scale-[var(--scale)] transition-[opacity,transform] duration-200',
                cardCount - idx > 3
                  ? [
                      'opacity-0 sm:group-hover:translate-y-[var(--y)] sm:group-hover:opacity-[var(--opacity)]',
                      'sm:group-has-[*[data-dragging=true]]:translate-y-[var(--y)] sm:group-has-[*[data-dragging=true]]:opacity-[var(--opacity)]',
                    ]
                  : 'translate-y-[var(--y)] opacity-[var(--opacity)]'
              )}
              style={
                {
                  '--y': `-${(cardCount - (idx + 1)) * OFFSET_FACTOR}%`,
                  '--scale': 1 - (cardCount - (idx + 1)) * SCALE_FACTOR,
                  '--opacity':
                    cardCount - (idx + 1) >= 6
                      ? 0
                      : 1 - (cardCount - (idx + 1)) * OPACITY_FACTOR,
                } as React.CSSProperties
              }
              aria-hidden={idx !== cardCount - 1}
            >
              <NewsCard
                title={title}
                description={summary}
                image={image}
                href={href}
                hideContent={cardCount - idx > 2}
                active={idx === cardCount - 1}
                onDismiss={() => {
                  if (dismissedNews) {
                    const updated = [
                      id,
                      ...dismissedNews.filter(d => d !== id),
                    ].slice(0, 50);
                    setDismissedNews(updated);
                    setHasDismissedNews(true);
                    localStorage.setItem(STORAGE_KEY, JSON.stringify(updated));
                  }
                }}
              />
            </div>
          ))}
        <div className="pointer-events-none invisible" aria-hidden>
          <NewsCard title="Title" description="Description" />
        </div>
        {showCompleted && !cardCount && (
          <div
            className="animate-slide-up-fade absolute inset-0 flex size-full flex-col items-center justify-center gap-3 [animation-duration:1s]"
            style={{ '--offset': '10px' } as React.CSSProperties}
          >
            <div className="animate-fade-in absolute inset-0 rounded-lg border border-neutral-300 [animation-delay:2.3s] [animation-direction:reverse] [animation-duration:0.2s]" />
            <span className="animate-fade-in text-small-label font-medium text-muted-foreground [animation-delay:2.3s] [animation-direction:reverse] [animation-duration:0.2s]">
              You're all caught up!
            </span>
          </div>
        )}
      </div>
    </div>
  ) : null;
}

function NewsCard({
  title,
  description,
  image,
  onDismiss,
  hideContent,
  href,
  active,
}: {
  title: string;
  description: string;
  image?: string;
  onDismiss?: () => void;
  hideContent?: boolean;
  href?: string;
  active?: boolean;
}) {
  //const { isMobile } = useMediaQuery();
  const isMobile = false;

  const ref = React.useRef<HTMLDivElement>(null);
  const drag = React.useRef<{
    start: number;
    delta: number;
    startTime: number;
    maxDelta: number;
  }>({
    start: 0,
    delta: 0,
    startTime: 0,
    maxDelta: 0,
  });
  const animation = React.useRef<Animation | undefined>(undefined);
  const [dragging, setDragging] = React.useState(false);

  const onDragMove = (e: PointerEvent) => {
    if (!ref.current) return;
    const { clientX } = e;
    const dx = clientX - drag.current.start;
    drag.current.delta = dx;
    drag.current.maxDelta = Math.max(drag.current.maxDelta, Math.abs(dx));
    ref.current.style.setProperty('--dx', dx.toString());
  };

  const dismiss = () => {
    if (!ref.current) return;

    const cardWidth = ref.current.getBoundingClientRect().width;
    const translateX = Math.sign(drag.current.delta) * cardWidth;

    // Dismiss card
    animation.current = ref.current.animate(
      { opacity: 0, transform: `translateX(${translateX}px)` },
      { duration: 150, easing: 'ease-in-out', fill: 'forwards' }
    );
    animation.current.onfinish = () => onDismiss?.();
  };

  const stopDragging = (cancelled: boolean) => {
    if (!ref.current) return;
    unbindListeners();
    setDragging(false);

    const dx = drag.current.delta;
    if (Math.abs(dx) > ref.current.clientWidth / (cancelled ? 2 : 3)) {
      dismiss();
      return;
    }

    // Animate back to original position
    animation.current = ref.current.animate(
      { transform: 'translateX(0)' },
      { duration: 150, easing: 'ease-in-out' }
    );
    animation.current.onfinish = () =>
      ref.current?.style.setProperty('--dx', '0');

    drag.current = { start: 0, delta: 0, startTime: 0, maxDelta: 0 };
  };

  const onDragEnd = () => stopDragging(false);
  const onDragCancel = () => stopDragging(true);

  const onPointerDown = (e: React.PointerEvent) => {
    if (!active || !ref.current || animation.current?.playState === 'running')
      return;

    bindListeners();
    setDragging(true);
    drag.current.start = e.clientX;
    drag.current.startTime = Date.now();
    drag.current.delta = 0;
    ref.current.style.setProperty('--w', ref.current.clientWidth.toString());
  };

  const onClick = () => {
    if (!ref.current) return;
    if (
      isMobile &&
      drag.current.maxDelta < ref.current.clientWidth / 10 &&
      (!drag.current.startTime || Date.now() - drag.current.startTime < 250)
    ) {
      // Touch user didn't drag far or for long, open the link
      window.open(href, '_blank');
    }
  };

  const bindListeners = () => {
    document.addEventListener('pointermove', onDragMove);
    document.addEventListener('pointerup', onDragEnd);
    document.addEventListener('pointercancel', onDragCancel);
  };

  const unbindListeners = () => {
    document.removeEventListener('pointermove', onDragMove);
    document.removeEventListener('pointerup', onDragEnd);
    document.removeEventListener('pointercancel', onDragCancel);
  };

  return (
    <Card
      ref={ref}
      className={cn(
        'bg-gradient-to-br from-background to-muted',
        'relative select-none gap-2 p-2 text-[0.8125rem]',
        'translate-x-[calc(var(--dx)*1px)] rotate-[calc(var(--dx)*0.05deg)] opacity-[calc(1-max(var(--dx),-1*var(--dx))/var(--w)/2)]',
        'transition-shadow data-[dragging=true]:shadow-md'
      )}
      data-dragging={dragging}
      onPointerDown={onPointerDown}
      onClick={onClick}
    >
      <div className={cn(hideContent && 'invisible')}>
        <div className="flex flex-col gap-1">
          <span className="line-clamp-1 font-medium text-foreground">
            {title}
          </span>
          <p className="line-clamp-2 h-8 leading-4 text-muted-foreground">
            {description}
          </p>
        </div>
        <div className="relative mt-2 aspect-[16/10] w-full shrink-0 overflow-hidden rounded border bg-muted">
          {image && (
            <a href={href} target="_blank" rel="noopener noreferrer">
              <img
                src={BASE_URL + image}
                alt=""
                className="h-full w-full rounded object-cover object-center"
                draggable={false}
              />
            </a>
          )}
        </div>
        <div
          className={cn(
            'h-0 overflow-hidden opacity-0 transition-[height,opacity] duration-200',
            'sm:group-has-[*[data-dragging=true]]:h-7 sm:group-has-[*[data-dragging=true]]:opacity-100 sm:group-hover:group-data-[active=true]:h-7 sm:group-hover:group-data-[active=true]:opacity-100'
          )}
        >
          <div className="flex items-center justify-between pt-3 text-small-label">
            <a
              href={href}
              target="_blank"
              className="font-medium text-muted-foreground hover:text-foreground transition-colors duration-75"
              rel="noopener noreferrer"
            >
              Read more
            </a>
            <button
              type="button"
              onClick={dismiss}
              className="text-muted-foreground hover:text-foreground transition-colors duration-75"
            >
              Dismiss
            </button>
          </div>
        </div>
      </div>
    </Card>
  );
}
