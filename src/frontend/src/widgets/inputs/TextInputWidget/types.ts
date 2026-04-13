import React from "react";
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

export interface TextInputWidgetProps {
  id: string;
  placeholder?: string;
  value?: string;
  variant: TextInputVariant;
  disabled: boolean;
  ghost?: boolean;
  invalid?: string;
  nullable?: boolean;
  events: string[];
  width?: string;
  height?: string;
  shortcutKey?: string;
  density?: Densities;
  slots?: { Prefix?: React.ReactNode[]; Suffix?: React.ReactNode[] };
  maxLength?: number;
  minLength?: number;
  pattern?: string;
  rows?: number;
  autoFocus?: boolean;
  dictation?: boolean;
  dictationUploadUrl?: string;
  dictationTranscription?: string;
  dictationTranscriptionVersion?: number;
  "data-testid"?: string;
}
