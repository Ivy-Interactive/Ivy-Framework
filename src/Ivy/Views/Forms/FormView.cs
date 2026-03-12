using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;

// ReSharper disable once CheckNamespace
namespace Ivy;

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

public class FormSubmitSignal : AbstractSignal<Unit, Unit>;

public enum FormValidationStrategy
{
    OnBlur,
    OnSubmit
}

public class FormFieldView(
    IAnyState bindingState,
    Func<IAnyState, IViewContext, IAnyInput> inputFactory,
    Func<bool> visible,
    ISignal<Unit, Unit> updateSender,
    string? label = null,
    string? description = null,
    string? help = null,
    string? placeholder = null,
    bool required = false,
    FormFieldLayoutOptions? layoutOptions = null,
    Func<object?, (bool, string)>[]? validators = null,
    FormValidationStrategy validationStrategy = FormValidationStrategy.OnBlur,
    Density density = Density.Medium,
    FormSubmitStrategy submitStrategy = FormSubmitStrategy.OnSubmit)
    : ViewBase, IFormFieldView
{
    public FormFieldLayoutOptions Layout { get; } = layoutOptions ?? new FormFieldLayoutOptions(Guid.NewGuid());

    private bool Validate<T>(T value, IState<string> invalid, IAnyInput? input = null)
    {
        if (!visible()) return true;

        if (validators == null && input == null)
            return true;

        bool isValid;
        string? message;
        if (input != null)
        {
            (isValid, message) = Validators.RunValidation(value, input, label, validators);
        }
        else
        {
            isValid = true;
            message = null;
            foreach (var validator in validators!)
            {
                (isValid, var msg) = validator(value);
                if (!isValid)
                {
                    message = string.IsNullOrEmpty(msg) ? "Invalid value" : msg;
                    break;
                }
            }
        }

        invalid.Set(isValid ? null! : message ?? "");
        return isValid;
    }

    public override object? Build()
    {
        IAnyState inputState = Context.UseClonedAnyState(bindingState);
        var invalidState = UseState((string?)null!);
        var blurOnceState = UseState(false);
        var validationReceiver = UseSignal<FormValidateSignal, Unit, bool>();
        var updateReceiver = UseSignal<FormUpdateSignal, Unit, Unit>();
        var submitSender = UseSignal<FormSubmitSignal, Unit, Unit>();
        var visibleState = UseState(visible);

        var inputRef = UseRef<IAnyInput?>(() => default);

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
                    return Validate(value, invalidState, inputRef.Value);
                })
            );
        });

        UseEffect(() =>
        {
            var value = inputState.As<object>().Value;
            if (blurOnceState.Value)
            {
                Validate(value, invalidState, inputRef.Value);
            }
            bindingState.As<object>().Set(value);
            updateSender.Send(new Unit());
            if (submitStrategy == FormSubmitStrategy.OnChange)
            {
                submitSender.Send(new Unit());
            }
        }, [inputState, blurOnceState]);

        void OnBlur(Event<IAnyInput> _)
        {
            blurOnceState.Set(true);
            if (submitStrategy == FormSubmitStrategy.OnBlur)
            {
                submitSender.Send(new Unit());
            }
        }

        var input = inputFactory(inputState, Context);
        inputRef.Set(input);
        input = input.Invalid(invalidState.Value ?? "");
        if (validationStrategy == FormValidationStrategy.OnBlur || submitStrategy == FormSubmitStrategy.OnBlur)
        {
            input.OnBlur(OnBlur);
        }

        // Set placeholder if the input supports it
        if (!string.IsNullOrEmpty(placeholder))
        {
            input.Placeholder = placeholder;
        }

        if (density != Density.Medium)
        {
            WidgetBaseExtensions.SetDensityViaReflection(input, density);
        }

        return visibleState.Value ? new Field(input, label, description, required, help, density) : null;
    }
}

public record FormFieldLayoutOptions(Guid RowKey, int Column = 0, int Order = 0, string? Group = null);

public class FormFieldBinding<TModel>(
    Expression<Func<TModel, object>> selector,
    Func<IAnyState, IViewContext, IAnyInput> factory,
    Func<bool> visible,
    ISignal<Unit, Unit> updateSignal,
    string? label = null,
    string? description = null,
    bool required = false,
    FormFieldLayoutOptions? layoutOptions = null,
    Func<object?, (bool, string)>[]? validators = null,
    FormValidationStrategy validationStrategy = FormValidationStrategy.OnBlur,
    Density density = Density.Medium,
    string? help = null,
    string? placeholder = null,
    FormSubmitStrategy submitStrategy = FormSubmitStrategy.OnSubmit
    ) : IFormFieldBinding<TModel>
{
    public (IFormFieldView, IDisposable) Bind(IState<TModel> model)
    {
        var (fieldState, disposable) = StateHelpers.MemberState(model, selector);
        var fieldView = new FormFieldView(fieldState, factory, visible, updateSignal, label, description, help, placeholder, required, layoutOptions, validators, validationStrategy, density, submitStrategy);
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

public class FormView<TModel>(IFormFieldView[] fieldViews, Func<Event<Form>, ValueTask>? handleSubmit = null, Density density = Density.Medium, Dictionary<string, bool>? groupOpenStates = null) : ViewBase
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
            var gap = density switch
            {
                Density.Medium => 5,
                Density.Small => 4,
                Density.Large => 6,
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
                                    .Density(density)
                        )).Cast<object>().ToArray()));

        var form = new Form(Layout.Horizontal(columns));
        if (handleSubmit != null)
        {
            form = form.OnSubmit(handleSubmit);
        }
        return form;
    }
}
