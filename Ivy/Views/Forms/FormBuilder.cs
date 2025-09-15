using System.Linq.Expressions;
using System.Reflection;
using Ivy.Core;
using Ivy.Core.Helpers;
using Ivy.Core.Hooks;
using Ivy.Hooks;
using Ivy.Widgets.Inputs;

namespace Ivy.Views.Forms;

/// <summary>
/// Represents a field configuration within a form builder, containing metadata, validation rules, and layout information.
/// </summary>
/// <typeparam name="TModel">The type of the model object that the form is bound to.</typeparam>
/// <remarks>
/// This class encapsulates all the configuration for a single form field, including its input factory,
/// validation rules, layout positioning, visibility conditions, and metadata. It is used internally
/// by FormBuilder to manage field configurations before they are rendered as form controls.
/// </remarks>
public class FormBuilderField<TModel>
{
    /// <summary>
    /// Initializes a new form builder field with the specified configuration and metadata.
    /// </summary>
    /// <param name="name">The name of the field, typically matching the property or field name in the model.</param>
    /// <param name="label">The display label for the field, automatically formatted from the field name.</param>
    /// <param name="order">The initial order position for the field in the form layout.</param>
    /// <param name="inputFactory">Optional factory function to create the input control for this field.</param>
    /// <param name="fieldInfo">Reflection information for the field if it represents a class field.</param>
    /// <param name="propertyInfo">Reflection information for the property if it represents a class property.</param>
    /// <param name="required">Whether the field is required and should have validation applied.</param>
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

    /// <summary>Gets or sets the visibility predicate that determines whether this field should be displayed based on the current model state.</summary>
    /// <value>A function that takes the model and returns true if the field should be visible, false otherwise.</value>
    public Func<TModel, bool> Visible { get; set; }

    //public List<(EditorField<T> field, Func<T, object> transformer)> Dependencies = new();

    /// <summary>Gets or sets the name of the field, typically matching the property or field name in the model.</summary>
    /// <value>The field name used for data binding and identification.</value>
    public string Name { get; set; }

    /// <summary>Gets the reflection information for the field if it represents a class field.</summary>
    /// <value>FieldInfo for class fields, or null if this represents a property.</value>
    private FieldInfo? FieldInfo { get; set; }

    /// <summary>Gets the reflection information for the property if it represents a class property.</summary>
    /// <value>PropertyInfo for class properties, or null if this represents a field.</value>
    private PropertyInfo? PropertyInfo { get; set; }

    /// <summary>Gets the type of the field or property that this form field represents.</summary>
    /// <value>The Type of the underlying field or property, used for input scaffolding and validation.</value>
    public Type Type => (FieldInfo?.FieldType ?? PropertyInfo?.PropertyType)!;

    /// <summary>Gets or sets whether this field should be disabled (read-only) in the form.</summary>
    /// <value>True if the field should be disabled, false if it should be editable. Defaults to true.</value>
    public bool Disabled { get; set; } = true;

    /// <summary>Gets or sets the order position of this field within its column and group.</summary>
    /// <value>The numeric order used for sorting fields in the form layout. Lower values appear first.</value>
    public int Order { get; set; }

    /// <summary>Gets or sets the column index for multi-column form layouts.</summary>
    /// <value>The zero-based column index where this field should be placed in the form grid.</value>
    public int Column { get; set; }

    /// <summary>Gets or sets the unique identifier for the row that contains this field.</summary>
    /// <value>A GUID that groups fields into the same horizontal row in the form layout.</value>
    public Guid RowKey { get; set; }

    /// <summary>Gets or sets the group name for organizing related fields together.</summary>
    /// <value>The group name for sectioning related fields, or null if the field is not grouped.</value>
    public string? Group { get; set; }

    /// <summary>Gets or sets the display label for the field shown to users.</summary>
    /// <value>The human-readable label text displayed alongside the input control.</value>
    public string Label { get; set; }

    /// <summary>Gets or sets the optional description text that provides additional context for the field.</summary>
    /// <value>Help text or description displayed below the field, or null if no description is provided.</value>
    public string? Description { get; set; }

    /// <summary>Gets or sets the factory function that creates the input control for this field.</summary>
    /// <value>A function that takes a state object and returns an input control, or null if no input is configured.</value>
    public Func<IAnyState, IAnyInput>? InputFactory { get; set; }

