# Toast API Improvements

We have significantly improved the Toast API to reduce misuse and support variants cleanly.

- Previously, the only way to send a destructive toast was to use `client.Error(Exception)`, which was unintuitive when you just wanted to display an error message.
- The `client.Toast(...)` extension methods now include an optional `ToastVariant` parameter.
- The supported variants are `Default`, `Destructive`, `Success`, `Warning`, and `Info`.

**Example:**

```csharp
// Previously you couldn't do this:
client.Toast("Invalid username or password", "Login Failed", ToastVariant.Destructive);
```

**Breaking Changes:**
If you previously injected complex custom structures that relied on the very specific internal shape of `ToasterMessage`, they might need an update to set the new `Variant` property properly, though binary compatibility expects `Default` by default.
