---
prepare: |
  var client = this.UseService<IClientProvider>();
---

# CodeInput

The `CodeInput` widget provides a specialized text input field for entering and editing code with syntax highlighting. 
It supports various programming languages and offers features like line numbers and code formatting.

## Basic Usage

Here's a simple example of a `CodeInput` widget:

## Supported Languages

The `CodeInput` widget supports syntax highlighting for multiple programming languages:

```csharp demo-tabs
public class CSharpDemo : ViewBase 
{
    public override object? Build()
    {    
        var csCode = UseState("Console.WriteLine(\"Hello, World!\");");
        return Layout.Vertical()
                    | Text.H3("C#")
                    | csCode.ToCodeInput().Language(Languages.Csharp);
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
                    | jsCode.ToCodeInput().Language(Languages.Javascript);
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
                    | pyCode.ToCodeInput().Language(Languages.Python);
    }
}
```

```csharp demo-tabs
public class SqlDemo : ViewBase 
{
    public override object? Build()
    {    
        var sqlCode = UseState("SELECT name, email FROM users WHERE active = true;");
        return Layout.Vertical()
                    | Text.H3("SQL")
                    | sqlCode.ToCodeInput().Language(Languages.Sql);
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
                    | htmlCode.ToCodeInput().Language(Languages.Html);
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
                    | cssCode.ToCodeInput().Language(Languages.Css);
    }
}
```

```csharp demo-tabs
public class JsonDemo : ViewBase 
{
    public override object? Build()
    {    
        var jsonCode = UseState("{\n  \"name\": \"Ivy\",\n  \"version\": \"1.0.0\"\n}");
        return Layout.Vertical()
                    | Text.H3("JSON")
                    | jsonCode.ToCodeInput().Language(Languages.Json);
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
                    | dbmlCode.ToCodeInput().Language(Languages.Dbml);
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
                    | tsCode.ToCodeInput().Language(Languages.Typescript);
    }
}
```

## Styling Options

### Invalid State
Mark a `CodeInput` as invalid when content has syntax errors:

```csharp demo-tabs
public class InvalidCodeDemo: ViewBase
{
    public override object? Build()
    {
        var jsCode = UseState("console.log('hello world!';");
        return Layout.Vertical()
                | Text.H4("Incomplete JavaScript code") 
                | jsCode.ToCodeInput().Language(Languages.Javascript)
                .Invalid("Missing closing parenthesis!");
    }
}
```

### Disabled State
Disable a `CodeInput` when needed:

```csharp demo-tabs
public class DisabledCodeDemo : ViewBase
{
    public override object? Build()
    {
        var disabledCode = UseState("print('hello world!')");
        return Layout.Vertical()
            | Text.H4("Disabled Python code")
            | disabledCode.ToCodeInput().Language(Languages.Python).Disabled();
    }
}
```

## Event Handling

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
                    .Width(200)
                    .Height(100)
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
                | dbml.ToCodeInput().Width(90).Height(Size.Full()).Language(Languages.Dbml)
                | new DbmlCanvas(dbml.Value).Width(Size.Grow());
   }
}
``` 