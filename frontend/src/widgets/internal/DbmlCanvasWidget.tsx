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
import { graphlib, layout } from '@dagrejs/dagre';
import 'reactflow/dist/style.css';
import { Parser } from '@dbml/core';
import { getWidth, getHeight } from '@/lib/styles';
import { useTheme } from '@/components/theme-provider/hooks';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from '@/components/ui/tooltip';
import './DbmlCanvasWidget.css';

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
    <div className="flex items-center justify-between gap-4 py-1">
      {children}
    </div>
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
        <div
          className={`text-large-body text-muted-foreground ${labelClassName}`}
        >
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
            <DatabaseSchemaTableCell className="min-w-0 flex-1">
              <div className="flex items-center min-w-0">
                {field.pk && (
                  <span
                    className="mr-1.5 text-amber-500 shrink-0"
                    title="Primary Key"
                  >
                    🔑
                  </span>
                )}
                <TooltipProvider>
                  <Tooltip>
                    <TooltipTrigger asChild>
                      <span
                        className={`font-medium truncate ${field.pk ? 'text-primary' : ''}`}
                      >
                        {field.name}
                      </span>
                    </TooltipTrigger>
                    <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-xs">
                      {field.name}
                    </TooltipContent>
                  </Tooltip>
                </TooltipProvider>
              </div>
            </DatabaseSchemaTableCell>
            <DatabaseSchemaTableCell className="text-muted-foreground ml-auto text-right shrink-0">
              <TooltipProvider>
                <Tooltip>
                  <TooltipTrigger asChild>
                    <span className="truncate max-w-[120px] inline-block">
                      {field.type}
                      {field.nullable ? '?' : ''}
                    </span>
                  </TooltipTrigger>
                  <TooltipContent className="bg-popover text-popover-foreground shadow-md max-w-xs">
                    {`${field.type}${field.nullable ? '?' : ''}`}
                  </TooltipContent>
                </Tooltip>
              </TooltipProvider>
            </DatabaseSchemaTableCell>
            {field.isSource && (
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

const calculateNodeDimensions = (tableData: DbmlTableData) => {
  // Fixed dimensions for all tables
  const fixedWidth = 240;
  const minHeight = 120;
  const fieldHeight = 28; // Height per field row
  const headerHeight = 40; // Height for table header
  const padding = 16; // Padding inside the table

  // Calculate height based on number of fields
  const calculatedHeight = Math.max(
    headerHeight + tableData.fields.length * fieldHeight + padding,
    minHeight
  );

  return {
    width: fixedWidth,
    height: calculatedHeight,
  };
};

const getLayoutedElements = (
  nodes: Node[],
  edges: Edge[],
  direction = 'TB'
) => {
  const dagreGraph = new graphlib.Graph();
  dagreGraph.setDefaultEdgeLabel(() => ({}));

  // Create a map to store node dimensions
  const nodeDimensions = new Map<string, { width: number; height: number }>();

  // Calculate dimensions for each node based on its content
  nodes.forEach(node => {
    if (node.data && node.type === 'tableNode') {
      const dimensions = calculateNodeDimensions(node.data as DbmlTableData);
      nodeDimensions.set(node.id, dimensions);
    } else {
      // Fallback dimensions for non-table nodes
      nodeDimensions.set(node.id, { width: 250, height: 200 });
    }
  });

  // Determine optimal layout direction based on node count and relationships
  let optimalDirection = direction;
  if (nodes.length > 6) {
    // For larger schemas, use top-bottom layout for better vertical space usage
    optimalDirection = 'TB';
  } else if (nodes.length <= 3) {
    // For smaller schemas, left-right works well
    optimalDirection = 'LR';
  }

  // Calculate dynamic spacing based on average node size
  const avgWidth =
    Array.from(nodeDimensions.values()).reduce(
      (sum, dim) => sum + dim.width,
      0
    ) / nodeDimensions.size;
  const avgHeight =
    Array.from(nodeDimensions.values()).reduce(
      (sum, dim) => sum + dim.height,
      0
    ) / nodeDimensions.size;

  const dynamicNodeSep = Math.max(80, avgWidth * 0.3);
  const dynamicRankSep = Math.max(100, avgHeight * 0.5);

  // Configure the direction and dynamic node spacing
  dagreGraph.setGraph({
    rankdir: optimalDirection,
    nodesep: dynamicNodeSep,
    ranksep: dynamicRankSep,
    edgesep: Math.max(60, avgWidth * 0.2),
    marginx: 40,
    marginy: 40,
  });

  // Add nodes to dagre with their calculated dimensions
  nodes.forEach(node => {
    const dimensions = nodeDimensions.get(node.id) || {
      width: 250,
      height: 200,
    };
    dagreGraph.setNode(node.id, dimensions);
  });

  // Add edges to dagre
  edges.forEach(edge => {
    dagreGraph.setEdge(edge.source, edge.target);
  });

  // Let dagre do its magic
  layout(dagreGraph);

  // Get the positioned nodes from dagre
  const layoutedNodes = nodes.map(node => {
    const nodeWithPosition = dagreGraph.node(node.id);
    const dimensions = nodeDimensions.get(node.id) || {
      width: 250,
      height: 200,
    };

    return {
      ...node,
      position: {
        x: nodeWithPosition.x - dimensions.width / 2,
        y: nodeWithPosition.y - dimensions.height / 2,
      },
      // Store the calculated dimensions in the node for rendering
      style: {
        ...node.style,
        width: dimensions.width,
        height: dimensions.height,
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
  const { theme } = useTheme();

  const getConnectionLineColor = useCallback(() => {
    return theme === 'dark' ? 'var(--primary)' : 'var(--foreground)';
  }, [theme]);

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
                isSource: tableRelations.sources.has(field.name),
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
          style: { stroke: getConnectionLineColor() },
          markerEnd: {
            type: MarkerType.ArrowClosed,
            color: getConnectionLineColor(),
          },
          label: `${ref.endpoints[0].fieldNames[0]} → ${ref.endpoints[1].fieldNames[0]}`,
          labelStyle: { fill: getConnectionLineColor(), fontWeight: 500 },
          labelBgStyle: { fill: 'var(--background)' },
        })
      );

      // Apply automatic layout with dynamic direction selection
      const { nodes: layoutedNodes, edges: layoutedEdges } =
        getLayoutedElements(
          initialNodes,
          initialEdges,
          'TB' // Default direction - will be optimized automatically
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
  }, [dbml, setNodes, setEdges, getConnectionLineColor]);

  useEffect(() => {
    queueMicrotask(parseDbml);
  }, [dbml, parseDbml]);

  const styles = {
    ...getWidth(width),
    ...getHeight(height),
  };

  if (error) {
    return (
      <div className="m-4 p-4 bg-destructive/10 border border-destructive/20 rounded-md h-fit w-full">
        <h3 className="text-destructive font-medium mb-2">DBML Parse Error</h3>

        <div className="text-destructive/80 text-large-body mb-3">
          {error.message}
        </div>

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
    <div style={styles} className="dbml-canvas-widget">
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
          style: { stroke: getConnectionLineColor() },
          markerEnd: {
            type: MarkerType.ArrowClosed,
            color: getConnectionLineColor(),
          },
        }}
      >
        <Background color="var(--primary)" gap={16} size={1} />
        <Controls
          style={{
            backgroundColor: 'var(--background)',
            border: '1px solid var(--border)',
            borderRadius: '4px',
            boxShadow: 'var(--shadow-sm)',
          }}
          showZoom={true}
          showFitView={true}
          showInteractive={true}
        />
        <MiniMap
          nodeColor="var(--primary)"
          nodeStrokeColor="var(--border)"
          nodeStrokeWidth={1}
          nodeBorderRadius={4}
          maskColor="var(--accent)"
          maskStrokeColor="var(--border)"
          maskStrokeWidth={1}
          style={{
            backgroundColor: 'var(--background)',
            border: '1px solid var(--border)',
            borderRadius: '4px',
          }}
        />
      </ReactFlow>
    </div>
  );
};

export default DbmlCanvasWidget;
