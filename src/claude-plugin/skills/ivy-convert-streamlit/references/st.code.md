# st.code

Display a code block with optional syntax highlighting.

## Streamlit

```python
import streamlit as st

code = '''def hello():
    print("Hello, Streamlit!")'''
st.code(code, language="python")
```

## Ivy

```csharp
new CodeBlock(@"def hello():
    print(""Hello, Streamlit!"")", Languages.Python)
    .ShowLineNumbers()
    .ShowCopyButton()
```

## Parameters

| Parameter    | Documentation                                                                                              | Ivy                            |
|--------------|------------------------------------------------------------------------------------------------------------|---------------------------------|
| body         | The string to display as code (str, required)                                                              | `content` (string)              |
| language     | Language for syntax highlighting, defaults to `"python"`. Set to `None` for plain monospace text.          | `.Language(Languages.x)`, defaults to `Languages.Csharp` |
| line_numbers | Show line numbers to the left of the code block (bool, default `False`)                                    | `.ShowLineNumbers()` (bool, default `false`) |
| wrap_lines   | Wrap lines in the code block (bool, default `False`)                                                       | Not supported                   |
