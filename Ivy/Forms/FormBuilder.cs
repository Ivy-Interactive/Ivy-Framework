using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Helpers;
using Ivy.Hooks;
using Ivy.Views;
using Ivy.Widgets.Inputs;
using Microsoft.Extensions.DependencyInjection;

namespace Ivy.Forms;

public class FormBuilderField<TModel>
{
    public FormBuilderField(
        string name,
        string label,
        int order,
        Func<IAnyState, IAnyInput>? inputFactory,
        FieldInfo? fieldInfo,
        PropertyInfo? propertyInfo,
        bool required)
    {
        Name = name;
        Label = label;

        if (!name.EndsWith("GovId") && name != "Id" && name.EndsWith("Id"))
        {
            Label = Label[..^3];
        }

        Order = order;
        InputFactory = inputFactory;
        FieldInfo = fieldInfo;
        PropertyInfo = propertyInfo;
        Column = 0;
        Order = int.MaxValue;
        RowKey = Guid.NewGuid();
        Required = required;

        if (Required)
        {
            Validators.Add(e => (Utils.IsValidRequired(e), "Required field"));
        }

        Visible = _ => true;
    }

    //public Func<Control, object> Helper { get; set; }

    public Func<TModel, bool> Visible { get; set; }

    //public List<(EditorField<T> field, Func<T, object> transformer)> Dependencies = new();

    public string Name { get; set; }

    private FieldInfo? FieldInfo { get; set; }

    private PropertyInfo? PropertyInfo { get; set; }

    public Type Type => (FieldInfo?.FieldType ?? PropertyInfo?.PropertyType)!;

    public bool Disabled { get; set; } = true;

    public int Order { get; set; }

    public int Column { get; set; }

    public Guid RowKey { get; set; }

    public string? Group { get; set; }

    public string Label { get; set; }

    public string? Description { get; set; }

    public Func<IAnyState, IAnyInput>? InputFactory { get; set; }

    public bool Removed { get; set; }

    public bool Required { get; set; }

    public List<Func<object?, (bool, string)>> Validators { get; set; } = new();
}

public class FormBuilder<TModel> : ViewBase
{
    private readonly Dictionary<string, FormBuilderField<TModel>> _fields;

    private readonly IState<TModel> _model;

    public readonly string SubmitTitle = "Save";

    private readonly List<string> _groups = new();

    public FormBuilder(IState<TModel> model)
    {
        _model = model;
        _fields = new Dictionary<string, FormBuilderField<TModel>>();
        _Scaffold();
    }

    private void _Scaffold()
    {
        var type = _model.GetStateType();

        var fields = type
            .GetFields()
            .Select(e => new
            {
                e.Name,
                Type = e.FieldType,
                FieldInfo = e,
                PropertyInfo = (PropertyInfo)null!,
                Required = FormHelpers.IsRequired(e)
            })
            .Union(
                type
                    .GetProperties()
                    .Select(e => new
                    {
                        e.Name,
                        Type = e.PropertyType,
                        FieldInfo = (FieldInfo)null!,
                        PropertyInfo = e,
                        Required = FormHelpers.IsRequired(e)
                    }
                    )
            )
            .ToList();

        var order = fields.Count;
        foreach (var field in fields)
        {
            var label = Utils.SplitPascalCase(field.Name) ?? field.Name;

            _fields[field.Name] =
                new FormBuilderField<TModel>(field.Name, label, order++, ScaffoldEditor(field.Name, field.Type),
                    field.FieldInfo, field.PropertyInfo, field.Required);
        }
    }

    private Func<IAnyState, IAnyInput>? ScaffoldEditor(string name, Type type)
    {
        if (type == typeof(FileInput))
        {
            return (state) => state.ToFileInput();
        }

        if (name.EndsWith("Id") && (type == typeof(Guid) || type == typeof(int) || type == typeof(string)))
        {
            return (state) => state.ToReadOnlyInput();
        }

        if (name.EndsWith("Email") && type == typeof(string))
        {
            return (state) => state.ToEmailInput();
        }

        if (type == typeof(bool) || type == typeof(bool?))
        {
            return (state) => state.ToBoolInput().ScaffoldDefaults(name, type);
        }

        if (type == typeof(string))
        {
            if (name.EndsWith("Password"))
            {
                return (state) => state.ToPasswordInput();
            }

            return (state) => state.ToTextInput();
        }

        if (type.IsEnum)
        {
            return (state) => state.ToSelectInput();
        }

        if (type.IsCollectionType() && type.GetCollectionTypeParameter() is { IsEnum: true })
        {
            return (state) => state.ToSelectInput().List();
        }

        if (type.IsNumeric())
        {
            return (state) => state.ToNumberInput().ScaffoldDefaults(name, type);
        }

        if (type.IsDate())
        {
            return (state) => state.ToDateTimeInput();
        }

        return null;
    }

