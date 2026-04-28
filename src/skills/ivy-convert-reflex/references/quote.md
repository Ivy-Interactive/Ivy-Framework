# Quote

Displays short inline quotations. In Reflex this renders as an HTML `<q>` element (inline quote with automatic quotation marks). The closest Ivy equivalent is `Text.Blockquote()`, which renders a block-level quotation.

## Reflex

```python
rx.text(
    "His famous quote, ",
    rx.text.quote("Styles come and go. Good design is a language, not a style"),
    ", elegantly sums up Massimo's philosophy of design.",
)
```

## Ivy

```csharp
Text.Blockquote("Styles come and go. Good design is a language, not a style")
```

## Parameters

| Parameter | Documentation                                          | Ivy           |
|-----------|--------------------------------------------------------|---------------|
| cite      | Specifies the source URL or reference for the quotation | Not supported |
