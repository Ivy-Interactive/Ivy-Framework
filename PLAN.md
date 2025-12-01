# Plan: Backend-Generated LLM Markdown Files

## Overview

The frontend has copy/download functionality that extracts rendered markdown page contents (including runtime-rendered results like API sections), but this functionality doesn't exist in the backend. The goal is to compute this during the `.Regenerate` execution, creating `.llm.md` files for each `.md` file that contain all the information similar to the copy/md feature, and serve these files in Ivy.Docs for LLMs over an endpoint.

## Current State

### Frontend Copy/Download Feature (`DocumentTools.tsx`)
- Expands all details/expandable sections
- Loads all tabs
- Extracts API sections from rendered DOM:
  - Headings (h1-h6)
  - Paragraphs
  - Lists (ul, ol)
  - Tables
  - Code blocks (including terminal blocks)
  - API sections (Constructors, Properties, Events, Supported Types)
  - Details/expandable sections
- Converts rendered HTML/DOM back to markdown format
- Provides copy to clipboard and download as `.md` file

### Backend Regeneration Process
- `Regenerate.sh`/`Regenerate.ps1` scripts run `Ivy.Docs.Tools`
- `ConvertCommand` processes all `.md` files in `Docs/` directory
- `MarkdownConverter.ConvertAsync()` converts each `.md` to `.g.cs` C# file
- Generated files placed in `Generated/` directory
- MSBuild target `TransformMarkdown` runs this during build

### Runtime API Generation (`WidgetDocsView.cs`)
- Generates API sections at runtime using reflection:
  - **Constructors**: Extracted from type constructors and extension methods
  - **Supported Types**: For input widgets, shows supported state types
  - **Properties**: Properties with `[Prop]` attribute
  - **Events**: Properties with `[Event]` attribute
- These sections are rendered as tables and markdown in the UI
- Currently only available at runtime, not during regeneration

## Goals

1. **Backend Computation**: Generate `.llm.md` files during `.Regenerate` execution that include:
   - Original markdown content from `.md` files
   - Runtime-rendered API sections (Constructors, Properties, Events, Supported Types)
   - All expandable/details sections expanded
   - All tab content included
   - Code blocks and terminal blocks
   - Tables with associated content

2. **File Generation**: For each `.md` file in `Docs/`, create a corresponding `.llm.md` file in the same location (or a designated output directory)

3. **Serving**: Add an endpoint in `Ivy.Docs` to serve these `.llm.md` files for LLM consumption

## Implementation Strategy

### Phase 1: Extend MarkdownConverter

**File**: `Ivy.Docs.Tools/MarkdownConverter.cs`

1. **Add LLM Markdown Generation Method**
   - Create `GenerateLlmMarkdownAsync()` method
   - Takes the same inputs as `ConvertAsync()` (name, paths, etc.)
   - Outputs to `.llm.md` file alongside the `.g.cs` file

2. **Extract Original Markdown Content**
   - Read and preserve the original markdown content
   - Process through markdown pipeline to understand structure
   - Handle YAML front matter appropriately

3. **Generate API Sections**
   - For widgets referenced in markdown (via `WidgetDocsView` pattern):
     - Use reflection to get type information
     - Generate Constructors section (similar to `WidgetDocsView.cs` lines 22-38)
     - Generate Supported Types section (lines 40-89)
     - Generate Properties section (lines 92-101)
     - Generate Events section (lines 103-117)
   - Convert these sections to markdown format:
     - Headings (`## API`, `### Constructors`, etc.)
     - Tables in markdown format
     - Code blocks for examples

4. **Process Markdown Blocks**
   - Expand all `<details>` blocks (extract summary and body)
   - Include all tab content (process tab structures)
   - Preserve code blocks with language identifiers
   - Preserve terminal blocks with `terminal` language tag
   - Convert tables to markdown format
   - Preserve lists, headings, paragraphs

5. **Combine Content**
   - Merge original markdown with generated API sections
   - Ensure proper markdown formatting
   - Add metadata header if needed

### Phase 2: Integrate into ConvertCommand

**File**: `Ivy.Docs.Tools/ConvertCommand.cs`

1. **Add LLM Generation Step**
   - After `MarkdownConverter.ConvertAsync()` succeeds
   - Call `MarkdownConverter.GenerateLlmMarkdownAsync()`
   - Generate `.llm.md` file in same directory as `.g.cs` file
   - Or in a parallel `GeneratedLlm/` directory structure

2. **Handle File Paths**
   - Determine output path for `.llm.md` files
   - Consider: same directory as source, or separate output directory
   - Preserve directory structure

### Phase 3: Widget Type Detection

**File**: `Ivy.Docs.Tools/MarkdownConverter.cs` (or new helper)