    public FormBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IAnyState, IAnyInput> factory)
    {
        var hint = GetField(field);

        Func<IAnyState, IAnyInput> ScaffoldWrapper(Func<IAnyState, IAnyInput> inner)
        {
            return (state) =>
            {
                var input = inner(state);
                if (input is IAnyBoolInput boolInput)
                {
                    boolInput.ScaffoldDefaults(hint.Name, hint.Type);
                }
                else if (input is IAnyNumberInput numberInput)
                {
                    numberInput.ScaffoldDefaults(hint.Name, hint.Type);
                }
                return input;
            };
        }

        hint.InputFactory = ScaffoldWrapper(factory);
        return this;
    }

    public FormBuilder<TModel> Builder<TU>(Func<IAnyState, IAnyInput> input)
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

    public FormBuilder<TModel> Label(Expression<Func<TModel, object>> field, string label)
    {
        var hint = GetField(field);
        hint.Label = label;
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

    public FormBuilder<TModel> Place(bool row, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, row ? Guid.NewGuid() : null, fields);
    }

    public FormBuilder<TModel> Place(int col, bool row, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(col, row ? Guid.NewGuid() : null, fields);
    }

    public FormBuilder<TModel> Group(string group, int column, params Expression<Func<TModel, object>>[] fields)
    {
        int order = 0;

        if (!_groups.Contains(group))
        {
            _groups.Add(group);
        }

        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Group = group;
            hint.Order = order++;
            hint.Column = column;
        }
        return this;
    }

    public FormBuilder<TModel> Group(string group, params Expression<Func<TModel, object>>[] fields)
    {
        return Group(group, 0, fields);
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

    // public EntityEditor<T> Helper(Expression<Func<T, object>> field, Func<Control, object> helper)
    // {
    //     var hint = GetField(field);
    //     hint.Helper = helper;
    //     return this;
    // }
    //

    // public EntityEditor<T> Derived<TU, TV>(Expression<Func<T, TU>> field, Expression<Func<T, TV>> derivedFrom, Func<T, TU> transformer)
    // {
    //     var _derivedFrom = GetField(derivedFrom);
    //     var _field = GetField(field);
    //     _derivedFrom.Dependencies.Add((_field, x => transformer(x)));
    //     return this;
    // }

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
            .Select(e => new FormFieldBinding<TModel>(
                CreateSelector(e.Name),
                e.InputFactory!,
                () => e.Visible(currentModel.Value),
                updateSignal,
                e.Label,
                e.Description,
                e.Required,
                new FormFieldLayoutOptions(e.RowKey, e.Column, e.Order, e.Group),
                e.Validators.ToArray()
            ))
            .Cast<IFormFieldBinding<TModel>>()
            .ToArray();

        async Task<bool> OnSubmit()
        {
            var results = await validationSignal.Send(new Unit());
            if (results.All(e => e))
            {
                _model.Set(StateHelpers.DeepClone(currentModel.Value)!);
                invalidFields.Set(0);
                return true;
            }
            invalidFields.Set(results.Count(e => !e));
            return false;
        }
        ;

        var bindings = fields.Select(e => e.Bind(currentModel)).ToArray();
        context.TrackDisposable(bindings.Select(e => e.disposable));

        var fieldViews = bindings.Select(e => e.fieldView).ToArray();

        var formView = new FormView<TModel>(
            fieldViews
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

        async void HandleSubmit() //todo: handle errors
        {
            await onSubmit();
        }

        return Layout.Vertical()
               | formView
               | Layout.Horizontal(new Button(SubmitTitle).HandleClick(new Action(HandleSubmit).ToEventHandler<Button>())
                   .Loading(submitting).Disabled(submitting), validationView);
    }

    private static string InvalidMessage(int invalidFields)
    {
        return invalidFields == 1 ? "There is 1 invalid field." : $"There are {invalidFields} invalid fields.";
    }
}