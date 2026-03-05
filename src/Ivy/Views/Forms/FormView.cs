using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Ivy;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Validation;
using Ivy.Widgets.Inputs;
using Ivy.Widgets.Inputs.Validated;

namespace Ivy.Views.Forms;

internal static class FormFieldViewHelpers
{
    public static IAnyState UseClonedAnyState(this IViewContext context, IAnyState state, bool renderOnChange = true)
    {
        var type = state.GetStateType();

        var methodInfo = typeof(ViewContext)
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m => m is { Name: nameof(ViewContext.UseState), IsGenericMethodDefinition: true }
                                 && m.GetParameters().Length == 2);

        var closedMethod = methodInfo!.MakeGenericMethod(type);

        object? initialValue = ((dynamic)state).Value;

        var result = closedMethod.Invoke(context, [initialValue, renderOnChange]);
        return (IAnyState)result!;
    }
}

public class FormValidateSignal : AbstractSignal<Unit, bool>;

public class FormUpdateSignal : AbstractSignal<Unit, Unit>;

public enum FormValidationStrategy
{
    OnBlur,
    OnSubmit
}

public class FormFieldView(
    IAnyState bindingState,
    Func<IAnyState, IViewContext, object> inputFactory,
    Func<bool> visible,
    ISignalSender<Unit, Unit> updateSender,
    string? label = null,
    string? description = null,
    string? help = null,
    string? placeholder = null,
    bool required = false,
    FormFieldLayoutOptions? layoutOptions = null,
    Func<object?, (bool, string)>[]? validators = null,
    FormValidationStrategy validationStrategy = FormValidationStrategy.OnBlur,
    Scale scale = Scale.Medium)
    : ViewBase, IFormFieldView
{
    public FormFieldLayoutOptions Layout { get; } = layoutOptions ?? new FormFieldLayoutOptions(Guid.NewGuid());

    private IState<Func<object?, (bool, string)>[]?>? _effectiveValidatorsRef;

    private bool Validate<T>(T value, IState<string> invalid, IAnyInput input)
    {
        if (!visible()) return true;

        var existing = _effectiveValidatorsRef?.Value;
        var (isValid, errorMessage) = Validators.RunValidation(value, input, label, validators);
        invalid?.Set(isValid ? null! : errorMessage ?? "");
        return isValid;
    }

    public override object? Build()
    {
        IAnyState inputState = Context.UseClonedAnyState(bindingState);
        var visibleState = UseState(visible);
        var updateReceiver = UseSignal<FormUpdateSignal, Unit, Unit>();

        UseEffect(() => updateReceiver.Receive(_ =>
        {
            visibleState.Set(visible());
            return default;
        }));

        var result = inputFactory(inputState, Context);

        if (result is ValidatedTextInputBuilder validatedBuilder)
        {
            var fieldView = validatedBuilder.WithField().Label(label ?? "").Description(description ?? "");
            if (required) fieldView = fieldView.Required();
            if (!string.IsNullOrEmpty(help)) fieldView = fieldView.Help(help);
            if (!string.IsNullOrEmpty(placeholder)) fieldView = fieldView.Placeholder(placeholder);
            fieldView = scale switch { Scale.Small => fieldView.Small(), Scale.Large => fieldView.Large(), _ => fieldView.Medium() };
            UseEffect(() =>
            {
                bindingState.As<object>().Set(inputState.As<object>().Value);
                updateSender.Send(new Unit());
            }, inputState);
            return visibleState.Value ? fieldView : null;
        }

        if (result is ValidatedFieldView validatedField)
        {
            validatedField = validatedField.Label(label ?? "").Description(description ?? "");
            if (required) validatedField = validatedField.Required();
            if (!string.IsNullOrEmpty(help)) validatedField = validatedField.Help(help);
            if (!string.IsNullOrEmpty(placeholder)) validatedField = validatedField.Placeholder(placeholder);
            UseEffect(() =>
            {
                bindingState.As<object>().Set(inputState.As<object>().Value);
                updateSender.Send(new Unit());
            }, inputState);
            return visibleState.Value ? validatedField : null;
        }

        var input = (IAnyInput)result;
        var invalidState = UseState((string?)null!);
        var blurOnceState = UseState(false);
        var validationReceiver = UseSignal<FormValidateSignal, Unit, bool>();

        _effectiveValidatorsRef = Context.UseRef<Func<object?, (bool, string)>[]?>(validators);
        _effectiveValidatorsRef.Set(Validators.GetEffectiveValidators(input, label, validators));

        UseEffect(() =>
        {
            return new Disposables(
                validationReceiver.Receive(_ =>
                {
                    var value = inputState.As<object>().Value;
                    return Validate(value, invalidState, input);
                })
            );
        });

        UseEffect(() =>
        {
            var value = inputState.As<object>().Value;
            if (blurOnceState.Value)
            {
                Validate(value, invalidState, input);
            }
            bindingState.As<object>().Set(value);
            updateSender.Send(new Unit());
        }, inputState, blurOnceState);

        void OnBlur(Event<IAnyInput> _)
        {
            blurOnceState.Set(true);
        }

        input = input.Invalid(invalidState.Value);
        if (validationStrategy == FormValidationStrategy.OnBlur)
        {
            input = input.HandleBlur(OnBlur);
        }

        if (!string.IsNullOrEmpty(placeholder))
        {
            input.Placeholder = placeholder;
        }

        if (scale != Scale.Medium)
        {
            WidgetBaseExtensions.SetScaleViaReflection(input, scale);
        }

        return visibleState.Value ? new Field(input, label, description, required, help, scale) : null;
    }
}

