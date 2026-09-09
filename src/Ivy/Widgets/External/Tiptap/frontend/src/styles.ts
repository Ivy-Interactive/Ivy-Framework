import React from "react";

export const getWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};

  const [wantedWidth, minWidth, maxWidth] = width.split(",");

  return {
    ...getWantedWidth(wantedWidth),
    ...getMinWidth(minWidth),
    ...getMaxWidth(maxWidth),
  };
};

const getWantedWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};
  const [sizeType, value] = width.split(":");
  const remValue = parseFloat(value) * 0.25;
  switch (sizeType.toLowerCase()) {
    case "units":
      return { width: `${remValue}rem`, maxWidth: `${remValue}rem` };
    case "px":
      return { width: `${value}px` };
    case "rem":
      return { width: `${value}rem` };
    case "fraction":
      return { width: `${parseFloat(value) * 100}%` };
    case "full":
      return { width: "100%" };
    case "fit":
      return { width: "fit-content" };
    case "screen":
      return { width: "100vw" };
    case "mincontent":
      return { width: "min-content" };
    case "maxcontent":
      return { width: "max-content" };
    case "auto":
      return { width: "auto" };
    case "grow":
      return { flexGrow: parseFloat(value) || 1 };
    case "shrink":
      return { flexShrink: parseFloat(value) || 1 };
    default:
      return {};
  }
};

const getMinWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};
  const [sizeType, value] = width.split(":");
  switch (sizeType.toLowerCase()) {
    case "units":
      return { minWidth: `${parseFloat(value) * 0.25}rem` };
    case "px":
      return { minWidth: `${value}px` };
    case "rem":
      return { minWidth: `${value}rem` };
    case "fraction":
      return { minWidth: `${parseFloat(value) * 100}%` };
    case "full":
      return { minWidth: "100%" };
    case "fit":
      return { minWidth: "fit-content" };
    case "screen":
      return { minWidth: "100vw" };
    case "mincontent":
      return { minWidth: "min-content" };
    case "maxcontent":
      return { minWidth: "max-content" };
    case "auto":
      return { minWidth: "auto" };
    default:
      return {};
  }
};

const getMaxWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};
  const [sizeType, value] = width.split(":");
  switch (sizeType.toLowerCase()) {
    case "units":
      return { maxWidth: `${parseFloat(value) * 0.25}rem` };
    case "px":
      return { maxWidth: `${value}px` };
    case "rem":
      return { maxWidth: `${value}rem` };
    case "fraction":
      return { maxWidth: `${parseFloat(value) * 100}%` };
    case "full":
      return { maxWidth: "100%" };
    case "fit":
      return { maxWidth: "fit-content" };
    case "screen":
      return { maxWidth: "100vw" };
    case "mincontent":
      return { maxWidth: "min-content" };
    case "maxcontent":
      return { maxWidth: "max-content" };
    case "auto":
      return { maxWidth: "auto" };
    default:
      return {};
  }
};

export const getHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [wantedHeight, minHeight, maxHeight] = height.split(",");

  return {
    ...getWantedHeight(wantedHeight),
    ...getMinHeight(minHeight),
    ...getMaxHeight(maxHeight),
  };
};

const getWantedHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [sizeType, value] = height.split(":");

  switch (sizeType.toLowerCase()) {
    case "units": {
      const units = parseFloat(value);
      return { height: `${units * 0.25}rem` };
    }
    case "px":
      return { height: `${value}px` };
    case "rem":
      return { height: `${value}rem` };
    case "fraction": {
      const fraction = parseFloat(value);
      return { height: `${fraction * 100}%` };
    }
    case "full":
      return { height: "100%", flexGrow: 1 };
    case "fit":
      return { height: "fit-content" };
    case "screen":
      return { height: "100vh" };
    case "mincontent":
      return { height: "min-content" };
    case "maxcontent":
      return { height: "max-content" };
    case "auto":
      return { height: "auto" };
    case "grow":
      return { flexGrow: parseFloat(value) || 1 };
    case "shrink":
      return { flexShrink: parseFloat(value) || 1 };
    default:
      return {};
  }
};

const getMinHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [sizeType, value] = height.split(":");
  switch (sizeType.toLowerCase()) {
    case "units":
      return { minHeight: `${parseFloat(value) * 0.25}rem` };
    case "px":
      return { minHeight: `${value}px` };
    case "rem":
      return { minHeight: `${value}rem` };
    case "fraction":
      return { minHeight: `${parseFloat(value) * 100}%` };
    case "full":
      return { minHeight: "100%" };
    case "fit":
      return { minHeight: "fit-content" };
    case "screen":
      return { minHeight: "100vh" };
    case "mincontent":
      return { minHeight: "min-content" };
    case "maxcontent":
      return { minHeight: "max-content" };
    case "auto":
      return { minHeight: "auto" };
    default:
      return {};
  }
};

const getMaxHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};

  const [sizeType, value] = height.split(":");
  switch (sizeType.toLowerCase()) {
    case "units":
      return { maxHeight: `${parseFloat(value) * 0.25}rem` };
    case "px":
      return { maxHeight: `${value}px` };
    case "rem":
      return { maxHeight: `${value}rem` };
    case "fraction":
      return { maxHeight: `${parseFloat(value) * 100}%` };
    case "full":
      return { maxHeight: "100%" };
    case "fit":
      return { maxHeight: "fit-content" };
    case "screen":
      return { maxHeight: "100vh" };
    case "mincontent":
      return { maxHeight: "min-content" };
    case "maxcontent":
      return { maxHeight: "max-content" };
    case "auto":
      return { maxHeight: "auto" };
    default:
      return {};
  }
};
