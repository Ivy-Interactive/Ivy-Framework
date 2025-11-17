import React from 'react';
import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import type { Task } from '@/components/ui/shadcn-io/kanban';
import type { CardData } from './types';

interface KanbanCardRendererProps {
  task: Task;
  card: CardData | undefined;
  onCardClick: (cardId: string) => void;
  KanbanCard: React.ComponentType<{
    id: string;
    column: string;
    children: React.ReactNode;
  }>;
  KanbanHeader: React.ComponentType<{ children: React.ReactNode }>;
  KanbanCardContent: React.ComponentType<{ children: React.ReactNode }>;
}

export const KanbanCardRenderer: React.FC<KanbanCardRendererProps> = ({
  task,
  card,
  onCardClick,
  KanbanCard,
  KanbanHeader,
  KanbanCardContent,
}) => {
  return (
    <KanbanCard key={task.id} id={task.id} column={task.status}>
      {card ? (
        card.content
      ) : (
        <Card>
          <CardHeader>
            <KanbanHeader>
              <div className="flex items-center justify-between gap-2">
                <div className="flex items-center gap-2">
                  <CardTitle
                    className="text-sm cursor-pointer hover:underline hover:text-primary transition-colors"
                    onClick={(e: React.MouseEvent) => {
                      e.stopPropagation();
                      onCardClick(task.id);
                    }}
                  >
                    {task.title}
                  </CardTitle>
                </div>
                {task.priority && (
                  <Badge variant="secondary">P{task.priority}</Badge>
                )}
              </div>
            </KanbanHeader>
          </CardHeader>
          <CardContent>
            <KanbanCardContent>
              {task.description && (
                <p className="text-xs text-muted-foreground whitespace-pre-line">
                  {task.description}
                </p>
              )}
              {task.assignee && (
                <p className="text-xs text-muted-foreground">
                  Assignee: {task.assignee}
                </p>
              )}
            </KanbanCardContent>
          </CardContent>
        </Card>
      )}
    </KanbanCard>
  );
};
