# st.exception

Display an exception in the app. Shows the exception type, message, and traceback information.

## Streamlit

```python
import streamlit as st

e = RuntimeError("This is an exception of type RuntimeError")
st.exception(e)
```

## Ivy

```csharp
new Error()
    .Title("RuntimeError")
    .Message("This is an exception of type RuntimeError")
    .StackTrace("at MyApp.Program.Main() in /src/Program.cs:line 12");
```

## Parameters

| Parameter  | Documentation                     | Ivy                          |
|------------|-----------------------------------|------------------------------|
| exception  | The exception object to display   | Title / Message / StackTrace |
