export type EventHandler = (event: string, id: string, args: unknown[]) => void;

export enum DrawingTool {
  Freehand = 'freehand',
  Line = 'line',
  Rectangle = 'rectangle',
  Circle = 'circle',
  Text = 'text',
}

export interface Point {
  x: number;
  y: number;
}

export interface BaseShape {
  tool: DrawingTool;
  color: string;
  lineWidth: number;
}

export interface FreehandShape extends BaseShape {
  tool: DrawingTool.Freehand;
  points: Point[];
}

export interface LineShape extends BaseShape {
  tool: DrawingTool.Line;
  start: Point;
  end: Point;
}

export interface RectangleShape extends BaseShape {
  tool: DrawingTool.Rectangle;
  start: Point;
  end: Point;
}

export interface CircleShape extends BaseShape {
  tool: DrawingTool.Circle;
  center: Point;
  radius: number;
}

export interface TextShape extends BaseShape {
  tool: DrawingTool.Text;
  position: Point;
  text: string;
  fontSize: number;
}

export type Shape = FreehandShape | LineShape | RectangleShape | CircleShape | TextShape;