public record FormFieldLayoutOptions(Guid RowKey, int Column = 0, int Order = 0, string? Group = null);

public class FormFieldBinding<TModel>(
    Expression<Func<TModel, object>> selector,
    Func<IAnyState, IViewContext, object> factory,
    Func<bool> visible,
    ISignalSender<Unit, Unit> updateSignal,
    string? label = null,
    string? description = null,
    bool required = false,
    FormFieldLayoutOptions? layoutOptions = null,
    Func<object?, (bool, string)>[]? validators = null,
    FormValidationStrategy validationStrategy = FormValidationStrategy.OnBlur,
    Scale scale = Scale.Medium,
    string? help = null,
    string? placeholder = null
    ) : IFormFieldBinding<TModel>
{
    public (IFormFieldView, IDisposable) Bind(IState<TModel> model)
    {
        var (fieldState, disposable) = StateHelpers.MemberState(model, selector);
        var fieldView = new FormFieldView(fieldState, factory, visible, updateSignal, label, description, help, placeholder, required, layoutOptions, validators, validationStrategy, scale);
        return (fieldView, disposable);
    }
}

public interface IFormFieldView : IView
{
    public FormFieldLayoutOptions Layout { get; }
}

public interface IFormFieldBinding<TModel>
{
    (IFormFieldView fieldView, IDisposable disposable) Bind(IState<TModel> model);
}

public class FormView<TModel>(IFormFieldView[] fieldViews, Func<Event<Form>, ValueTask>? handleSubmit = null, Scale scale = Scale.Medium, Dictionary<string, bool>? groupOpenStates = null) : ViewBase
{
    public override object? Build()
    {
        object RenderRow(IFormFieldView[] fs)
        {
            if (fs.Length != 1) return Layout.Horizontal(fs.Cast<object>().ToArray());
            var field = fs.First();
            return field;
        }

        object RenderRows(IFormFieldView[] fs)
        {
            var gap = scale switch
            {
                Scale.Medium => 5,
                Scale.Small => 4,
                Scale.Large => 6,
                _ => 5
            };

            return Layout
                .Vertical(fs.OrderBy(h => h.Layout.Order)
                    .GroupBy(f => f.Layout.RowKey).Select(e => e.ToArray()).Select(RenderRow))
                .Gap(gap);
        }

        var columns = fieldViews
            .GroupBy(e => e.Layout.Column)
            .OrderBy(e => e.Key)
            .Select(e => Layout.Vertical(
                e.GroupBy(f => f.Layout.Group)
                    //.OrderBy(f => _groups.IndexOf(f.Key))
                    .Select(f =>
                        Layout.Vertical(
                            f.Key == null
                                ? RenderRows(f.Select(g => g).ToArray())
                                : new Expandable(f.Key, RenderRows(f.ToArray()))
                                    .Open(groupOpenStates?.GetValueOrDefault(f.Key, false) ?? false)
                                    .Scale(scale)
                        )).Cast<object>().ToArray()));

        var form = new Form(Layout.Horizontal(columns));
        if (handleSubmit != null)
        {
            form = form.HandleSubmit(handleSubmit);
        }
        return form;
    }
}