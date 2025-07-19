# Changelog

All notable changes to the Ivy Framework will be documented in this file.

## [Unreleased]

### Features

#### UI Improvements
- **Clickable Cursor Events (#237)**
  - Added `cursor-pointer` class to interactive elements for better UX
  - Updated UI components like NumberInput, Slider, and buttons to use proper cursor styling
  - Improved hover states with cursor feedback

#### Theming Enhancements
- **Dark Theme for Codeblocks (#234)**
  - Completely refactored the PrismJS theme to use CSS variables for dynamic theming
  - Created `createPrismTheme` factory function to support theme-aware syntax highlighting
  - Added support for dark theme in code highlighting across the framework

- **Dark Theme for Scrollbars (#233)**
  - Improved scrollbar styling to match the current theme

- **Number Input Dark Theme Support (#229)**
  - Updated color variables to use theme tokens instead of hard-coded colors
  - Changed hover states to use `bg-accent` instead of `bg-gray-100`

- **Copy Button Theme Support (#234)**
  - Refactored CopyToClipboardButton component to use theme variables
  - Improved visual feedback using primary color for copied state

- **Code Input Theme Support (#234)**
  - Added extensive CSS variable support in CodeInputWidget.css
  - Complete token-by-token styling for CodeMirror to support light and dark themes
  - Updated background, text, and syntax highlighting colors to use CSS variables

- **Better Theming and Colors (#237, #230)**
  - Replaced hard-coded color values with CSS variables throughout the codebase
  - Updated DbmlCanvasWidget to use theme variables for better dark mode support
  - Improved colors in HTML examples

#### Charts and Visualizations
- **Line Chart Examples (#224)**
  - Added example implementations for line charts

- **Pie Chart Examples (#223)**
  - Added example implementations for pie charts

- **Area Chart Improvements (#222)**
  - Enhanced area chart functionality and styling

### Bug Fixes
- **Button Improvements (#237)**
  - Removed redundant `disabled:cursor-not-allowed` style from button variants
  - The `disabled:pointer-events-none` style already prevents cursor changes

- **File Input Handling (#221)**
  - Improved max file count and file format handling on the backend

- **Sidebar Search Fix (#236)**
  - Fixed overlapping icons in the sidebar search functionality

- **Date Input Placeholder vs Title (#193)**
  - Resolved confusion between placeholder and title in date inputs

- **Tab Overflow Issue (#172)**
  - Fixed tab overflow behavior after Tailwind update

- **Code Styling for CSS, HTML, DBML (#170)**
  - Fixed code styling that was broken after dependency updates

- **Color Input Enum Names Mismatch (#169)**
  - Corrected mismatched enum names in color input components

- **Number Input Button Error (#216)**
  - Fixed issue with number input button being descendant of button

- **Code Input with Copy Button (#215)**
  - Fixed validation issue with code input when using copy button

### Documentation
- **Frontend Logger Added (#227)**
  - Added logging functionality for the frontend

- **Button Documentation Generation (#219)**
  - Generated comprehensive documentation for Button components

- **Claude Documentation Issue Template (#211)**
  - Added template for Claude documentation issues

### Chores & Maintenance
- **Updated Claude Instructions (#214)**
  - Improved guidance for Claude AI

- **Fixed Docs Build on Linux (#205)**
  - Resolved issues with documentation building process on Linux

- **CI/CD Workflow Split (#168)**
  - Separated CI/CD workflows for better maintainability

- **File Input Strict Type Handling (#173)**
  - Added strict file type handling options for file inputs

- **Boolean Input E2E Testing (#120)**
  - Added end-to-end tests for boolean input components