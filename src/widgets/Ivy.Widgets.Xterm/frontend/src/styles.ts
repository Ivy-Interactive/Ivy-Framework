import React from "react";

/**
 * Helper functions for handling Ivy size props.
 * These convert Ivy's size format to CSS properties.
 */

export const getWidth = (width?: string): React.CSSProperties => {
  if (!width) return {};
  const [sizeType, value] = width.split(":");
  switch (sizeType.toLowerCase()) {
    case "units":
      return { width: `${parseFloat(value) * 0.25}rem` };
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
    case "auto":
      return { width: "auto" };
    default:
      return {};
  }
};

export const getHeight = (height?: string): React.CSSProperties => {
  if (!height) return {};
  const [sizeType, value] = height.split(":");
  switch (sizeType.toLowerCase()) {
    case "units":
      return { height: `${parseFloat(value) * 0.25}rem` };
    case "px":
      return { height: `${value}px` };
    case "rem":
      return { height: `${value}rem` };
    case "fraction":
      return { height: `${parseFloat(value) * 100}%` };
    case "full":
      return { height: "100%" };
    case "fit":
      return { height: "fit-content" };
    case "auto":
      return { height: "auto" };
    default:
      return {};
  }
};
