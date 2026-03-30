import { Densities } from "@/types/density";

export enum TextInputVariant {
  Text = "Text",
  Textarea = "Textarea",
  Email = "Email",
  Tel = "Tel",
  Url = "Url",
  Password = "Password",
  Search = "Search",
}

export interface Affix {
  icon?: string;
  text?: string;
}

export interface TextInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  variant: TextInputVariant;
  disabled: boolean;
  invalid?: string;
  nullable?: boolean;
  events: string[];
  width?: string;
  height?: string;
  shortcutKey?: string;
  density?: Densities;
  prefix?: Affix;
  suffix?: Affix;
  maxLength?: number;
  minLength?: number;
  pattern?: string;
  rows?: number;
  dictation?: boolean;
  dictationUploadUrl?: string;
  dictationLanguage?: string;
  dictationTranscription?: string;
  dictationTranscriptionVersion?: number;
  "data-testid"?: string;
}
