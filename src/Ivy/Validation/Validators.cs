using System.Collections.Generic;
using System.Linq;
using Ivy;
using Ivy.Widgets.Inputs;

namespace Ivy.Validation;

/// <summary>
/// Shared validation for forms. Email, tel, url, and password are validated on blur and submit via FormFieldView.
/// Validation logic lives in TextInputExtensions; Validators delegates to it.
/// </summary>
public static class Validators
{
    public static Func<object?, (bool, string)> CreateEmailValidator(string fieldName) =>
        v => { var (ok, err) = TextInputExtensions.ValidateForVariant(v, TextInputs.Email); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)> CreateTelValidator(string fieldName) =>
        v => { var (ok, err) = TextInputExtensions.ValidateForVariant(v, TextInputs.Tel); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)> CreateUrlValidator(string fieldName) =>
        v => { var (ok, err) = TextInputExtensions.ValidateForVariant(v, TextInputs.Url); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)> CreatePasswordValidator(string fieldName, int minLength = 8) =>
        v => { var (ok, err) = TextInputExtensions.ValidateForVariant(v, TextInputs.Password); return (ok, err ?? ""); };

    public static Func<object?, (bool, string)>? ForVariant(TextInputs variant, string fieldName)
    {
        return variant switch
        {
            TextInputs.Email => CreateEmailValidator(fieldName),
            TextInputs.Tel => CreateTelValidator(fieldName),
            TextInputs.Url => CreateUrlValidator(fieldName),
            TextInputs.Password => CreatePasswordValidator(fieldName),
            _ => null
        };
    }

    public static Func<object?, (bool, string)>[] GetEffectiveValidators(IAnyInput input, string? label, IEnumerable<Func<object?, (bool, string)>?>? existingValidators)
    {
        var list = (existingValidators ?? []).Where(v => v != null).Cast<Func<object?, (bool, string)>>().ToList();
        if (input is IAnyTextInput textInput)
        {
            var v = ForVariant(textInput.Variant, label ?? "");
            if (v != null)
                list.Add(v);
        }
        return list.ToArray();
    }

    public static (bool isValid, string? errorMessage) ValidateValue(object? value, TextInputs variant, string? label)
    {
        var validator = ForVariant(variant, label ?? "");
        if (validator == null)
            return (true, null);
        var (valid, message) = validator(value);
        return (valid, valid ? null : message);
    }

    public static (bool isValid, string? errorMessage) RunValidation(object? value, IAnyInput input, string? label, IEnumerable<Func<object?, (bool, string)>?>? existingValidators)
    {
        var validators = GetEffectiveValidators(input, label, existingValidators);
        foreach (var validator in validators)
        {
            var (valid, message) = validator(value);
            if (!valid)
                return (false, message);
        }
        return (true, null);
    }
}
