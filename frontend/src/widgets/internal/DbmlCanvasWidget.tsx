import React, { useCallback, useEffect, useState, ReactNode } from 'react';
import ReactFlow, {
  Node,
  Edge,
  Background,
  Controls,
  NodeTypes,
  useNodesState,
  useEdgesState,
  MiniMap,
  Position,
  MarkerType,
  Handle,
  NodeProps,
} from 'reactflow';
import dagre from 'dagre';
import 'reactflow/dist/style.css';
import { Parser } from '@dbml/core';
import { getWidth, getHeight } from '@/lib/styles';

interface DbmlCanvasWidgetProps {
  id: string;
  dbml: string;
  width?: string;
  height?: string;
}

interface DbmlError {
  message: string;
  location?: {
    start?: { line: number; col: number };
    end?: { line: number; col: number };
  };
  snippet?: string;
}

interface DatabaseSchemaNodeProps {
  children: ReactNode;
  selected?: boolean;
  className?: string;
}

interface DatabaseSchemaNodeHeaderProps {
  children: ReactNode;
}

interface DatabaseSchemaNodeBodyProps {
  children: ReactNode;
}

interface DatabaseSchemaTableRowProps {
  children: ReactNode;
}

interface DatabaseSchemaTableCellProps {
  children: ReactNode;
  className?: string;
}

interface LabeledHandleProps {
  id: string;
  title: string;
  type: string;
  position: string;
  className?: string;
  handleClassName?: string;
  labelClassName?: string;
}

interface DbmlField {
  name: string;
  type: string;
  pk: boolean;
  isSource: boolean;
  isTarget: boolean;
  nullable: boolean;
}

interface DbmlTableData {
  label: string;
  fields: DbmlField[];
}

interface DbmlTableNodeProps extends NodeProps<DbmlTableData> {
  data: DbmlTableData;
  selected: boolean;
}

interface DbmlRef {
  endpoints: Array<{
    tableName: string;
    fieldNames: string[];
  }>;
}

interface DbmlTable {
  name: string;
  fields: Array<{
    name: string;
    type: { type_name: string };
    pk: boolean;
    not_null: boolean;
  }>;
}

interface DbmlParseError {
  diags: Array<{
    message: string;
    location?: {
      start?: { line: number; col: number };
      end?: { line: number; col: number };
    };
  }>;
}

export const DatabaseSchemaNode = ({
  children,
  selected,
  className = '',
}: DatabaseSchemaNodeProps) => {
  return (
    <div
      className={`rounded-md border bg-background shadow-sm ${selected ? 'border-primary' : 'border-border'} ${className}`}
    >
      {children}
    </div>
  );
};

export const DatabaseSchemaNodeHeader = ({
  children,
}: DatabaseSchemaNodeHeaderProps) => {
  return (
    <div className="border-b border-border px-4 py-2 font-semibold">
      {children}
    </div>
  );
};

export const DatabaseSchemaNodeBody = ({
  children,
}: DatabaseSchemaNodeBodyProps) => {
  return <div className="px-4 py-2">{children}</div>;
};

export const DatabaseSchemaTableRow = ({
  children,
}: DatabaseSchemaTableRowProps) => {
  return (
    <div className="flex items-center justify-between py-1">{children}</div>
  );
};

export const DatabaseSchemaTableCell = ({
  children,
  className = '',
}: DatabaseSchemaTableCellProps) => {
  return <div className={`flex items-center ${className}`}>{children}</div>;
};

export const LabeledHandle = ({
  id,
  title,
  type,
  position,
  className = '',
  handleClassName = '',
  labelClassName = '',
}: LabeledHandleProps) => {
  return (
    <div className={`flex items-center ${className}`}>
      <div
        className={`handle ${handleClassName}`}
        data-id={id}
        data-type={type}
        data-position={position}
      >
        <div className={`text-sm text-muted-foreground ${labelClassName}`}>
          {title}
        </div>
      </div>
    </div>
  );
};

