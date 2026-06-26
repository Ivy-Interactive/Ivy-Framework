# Ivy Framework Weekly Notes - Week of 2026-06-26

> [!NOTE]
> We usually release on Fridays every week. Sign up on [https://ivy.app/](https://ivy.app/auth/sign-up) to get release notes directly to your inbox.

This release introduces new features and layouts for Tables, Xterm Terminal AutoFocus, macOS style Keyboard cap rendering, and addresses several input and UI styling bugs.

---

## New Features

### Compact & Content-Sized Table Layouts
When all columns of a table opt into content sizing (`Size.Fit`, `Size.Auto`, `Size.MinContent`, or `Size.MaxContent`), the table now automatically renders at `fit-content` width rather than stretching to fill the entire screen or parent container.
* This allows columns to collapse compactly around their content.
* The table automatically falls back to full-width styling if any column is not content-sized, or if an explicit table width is set.

### Xterm Terminal Auto-Focus on Mount
Added a new `AutoFocus` property to the `Terminal` widget (default is `true`):
* Automatically focuses the terminal on mount so it receives keyboard input immediately.
* It is gated by `AutoFocus && !isReadOnly`, ensuring read-only or closed terminals are unaffected.

---

## Improvements & Refinements

### Platform-Appropriate Key Caps (`Kbd`)
We have improved keyboard shortcut rendering to be cleaner and look native on macOS:
* **macOS Symbols**: Modifier keys now render as platform-appropriate symbols on Mac (such as `⌘` for Command, `⇧` for Shift, `⌥` for Option, and `⌃` for Control).
* **Enter and Backspace Symbols**: Key caps for Enter and Backspace now render as symbols (`↵` and `⌫` respectively).
* **Refined Cap Styling**: Shrunk key caps to 16px, with 10px glyphs and less-rounded corners for a cleaner, modern look.
* **Text Shortcuts**: Renders shortcuts as text caps instead of icon indicators.

---

## Bug Fixes

* **Invalid Input Text Color**: Fixed input text color inside invalid text inputs so it doesn't get colored white (rendering it invisible) in light mode.
* **Affix Text Overlaps**: Resolved layout overlaps when inputs have prefix/suffix affixes.
* **Code Input Copy Button**: Positioned the copy button in the top-right of the code input widget, making it visible only on hover, and keeping the scrollbar aligned with the right edge. Added a Check icon for 2 seconds when Copy is clicked for visual feedback.
* **Datatable Truncation**: Prevented premature text truncation in datatable columns that grow.
* **Non-Scrollable Scroll Shade**: Fixed a bug where scroll shade indicators showed up on containers that weren't scrollable.
* **Inline Code in Lists**: Corrected CSS styling for inline code blocks inside list items.
