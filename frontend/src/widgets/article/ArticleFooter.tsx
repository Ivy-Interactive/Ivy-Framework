import { InternalLink } from '@/types/widgets';
import { Github } from 'lucide-react';
import React from 'react';

interface ArticleFooterProps {
  id: string;
  previous?: InternalLink;
  next?: InternalLink;
  documentSource?: string;
  onLinkClick: (event: string, id: string, args: string[]) => void;
}

export const ArticleFooter: React.FC<ArticleFooterProps> = ({
  id,
  previous,
  next,
  documentSource,
  onLinkClick,
}) => {
  return (
    <footer className="border-t py-8 mt-20">
      <div className="flex flex-col gap-6">
        <div className="flex justify-between items-center">
          <div className="flex-1">
            {previous && (
              <a
                onClick={() =>
                  onLinkClick('OnLinkClick', id, ['app://' + previous.appId])
                }
                href={'app://' + previous.appId}
                className="group flex flex-col gap-2 hover:text-primary transition-colors"
              >
                <div className="text-body">← Previous</div>
                <div className="text-body text-muted-foreground">
                  {previous.title}
                </div>
              </a>
            )}
          </div>
          <div className="flex-1 flex justify-end">
            {next && (
              <a
                onClick={() =>
                  onLinkClick('OnLinkClick', id, ['app://' + next.appId])
                }
                href={'app://' + next.appId}
                className="group flex flex-col text-right gap-2 hover:text-primary transition-colors"
              >
                <div className="text-body">Next →</div>
                <div className="text-body text-muted-foreground">
                  {next.title}
                </div>
              </a>
            )}
          </div>
        </div>
        {documentSource && (
          <div className="flex justify-center">
            <a
              href={documentSource}
              target="_blank"
              rel="noopener noreferrer"
              className="group flex items-center gap-2 text-body text-muted-foreground hover:text-primary transition-colors"
            >
              <Github className="w-4 h-4" />
              Edit this document
            </a>
          </div>
        )}
      </div>
    </footer>
  );
};
