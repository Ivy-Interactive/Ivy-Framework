using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Widgets.Inputs;

namespace Ivy.Forms;

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
    Func<IAnyState, IAnyInput> inputFactory,
    Func<bool> visible,
    ISignalSender<Unit, Unit> updateSender,
    string? label,
    string? description,
    bool required,
    FormFieldLayoutOptions? layoutOptions,
    Func<object?, (bool, string)>[]? validators,
    FormValidationStrategy validationStrategy) : ViewBase, IFormFieldView
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

        var input = inputFactory(inputState).Invalid(invalidState.Value);
        if (validationStrategy == FormValidationStrategy.OnBlur)
        {
            input.HandleBlur(OnBlur);
        }

        return visibleState.Value ? new FormField(input, label, description, required) : null;
    }
}

public record FormFieldLayoutOptions(Guid RowKey, int Column = 0, int Order = 0, string? Group = null);

public class FormFieldBinding<TModel>(
    Expression<Func<TModel, object>> selector,
    Func<IAnyState, IAnyInput> factory,
    Func<bool> visible,
    ISignalSender<Unit, Unit> updateSignal,
    string? label = null,
    string? description = null,
    bool required = false,
    FormFieldLayoutOptions? layoutOptions = null,
    Func<object?, (bool, string)>[]? validators = null
    ) : IFormFieldBinding<TModel>
{
    public (IFormFieldView, IDisposable) Bind(IState<TModel> model)
    {
        var (fieldState, disposable) = StateHelpers.MemberState(model, selector);
        var fieldView = new FormFieldView(fieldState, factory, visible, updateSignal, label, description, required, layoutOptions, validators, FormValidationStrategy.OnSubmit);
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

public class FormView<TModel>(IFormFieldView[] fieldViews) : ViewBase
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
            return Layout
                .Vertical(fs.OrderBy(h => h.Layout.Order)
                    .GroupBy(f => f.Layout.RowKey).Select(e => e.ToArray()).Select(RenderRow));
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
                        )).Cast<object>().ToArray()
                    .ToArray()));

        return new Form(Layout.Horizontal(columns));
    }
}