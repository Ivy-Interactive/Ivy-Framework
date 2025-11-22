using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

namespace Ivy.Views.Forms;

/// <summary>Internal helpers for form field state management.</summary>
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
    /// <summary>Validate when field loses focus.</summary>
    OnBlur,
    /// <summary>Validate when form is submitted.</summary>
    OnSubmit
}

public class FormFieldView(
    IAnyState bindingState,
    Func<IAnyState, IViewContext, IAnyInput> inputFactory,
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
    Sizes size = Sizes.Medium)
    : ViewBase, IFormFieldView
{
    public FormFieldLayoutOptions Layout { get; } = layoutOptions ?? new FormFieldLayoutOptions(Guid.NewGuid());

    private bool Validate<T>(T value, IState<string> invalid)
    {
        if (!visible()) return true;

        if (validators != null)
        {

            var isValid = true;
            var message = string.Empty;
            foreach (var validator in validators)
            {
                (isValid, message) = validator(value);
                if (!isValid)
                {
                    break;
                }
            }
            invalid?.Set(isValid ? null! : message);
            return isValid;
        }
        return true;
    }

    public override object? Build()
    {
        IAnyState inputState = Context.UseClonedAnyState(bindingState);
        var invalidState = UseState((string?)null!);
        var blurOnceState = UseState(false);
        var validationReceiver = Context.UseSignal<FormValidateSignal, Unit, bool>();
        var updateReceiver = Context.UseSignal<FormUpdateSignal, Unit, Unit>();
        var visibleState = Context.UseState(visible);

        UseEffect(() =>
        {
            return new Disposables(
                updateReceiver.Receive(_ =>
                {
                    visibleState.Set(visible());
                    return default;
                }),
                validationReceiver.Receive(_ =>
                {
                    var value = inputState.As<object>().Value;
                    return Validate(value, invalidState);
                })
            );
        });

        UseEffect(() =>
        {
            var value = inputState.As<object>().Value;
            if (blurOnceState.Value)
            {
                Validate(value, invalidState);
            }
            bindingState.As<object>().Set(value);
            updateSender.Send(new Unit());
        }, [inputState, blurOnceState]);

        void OnBlur(Event<IAnyInput> _)
        {
            blurOnceState.Set(true);
        }

        var input = inputFactory(inputState, Context).Invalid(invalidState.Value);
        if (validationStrategy == FormValidationStrategy.OnBlur)
        {
            input.HandleBlur(OnBlur);
        }

        // Set placeholder if the input supports it
        if (!string.IsNullOrEmpty(placeholder))
        {
            input.Placeholder = placeholder;
        }

        return visibleState.Value ? new Field(input, label, description, required, help) { Size = size } : null;
    }
}

/// <summary>Layout configuration for form field positioning and grouping.</summary>
/// <param name="RowKey">Unique identifier for grouping fields in the same row.</param>
/// <param name="Column">Column index for multi-column layouts.</param>
/// <param name="Order">Sort order within the column/group.</param>
/// <param name="Group">Optional group name for sectioning fields.</param>
public record FormFieldLayoutOptions(Guid RowKey, int Column = 0, int Order = 0, string? Group = null);

/// <summary>Binds a form field to a model property with validation and layout configuration.</summary>
public class FormFieldBinding<TModel>(
    Expression<Func<TModel, object>> selector,
    Func<IAnyState, IViewContext, IAnyInput> factory,
    Func<bool> visible,
    ISignalSender<Unit, Unit> updateSignal,
    string? label = null,
    string? description = null,
    bool required = false,
    FormFieldLayoutOptions? layoutOptions = null,
    Func<object?, (bool, string)>[]? validators = null,
    FormValidationStrategy validationStrategy = FormValidationStrategy.OnBlur,
    Sizes size = Sizes.Medium,
    string? help = null,
    string? placeholder = null
    ) : IFormFieldBinding<TModel>
{
    public (IFormFieldView, IDisposable) Bind(IState<TModel> model)
    {
        var (fieldState, disposable) = StateHelpers.MemberState(model, selector);
        var fieldView = new FormFieldView(fieldState, factory, visible, updateSignal, label, description, help, placeholder, required, layoutOptions, validators, validationStrategy, size);
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

/// <summary>Renders form fields in a structured layout with columns, rows, and groups.</summary>
public class FormView<TModel>(IFormFieldView[] fieldViews, Func<Event<Form>, ValueTask>? handleSubmit = null, Sizes size = Sizes.Medium, Dictionary<string, bool>? groupOpenStates = null) : ViewBase
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
            var gap = size switch
            {
                Sizes.Small => 4,
                Sizes.Medium => 6,
                Sizes.Large => 8,
                _ => 8
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
                        )).Cast<object>().ToArray()
                    .ToArray()));

        var form = new Form(Layout.Horizontal(columns));
        if (handleSubmit != null)
        {
            form = form.HandleSubmit(handleSubmit);
        }
        return form;
    }
}