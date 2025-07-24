---
prepare: |
  var client = this.UseService<IClientProvider>();
  
  public enum Gender
  {
      Male,
      Female,
      Other
  }

  public enum Fruits
  {
      Banana,
      Apple,
      Orange,
      Pear,
      Strawberry
  }

  public record UserModel(
      string Name, 
      string Password, 
      bool IsAwesome, 
      DateTime BirthDate, 
      int Height, 
      int UserId = 123, 
      Gender Gender = Gender.Male, 
      string Json = "{}", 
      List<Fruits> FavoriteFruits = null!
  );
---

# Form

The Form system in Ivy provides a powerful and flexible way to create data entry forms with automatic scaffolding, validation, and layout management. It supports type-safe binding to model objects with automatic input generation based on property types.

## Basic Usage

The simplest way to create a form is using the `ToForm()` extension method on a state object:

```csharp demo-below
var model = UseState(() => new UserModel("John Doe", "password123", true, DateTime.Parse("1990-01-01"), 180));

model.ToForm()
```

## Form Builder

The `FormBuilder<T>` class provides extensive customization options for forms:

### Custom Labels and Descriptions

```csharp demo-below
var model = UseState(() => new UserModel("Jane Smith", "mypassword", false, DateTime.Parse("1985-05-15"), 165));

model.ToForm()
    .Label(m => m.Name, "Full Name")
    .Description(m => m.Name, "Please enter your complete name")
    .Label(m => m.Height, "Height (cm)")
    .Description(m => m.Height, "Your height in centimeters")
```

### Custom Input Builders

You can override the default input types for specific fields:

```csharp demo-below
var model = UseState(() => new UserModel("Bob Wilson", "secret", true, DateTime.Parse("1992-03-20"), 175));

model.ToForm()
    .Builder(m => m.IsAwesome, s => s.ToBoolInput().Description("Are you awesome?"))
    .Builder(m => m.Gender, s => s.ToSelectInput())
    .Builder(m => m.Json, s => s.ToCodeInput().Language(Languages.Json))
```

### Field Layout and Organization

#### Basic Field Placement

```csharp demo-below
var model = UseState(() => new UserModel("Alice Johnson", "password", true, DateTime.Parse("1988-12-10"), 170));

model.ToForm()
    .Place(m => m.Name, m => m.Gender)  // First row
    .Place(m => m.Height, m => m.BirthDate)  // Second row
    .Remove(m => m.Password, m => m.Json)  // Remove unwanted fields
```

#### Column Layout

```csharp demo-below
var model = UseState(() => new UserModel("Charlie Brown", "pass123", false, DateTime.Parse("1995-07-08"), 178));

model.ToForm()
    .Place(0, m => m.Name, m => m.Height)  // Column 0
    .Place(1, m => m.Gender, m => m.BirthDate)  // Column 1
    .Remove(m => m.Password, m => m.Json, m => m.IsAwesome)
```

#### Grouping Fields

```csharp demo-below
var model = UseState(() => new UserModel("Diana Prince", "wonderwoman", true, DateTime.Parse("1984-06-01"), 168));

model.ToForm()
    .Group("Personal Info", m => m.Name, m => m.Gender, m => m.BirthDate)
    .Group("Physical Info", m => m.Height)
    .Remove(m => m.Password, m => m.Json, m => m.UserId)
```

### Validation

Forms support custom validation with automatic error display:

```csharp demo
var model = UseState(() => new UserModel("", "123", false, DateTime.Parse("2010-01-01"), 50));

model.ToForm()
    .Required(m => m.Name)
    .Validate<string>(m => m.Name, name => 
        name.Length >= 2 ? (true, "") : (false, "Name must be at least 2 characters"))
    .Validate<string>(m => m.Password, pwd => 
        pwd.Length >= 6 ? (true, "") : (false, "Password must be at least 6 characters"))
    .Validate<int>(m => m.Height, height => 
        height >= 100 && height <= 250 ? (true, "") : (false, "Height must be between 100-250 cm"))
    .Remove(m => m.Json, m => m.UserId)
```

### Conditional Field Visibility

You can show or hide fields based on other field values:

```csharp demo
var model = UseState(() => new UserModel("Test User", "password", false, DateTime.Parse("1990-01-01"), 175));

model.ToForm()
    .Visible(m => m.Height, m => m.IsAwesome)  // Only show height if user is awesome
    .Remove(m => m.Password, m => m.Json, m => m.UserId)
```

## Form Integration with UI Components

### Form in Dialog

```csharp demo
var model = UseState(() => new UserModel("Modal User", "password", true, DateTime.Parse("1987-04-12"), 172));
var isDialogOpen = UseState(false);

Layout.Vertical()
    | new Button("Open Form Dialog", _ => isDialogOpen.Set(true))
    | model.ToForm()
        .Remove(m => m.Password, m => m.Json, m => m.UserId)
        .ToDialog(isDialogOpen, "Edit User", "Please update the user information.", width: Size.Units(400))
```

### Form in Sheet

```csharp demo
var model = UseState(() => new UserModel("Sheet User", "password", false, DateTime.Parse("1993-09-25"), 168));
var isSheetOpen = UseState(false);

Layout.Vertical()
    | new Button("Open Form Sheet", _ => isSheetOpen.Set(true))
    | model.ToForm()
        .Remove(m => m.Password, m => m.Json, m => m.UserId)
        .ToSheet(isSheetOpen, "User Information", "Fill in your details below.")
```

## Advanced Features

### Automatic Type Scaffolding

The form system automatically generates appropriate input types based on property types:

- `string` → TextInput
- `bool` → BoolInput  
- `int`, `double`, `decimal` → NumberInput
- `DateTime` → DateTimeInput
- `Enum` → SelectInput
- Properties ending with "Id" → ReadOnlyInput
- Properties ending with "Email" → EmailInput (TextInput with email validation)
- Properties ending with "Password" → PasswordInput
- `List<Enum>` → Multi-select SelectInput

### Form State Management

```csharp demo
var model = UseState(() => new UserModel("State Demo", "password", true, DateTime.Parse("1991-11-30"), 175));

Layout.Horizontal()
    | new Card(
        model.ToForm()
            .Remove(m => m.Password, m => m.Json, m => m.UserId)
    ).Title("Edit Form").Width(1/2f)
    | new Card(
        model.ToDetails()
    ).Title("Current Values").Width(1/2f)
```

## Form Builder Methods Reference

### Configuration Methods
- `.Label(field, label)` - Set custom field label
- `.Description(field, description)` - Set field description
- `.Builder(field, inputFactory)` - Override input type for specific field
- `.Required(fields...)` - Mark fields as required
- `.Validate<T>(field, validator)` - Add custom validation
- `.Visible(field, predicate)` - Control field visibility

### Layout Methods
- `.Place(fields...)` - Place fields in order
- `.Place(column, fields...)` - Place fields in specific column
- `.Place(row: true, fields...)` - Place fields in same row
- `.Group(groupName, fields...)` - Group fields under expandable section
- `.Remove(fields...)` - Remove fields from form
- `.Clear()` - Remove all fields

### Integration Methods
- `.ToDialog(isOpen, title, description, width)` - Show form in dialog
- `.ToSheet(isOpen, title, description, width)` - Show form in sheet

<WidgetDocs Type="Ivy.Form" ExtensionTypes="Ivy.Views.Forms.FormExtensions;Ivy.Views.Forms.UseFormExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Forms/Form.cs"/>
