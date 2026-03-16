# Review: AutoFocus support for Button and Input widgets

## What to verify

- [ ] Open ButtonApp sample and confirm "Focused on Load" button has focus on page load
- [ ] Open a form with `TextInput(...).AutoFocus()` and confirm the input is focused on mount
- [ ] Test AutoFocus in a dialog scenario (e.g., cancel button focused by default)
- [ ] Verify that disabled elements with AutoFocus do not receive focus
- [ ] Test keyboard navigation (Tab) works correctly after AutoFocus
- [ ] Confirm only one element gets focus when multiple have AutoFocus (browser default behavior)
