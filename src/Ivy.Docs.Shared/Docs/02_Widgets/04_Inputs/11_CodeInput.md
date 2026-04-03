---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - editor
  - syntax
  - programming
  - monaco
  - highlighting
  - code
---

# CodeInput

<Ingress>
Edit code with syntax highlighting, line numbers, and formatting support for multiple programming languages in a specialized input [field](01_Field.md). Use [Size](../../04_ApiReference/Ivy/Size.md) for `.Width()` and `.Height()` to control dimensions.
</Ingress>

The `CodeInput` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) provides a specialized text input field for entering and editing code with syntax highlighting.
It supports various programming languages and offers features like line numbers and code formatting.

## Supported Languages

### C

```csharp demo-tabs
UseState("using System;\n\npublic class Program\n{\n    static void Main()\n    {\n        Console.WriteLine(\"Hello, World!\");\n    }\n}")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Csharp)
```

### Javascript

```csharp demo-tabs
UseState("function greet(name) {\n  console.log(`Hello, ${name}!`);\n}\ngreet('World');")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Javascript)
```

### Python

```csharp demo-tabs
UseState("def greet(name):\n    print(f'Hello, {name}!')\n\ngreet('World')")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Python)
```

### SQL

```csharp demo-tabs
UseState("SELECT u.name, u.email, p.title\nFROM users u\nJOIN posts p ON u.id = p.user_id\nWHERE u.active = true\nORDER BY p.created_at DESC;")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Sql)
```

### HTML

```csharp demo-tabs
UseState("<html>\n<body>\n  <h1>Hello World!</h1>\n</body>\n</html>")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Html)
```

### CSS

```csharp demo-tabs
UseState("body {\n  font-family: Arial, sans-serif;\n  color: #333;\n}")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Css)
```

### Json

```csharp demo-tabs
UseState("{\n  \"name\": \"Ivy\",\n  \"version\": \"1.0.0\",\n  \"features\": [\"syntax highlighting\", \"auto-complete\"],\n  \"config\": {\n    \"theme\": \"dark\",\n    \"fontSize\": 14\n  }\n}")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Json)
```

### DBML

```csharp demo-tabs
UseState("Table users {\n  id integer [primary key]\n  username varchar\n  role varchar\n  created_at timestamp\n}")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Dbml)
```

### Typescript

```csharp demo-tabs
UseState("interface User {\n  name: string;\n  age: number;\n}\n\nconst user: User = { name: 'John', age: 30 };")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Typescript)
```

### YAML

```csharp demo-tabs
UseState("name: my-app\nversion: 1.0.0\nservices:\n  web:\n    image: nginx:latest\n    ports:\n      - \"80:80\"")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Yaml)
```

### Plain Text

```csharp demo-tabs
UseState("Here is some plain text, with no syntax highlighting whatsoever.\nUnlike the TextInput widget, this uses a monospaced font, which\nmakes some types of text easier to read. For example:\n\n  +----------------------------+\n  |                            |\n  |       ASCII Diagrams       |\n  |                            |\n  +----------------------------+")
    .ToCodeInput()
    .Width(Size.Full())
    .Height(Size.Auto())
    .Language(Languages.Text)
```

## Styling Options

### Invalid State

The `Invalid` state provides visual feedback when code contains syntax errors or validation issues. It displays an error message and typically shows a red border to indicate problems.

Mark a `CodeInput` as invalid when content has syntax errors:

```csharp demo-tabs
UseState("function greet(name) {\n    console.log('Hello, ' + name);\n    return 'Welcome ' + name;\n}")
    .ToCodeInput()
    .Language(Languages.Javascript)
    .Invalid("Missing closing parenthesis!")
```

### Disabled State

The `Disabled` state prevents editing while allowing users to view the code. It's useful for displaying read-only examples or temporarily preventing modifications.

Disable a `CodeInput` when needed:

```csharp demo-tabs
UseState("def calculate_fibonacci(n):\n    if n <= 1:\n        return n\n    return calculate_fibonacci(n-1) + calculate_fibonacci(n-2)\n\nresult = calculate_fibonacci(10)")
    .ToCodeInput()
    .Language(Languages.Python)
    .Disabled()
```

## Supported Languages

<Details>
<Summary>
Detailed language support
</Summary>
<Body>

The `CodeInput` widget supports syntax highlighting and formatting for the following languages via the `Languages` enum:

- `Csharp`
- `Javascript`
- `Typescript`
- `Python`
- `Sql`
- `Html`
- `Css`
- `Json`
- `Dbml`
- `Markdown` (standard markdown with code block support)
- `Text` (plain text with monospaced font)
- `Xml`
- `Yaml`
- `Csv`

</Body>
</Details>

## Event Handling

Code inputs support focus, blur, and manual `AutoFocus` behavior.

```csharp demo-tabs
public class CodeInputEventsDemo : ViewBase
{
    public override object? Build()
    {
        var blurCount = UseState(0);
        var focusCount = UseState(0);
        var state = UseState("// Code editor\nconsole.log('Test');");

        return Layout.Tabs(
            new Tab("OnFocus", Layout.Vertical()
                | Text.P("The OnFocus event fires when the code editor gains focus.")
                | state.ToCodeInput().Language(Languages.Javascript)
                    .OnFocus(() => focusCount.Set(focusCount.Value + 1))
                | Text.Literal($"Focus Count {focusCount.Value}")
            ),
            new Tab("OnBlur", Layout.Vertical()
                | Text.P("The OnBlur event fires when the code editor loses focus.")
                | state.ToCodeInput().Language(Languages.Javascript)
                    .OnBlur(() => blurCount.Set(blurCount.Value + 1))
                | Text.Literal($"Blur Count {blurCount.Value}")
            ),
            new Tab("AutoFocus", Layout.Vertical()
                | Text.P("The AutoFocus property automatically focuses the editor and places the cursor inside upon mounting.")
                | state.ToCodeInput().Language(Languages.Javascript)
                    .AutoFocus()
                | Text.Lead("Focused!")
            )
        ).Variant(TabsVariant.Tabs);
    }
}
```

<WidgetDocs Type="Ivy.CodeInput" ExtensionTypes="Ivy.CodeInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Inputs/CodeInput.cs"/>
