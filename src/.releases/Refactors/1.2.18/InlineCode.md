# API Change: Text.InlineCode() to Text.Monospaced()

## Description
The `Text.InlineCode()` factory method and its associated `TextVariant.InlineCode` have been renamed to `Text.Monospaced()` and `TextVariant.Monospaced`, respectively. This renaming is part of an effort to make the UI component library more semantically descriptive, as "monospaced" more accurately describes the visual rendering of the text than "inline code".

## How to Fix
If you encounter errors about `Text.InlineCode` not being found, update your code as follows:
- Change `Text.InlineCode("hello")` to `Text.Monospaced("hello")`
- Change `TextVariant.InlineCode` to `TextVariant.Monospaced`

## Verification
Ensure the UI renders the text in a monospace font by observing the frontend application or checking the generated HTML (`<code class="typography-code">`). No changes to string literals inside text blocks are strictly required unless they directly reference the API as an example.
