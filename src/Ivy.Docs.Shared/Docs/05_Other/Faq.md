# Faq

## How do I copy text to the clipboard in Ivy?

Use the `CopyToClipboard` extension method on `IClientProvider`:

```csharp
var client = UseService<IClientProvider>();
client.CopyToClipboard(content);
client.Toast("Copied to clipboard!");
```

## How do I create a multiline textarea TextInput in Ivy?

Use the `TextInputs.Textarea` variant or the dedicated `ToTextAreaInput` extension:

```csharp
state.ToTextAreaInput(placeholder: "Enter text...")
```

