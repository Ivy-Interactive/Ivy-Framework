export type EventHandler = (eventName: string, widgetId: string, args: unknown[]) => void;

export interface ContentBlock {
  type: "text" | "tool_use" | "tool_result" | "thinking";
  text?: string;
  thinking?: string;
  id?: string;
  name?: string;
  input?: Record<string, unknown>;
  tool_use_id?: string;
  content?: string;
}

export interface UsageStats {
  input_tokens: number;
  output_tokens: number;
  cache_creation_input_tokens?: number;
  cache_read_input_tokens?: number;
}

export interface MessagePayload {
  id: string;
  type: string;
  role: string;
  content: ContentBlock[];
  model?: string;
  stop_reason?: string;
  usage?: UsageStats;
}

export interface SystemEvent {
  type: "system";
  subtype: string;
  session_id?: string;
  tools?: unknown[];
  model?: string;
  mcp_servers?: unknown[];
}

export interface AssistantEvent {
  type: "assistant";
  message: MessagePayload;
}

export interface UserEvent {
  type: "user";
  message: MessagePayload;
  tool_use_result?: {
    tool_use_id: string;
    content: string;
  };
}

export interface ResultEvent {
  type: "result";
  subtype: string;
  cost_usd: number;
  duration_ms: number;
  duration_api_ms: number;
  is_error: boolean;
  num_turns: number;
  result: string;
  session_id: string;
  total_cost_usd: number;
  usage: UsageStats;
}

export type ClaudeEvent = SystemEvent | AssistantEvent | UserEvent | ResultEvent;
