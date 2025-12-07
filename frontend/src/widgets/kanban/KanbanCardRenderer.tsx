import React from 'react';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import type { Task } from '@/components/ui/shadcn-io/kanban';
import type { CardData } from './types';

interface KanbanCardRendererProps {
  task: Task;
  index: number;
  card: CardData | undefined;
  KanbanCard: React.ComponentType<{
    id: string;
    column: string;
    index: number;
    children: React.ReactNode;
  }>;
  KanbanHeader: React.ComponentType<{ children: React.ReactNode }>;
  KanbanCardContent: React.ComponentType<{ children: React.ReactNode }>;
}

export const KanbanCardRenderer: React.FC<KanbanCardRendererProps> = ({
  task,
  index,
  card,
  KanbanCard,
  KanbanHeader,
  KanbanCardContent,
}) => {
  return (
    <KanbanCard key={task.id} id={task.id} column={task.status} index={index}>
      {card ? (
        <div className="w-full">{card.content}</div>
      ) : (
        <Card className="w-full">
          <CardHeader className="flex-none pb-2">
            <KanbanHeader>
              <CardTitle className="text-sm leading-tight line-clamp-2">
                {task.title || 'Untitled Task'}
              </CardTitle>
            </KanbanHeader>
          </CardHeader>
          <CardContent className="flex-1 min-h-0 overflow-hidden pt-0">
            <KanbanCardContent>
              {task.description && (
                <p className="text-xs text-muted-foreground line-clamp-4 leading-relaxed overflow-hidden text-ellipsis break-words">
                  {task.description}
                </p>
              )}
            </KanbanCardContent>
          </CardContent>
        </Card>
      )}
    </KanbanCard>
  );
};