const DbmlTableNode: React.FC<DbmlTableNodeProps> = ({ data, selected }) => {
  return (
    <DatabaseSchemaNode selected={selected}>
      <DatabaseSchemaNodeHeader>{data.label}</DatabaseSchemaNodeHeader>
      <DatabaseSchemaNodeBody>
        {data.fields.map((field: DbmlField) => (
          <DatabaseSchemaTableRow key={field.name}>
            {field.isTarget && (
              <Handle
                type="target"
                position={Position.Left}
                id={`${field.name}-left`}
                style={{
                  background: 'var(--primary)',
                  width: 8,
                  height: 8,
                }}
              />
            )}
            <DatabaseSchemaTableCell>
              <div className="flex items-center">
                {field.pk && (
                  <span className="mr-1.5 text-amber-500" title="Primary Key">
                    🔑
                  </span>
                )}
                <span
                  className={`font-medium ${field.pk ? 'text-primary' : ''}`}
                >
                  {field.name}
                </span>
              </div>
            </DatabaseSchemaTableCell>
            <DatabaseSchemaTableCell className="text-muted-foreground ml-2">
              {field.type}
              {field.nullable ? '?' : ''}
            </DatabaseSchemaTableCell>
            {(field.isSource || field.pk) && (
              <Handle
                type="source"
                position={Position.Right}
                id={`${field.name}-right`}
                style={{
                  background: 'var(--primary)',
                  width: 8,
                  height: 8,
                }}
              />
            )}
          </DatabaseSchemaTableRow>
        ))}
      </DatabaseSchemaNodeBody>
    </DatabaseSchemaNode>
  );
};

const nodeTypes: NodeTypes = {
  tableNode: DbmlTableNode,
};

const getLayoutedElements = (
  nodes: Node[],
  edges: Edge[],
  direction = 'TB'
) => {
  const dagreGraph = new dagre.graphlib.Graph();
  dagreGraph.setDefaultEdgeLabel(() => ({}));

  const nodeWidth = 250;
  const nodeHeight = 200;

  // Configure the direction and node spacing
  dagreGraph.setGraph({
    rankdir: direction,
    nodesep: 80,
    ranksep: 100,
    edgesep: 80,
  });

  // Add nodes to dagre
  nodes.forEach(node => {
    dagreGraph.setNode(node.id, { width: nodeWidth, height: nodeHeight });
  });

  // Add edges to dagre
  edges.forEach(edge => {
    dagreGraph.setEdge(edge.source, edge.target);
  });

  // Let dagre do its magic
  dagre.layout(dagreGraph);

  // Get the positioned nodes from dagre
  const layoutedNodes = nodes.map(node => {
    const nodeWithPosition = dagreGraph.node(node.id);
    return {
      ...node,
      position: {
        x: nodeWithPosition.x - nodeWidth / 2,
        y: nodeWithPosition.y - nodeHeight / 2,
      },
    };
  });

  return { nodes: layoutedNodes, edges };
};

