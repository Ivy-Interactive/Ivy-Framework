using System.Linq.Expressions;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Shared;
using Ivy.Widgets.Inputs;
using static Ivy.Views.Forms.FormHelpers;

namespace Ivy.Views.Forms;

public class FormBuilder<TModel> : ViewBase
{
    private readonly Dictionary<string, FormBuilderField<TModel>> _fields;
    private readonly IState<TModel> _model;
    private readonly List<string> _groups = [];
    private readonly Dictionary<string, bool> _groupOpenStates = [];

    internal Scale _scale = Shared.Scale.Medium;
    internal Func<bool, Button> _submitBuilder = DefaultSubmitBuilder("Save");
    internal FormValidationStrategy _validationStrategy;
    internal Func<TModel, Task>? _onSubmit;

    public FormBuilder(
        IState<TModel> model,
        string submitTitle = "Save",
        FormValidationStrategy validationStrategy = FormValidationStrategy.OnBlur)
    {
        _model = model;
        _fields = FormScaffolder.ScaffoldFields<TModel>(_model.GetStateType());
        SubmitTitle(submitTitle);
        _validationStrategy = validationStrategy;
    }

    internal static Func<bool, Button> DefaultSubmitBuilder(string title) => (isLoading) => new Button(title).Loading(isLoading).Disabled(isLoading);

    public FormBuilder<TModel> SubmitBuilder(Func<bool, Button> builder)
    {
        _submitBuilder = builder;
        return this;
    }

    public FormBuilder<TModel> SubmitTitle(string title)
    {
        _submitBuilder = DefaultSubmitBuilder(title);
        return this;
    }

    public FormBuilder<TModel> ValidationStrategy(FormValidationStrategy strategy)
    {
        _validationStrategy = strategy;
        return this;
    }

    internal IState<TModel> GetModel() => _model;

    /// <summary>
    /// Sets a callback that is invoked after the form passes validation but before the model state is updated.
    /// Use this for async operations like saving to a database.
    /// </summary>
    public FormBuilder<TModel> HandleSubmit(Func<TModel, Task> onSubmit)
    {
        _onSubmit = onSubmit;
        return this;
    }

