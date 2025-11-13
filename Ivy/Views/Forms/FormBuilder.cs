using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;
using Ivy.Client;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Services;
using Ivy.Shared;
using Ivy.Widgets.Inputs;

namespace Ivy.Views.Forms;

/// <summary>Field configuration within form builder containing metadata, validation rules, and layout information.</summary>
/// <typeparam name="TModel">Type of model object that form is bound to.</typeparam>
public class FormBuilderField<TModel>
{
    /// <summary>Initializes form builder field with specified configuration and metadata.</summary>
    /// <param name="name">Name of field, typically matching property or field name in model.</param>
    /// <param name="label">Display label for field, automatically formatted from field name.</param>
    /// <param name="order">Initial order position for field in form layout.</param>
    /// <param name="inputFactory">Optional factory function to create input control for this field.</param>
    /// <param name="fieldInfo">Reflection information for field if it represents class field.</param>
    /// <param name="propertyInfo">Reflection information for property if it represents class property.</param>
    /// <param name="required">Whether field is required and should have validation applied.</param>
    public FormBuilderField(
        string name,
        string label,
        int order,
        Func<IAnyState, IViewContext, IAnyInput>? inputFactory,
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

        // Add validators from DataAnnotations attributes
        if (propertyInfo != null)
        {
            Validators.AddRange(FormHelpers.GetValidators(propertyInfo));
        }
        else if (fieldInfo != null)
        {
            Validators.AddRange(FormHelpers.GetValidators(fieldInfo));
        }

        Visible = _ => true;
    }

    //public Func<Control, object> Helper { get; set; }

    /// <summary>Visibility predicate determining whether field should be displayed based on current model state.</summary>
    public Func<TModel, bool> Visible { get; set; }

    //public List<(EditorField<T> field, Func<T, object> transformer)> Dependencies = new();

    /// <summary>Name of field, typically matching property or field name in model.</summary>
    public string Name { get; set; }

    private FieldInfo? FieldInfo { get; set; }

    private PropertyInfo? PropertyInfo { get; set; }

    /// <summary>Type of field or property that this form field represents.</summary>
    public Type Type => (FieldInfo?.FieldType ?? PropertyInfo?.PropertyType)!;

    /// <summary>Whether field should be disabled (read-only) in form. Defaults to true.</summary>
    public bool Disabled { get; set; } = true;

    /// <summary>Order position of field within its column and group. Lower values appear first.</summary>
    public int Order { get; set; }

    /// <summary>Column index for multi-column form layouts.</summary>
    public int Column { get; set; }

    /// <summary>Unique identifier for row containing this field.</summary>
    public Guid RowKey { get; set; }

    /// <summary>Group name for organizing related fields together.</summary>
    public string? Group { get; set; }

    /// <summary>Display label for field shown to users.</summary>
    public string Label { get; set; }

    /// <summary>Optional description text providing additional context for field.</summary>
    public string? Description { get; set; }

    /// <summary>Optional help text displayed as tooltip on info icon next to label.</summary>
    public string? Help { get; set; }

    /// <summary>Factory function creating input control for this field with access to view context.</summary>
    public Func<IAnyState, IViewContext, IAnyInput>? InputFactory { get; set; }

    /// <summary>Whether field has been removed from form and should not be rendered.</summary>
    public bool Removed { get; set; }

    /// <summary>Whether field is required and must have value for form submission.</summary>
    public bool Required { get; set; }

    /// <summary>Collection of validation functions applied to this field's value.</summary>
    public List<Func<object?, (bool, string)>> Validators { get; set; } = new();
}

/// <summary>Fluent form builder automatically scaffolding forms from model types with intelligent input selection, validation, and layout management.</summary>
/// <typeparam name="TModel">Type of model object that form will edit.</typeparam>
public class FormBuilder<TModel> : ViewBase
{
    private readonly Dictionary<string, FormBuilderField<TModel>> _fields;

    private readonly IState<TModel> _model;

