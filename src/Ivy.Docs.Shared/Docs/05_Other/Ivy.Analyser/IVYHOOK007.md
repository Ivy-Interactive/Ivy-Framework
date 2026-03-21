# IVYHOOK007: Hook Called Inline in Expression

**Severity:** Warning

## Description

Hooks must be assigned to a top-level local variable in `Build()` or called as a standalone expression statement. Do not call hooks inline within widget construction expressions, pipe chains, constructor arguments, or return statements.

Even though inline hooks may work at runtime (since hook ordering is index-based and consistent across renders), this pattern obscures state management, makes debugging harder, and silently corrupts state indices if expressions are reordered.

## Cause

```csharp
// ❌ Hooks called inline in a pipe chain — triggers IVYHOOK007
public override object? Build()
{
    return new Card(
        Layout.Vertical().Gap(3)
            | UseState(true).ToBoolInput().WithField().Label("Email notifications")
            | UseState(false).ToBoolInput().WithField().Label("SMS notifications")
            | UseState(true).ToBoolInput().WithField().Label("Marketing emails")
    );
}
```

```csharp
// ❌ Hook inline in return statement — triggers IVYHOOK007
public override object? Build()
{
    return UseState(true).ToBoolInput();
}
```

```csharp
// ❌ Hook inline as constructor argument — triggers IVYHOOK007
public override object? Build()
{
    return new Card(UseState(0).Value);
}
```

## Fix

Extract each hook call to a local variable at the top of `Build()`:

```csharp
// ✅ Hooks assigned to variables, then used in expressions
public override object? Build()
{
    var emailNotifications = UseState(true);
    var smsNotifications = UseState(false);
    var marketingEmails = UseState(true);

    return new Card(
        Layout.Vertical().Gap(3)
            | emailNotifications.ToBoolInput().WithField().Label("Email notifications")
            | smsNotifications.ToBoolInput().WithField().Label("SMS notifications")
            | marketingEmails.ToBoolInput().WithField().Label("Marketing emails")
    );
}
```

## See Also

- [Rules of Hooks](../../../03_Hooks/02_RulesOfHooks.md)