1. **Detect Widget References**
   - Parse markdown for widget references
   - Look for patterns like `WidgetDocsView` usage
   - Extract type names from markdown or YAML front matter
   - May need to parse generated `.g.cs` files to find widget types

2. **Type Resolution**
   - Use `TypeUtils.GetTypeFromName()` (similar to `WidgetDocsView`)
   - Handle extension types
   - Resolve types from Ivy assemblies

3. **API Section Generation**
   - Reuse logic from `WidgetDocsView.cs`
   - Convert Ivy view objects to markdown:
     - Tables → markdown tables
     - Headings → markdown headings
     - Code blocks → markdown code blocks

### Phase 4: Markdown Conversion Utilities

**New File**: `Ivy.Docs.Tools/LlmMarkdownGenerator.cs`

1. **Ivy View to Markdown Converter**
   - Convert `Table` widgets to markdown tables
   - Convert `Text.H2()`, `Text.H3()` to markdown headings
   - Convert `Code()` blocks to markdown code fences
   - Handle `Layout.Vertical()` and other layout structures

2. **Table Conversion**
   - Extract table headers and rows
   - Handle complex cells (like the Supported Types table with nested layouts)
   - Convert to markdown table format with proper alignment

3. **Code Block Handling**
   - Preserve language identifiers
   - Handle terminal blocks specially
   - Preserve formatting

### Phase 5: Details and Tabs Processing

**File**: `Ivy.Docs.Tools/MarkdownConverter.cs`

1. **Details Block Expansion**
   - Parse `<details>` blocks in markdown
   - Extract `<summary>` content as heading
   - Extract `<Body>` content and process recursively
   - Expand all details blocks (don't leave any collapsed)

2. **Tab Processing**
   - Detect tab structures in markdown or generated code
   - Extract content from all tabs
   - Include all tab content in output (not just active tab)
   - Format as sections or use markdown conventions

### Phase 6: Serving Endpoint

**File**: `Ivy.Docs.Shared/DocsServer.cs` or new endpoint handler

1. **Add LLM Markdown Endpoint**
   - Route: `/api/llm/{path}` or `/llm/{path}`
   - Maps to `.llm.md` files in `Generated/` or source directory
   - Returns markdown content with appropriate content-type

2. **File Resolution**
   - Map URL path to file system path
   - Handle directory structure preservation
   - Return 404 for non-existent files

3. **Content-Type Headers**
   - Set `Content-Type: text/markdown` or `text/plain`
   - Consider CORS headers if needed for LLM access

## Technical Considerations

### Type Resolution Challenges
- Widget types may be referenced indirectly
- Need to parse generated `.g.cs` files or markdown to find widget types
- May need to maintain a mapping of markdown files to widget types

### Markdown Conversion Complexity
- Ivy view objects are complex and may contain nested structures
- Tables with complex cells (like Supported Types) need special handling
- Need to convert layout structures to appropriate markdown

### Performance
- Reflection-based API generation may be slow for many files
- Consider caching type information
- Parallel processing already exists in `ConvertCommand`

### File Organization
- Options:
  1. `.llm.md` files alongside `.md` files in `Docs/`
  2. `.llm.md` files in `Generated/` directory mirroring structure
  3. Separate `GeneratedLlm/` directory
- Recommendation: Option 2 (in `Generated/`) to keep generated files together

## Implementation Steps

1. **Create `LlmMarkdownGenerator.cs`**
   - Implement Ivy view to markdown conversion
   - Handle tables, headings, code blocks
   - Test with sample widget types

2. **Extend `MarkdownConverter.cs`**
   - Add `GenerateLlmMarkdownAsync()` method
   - Integrate API section generation
   - Process details blocks and tabs

3. **Update `ConvertCommand.cs`**
   - Call LLM markdown generation after C# conversion
   - Handle output paths

4. **Add Serving Endpoint**
   - Create endpoint handler in `Ivy.Docs`
   - Map paths to `.llm.md` files
   - Test serving files

5. **Testing**
   - Test with various markdown files
   - Verify API sections are generated correctly
   - Verify details/tabs are expanded
   - Test endpoint serves files correctly

## Success Criteria

- ✅ For each `.md` file, a corresponding `.llm.md` file is generated during regeneration
- ✅ `.llm.md` files contain original markdown content plus runtime-rendered API sections
- ✅ All expandable sections are expanded in `.llm.md` files
- ✅ All tab content is included in `.llm.md` files
- ✅ API sections (Constructors, Properties, Events, Supported Types) are correctly generated
- ✅ `.llm.md` files are served via endpoint in `Ivy.Docs`
- ✅ Files are accessible to LLMs for consumption

## Future Enhancements

- Add metadata to `.llm.md` files (last updated, source file, etc.)
- Support incremental updates (only regenerate changed files)
- Add validation to ensure `.llm.md` files are complete
- Consider adding LLM-specific optimizations (token limits, formatting, etc.)

