export type EventHandler = (event: string, id: string, args: unknown[]) => void;

export enum DrawingTool {
  Callout = 'callout',
  Freehand = 'freehand',
  Arrow = 'arrow',
  Line = 'line',
  Rectangle = 'rectangle',
  Circle = 'circle',
  Censor = 'censor',
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

export interface ArrowShape extends BaseShape {
  tool: DrawingTool.Arrow;
  start: Point;
  end: Point;
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

export interface CalloutShape extends BaseShape {
  tool: DrawingTool.Callout;
  anchor: Point;
  label: Point;
  number: number;
  text: string;
  fontSize: number;
}

export interface CensorShape extends BaseShape {
  tool: DrawingTool.Censor;
  start: Point;
  end: Point;
}

export type Shape = CalloutShape | FreehandShape | ArrowShape | LineShape | RectangleShape | CircleShape | CensorShape | TextShape;

export interface AnnotationData {
  shapes: Shape[];
  screenshotWidth: number;
  screenshotHeight: number;
}