    /// <summary>The text displayed on the form's submit button.</summary>
    public readonly string SubmitTitle;
    private readonly List<string> _groups = [];
    private readonly Dictionary<string, bool> _groupOpenStates = [];

    /// <summary>The validation strategy for form fields. Default is OnBlur.</summary>
    public FormValidationStrategy ValidationStrategy { get; set; } = FormValidationStrategy.OnBlur;

    /// <summary>The size of the form affecting spacing between fields. Default is Medium.</summary>
    public Sizes Size { get; set; } = Sizes.Medium;

    /// <summary>Initializes form builder for specified model state with automatic field scaffolding.</summary>
    /// <param name="model">Reactive state containing model object to be edited by form.</param>
    /// <param name="submitTitle">The text displayed on the form's submit button. Default is "Save".</param>
    public FormBuilder(IState<TModel> model, string submitTitle = "Save")
    {
        _model = model;
        SubmitTitle = submitTitle;
        _fields = [];
        _Scaffold();
    }

    private void _Scaffold()
    {
        var type = _model.GetStateType();

        var fields = type
            .GetFields()
            .Where(f => f.GetCustomAttribute<ScaffoldColumnAttribute>()?.Scaffold != false)
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
                    .Where(p => p.GetCustomAttribute<ScaffoldColumnAttribute>()?.Scaffold != false)
                    .Select(e => new
                    {
                        e.Name,
                        Type = e.PropertyType,
                        FieldInfo = (FieldInfo)null!,
                        PropertyInfo = e,
                        Required = FormHelpers.IsRequired(e)
                    })
            )
            .ToList();

        var order = fields.Count;
        foreach (var field in fields)
        {
            var label = Utils.LabelFor(field.Name, field.Type);
            _fields[field.Name] =
                new FormBuilderField<TModel>(field.Name, label, order++, ScaffoldEditor(field.Name, field.Type),
                    field.FieldInfo, field.PropertyInfo, field.Required);
        }

