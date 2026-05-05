# Clipboard

Handles clipboard paste events containing complex data (text, images, files). Can listen globally on the document or be scoped to specific elements. Pasted data is received as a list of `(mime_type, data)` tuples, with binary data base64-encoded as data URIs.

## Reflex

```python
class ClipboardPasteState(rx.State):
    @rx.event
    def on_paste(self, data: list[tuple[str, str]]):
        for mime_type, item in data:
            yield rx.toast(f"Pasted {mime_type} data: {item}")

def clipboard_example():
    return rx.fragment(
        rx.clipboard(on_paste=ClipboardPasteState.on_paste),
        "Paste Content Here",
    )
```

## Ivy

Ivy does not have a dedicated clipboard/paste-event component. The `IClientProvider` supports copying text **to** the clipboard, but not listening for paste events:

```csharp
var client = UseService<IClientProvider>();

client.CopyToClipboard("Copied to clipboard!");
client.Toast("Text copied!");
```

## Parameters

| Parameter          | Documentation                                                                                                        | Ivy           |
|--------------------|----------------------------------------------------------------------------------------------------------------------|---------------|
| `on_paste`         | Called when the user pastes data into the document. Data is a list of tuples of (mime_type, data). Binary types will be base64 encoded as a data URI. | Not supported |
| `targets`          | Sequence of target element IDs to scope the paste event to.                                                          | Not supported |
| `.stop_propagation`| Event action to prevent the paste event from bubbling up through the DOM.                                            | Not supported |
| `.prevent_default` | Event action to block the default paste behavior (e.g. pasting text into an input).                                  | Not supported |
| `CopyToClipboard`  | N/A in Reflex clipboard component (Reflex uses `rx.set_clipboard`)                                                   | `client.CopyToClipboard(string text)` — copies text to the system clipboard |
