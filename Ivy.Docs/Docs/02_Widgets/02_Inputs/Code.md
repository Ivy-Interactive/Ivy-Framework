---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# CodeInput

The `CodeInput` widget provides a specialized text input field for entering and editing code with syntax highlighting. 
It supports various programming languages and offers features like line numbers and code formatting.

## Supported Languages

The `CodeInput` widget supports syntax highlighting for multiple programming languages:

```csharp demo-tabs
public class CSharpDemo : ViewBase 
{
    public override object? Build()
    {    
        var csCode = UseState("using System;\n\npublic class Program\n{\n    public static void Main()\n    {\n        Console.WriteLine(\"Hello, World!\");\n        var name = \"Ivy\";\n        Console.WriteLine($\"Welcome to {name}!\");\n    }\n}");
        return Layout.Vertical()
                    | Text.H3("C#")
                    | csCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Csharp);
    }
}
```

```csharp demo-tabs
public class JavaScriptDemo : ViewBase 
{
    public override object? Build()
    {    
        var jsCode = UseState("function greet(name) {\n  console.log(`Hello, ${name}!`);\n}\ngreet('World');");
        return Layout.Vertical()
                    | Text.H3("JavaScript")
                    | jsCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Javascript);
    }
}
```

```csharp demo-tabs
public class PythonDemo : ViewBase 
{
    public override object? Build()
    {    
        var pyCode = UseState("def greet(name):\n    print(f'Hello, {name}!')\n\ngreet('World')");
        return Layout.Vertical()
                    | Text.H3("Python")
                    | pyCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Python);
    }
}
```

```csharp demo-tabs
public class SqlDemo : ViewBase 
{
    public override object? Build()
    {    
        var sqlCode = UseState("SELECT u.name, u.email, p.title\nFROM users u\nJOIN posts p ON u.id = p.user_id\nWHERE u.active = true\nORDER BY p.created_at DESC;");
        return Layout.Vertical()
                    | Text.H3("SQL")
                    | sqlCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Sql);
    }
}
```

```csharp demo-tabs
public class HtmlDemo : ViewBase 
{
    public override object? Build()
    {    
        var htmlCode = UseState("<html>\n<body>\n  <h1>Hello World!</h1>\n</body>\n</html>");
        return Layout.Vertical()
                    | Text.H3("HTML")
                    | htmlCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Html);
    }
}
```

```csharp demo-tabs
public class CssDemo : ViewBase 
{
    public override object? Build()
    {    
        var cssCode = UseState("body {\n  font-family: Arial, sans-serif;\n  color: #333;\n}");
        return Layout.Vertical()
                    | Text.H3("CSS")
                    | cssCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Css);
    }
}
```

```csharp demo-tabs
public class JsonDemo : ViewBase 
{
    public override object? Build()
    {    
        var jsonCode = UseState("{\n  \"name\": \"Ivy\",\n  \"version\": \"1.0.0\",\n  \"features\": [\"syntax highlighting\", \"auto-complete\"],\n  \"config\": {\n    \"theme\": \"dark\",\n    \"fontSize\": 14\n  }\n}");
        return Layout.Vertical()
                    | Text.H3("JSON")
                    | jsonCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Json);
    }
}
```

```csharp demo-tabs
public class DbmlDemo : ViewBase 
{
    public override object? Build()
    {    
         var dbmlCode = UseState(
            "Table users {\n  id integer [primary key]\n  username varchar\n  role varchar\n  created_at timestamp\n}");
         return Layout.Vertical()
                    | Text.H3("DBML")
                    | dbmlCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Dbml);
    }
}
```

```csharp demo-tabs
public class TypeScriptDemo : ViewBase 
{
    public override object? Build()
    {    
        var tsCode = UseState("interface User {\n  name: string;\n  age: number;\n}\n\nconst user: User = { name: 'John', age: 30 };");
        return Layout.Vertical()
                    | Text.H3("TypeScript")
                    | tsCode.ToCodeInput()
                        .Width(Size.Auto())
                        .Height(Size.Auto())
                        .Language(Languages.Typescript);
    }
}
```

## Styling Options

### Invalid State
The `Invalid` state provides visual feedback when code contains syntax errors or validation issues. It displays an error message and typically shows a red border to indicate problems.

Mark a `CodeInput` as invalid when content has syntax errors:

```csharp demo-tabs
public class InvalidCodeDemo: ViewBase
{
    public override object? Build()
    {
        var jsCode = UseState("function greet(name) {\n    console.log('Hello, ' + name);\n    return 'Welcome ' + name;\n}");
        return Layout.Vertical()
                | jsCode.ToCodeInput()
                    .Width(Size.Auto())
                    .Height(Size.Auto())
                    .Language(Languages.Javascript)
                    .Invalid("Missing closing parenthesis!");
    }
}
```

### Disabled State
The `Disabled` state prevents editing while allowing users to view the code. It's useful for displaying read-only examples or temporarily preventing modifications.

Disable a `CodeInput` when needed:

```csharp demo-tabs
public class DisabledCodeDemo : ViewBase
{
    public override object? Build()
    {
        var disabledCode = UseState("def calculate_fibonacci(n):\n    if n <= 1:\n        return n\n    return calculate_fibonacci(n-1) + calculate_fibonacci(n-2)\n\nresult = calculate_fibonacci(10)");
        return Layout.Vertical()
            | disabledCode.ToCodeInput()
                .Width(Size.Auto())
                .Height(Size.Auto())
                .Language(Languages.Python)
                .Disabled();
    }
}
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

## Advanced Example

### DBML Editor with Live Preview

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
                    .Width(Size.Auto())
                    .Height(Size.Auto())
                    .Language(Languages.Dbml)
                | new DbmlCanvas(dbml.Value).Width(Size.Grow());
   }
}
``` 