        // Add automatic validators after fields are created
        foreach (var field in _fields.Values)
        {
            // Automatic email validation for fields ending with "Email"
            var nonNullableType = Nullable.GetUnderlyingType(field.Type) ?? field.Type;
            if (field.Name.EndsWith("Email") && nonNullableType == typeof(string))
            {
                field.Validators.Add(Validators.CreateEmailValidator(field.Name));
            }
        }
    }

    private Func<IAnyState, IViewContext, IAnyInput>? ScaffoldEditor(string name, Type type)
    {
        Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

        static bool IsFileUploadType(Type t)
        {
            if (t == typeof(FileUpload)) return true;
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(FileUpload<>)) return true;
            return typeof(IFileUpload).IsAssignableFrom(t);
        }

        // FileUpload fields are not auto-scaffolded - use .Builder() to configure them manually
        if (IsFileUploadType(nonNullableType))
        {
            return null;
        }

        // Collections of FileUpload / FileUpload<T>
        foreach (var it in type.GetInterfaces().Concat([type]))
        {
            if (it.IsGenericType && it.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var arg = it.GetGenericArguments()[0];
                if (IsFileUploadType(arg))
                {
                    return null;
                }
            }
        }

        if (name.EndsWith("Id") && (type == typeof(Guid) || type == typeof(int) || type == typeof(string)))
        {
            return (state, _) => state.ToReadOnlyInput().Size(Size);
        }

        if (name.EndsWith("Email") && nonNullableType == typeof(string))
        {
            return (state, _) => state.ToEmailInput().Size(Size);
        }

        if ((name.EndsWith("Color") || name.EndsWith("Colour")) && nonNullableType == typeof(string))
        {
            return (state, _) => state.ToColorInput().Size(Size);
        }

        if (nonNullableType == typeof(bool))
        {
            return (state, _) =>
            {
                var input = state.ToBoolInput();
                // Only apply scaffold defaults if no custom label was set
                if (_fields.TryGetValue(name, out var field) && HasCustomLabel(field.Label, name))
                {
                    // Custom label was set, don't override it
                    input.Label = field.Label;
                }
                else
                {
                    // Use scaffold defaults
                    input.ScaffoldDefaults(name, type);
                }
                return input.Size(Size);
            };
        }

        if (nonNullableType == typeof(string))
        {
            if (name.EndsWith("Password"))
            {
                return (state, _) => state.ToPasswordInput().Size(Size);
            }

            return (state, _) => state.ToTextInput().Size(Size);
        }

        if (nonNullableType.IsEnum)
        {
            return (state, _) => state.ToSelectInput().Size(Size);
        }

        if (type.IsCollectionType() && type.GetCollectionTypeParameter() is { IsEnum: true })
        {
            return (state, _) => state.ToSelectInput().List().Size(Size);
        }

        if (type.IsNumeric())
        {
            return (state, _) => state.ToNumberInput().ScaffoldDefaults(name, type).Size(Size);
        }

        if (type.IsDate())
        {
            return (state, _) => state.ToDateTimeInput().Size(Size);
        }

        return null;
    }

    /// <summary>Configures custom input factory for specified field (convenience overload without view context).</summary>
    /// <param name="field">Expression identifying field to configure.</param>
    /// <param name="factory">Input factory function to use for creating input control.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IAnyState, IAnyInput> factory)
    {
        return Builder(field, (state, _) => factory(state));
    }

    /// <summary>Configures custom input factory for specified field with automatic scaffolding wrapper.</summary>
    /// <param name="field">Expression identifying field to configure.</param>
    /// <param name="factory">Input factory function that receives both state and view context.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Builder(Expression<Func<TModel, object>> field, Func<IAnyState, IViewContext, IAnyInput> factory)
    {
        var hint = GetField(field);

        Func<IAnyState, IViewContext, IAnyInput> ScaffoldWrapper(Func<IAnyState, IViewContext, IAnyInput> inner)
        {
            return (state, context) =>
            {
                var input = inner(state, context);
                if (input is IAnyBoolInput boolInput)
                {
                    // Only apply scaffold defaults if no custom label was set
                    if (HasCustomLabel(hint.Label, hint.Name))
                    {
                        // Custom label was set, don't override it
                        boolInput.Label = hint.Label;
                    }
                    else
                    {
                        // Use scaffold defaults
                        boolInput.ScaffoldDefaults(hint.Name, hint.Type);
                    }
                }
                else if (input is IAnyNumberInput numberInput)
                {
                    numberInput.ScaffoldDefaults(hint.Name, hint.Type);
                }
                return input.Size(Size);
            };
        }

        hint.InputFactory = ScaffoldWrapper(factory);
        return this;
    }

    /// <summary>Configures custom input factory for all fields of specified type (convenience overload without view context).</summary>
    /// <typeparam name="TU">Type of fields to configure.</typeparam>
    /// <param name="input">Input factory function to use for all fields of this type.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Builder<TU>(Func<IAnyState, IAnyInput> input)
    {
        return Builder<TU>((state, _) => input(state));
    }

    /// <summary>Configures custom input factory for all fields of specified type.</summary>
    /// <typeparam name="TU">Type of fields to configure.</typeparam>
    /// <param name="input">Input factory function that receives both state and view context.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Builder<TU>(Func<IAnyState, IViewContext, IAnyInput> input)
    {
        foreach (var hint in _fields.Values.Where(e => e.Type is TU))
        {
            hint.InputFactory = (state, context) => input(state, context).Size(Size);
        }

        return this;
    }

    /// <summary>Sets description text for specified field.</summary>
    /// <param name="field">Expression identifying field to configure.</param>
    /// <param name="description">Description text to display below field.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Description(Expression<Func<TModel, object>> field, string description)
    {
        var hint = GetField(field);
        hint.Description = description;
        return this;
    }

    /// <summary>Sets help text for specified field displayed as tooltip on info icon next to label.</summary>
    /// <param name="field">Expression identifying field to configure.</param>
    /// <param name="help">Help text to display in tooltip.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Help(Expression<Func<TModel, object>> field, string help)
    {
        var hint = GetField(field);
        hint.Help = help;
        return this;
    }

    /// <summary>Sets custom label for specified field, overriding automatically generated label.</summary>
    /// <param name="field">Expression identifying field to configure.</param>
    /// <param name="label">Custom label text to display for field.</param>
    /// <returns>Form builder instance for method chaining.</returns>
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

    /// <summary>Places specified fields vertically in the given column with automatic ordering.</summary>
    /// <param name="col">Zero-based column index where fields should be placed.</param>
    /// <param name="fields">Fields to place vertically in the specified column.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Place(int col, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(col, null, fields);
    }

    /// <summary>Places specified fields vertically in the first column (column 0) with automatic ordering.</summary>
    /// <param name="fields">Fields to place vertically in the first column.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Place(params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, null, fields);
    }

    /// <summary>Places specified fields horizontally side-by-side when row is true, or vertically when false.</summary>
    /// <param name="row">True to arrange fields side-by-side in the same row; false to stack vertically.</param>
    /// <param name="fields">Fields to arrange. When row is true, fields will be distributed evenly across the row width.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Place(bool row, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, row ? Guid.NewGuid() : null, fields);
    }

    /// <summary>Places specified fields in a specific column, optionally arranging them horizontally side-by-side.</summary>
    /// <param name="col">Zero-based column index where fields should be placed.</param>
    /// <param name="row">True to arrange fields side-by-side in the same row; false to stack vertically in the column.</param>
    /// <param name="fields">Fields to place in the specified column. When row is true, fields will share the same row.</param>
    /// <returns>Form builder instance for method chaining.</returns>
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

    /// <summary>Removes specified fields from form so they will not be rendered.</summary>
    /// <param name="fields">Fields to remove from form.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Remove(params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var field in fields)
        {
            var hint = GetField(field);
            hint.Removed = true;
        }
        return this;
    }

    /// <summary>Adds previously removed field back to form.</summary>
    /// <param name="field">Field to add back to form.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Add(Expression<Func<TModel, object>> field)
    {
        var hint = GetField(field);
        hint.Removed = false;
        return this;
    }

    /// <summary>Removes all fields from form, creating blank form that can be selectively populated.</summary>
    /// <returns>Form builder instance for method chaining.</returns>
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

    /// <summary>Sets conditional visibility predicate for specified field based on current model state.</summary>
    /// <param name="field">Field to configure conditional visibility for.</param>
    /// <param name="predicate">Function determining field visibility based on current model state.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Visible(Expression<Func<TModel, object>> field, Func<TModel, bool> predicate)
    {
        var hint = GetField(field);
        hint.Visible = predicate;
        return this;
    }

    /// <summary>Sets disabled state for specified fields.</summary>
    /// <param name="disabled">Whether fields should be disabled (read-only).</param>
    /// <param name="fields">Fields to enable or disable.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Disabled(bool disabled, params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Disabled = disabled;
        }
        return this;
    }

    /// <summary>Adds custom validation rule to specified field.</summary>
    /// <typeparam name="T">Type of field value for type-safe validation.</typeparam>
    /// <param name="field">Field to add validation to.</param>
    /// <param name="validator">Function validating field value and returning result and error message.</param>
    /// <returns>Form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Validate<T>(Expression<Func<TModel, object>> field, Func<T, (bool, string)> validator)
    {
        var hint = GetField(field);
        hint.Validators.Add((o) => validator((T)o!));
        return this;
    }

    /// <summary>Marks specified fields as required, adding automatic required field validation.</summary>
    /// <param name="fields">Fields to mark as required.</param>
    /// <returns>Form builder instance for method chaining.</returns>
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

    /// <summary>Sets the size of the form affecting spacing between fields.</summary>
    /// <param name="size">The size of the form (Small, Medium, Large).</param>
    /// <returns>Form builder instance for method chaining.</returns>
    internal FormBuilder<TModel> SetSize(Sizes size)
    {
        Size = size;
        return this;
    }

    /// <summary>Sets form size to small for compact display.</summary>
    /// <returns>Form builder instance with small size applied.</returns>
    public FormBuilder<TModel> Small()
    {
        return SetSize(Sizes.Small);
    }

    /// <summary>Sets form size to medium for standard display.</summary>
    /// <returns>Form builder instance with medium size applied.</returns>
    public FormBuilder<TModel> Medium()
    {
        return SetSize(Sizes.Medium);
    }

    /// <summary>Sets form size to large for prominent display.</summary>
    /// <returns>Form builder instance with large size applied.</returns>
    public FormBuilder<TModel> Large()
    {
        return SetSize(Sizes.Large);
    }

    //todo: this looks like a hack that should be fixed properly
    private static bool HasCustomLabel(string label, string name)
        => label != Utils.SplitPascalCase(name);

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

    /// <summary>Creates form instance with validation, data binding, and submission handling for use in custom layouts.</summary>
    /// <param name="context">View context for state management and signal handling.</param>
    /// <returns>Tuple containing submit handler, form view, validation view, and loading state.</returns>
    public (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool loading) UseForm(IViewContext context)
    {
        var currentModel = context.UseState(() => StateHelpers.DeepClone(_model.Value), buildOnChange: false);

        var validationSignal = context.CreateSignal<FormValidateSignal, Unit, bool>();
        var updateSignal = context.CreateSignal<FormUpdateSignal, Unit, Unit>();
        var invalidFields = context.UseState(0);

        var fields = _fields
            .Values
            .Where(e => e is { Removed: false } && e.InputFactory != null)
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
                    ValidationStrategy,
                    Size,
                    e.Help
                );
                return binding;
            })
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
            Size,
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

    /// <summary>Builds complete form with automatic layout, validation, and submission handling.</summary>
    /// <returns>Complete form widget with fields, validation messages, and submit button.</returns>
    public override object? Build()
    {
        (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool submitting) = UseForm(this.Context);

        // Track upload state to disable submit button
        var hasUploading = UseState(false);
        var client = UseService<IClientProvider>();

        UseEffect(() =>
        {
            hasUploading.Set(CheckForLoadingUploads(_model.Value));
        }, _model);

        async ValueTask HandleSubmit()
        {
            if (hasUploading.Value)
            {
                client.Toast(
                    "File uploads are still in progress. Please wait for them to complete.",
                    "Uploads in Progress"
                );
                return;
            }
            await onSubmit();
        }

        return Layout.Vertical()
               | formView
               | Layout.Horizontal(new Button(SubmitTitle).HandleClick(HandleSubmit)
                   .Loading(submitting).Disabled(submitting || hasUploading.Value).Size(Size), validationView);
    }

    private static string InvalidMessage(int invalidFields)
    {
        return invalidFields == 1 ? "There is 1 invalid field." : $"There are {invalidFields} invalid fields.";
    }

    /// <summary>Recursively checks if any FileUpload fields in the model are currently uploading.</summary>
    private static bool CheckForLoadingUploads(object? obj)
    {
        if (obj == null) return false;

        // Check single file upload
        if (obj is IFileUpload file)
            return file.Status == FileUploadStatus.Loading;

        // Check collection of uploads
        if (obj is IEnumerable<IFileUpload> files)
            return files.Any(f => f.Status == FileUploadStatus.Loading);

        // Recursively check all properties
        var type = obj.GetType();

        // Skip primitive types and strings
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return false;

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            // Skip indexed properties
            if (prop.GetIndexParameters().Length > 0)
                continue;

            try
            {
                var value = prop.GetValue(obj);
                if (CheckForLoadingUploads(value))
                    return true;
            }
            catch
            {
                // Skip properties that can't be read
            }
        }

        // Check fields as well
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            try
            {
                var value = field.GetValue(obj);
                if (CheckForLoadingUploads(value))
                    return true;
            }
            catch
            {
                // Skip fields that can't be read
            }
        }

        return false;
    }
}