    /// <summary>Gets or sets whether this field has been removed from the form and should not be rendered.</summary>
    /// <value>True if the field should be excluded from the form, false if it should be included.</value>
    public bool Removed { get; set; }

    /// <summary>Gets or sets whether this field is required and must have a value for form submission.</summary>
    /// <value>True if the field is required and will have validation applied, false if it is optional.</value>
    public bool Required { get; set; }

    /// <summary>Gets the collection of validation functions that will be applied to this field's value.</summary>
    /// <value>A list of validator functions that return validation results and error messages.</value>
    public List<Func<object?, (bool, string)>> Validators { get; set; } = new();
}

/// <summary>
/// A fluent form builder that automatically scaffolds forms from model types with intelligent input selection, validation, and layout management.
/// </summary>
/// <typeparam name="TModel">The type of the model object that the form will edit.</typeparam>
/// <remarks>
/// FormBuilder provides a comprehensive solution for creating data-bound forms with minimal configuration.
/// It automatically inspects model types to generate appropriate input controls, applies intelligent defaults
/// based on property names and types, and provides a fluent API for customizing field behavior, validation,
/// layout, and appearance. The builder supports complex scenarios including conditional visibility,
/// custom validation rules, multi-column layouts, field grouping, and dynamic field management.
/// </remarks>
public class FormBuilder<TModel> : ViewBase
{
    /// <summary>The internal dictionary that stores field configurations indexed by field name.</summary>
    private readonly Dictionary<string, FormBuilderField<TModel>> _fields;

    /// <summary>The reactive state that holds the model being edited by the form.</summary>
    private readonly IState<TModel> _model;

    /// <summary>The text displayed on the form's submit button.</summary>
    public readonly string SubmitTitle = "Save";

    /// <summary>The list of group names that have been defined for organizing fields.</summary>
    private readonly List<string> _groups = new();

    /// <summary>
    /// Initializes a new form builder for the specified model state with automatic field scaffolding.
    /// </summary>
    /// <param name="model">The reactive state containing the model object to be edited by the form.</param>
    /// <remarks>
    /// The constructor automatically inspects the model type using reflection to discover all fields and properties,
    /// then creates appropriate field configurations with intelligent input type selection based on property names,
    /// types, and attributes. This provides a complete form with minimal configuration required.
    /// </remarks>
    public FormBuilder(IState<TModel> model)
    {
        _model = model;
        _fields = new Dictionary<string, FormBuilderField<TModel>>();
        _Scaffold();
    }

