---
searchHints:
  - syntax
  - highlighting
  - code-block
  - snippet
  - pre
  - programming
---

# CodeBlock

<Ingress>
Display beautifully formatted code snippets with syntax highlighting, line numbers, and copy functionality for multiple programming languages.
</Ingress>

The `CodeBlock` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) displays formatted code snippets with syntax highlighting. It supports multiple programming languages and features line numbers and copy buttons for better user experience. Use [Size](../../04_ApiReference/IvyShared/Size.md) for `.Width()` and `.Height()` to control dimensions.

```csharp demo-tabs
Layout.Vertical()
    | new CodeBlock(@"function fibonacci(n) {
  if (n <= 1) return n;
  return fibonacci(n-1) + fibonacci(n-2);
}

// Print first 10 Fibonacci numbers
for (let i = 0; i < 10; i++) {
  console.log(fibonacci(i));
  }
  ")
      .ShowLineNumbers()
      .ShowCopyButton()
      .Language(Languages.Javascript)
      .Width(Size.Full())
      .Height(Size.Auto())
```

## Starting Line Number

Use `StartingLineNumber` to offset line numbering when displaying code excerpts. This is useful when showing a snippet from a larger file where you want to preserve the original line numbers.

```csharp demo-tabs
Layout.Vertical()
    | new CodeBlock(@"    private static int Calculate(int input)
    {
        return input * 2 + 1;
    }
}")
      .ShowLineNumbers()
      .StartingLineNumber(18)
      .Language(Languages.Csharp)
```

## Wrap Lines

Use `WrapLines` to enable wrapping of long lines within the code block. This improves readability for code with long lines, especially in constrained layouts. By default, long lines require horizontal scrolling.

```csharp demo-tabs
Layout.Vertical()
    | new CodeBlock(@"public class Example { public void VeryLongMethodName(string parameter1, int parameter2, bool parameter3) { Console.WriteLine(""This is a very long line that will wrap instead of requiring horizontal scrolling.""); } }")
      .WrapLines()
      .Language(Languages.Csharp)
```

<WidgetDocs Type="Ivy.CodeBlock" ExtensionTypes="Ivy.CodeBlockExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/src/Ivy/Widgets/Primitives/CodeBlock.cs"/>

