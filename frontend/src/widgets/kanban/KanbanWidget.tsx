import React from 'react';
import { Kanban } from '@/components/ui/shadcn-io/kanban';
import { useEventHandler } from '@/components/event-handler';
import { getWidth, getHeight } from '@/lib/styles';

interface ColumnInfo {
  id: string;
  name: string;
  columnKey: string;
  width?: string;
}

interface CardInfo {
  cardId: string;
  priority?: number;
  widgetId: string;
  content: React.ReactNode[]; // Array of React nodes from slots
}

interface KanbanWidgetProps {
  id: string;
  events?: Record<string, unknown>;
  width?: string;
  height?: string;
  allowDelete?: boolean;
  allowMove?: boolean;
  allowAdd?: boolean;
  showCounts?: boolean;
  slots?: {
    default?: React.ReactNode[];
  };
}

export const KanbanWidget: React.FC<KanbanWidgetProps> = ({
  id,
  width,
  height,
  allowDelete = false,
  allowMove = false,
  slots,
}) => {
  const eventHandler = useEventHandler();

  // Extract column and card structure from backend widgets
  const kanbanData = React.useMemo(() => {
    if (!slots?.default || slots.default.length === 0) {
      return { columns: [], cardsByColumn: new Map<string, CardInfo[]>() };
    }

    const columns: ColumnInfo[] = [];
    const cardsByColumn = new Map<string, CardInfo[]>();

    slots.default.forEach(columnNode => {
      if (React.isValidElement(columnNode)) {
        // Get column props - handle Suspense wrapper
        let columnProps: Record<string, unknown> = {};
        const props = columnNode.props as Record<string, unknown>;

        // Check if it's wrapped in Suspense
        if (props.children) {
          if (React.isValidElement(props.children)) {
            columnProps =
              (props.children.props as Record<string, unknown>) || {};
          } else if (Array.isArray(props.children)) {
            // Multiple children - take first one
            const firstChild = props.children[0];
            if (React.isValidElement(firstChild)) {
              columnProps = (firstChild.props as Record<string, unknown>) || {};
            }
          }
        } else {
          // Direct props access
          columnProps = props;
        }

        const columnKey = (columnProps?.columnKey as string) || '';
        const columnTitle = (columnProps?.title as string) || columnKey;

        if (columnKey) {
          columns.push({
            id: columnKey, // Use columnKey as id so it matches task.status
            name: columnTitle,
            columnKey: columnKey,
            width: columnProps?.width as string | undefined,
          });

          // Extract cards from column slots
          const columnSlots = (
            columnProps?.slots as { default?: React.ReactNode[] }
          )?.default;

          if (columnSlots && Array.isArray(columnSlots)) {
            const cards: CardInfo[] = [];

            columnSlots.forEach((cardNode: React.ReactNode) => {
              if (React.isValidElement(cardNode)) {
                // Get card props - handle Suspense wrapper
                let cardProps: Record<string, unknown> = {};
                const cardNodeProps = cardNode.props as Record<string, unknown>;

                if (cardNodeProps.children) {
                  if (React.isValidElement(cardNodeProps.children)) {
                    cardProps =
                      (cardNodeProps.children.props as Record<
                        string,
                        unknown
                      >) || {};
                  } else if (Array.isArray(cardNodeProps.children)) {
                    const firstChild = cardNodeProps.children[0];
                    if (React.isValidElement(firstChild)) {
                      cardProps =
                        (firstChild.props as Record<string, unknown>) || {};
                    }
                  }
                } else {
                  cardProps = cardNodeProps;
                }

                const cardId = (cardProps?.cardId as string) || '';
                const widgetId = (cardProps?.id as string) || '';

                // Get the actual card content from slots
                const cardContentSlots = (
                  cardProps?.slots as { default?: React.ReactNode[] }
                )?.default;

                if (
                  cardId &&
                  cardContentSlots &&
                  Array.isArray(cardContentSlots) &&
                  cardContentSlots.length > 0
                ) {
                  // Store the slots array directly - we'll render it in the component
                  cards.push({
                    cardId,
                    priority: cardProps?.priority as number | undefined,
                    widgetId,
                    content: cardContentSlots, // Store the array directly
                  });
                }
              }
            });

            if (cards.length > 0) {
              cardsByColumn.set(columnKey, cards);
            }
          }
        }
      }
    });

    return { columns, cardsByColumn };
  }, [slots]);

  const handleCardMove = (
    cardId: string,
    fromColumn: string,
    toColumn: string,
    targetIndex?: number
  ) => {
    eventHandler('OnMove', id, [cardId, fromColumn, toColumn, targetIndex]);
  };

  const handleCardClick = (cardId: string, widgetId: string) => {
    if (widgetId) {
      eventHandler('OnClick', widgetId, [cardId]);
    }
  };

  const handleCardDelete = (cardId: string) => {
    eventHandler('OnDelete', id, [cardId]);
  };

  if (kanbanData.columns.length === 0) {
    return (
      <div className="flex items-center justify-center p-8 text-gray-500">
        <div className="text-center">
          <p className="text-lg font-medium">No kanban data available</p>
          <p className="text-sm">
            The backend did not provide any kanban data to display.
          </p>
        </div>
      </div>
    );
  }

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
    overflowY: 'hidden' as const,
    overflowX: 'auto' as const,
  };

  // Convert to format expected by shadcn kanban component
  // Provide minimal task data for drag/drop functionality
  const tasks = Array.from(kanbanData.cardsByColumn.entries()).flatMap(
    ([columnKey, cards]) =>
      cards.map(card => ({
        id: card.cardId,
        title: '', // Not used - we render actual content
        status: columnKey, // This must match the column id passed to KanbanCards
        statusOrder:
          kanbanData.columns.findIndex(c => c.columnKey === columnKey) + 1,
        priority: card.priority || 0,
        description: '', // Not used - we render actual content
        assignee: '', // Not used - we render actual content
      }))
  );

  const columns = kanbanData.columns.map(col => ({
    id: col.columnKey, // Use columnKey as id to match task.status
    name: col.name,
    color: '',
  }));

  return (
    <div style={styles}>
      <Kanban
        data={tasks}
        columns={columns}
        onCardMove={allowMove ? handleCardMove : undefined}
        onCardClick={(cardId: string) => {
          // Find the widget ID for this card
          for (const cards of kanbanData.cardsByColumn.values()) {
            const card = cards.find(c => c.cardId === cardId);
            if (card) {
              handleCardClick(cardId, card.widgetId);
              break;
            }
          }
        }}
        onCardDelete={allowDelete ? handleCardDelete : undefined}
      >
        {({ KanbanBoard, KanbanColumn, KanbanCards, KanbanCard }) => (
          <KanbanBoard>
            {kanbanData.columns.map(column => {
              const cards =
                kanbanData.cardsByColumn.get(column.columnKey) || [];
              // Create a map for quick lookup
              const cardMap = new Map(cards.map(card => [card.cardId, card]));

              return (
                <KanbanColumn key={column.id} id={column.id} name={column.name}>
                  <KanbanCards id={column.id}>
                    {task => {
                      const card = cardMap.get(task.id);
                      if (!card) return null;

                      return (
                        <KanbanCard
                          key={card.cardId}
                          id={card.cardId}
                          column={column.id}
                        >
                          {/* Render the actual widget content from backend */}
                          <div
                            onClick={() =>
                              handleCardClick(card.cardId, card.widgetId)
                            }
                            className="cursor-pointer w-full"
                          >
                            {Array.isArray(card.content)
                              ? card.content.map((item, idx) => (
                                  <React.Fragment key={idx}>
                                    {item}
                                  </React.Fragment>
                                ))
                              : card.content}
                          </div>
                        </KanbanCard>
                      );
                    }}
                  </KanbanCards>
                </KanbanColumn>
              );
            })}
          </KanbanBoard>
        )}
      </Kanban>
    </div>
  );
};