    /// <summary>
    /// Automatically discovers and configures form fields by inspecting the model type using reflection.
    /// </summary>
    /// <remarks>
    /// This method examines all public fields and properties of the model type, creates field configurations
    /// with appropriate labels (converted from PascalCase), determines required status from attributes,
    /// and assigns intelligent input factories based on property names and types.
    /// </remarks>
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
    }

    /// <summary>
    /// Creates an appropriate input factory based on the field name and type using intelligent heuristics.
    /// </summary>
    /// <param name="name">The name of the field, used for pattern matching (e.g., "Email", "Password", "Id").</param>
    /// <param name="type">The type of the field, used for type-based input selection.</param>
    /// <returns>An input factory function that creates the appropriate input control, or null if no suitable input is found.</returns>
    /// <remarks>
    /// This method implements intelligent input selection based on naming conventions and type analysis:
    /// - Fields ending in "Id" become read-only inputs
    /// - Fields ending in "Email" become email inputs
    /// - Fields ending in "Password" become password inputs
    /// - Boolean types become checkbox inputs
    /// - Enum types become select inputs
    /// - Numeric types become number inputs
    /// - Date types become date/time inputs
    /// - Collection of enums become multi-select inputs
    /// </remarks>
    private Func<IAnyState, IAnyInput>? ScaffoldEditor(string name, Type type)
    {
        Type nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(FileInput))
        {
            return (state) => state.ToFileInput();
        }

        if (name.EndsWith("Id") && (type == typeof(Guid) || type == typeof(int) || type == typeof(string)))
        {
            return (state) => state.ToReadOnlyInput();
        }

        if (name.EndsWith("Email") && nonNullableType == typeof(string))
        {
            return (state) => state.ToEmailInput();
        }

        if ((name.EndsWith("Color") || name.EndsWith("Colour")) && nonNullableType == typeof(string))
        {
            return (state) => state.ToColorInput();
        }

        if (nonNullableType == typeof(bool))
        {
            return (state) =>
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
                return input;
            };
        }

        if (nonNullableType == typeof(string))
        {
            if (name.EndsWith("Password"))
            {
                return (state) => state.ToPasswordInput();
            }

            return (state) => state.ToTextInput();
        }

        if (nonNullableType.IsEnum)
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

    /// <summary>
    /// Configures a custom input factory for the specified field with automatic scaffolding wrapper.
    /// </summary>
    /// <param name="field">Expression identifying the field to configure.</param>
    /// <param name="factory">The input factory function to use for creating the input control.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// This method wraps the provided factory with scaffolding logic that applies default configurations
    /// for boolean and numeric inputs based on the field's name and type information.
    /// </remarks>
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
                return input;
            };
        }

        hint.InputFactory = ScaffoldWrapper(factory);
        return this;
    }

    /// <summary>
    /// Configures a custom input factory for all fields of the specified type.
    /// </summary>
    /// <typeparam name="TU">The type of fields to configure.</typeparam>
    /// <param name="input">The input factory function to use for all fields of this type.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// This method allows bulk configuration of input types, useful for applying consistent
    /// input controls across all fields of a particular type in the form.
    /// </remarks>
    public FormBuilder<TModel> Builder<TU>(Func<IAnyState, IAnyInput> input)
    {
        foreach (var hint in _fields.Values.Where(e => e.Type is TU))
        {
            hint.InputFactory = input;
        }

        return this;
    }

    /// <summary>
    /// Sets the description text for the specified field.
    /// </summary>
    /// <param name="field">Expression identifying the field to configure.</param>
    /// <param name="description">The description text to display below the field.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Description(Expression<Func<TModel, object>> field, string description)
    {
        var hint = GetField(field);
        hint.Description = description;
        return this;
    }

    /// <summary>
    /// Sets a custom label for the specified field, overriding the automatically generated label.
    /// </summary>
    /// <param name="field">Expression identifying the field to configure.</param>
    /// <param name="label">The custom label text to display for the field.</param>
    /// <returns>The form builder instance for method chaining.</returns>
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

    /// <summary>
    /// Places the specified fields in the given column with automatic vertical ordering.
    /// </summary>
    /// <param name="col">The zero-based column index where the fields should be placed.</param>
    /// <param name="fields">The fields to place in the specified column.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Place(int col, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(col, null, fields);
    }

    /// <summary>
    /// Places the specified fields in the first column (column 0) with automatic vertical ordering.
    /// </summary>
    /// <param name="fields">The fields to place in the first column.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Place(params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, null, fields);
    }

    /// <summary>
    /// Places the specified fields with optional horizontal row grouping.
    /// </summary>
    /// <param name="row">Whether to group the fields in the same horizontal row.</param>
    /// <param name="fields">The fields to place, optionally in the same row.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Place(bool row, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(0, row ? Guid.NewGuid() : null, fields);
    }

    /// <summary>
    /// Places the specified fields in a specific column with optional horizontal row grouping.
    /// </summary>
    /// <param name="col">The zero-based column index where the fields should be placed.</param>
    /// <param name="row">Whether to group the fields in the same horizontal row.</param>
    /// <param name="fields">The fields to place in the specified column and optional row.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Place(int col, bool row, params Expression<Func<TModel, object>>[] fields)
    {
        return _Place(col, row ? Guid.NewGuid() : null, fields);
    }

    /// <summary>
    /// Groups the specified fields under a named section in the specified column.
    /// </summary>
    /// <param name="group">The name of the group for organizing related fields.</param>
    /// <param name="column">The column index where the grouped fields should be placed.</param>
    /// <param name="fields">The fields to include in the named group.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// Grouped fields are organized together with a group header and maintain their own ordering
    /// within the group. This is useful for creating logical sections in complex forms.
    /// </remarks>
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

    /// <summary>
    /// Groups the specified fields under a named section in the first column.
    /// </summary>
    /// <param name="group">The name of the group for organizing related fields.</param>
    /// <param name="fields">The fields to include in the named group.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Group(string group, params Expression<Func<TModel, object>>[] fields)
    {
        return Group(group, 0, fields);
    }

    /// <summary>
    /// Removes the specified fields from the form so they will not be rendered.
    /// </summary>
    /// <param name="fields">The fields to remove from the form.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// Removed fields are excluded from form rendering but their configurations are preserved,
    /// allowing them to be re-added later if needed.
    /// </remarks>
    public FormBuilder<TModel> Remove(params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var field in fields)
        {
            var hint = GetField(field);
            hint.Removed = true;
        }
        return this;
    }

    /// <summary>
    /// Adds a previously removed field back to the form.
    /// </summary>
    /// <param name="field">The field to add back to the form.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Add(Expression<Func<TModel, object>> field)
    {
        var hint = GetField(field);
        hint.Removed = false;
        return this;
    }

    /// <summary>
    /// Removes all fields from the form, creating a blank form that can be selectively populated.
    /// </summary>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// This method is useful for creating forms that only show specific fields by starting
    /// with a clean slate and then selectively adding back desired fields.
    /// </remarks>
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

    /// <summary>
    /// Sets a conditional visibility predicate for the specified field based on the current model state.
    /// </summary>
    /// <param name="field">The field to configure conditional visibility for.</param>
    /// <param name="predicate">A function that determines field visibility based on the current model state.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// The visibility predicate is evaluated whenever the model changes, allowing for dynamic
    /// form behavior where fields can appear or disappear based on other field values.
    /// </remarks>
    public FormBuilder<TModel> Visible(Expression<Func<TModel, object>> field, Func<TModel, bool> predicate)
    {
        var hint = GetField(field);
        hint.Visible = predicate;
        return this;
    }

    /// <summary>
    /// Sets the disabled state for the specified fields.
    /// </summary>
    /// <param name="disabled">Whether the fields should be disabled (read-only).</param>
    /// <param name="fields">The fields to enable or disable.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    public FormBuilder<TModel> Disabled(bool disabled, params Expression<Func<TModel, object>>[] fields)
    {
        foreach (var expr in fields)
        {
            var hint = GetField(expr);
            hint.Disabled = disabled;
        }
        return this;
    }

    /// <summary>
    /// Adds a custom validation rule to the specified field.
    /// </summary>
    /// <typeparam name="T">The type of the field value for type-safe validation.</typeparam>
    /// <param name="field">The field to add validation to.</param>
    /// <param name="validator">A function that validates the field value and returns a result and error message.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// Validation functions should return a tuple where the first value indicates whether
    /// the validation passed, and the second value provides an error message for display
    /// when validation fails.
    /// </remarks>
    public FormBuilder<TModel> Validate<T>(Expression<Func<TModel, object>> field, Func<T, (bool, string)> validator)
    {
        var hint = GetField(field);
        hint.Validators.Add((o) => validator((T)o!));
        return this;
    }

    /// <summary>
    /// Marks the specified fields as required, adding automatic required field validation.
    /// </summary>
    /// <param name="fields">The fields to mark as required.</param>
    /// <returns>The form builder instance for method chaining.</returns>
    /// <remarks>
    /// Required fields automatically receive validation that ensures they have a non-empty value
    /// before the form can be successfully submitted.
    /// </remarks>
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

    /// <summary>
    /// Creates a form instance with validation, data binding, and submission handling for use in custom layouts.
    /// </summary>
    /// <param name="context">The view context for state management and signal handling.</param>
    /// <returns>A tuple containing the submit handler, form view, validation view, and loading state.</returns>
    /// <remarks>
    /// This method provides the core form functionality for advanced scenarios where custom
    /// form layouts are needed. It handles field binding, validation coordination, and
    /// submission processing while allowing complete control over the form's visual presentation.
    /// </remarks>
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

    /// <summary>
    /// Builds the complete form with automatic layout, validation, and submission handling.
    /// </summary>
    /// <returns>A complete form widget with fields, validation messages, and submit button.</returns>
    /// <remarks>
    /// This method creates a standard form layout with all configured fields, validation display,
    /// and a submit button. It provides a complete form solution for most common scenarios.
    /// </remarks>
    public override object? Build()
    {
        (Func<Task<bool>> onSubmit, IView formView, IView validationView, bool submitting) = UseForm(this.Context);

        async ValueTask HandleSubmit()
        {
            await onSubmit();
        }

        return Layout.Vertical()
               | formView
               | Layout.Horizontal(new Button(SubmitTitle).HandleClick(HandleSubmit)
                   .Loading(submitting).Disabled(submitting), validationView);
    }
    private static string InvalidMessage(int invalidFields)
    {
        return invalidFields == 1 ? "There is 1 invalid field." : $"There are {invalidFields} invalid fields.";
    }
}