    public FormBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IAnyState, IAnyInput> factory)
    {
        return Builder(field, (state, _) => factory(state));
    }

    public FormBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IAnyState, IViewContext, IAnyInput> factory)
    {
        var fieldInfo = GetField(field);

        //todo: Why is this needed? Can we solve this differently in the scaffolding step?
        Func<IAnyState, IViewContext, IAnyInput> ScaffoldWrapper(Func<IAnyState, IViewContext, IAnyInput> inner)
        {
            return (state, context) =>
            {
                var input = inner(state, context);
                if (input is IAnyBoolInput boolInput)
                {
                    // Only apply scaffold defaults if no custom label was set
                    if (HasCustomLabel(fieldInfo.Label, fieldInfo.Name))
                    {
                        // Custom label was set, don't override it
                        boolInput.Label = fieldInfo.Label;
                    }
                    else
                    {
                        // Use scaffold defaults
                        boolInput.ScaffoldDefaults(fieldInfo.Name, fieldInfo.Type);
                    }
                }
                else if (input is IAnyNumberInput numberInput)
                {
                    numberInput.ScaffoldDefaults(fieldInfo.Name, fieldInfo.Type);
                }

                return input;
            };
        }

        fieldInfo.InputFactory = ScaffoldWrapper(factory);
        return this;

        bool HasCustomLabel(string label, string name) => label != Utils.SplitPascalCase(name);
    }

    public FormBuilder<TModel> Builder<TU>(Func<IAnyState, IAnyInput> input)
    {
        return Builder<TU>((state, _) => input(state));
    }

    public FormBuilder<TModel> Builder<TU>(Func<IAnyState, IViewContext, IAnyInput> input)
    {
        foreach (var hint in _fields.Values.Where(e => e.Type is TU))
        {
            hint.InputFactory = input;
        }

        return this;
    }

    public FormBuilder<TModel> Description(Expression<Func<TModel, object>> field, string description)
    {
        var hint = GetField(field);
        hint.Description = description;
        return this;
    }

    public FormBuilder<TModel> Help(Expression<Func<TModel, object>> field, string help)
    {
        var hint = GetField(field);
        hint.Help = help;
        return this;
    }

    public FormBuilder<TModel> Label(Expression<Func<TModel, object>> field, string label)
    {
        var hint = GetField(field);
        hint.Label = label;
        return this;
    }

    public FormBuilder<TModel> Placeholder(Expression<Func<TModel, object>> field, string placeholder)
    {
        var hint = GetField(field);
        hint.Placeholder = placeholder;
        return this;
    }

    private FormBuilder<TModel> _Place(int col, Guid? row, params Expression<Func<TModel, object>>[] fields)
    {
        int order = _fields.Values
            .Where(e => e.Column == col)
            .Where(e => e.Order != int.MaxValue)
            .Select(e => (int?)e.Order).Max() ?? 0;

        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Removed = false;
            if (hint.Group == null)
            {
                hint.Order = ++order;
            }
            hint.Column = col;
            hint.RowKey = row ?? Guid.NewGuid();
        }

        return this;
    }

    public FormBuilder<TModel> Place(int col, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(col, null, fields);
    }

    public FormBuilder<TModel> Place(params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, null, fields);
    }

    [Obsolete("Use PlaceHorizontal")]
    public FormBuilder<TModel> Place(bool row, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, row ? Guid.NewGuid() : null, fields);
    }

    public FormBuilder<TModel> PlaceHorizontal(params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, Guid.NewGuid(), fields);
    }

    public FormBuilder<TModel> Place(int col, bool row, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(col, row ? Guid.NewGuid() : null, fields);
    }

    public FormBuilder<TModel> Group(string group, params Expression<Func<TModel, object>>[] fields)
    {
        return Group(group, 0, false, fields);
    }

    public FormBuilder<TModel> Group(string group, bool open, params Expression<Func<TModel, object>>[] fields)
    {
        return Group(group, 0, open, fields);
    }

    public FormBuilder<TModel> Group(string group, int column, params Expression<Func<TModel, object>>[] fields)
    {
        return Group(group, column, false, fields);
    }

    public FormBuilder<TModel> Group(string group, int column, bool open, params Expression<Func<TModel, object>>[] fields)
    {
        int order = 0;

        if (!_groups.Contains(group))
        {
            _groups.Add(group);
        }

        _groupOpenStates[group] = open;

        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Group = group;
            hint.Order = order++;
            hint.Column = column;
        }
        return this;
    }

    public FormBuilder<TModel> Remove(params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var field in fields)
        {
            var hint = GetField(field);
            hint.Removed = true;
        }
        return this;
    }

    public FormBuilder<TModel> Add(Expression<Func<TModel, object>> field)
    {
        var hint = GetField(field);
        hint.Removed = false;
        return this;
    }

    public FormBuilder<TModel> Clear()
    {
        foreach (var field in _fields.Values)
        {
            field.Removed = true;
        }
        return this;
    }

    public FormBuilder<TModel> Visible(Expression<Func<TModel, object>> field, Func<TModel, bool> predicate)
    {
        var hint = GetField(field);
        hint.Visible = predicate;
        return this;
    }

    public FormBuilder<TModel> Disabled(bool disabled, params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Disabled = disabled;
        }
        return this;
    }

    public FormBuilder<TModel> Validate<T>(Expression<Func<TModel, object>> field, Func<T, (bool, string)> validator)
    {
        var hint = GetField(field);
        hint.Validators.Add((o) => validator((T)o!));
        return this;
    }

    public FormBuilder<TModel> Required(params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Required = true;
            hint.Validators.Add(e => (Utils.IsValidRequired(e), "Required field"));
        }
        return this;
    }

    public FormBuilder<TModel> Scale(Scale scale)
    {
        _scale = scale;
        return this;
    }

    public FormBuilder<TModel> Small() => Scale(Shared.Scale.Small);
    public FormBuilder<TModel> Medium() => Scale(Shared.Scale.Medium);
    public FormBuilder<TModel> Large() => Scale(Shared.Scale.Large);

    private FormBuilderField<TModel> GetField<TU>(Expression<Func<TModel, TU>> field)
    {
        var name = Utils.GetNameFromMemberExpression(field.Body);
        return _fields[name];
    }

    private Expression<Func<TModel, object>> CreateSelector(string name)
    {
        var parameter = Expression.Parameter(typeof(TModel), "x");
        var member = Expression.PropertyOrField(parameter, name);
        var converted = Expression.Convert(member, typeof(object));
        return Expression.Lambda<Func<TModel, object>>(converted, parameter);
    }

    public (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm(IViewContext context)
    {
        var currentModel = context.UseState(() => StateHelpers.DeepClone(_model.Value), buildOnChange: false);

        var validationSignal = context.CreateSignal<FormValidateSignal, Unit, bool>();
        var updateSignal = context.CreateSignal<FormUpdateSignal, Unit, Unit>();
        var invalidFields = context.UseState(0);

        var fields = _fields
            .Values
            .Where(e => e is { Removed: false, InputFactory: not null })
            .Select(e =>
            {
                IFormFieldBinding<TModel> binding = new FormFieldBinding<TModel>(
                    CreateSelector(e.Name),
                    e.InputFactory!,
                    () => e.Visible(currentModel.Value),
                    updateSignal,
                    e.Label,
                    e.Description,
                    e.Required,
                    new FormFieldLayoutOptions(e.RowKey, e.Column, e.Order, e.Group),
                    e.Validators.ToArray(),
                    _validationStrategy,
                    _scale,
                    e.Help,
                    e.Placeholder
                );
                return binding;
            })
            .ToArray();

        async Task<bool> OnSubmit()
        {
            var results = await validationSignal.Send(new Unit());
            if (results.All(e => e))
            {
                if (_onSubmit != null)
                {
                    await _onSubmit(currentModel.Value);
                }
                _model.Set(StateHelpers.DeepClone(currentModel.Value)!);
                invalidFields.Set(0);
                return true;
            }
            invalidFields.Set(results.Count(e => !e));
            return false;
        }

        var bindings = fields.Select(e => e.Bind(currentModel)).ToArray();
        context.TrackDisposable(bindings.Select(e => e.disposable));

        var fieldViews = bindings.Select(e => e.fieldView).ToArray();

        async ValueTask HandleSubmitEvent(Event<Form> _)
        {
            await OnSubmit();
        }

        var formView = new FormView<TModel>(
            fieldViews,
            HandleSubmitEvent,
            _scale,
            _groupOpenStates
        );

        var validationView = new WrapperView(Layout.Vertical(
            (invalidFields.Value > 0 ?
                Layout.Horizontal(
                    Text.Muted(InvalidMessage(invalidFields.Value))
                ).Left().Gap(1)
            : null!)
        ).Grow());

        return (OnSubmit, formView, validationView, false);
    }

    public override object? Build()
    {
        (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool submitting) = UseForm(this.Context);

        var (handleSubmit, isUploading) = Context.UseUploadAwareSubmit(_model, onSubmit);

        var buttonGap = _scale switch
        {
            Shared.Scale.Small => 4,
            Shared.Scale.Large => 8,
            _ => 6
        };

        return Layout.Vertical().Gap(buttonGap)
               | formView
               | Layout.Horizontal(
                   _submitBuilder(submitting || isUploading).HandleClick(_ => handleSubmit()).Scale(_scale),
                   validationView
                );
    }

    private static string InvalidMessage(int invalidFields)
    {
        return invalidFields == 1 ? "There is 1 invalid field." : $"There are {invalidFields} invalid fields.";
    }
}
