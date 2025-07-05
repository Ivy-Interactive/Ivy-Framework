import { CSSProperties } from "react";

const ivyPrismTheme: Record<string, CSSProperties> = {
  'code[class*="language-"]': {
    fontFamily: '"IBM Plex Mono", monospace',
    fontSize: "1em",
    background: "transparent",
    color: "#061109",
  },
  'pre[class*="language-"]': {
    fontFamily: '"IBM Plex Mono", monospace',
    fontSize: "1em",
    background: "transparent",
    padding: "1em",
    margin: 0,
    color: "#061109",
    overflow: "auto",
    border: "1px solid #C0CCC8",
    borderRadius: "0.5em",
  },
  comment: { color: "#6a9955", fontStyle: "italic" },
  string: { color: "#05986A" },
  number: { color: "#061109" },
  boolean: { color: "#8030AF" },
  keyword: { color: "#8030AF" },
  function: { color: "#0000FF" },
  className: { color: "#0000FF" },
  operator: { color: "#061109" },
  punctuation: { color: "#061109" },
  tag: { color: "#061109" },
  attrName: { color: "#061109" },
  attrValue: { color: "#061109" },
  plain: { color: "#061109" },
};

export default ivyPrismTheme;
