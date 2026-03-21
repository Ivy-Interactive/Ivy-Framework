---
searchHints:
  - clipboard
  - copy
  - copy-to-clipboard
  - useclipboard
  - paste
  - text-copy
---

# UseClipboard

<Ingress>
Copy text to the user's clipboard with a single hook call.
</Ingress>

## Signature

```csharp
Action<string> UseClipboard()
```

Returns an `Action<string>` that copies the given text to the user's clipboard when invoked.

## Usage

```csharp demo-below
public class UseClipboardDemo : ViewBase
{
    public override object? Build()
    {
        var copyToClipboard = UseClipboard();

        return new Button("Copy greeting", _ =>
        {
            copyToClipboard("Hello, World!");
        });
    }
}
```

## Extracting a Shareable Link

A common pattern is copying a generated URL or code snippet:

```csharp
var copyToClipboard = UseClipboard();
var client = UseService<IClientProvider>();

var shareUrl = $"https://example.com/item/{itemId}";
copyToClipboard(shareUrl);
client.Toast("Link copied!");
```

## Notes

- **Write-only** — this hook copies text to the clipboard. There is no read-from-clipboard support.
- Under the hood this calls `IClientProvider.CopyToClipboard()`, which sends a message to the frontend client.