export const DbmlCanvasWidget: React.FC<DbmlCanvasWidgetProps> = ({
  dbml,
  width,
  height,
}) => {
  const [nodes, setNodes, onNodesChange] = useNodesState([]);
  const [edges, setEdges, onEdgesChange] = useEdgesState([]);
  const [error, setError] = useState<DbmlError | null>(null);

  const parseDbml = useCallback(() => {
    try {
      setError(null);

      // Handle empty or whitespace-only input
      if (!dbml || !dbml.trim()) {
        setNodes([]);
        setEdges([]);
        return;
      }

      const parsed = Parser.parse(dbml, 'dbml');

      // Validate that we have a schema and tables
      if (!parsed?.schemas?.[0]?.tables) {
        setNodes([]);
        setEdges([]);
        return;
      }

      // First, collect all relationships to mark fields that are part of relationships
      const relationships = new Map<
        string,
        { sources: Set<string>; targets: Set<string> }
      >();
      parsed.schemas[0].refs.forEach((ref: DbmlRef) => {
        const sourceTable = ref.endpoints[0].tableName;
        const targetTable = ref.endpoints[1].tableName;
        const sourceField = ref.endpoints[0].fieldNames[0];
        const targetField = ref.endpoints[1].fieldNames[0];

        if (!relationships.has(sourceTable)) {
          relationships.set(sourceTable, {
            sources: new Set(),
            targets: new Set(),
          });
        }
        if (!relationships.has(targetTable)) {
          relationships.set(targetTable, {
            sources: new Set(),
            targets: new Set(),
          });
        }

        const sourceTableRelations = relationships.get(sourceTable);
        const targetTableRelations = relationships.get(targetTable);
        if (!sourceTableRelations) {
          console.error(`Source table relations not found for: ${sourceTable}`);
        }
        if (!targetTableRelations) {
          console.error(`Target table relations not found for: ${targetTable}`);
        }
        sourceTableRelations!.sources.add(sourceField);
        targetTableRelations!.targets.add(targetField);
      });

      // Convert tables to nodes (without positions first)
      const initialNodes: Node[] = parsed.schemas[0].tables.map(
        (table: DbmlTable) => {
          const tableRelations = relationships.get(table.name) || {
            sources: new Set(),
            targets: new Set(),
          };

          return {
            id: table.name,
            type: 'tableNode',
            position: { x: 0, y: 0 }, // Initial position will be updated by dagre
            data: {
              label: table.name,
              fields: table.fields.map((field: DbmlTable['fields'][0]) => ({
                name: field.name,
                type: field.type.type_name,
                pk: field.pk || false,
                isSource: tableRelations.sources.has(field.name) || field.pk,
                isTarget: tableRelations.targets.has(field.name),
                nullable: field.not_null === false,
              })),
            },
          };
        }
      );

      // Create edges
      const initialEdges: Edge[] = parsed.schemas[0].refs.map(
        (ref: DbmlRef, index: number) => ({
          id: `edge-${index}`,
          source: ref.endpoints[0].tableName,
          target: ref.endpoints[1].tableName,
          sourceHandle: `${ref.endpoints[0].fieldNames[0]}-right`,
          targetHandle: `${ref.endpoints[1].fieldNames[0]}-left`,
          type: 'smoothstep',
          style: { stroke: 'var(--primary)' },
          markerEnd: {
            type: MarkerType.ArrowClosed,
            color: 'var(--primary)',
          },
          label: `${ref.endpoints[0].fieldNames[0]} → ${ref.endpoints[1].fieldNames[0]}`,
          labelStyle: { fill: 'var(--primary)', fontWeight: 500 },
          labelBgStyle: { fill: 'var(--background)' },
        })
      );

      // Apply automatic layout
      const { nodes: layoutedNodes, edges: layoutedEdges } =
        getLayoutedElements(
          initialNodes,
          initialEdges,
          'LR' // Left to Right layout
        );

      setNodes(layoutedNodes);
      setEdges(layoutedEdges);
    } catch (errors: unknown) {
      const error = (errors as DbmlParseError).diags[0];

      console.error('Failed to parse DBML:', error);

      const errorInfo: DbmlError = {
        message: error.message || 'Failed to parse DBML',
      };

      // Try to extract location information if available
      if (error.location) {
        errorInfo.location = error.location;
      }

      // Extract code snippet around the error
      if (error.location?.start?.line) {
        const lines = dbml.split('\n');
        const errorLine = error.location.start.line;
        const startLine = Math.max(0, errorLine - 2);
        const endLine = Math.min(lines.length, errorLine + 2);

        errorInfo.snippet = lines
          .slice(startLine, endLine)
          .map((line, idx) => {
            const lineNumber = startLine + idx + 1;
            const isErrorLine = lineNumber === errorLine;
            return `${lineNumber}${isErrorLine ? ' → ' : '   '}${line}`;
          })
          .join('\n');
      }

      setError(errorInfo);
    }
  }, [dbml, setNodes, setEdges]);

  useEffect(() => {
    parseDbml();
  }, [dbml, parseDbml]);

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  if (error) {
    return (
      <div className="m-4 p-4 bg-destructive/10 border border-destructive/20 rounded-md h-fit w-full">
        <h3 className="text-destructive font-medium mb-2">DBML Parse Error</h3>

        <div className="text-destructive/80 text-sm mb-3">{error.message}</div>

        {error.location && (
          <div className="mb-3 text-sm">
            <div className="font-medium text-destructive">Location:</div>
            <div className="text-destructive/80">
              Line {error.location.start?.line}, Column{' '}
              {error.location.start?.col}
            </div>
          </div>
        )}

        {error.snippet && (
          <div className="mb-2">
            <div className="font-medium text-destructive text-sm mb-1">
              Context:
            </div>
            <pre className="bg-destructive/10 p-2 rounded text-destructive text-sm font-mono whitespace-pre overflow-x-auto">
              {error.snippet}
            </pre>
          </div>
        )}
      </div>
    );
  }

  return (
    <div style={styles}>
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        nodeTypes={nodeTypes}
        fitView
        minZoom={0.1}
        maxZoom={1.5}
        defaultEdgeOptions={{
          type: 'smoothstep',
          style: { stroke: 'var(--primary)' },
          markerEnd: {
            type: MarkerType.ArrowClosed,
            color: 'var(--primary)',
          },
        }}
      >
        <Background color="var(--primary)" gap={16} size={1} />
        <Controls />
        <MiniMap
          nodeColor="var(--primary)"
          maskColor="rgba(var(--primary-rgb), 0.1)"
        />
      </ReactFlow>
    </div>
  );
};

export default DbmlCanvasWidget;
