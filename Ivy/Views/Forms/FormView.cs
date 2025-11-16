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
    /// <summary>Creates a cloned state for form field data binding.</summary>
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

/// <summary>Signal for coordinating form validation across all fields.</summary>
public class FormValidateSignal : AbstractSignal<Unit, bool>;

/// <summary>Signal for notifying form field updates.</summary>
public class FormUpdateSignal : AbstractSignal<Unit, Unit>;

/// <summary>Validation timing strategy for form fields.</summary>
public enum FormValidationStrategy
{
    /// <summary>Validate when field loses focus.</summary>
    OnBlur,
    /// <summary>Validate when form is submitted.</summary>
    OnSubmit
}

/// <summary>Form field view with validation, data binding, and visibility control.</summary>
public class FormFieldView : ViewBase, IFormFieldView
{
    private readonly IAnyState bindingState;
    private readonly Func<IAnyState, IViewContext, IAnyInput> inputFactory;
    private readonly Func<bool> visible;
    private readonly ISignalSender<Unit, Unit> updateSender;
    private readonly string? label;
    private readonly string? description;
    private readonly bool required;
    private readonly Func<object?, (bool, string)>[]? validators;
    private readonly FormValidationStrategy validationStrategy;
    private readonly Sizes size;
    private readonly string? help;

    /// <summary>Layout configuration for positioning this field in the form.</summary>
    public FormFieldLayoutOptions Layout { get; }

    public FormFieldView(
        IAnyState bindingState,
        Func<IAnyState, IViewContext, IAnyInput> inputFactory,
        Func<bool> visible,
        ISignalSender<Unit, Unit> updateSender,
        string? label = null,
        string? description = null,
        bool required = false,
        FormFieldLayoutOptions? layoutOptions = null,
        Func<object?, (bool, string)>[]? validators = null,
        FormValidationStrategy validationStrategy = FormValidationStrategy.OnBlur,
        Sizes size = Sizes.Medium,
        string? help = null)
    {
        this.bindingState = bindingState;
        this.inputFactory = inputFactory;
        this.visible = visible;
        this.updateSender = updateSender;
        this.label = label;
        this.description = description;
        this.required = required;
        this.Layout = layoutOptions ?? new FormFieldLayoutOptions(Guid.NewGuid());
        this.validators = validators;
        this.validationStrategy = validationStrategy;
        this.size = size;
        this.help = help;
    }

    /// <summary>Validates the field value using configured validators.</summary>
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

    /// <summary>Builds the form field with input control, validation, and data binding.</summary>
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
    string? help = null
    ) : IFormFieldBinding<TModel>
{
    /// <summary>Creates a bound field view connected to the model state.</summary>
    public (IFormFieldView, IDisposable) Bind(IState<TModel> model)
    {
        var (fieldState, disposable) = StateHelpers.MemberState(model, selector);
        var fieldView = new FormFieldView(fieldState, factory, visible, updateSignal, label, description, required, layoutOptions, validators, validationStrategy, size, help);
        return (fieldView, disposable);
    }
}

/// <summary>Interface for form field views with layout information.</summary>
public interface IFormFieldView : IView
{
    /// <summary>Layout configuration for this field.</summary>
    public FormFieldLayoutOptions Layout { get; }
}

/// <summary>Interface for binding form fields to model properties.</summary>
public interface IFormFieldBinding<TModel>
{
    /// <summary>Binds the field to a model state and returns the view and disposable.</summary>
    (IFormFieldView fieldView, IDisposable disposable) Bind(IState<TModel> model);
}

/// <summary>Renders form fields in a structured layout with columns, rows, and groups.</summary>
public class FormView<TModel>(IFormFieldView[] fieldViews, Func<Event<Form>, ValueTask>? handleSubmit = null, Sizes size = Sizes.Medium, Dictionary<string, bool>? groupOpenStates = null) : ViewBase
{
    /// <summary>Builds the complete form layout with multi-column support and field grouping.</summary>
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