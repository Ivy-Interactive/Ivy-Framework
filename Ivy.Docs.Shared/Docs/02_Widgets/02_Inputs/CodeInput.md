---
prepare: |
  var client = this.UseService<IClientProvider>();
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
Edit code with syntax highlighting, line numbers, and formatting support for multiple programming languages in a specialized input field.
</Ingress>

The `CodeInput` widget provides a specialized text input field for entering and editing code with syntax highlighting.
It supports various programming languages and offers features like line numbers and code formatting.

## Supported Languages

### C#

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

## Event Handling

Event handling enables you to respond to code changes and validate input in real-time. This allows for dynamic behavior like live validation and conditional UI updates.

Handle code changes and validation:

```csharp demo-tabs
public class CodeInputWithValidation : ViewBase 
{
    public override object? Build()
    {        
        var codeState = UseState("");
        var isValid = !string.IsNullOrWhiteSpace(codeState.Value);
        
        return Layout.Vertical()
            | Text.Label("Enter Code:")
            | codeState.ToCodeInput()
                    .Width(Size.Auto())
                    .Height(Size.Auto())
                    .Placeholder("Enter your code here...")
                    .Language(Languages.Javascript)
                                | new Button("Execute Code")
                .Disabled(!isValid)
            | Text.Small(isValid 
                ? "Ready to execute!" 
                : "Enter code to enable the button");
    }
}
```

<WidgetDocs Type="Ivy.CodeInput" ExtensionTypes="Ivy.CodeInputExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Inputs/CodeInput.cs"/>

## Examples

<Details>
<Summary>
DBML Editor with Live Preview
</Summary>
<Body>

```csharp demo-tabs
public class DBMLEditorDemo : ViewBase
{
    public override object? Build()
    {
        var sampleDbml = @"Table users {
                            id integer [primary key]
                            username varchar
                            role varchar
                            created_at timestamp
                    }";
        var dbml = this.UseState(sampleDbml);
        return Layout.Horizontal().RemoveParentPadding().Height(Size.Screen())
                | dbml.ToCodeInput()
                    .Width(Size.Units(50))
                    .Height(Size.Auto())
                    .Language(Languages.Dbml)
                | new DbmlCanvas(dbml.Value).Width(Size.Grow());
   }
}
```

</Body>
</Details>
