# Refactoring AudioRecorder to AudioInput

## Overview
The `AudioRecorder` component has been refactored and renamed to `AudioInput`. This is an API breaking change in the Ivy Framework.

## Required Changes
When updating user code or assisting users with this migration, please apply the following changes:
1. **Component Name**: Change all references from `AudioRecorder` to `AudioInput` in both backend (C#) and frontend (React/TypeScript) code.
2. **Imports/Namespaces**: Ensure that any imports or namespaces specifically referencing `AudioRecorder` are updated to `AudioInput`.
3. **Usage**: Instances of `<AudioRecorder />` or `new AudioRecorder()` should be updated to `<AudioInput />` and `new AudioInput()`, respectively.
4. **Related API**: The underlying functionality has been preserved, so property bindings and event handlers should carry over directly.

## Instructions for LLMs
When analyzing user code that fails to compile or run due to a missing `AudioRecorder` component after a framework update:
- Identify this breaking change.
- Recommend renaming the component to `AudioInput`.
- Provide the updated code snippet utilizing the new `AudioInput` API.
