# st.echo

Use in a `with` block to draw some code on the app, then execute it. This makes applications self-explanatory by displaying the source code alongside its output.

## Streamlit

```python
import streamlit as st

with st.echo():
    st.write('This code will be printed')
```

## Ivy

Ivy has no direct equivalent of `st.echo` (auto-displaying the source of executed code). The closest match is the `CodeBlock` primitive which displays formatted code snippets with syntax highlighting.

```csharp
new CodeBlock(@"Console.WriteLine(""This code will be printed"");")
    .Language(Languages.Csharp)
    .ShowLineNumbers()
    .ShowCopyButton()
```

## Parameters

| Parameter       | Documentation                                                              | Ivy           |
|-----------------|----------------------------------------------------------------------------|---------------|
| code_location   | `"above"` or `"below"` — whether to show code before or after the results | Not supported |
