---
prepare: |
  var client = this.UseService<IClientProvider>();
searchHints:
  - input
  - validation
  - submission
  - fields
  - form
  - builder
---

# Forms

<Ingress>
Build robust forms with built-in state management, validation, and submission handling for collecting and processing user input.
</Ingress>

<Callout Type="important">
Do not manually create form layouts. Always use `.ToForm()` on your state objects for type safety, automatic state management, and built-in validation.
</Callout>

## Basic Usage

The simplest way to create a form is to call `.ToForm()` on a state object. The FormBuilder automatically scaffolds appropriate input fields based on your model's property types.

```csharp demo-tabs
public class BasicFormExample : ViewBase
{
    public record UserModel(string Name, string Email, bool IsActive, int Age);

    public override object? Build()
    {
        var user = UseState(() => new UserModel("", "", false, 25));
        
        return user.ToForm();
    }
}
```

### Automatic Field Generation

The FormBuilder automatically maps C# types to appropriate input widgets:

| C# Type | Generated Input | Notes |
|---------|----------------|-------|
| `string` | `TextInput` | Can be customized to email, password, textarea, etc. |
| `bool`, `bool?` | `BoolInput` | Checkbox or toggle |
| `int`, `decimal`, `double` | `NumberInput` | Number input with validation |
| `DateTime`, `DateOnly` | `DateTimeInput` | Date/time picker |
| `Enum` | `SelectInput` | Dropdown with enum values |
| `List<Enum>` | `SelectInput` with multiple selection | Multi-select dropdown |
| Properties ending in "Id" | `ReadOnlyInput` | Typically for system-generated IDs |
| Properties ending in "Email" | `TextInput` with email validation | Email-specific input |
| Properties ending in "Password" | `PasswordInput` | Hidden text input |

## Field Configuration

### Custom Labels and Descriptions

Use `.Label()`, `.Description()`, and `.Help()` to customize field appearance and provide help text.

```csharp demo-tabs
public class ConfiguredFormExample : ViewBase
{
    public record ContactModel(
        string Name,
        string Email,
        string Phone,
        string Message,
        bool Subscribe,
        Gender Gender = Gender.Other
    );

    public enum Gender { Male, Female, Other }

    public override object? Build()
    {
        var contact = UseState(() => new ContactModel("", "", "", "", false));
        
        return contact.ToForm()
            .Label(m => m.Name, "Full Name")
            .Description(m => m.Name, "Enter your full name as it appears on official documents")
            .Label(m => m.Email, "Email Address")
            .Help(m => m.Email, "We'll use this email to send you updates and important notifications")
            .Description(m => m.Email, "We'll use this to send you updates")
            .Label(m => m.Phone, "Phone Number")
            .Label(m => m.Message, "Your Message")
            .Required(m => m.Name, m => m.Email);
    }
}
```

### Custom Input Builders

Use `.Builder()` to specify custom input types for specific fields.

```csharp demo-tabs
public class CustomInputsExample : ViewBase
{
    public record ProductModel(
        string Name,
        string Description,
        decimal Price,
        string JsonConfig,
        List<string> Tags,
        DateTime ReleaseDate
    );

    public override object? Build()
    {
        var product = UseState(() => new ProductModel("", "", 0.0m, "{}", new(), DateTime.Now));
        
        // Create sample tag options for the multi-select
        var tagOptions = new[] { "Electronics", "Clothing", "Books", "Home", "Sports", "Food" }.ToOptions();
        
        return product.ToForm()
            .Builder(m => m.Description, s => s.ToTextAreaInput())
            .Builder(m => m.JsonConfig, s => s.ToCodeInput().Language(Languages.Json))
            .Builder(m => m.Tags, s => s.ToSelectInput(tagOptions))
            .Builder(m => m.ReleaseDate, s => s.ToDateTimeInput())
            .Label(m => m.Description, "Product Description")
            .Label(m => m.JsonConfig, "Configuration (JSON)")
            .Label(m => m.Tags, "Product Tags")
            .Label(m => m.ReleaseDate, "Release Date");
    }
}
```

### Required Fields

Mark fields as required using `.Required()` or rely on automatic detection from non-nullable types.

```csharp demo-tabs
public class RequiredFieldsExample : ViewBase
{
    public record OrderModel(
        string CustomerName,
        string? CustomerEmail, 
        string ShippingAddress,
        int Quantity,
        bool IsPriority
    );

    public override object? Build()
    {
        var order = UseState(() => new OrderModel("", null, "", 1, false));
        
        return order.ToForm()
            .Required(m => m.CustomerEmail) 
            .Required(m => m.IsPriority)    
            .Label(m => m.CustomerName, "Customer Name")
            .Label(m => m.CustomerEmail, "Email Address")
            .Label(m => m.ShippingAddress, "Shipping Address")
            .Help(m => m.ShippingAddress, "Enter the complete shipping address including street, city, and postal code")
            .Label(m => m.Quantity, "Quantity")
            .Label(m => m.IsPriority, "Priority Order");
    }
}
```

## Layout Control

### Field Placement

Control field placement using `.Place()` and `.PlaceHorizontal()` methods for custom layouts.

```csharp demo-tabs
public class LayoutControlExample : ViewBase
{
    public record AddressModel(
        string Street,
        string City,
        string State,
        string ZipCode,
        string Country
    );

    public override object? Build()
    {
        var address = UseState(() => new AddressModel("", "", "", "", ""));
        
        return address.ToForm()
            .Place(m => m.Street)                    // Single field spans full width
            .PlaceHorizontal(m => m.City, m => m.State)  // Two fields side-by-side, sharing row width
            .PlaceHorizontal(m => m.ZipCode, m => m.Country) // Two fields side-by-side, sharing row width
            .Label(m => m.Street, "Street Address")
            .Label(m => m.City, "City")
            .Label(m => m.State, "State/Province")
            .Label(m => m.ZipCode, "ZIP/Postal Code")
            .Label(m => m.Country, "Country");
    }
}
```

### Grouped Fields

Organize related fields into logical groups using `.Group()`.

```csharp demo-tabs
public class GroupedFieldsExample : ViewBase
{
    public record EmployeeModel(
        string FirstName,
        string LastName,
        string Email,
        string Department,
        decimal Salary,
        DateTime HireDate,
        string Street,
        string City,
        string State
    );

    public override object? Build()
    {
        var employee = UseState(() => new EmployeeModel("", "", "", "", 0.0m, DateTime.Now, "", "", ""));
        
        return employee.ToForm()
            .Group("Personal Information", m => m.FirstName, m => m.LastName, m => m.Email)
            .Group("Employment", m => m.Department, m => m.Salary, m => m.HireDate)
            .Group("Address", m => m.Street, m => m.City, m => m.State)
            .Label(m => m.FirstName, "First Name")
            .Label(m => m.LastName, "Last Name")
            .Label(m => m.Email, "Email Address")
            .Label(m => m.Department, "Department")
            .Label(m => m.Salary, "Annual Salary")
            .Label(m => m.HireDate, "Hire Date")
            .Label(m => m.Street, "Street Address")
            .Label(m => m.City, "City")
            .Label(m => m.State, "State");
    }
}
```

### Field Ordering and Removal

Control which fields are shown and in what order using `.Clear()`, `.Add()`, and `.Remove()`.

```csharp demo-tabs
public class FieldManagementExample : ViewBase
{
    public record ProductModel(
        string Name,
        string Description,
        decimal Price,
        string Category,
        int Stock,
        string SKU,
        DateTime CreatedDate
    );

    public override object? Build()
    {
        var product = UseState(() => new ProductModel("", "", 0.0m, "", 0, "", DateTime.Now));
        
        return product.ToForm()
            .Clear()                                    // Hide all auto-generated fields
            .Add(m => m.Name)                          // Show Name first
            .Add(m => m.Description)                   // Show Description second
            .Add(m => m.Price)                         // Show Price third
            .Add(m => m.Category)                      // Show Category fourth
            .Add(m => m.Stock)                         // Show Stock last
            .Remove(m => m.SKU)                        // Hide SKU field
            .Remove(m => m.CreatedDate)                // Hide CreatedDate field
            .Label(m => m.Name, "Product Name")
            .Label(m => m.Description, "Product Description")
            .Label(m => m.Price, "Unit Price")
            .Label(m => m.Category, "Product Category")
            .Label(m => m.Stock, "Available Stock");
    }
}
```

## Validation

### Custom Submit Text

Change the text of the submit button by passing it as a parameter of the `.ToForm()` method

```csharp demo-tabs
public class CustomSubmitTitleFormExample : ViewBase
{
    public record UserModel(string Name, string Email, bool IsActive, int Age);

    public override object? Build()
    {
        var user = UseState(() => new UserModel("", "", false, 25));
        
        return user.ToForm("Create new user");
    }
}
```

### Form Validation

Forms support automatic validation using standard .NET DataAnnotations, with the ability to add custom validation logic for specific business rules. Validation errors appear when you try to submit the form.

```csharp demo-tabs
public class ValidationExample : ViewBase
{
    public class UserModel
    {
        [Required, MinLength(3)]
        public string Username { get; set; } = "";
        
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        
        [Required, MinLength(8)]
        public string Password { get; set; } = "";
        
        [Range(13, 120)]
        public int Age { get; set; } = 18;
        
        public DateTime BirthDate { get; set; } = DateTime.Now;
    }

    public override object? Build()
    {
        var user = UseState(() => new UserModel());
        var client = UseService<IClientProvider>();
        
        UseEffect(() =>
        {
            // This only fires when the form is submitted successfully (passes validation)
            if (!string.IsNullOrEmpty(user.Value.Username))
            {
                client.Toast($"Account created for {user.Value.Username}!");
            }
        }, user);
        
        return user.ToForm("Create Account")
            // Custom validation: birth date cannot be in the future
            .Validate<DateTime>(m => m.BirthDate, birthDate => 
                (birthDate <= DateTime.Now, "Birth date cannot be in the future"))
            // Custom validation: username cannot contain spaces
            .Validate<string>(m => m.Username, username =>
                (!username.Contains(' '), "Username cannot contain spaces"));
    }
}
```

This example demonstrates:
- **DataAnnotations** for standard validation (Required, MinLength, EmailAddress, Range)
- **Custom `.Validate()`** for business logic that operates on individual field values

<Callout Type="tip">
**Automatic Email Validation**: Fields ending with "Email" (like `UserEmail`, `ContactEmail`) automatically get email validation, even without the `[EmailAddress]` attribute.
</Callout>

**Supported DataAnnotations:**

- `[Required]` - Field must have a value
- `[MinLength(n)]` - Minimum string length
- `[MaxLength(n)]` - Maximum string length
- `[StringLength(max, MinimumLength = min)]` - String length constraints
- `[Length(min, max)]` - Length constraints for strings and collections
- `[Range(min, max)]` - Value must be within range
- `[EmailAddress]` - Valid email format
- `[Phone]` - Valid phone number format
- `[Url]` - Valid URL format
- `[CreditCard]` - Valid credit card number format
- `[RegularExpression(pattern)]` - Match a regex pattern
- `[AllowedValues(...)]` - Value must be from specified list
- `[DataType(...)]` - Specifies data type (Password, Date, DateTime, MultilineText, etc.)
- `[Display(...)]` - Controls field display properties (Name, Description, Order, GroupName, Prompt)

### Display Attributes

The `[Display]` attribute provides powerful control over how fields appear in your forms without requiring additional configuration code.

```csharp demo-tabs
public class DisplayAttributeExample : ViewBase
{
    public class UserRegistrationModel
    {
        [Display(Name = "Full Name", Description = "Enter your complete legal name", Order = 1)]
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2)]
        public string Name { get; set; } = "";

        [Display(Name = "Email Address", Description = "We'll use this for account verification", Order = 2)]
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Display(Name = "Phone Number", Description = "For account security purposes", Prompt = "+1-234-567-8900", Order = 3)]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(GroupName = "Account Security", Name = "Password", Order = 4)]
        [Required]
        [StringLength(100, MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Display(GroupName = "Account Security", Name = "Confirm Password", Order = 5)]
        [Required]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        [Display(GroupName = "Preferences", Name = "Newsletter Subscription", Description = "Receive weekly updates", Order = 6)]
        public bool SubscribeToNewsletter { get; set; } = false;

        [Display(GroupName = "Preferences", Name = "Preferred Theme", Order = 7)]
        [AllowedValues("Light", "Dark", "Auto")]
        public string Theme { get; set; } = "Auto";
    }

    public override object? Build()
    {
        var user = UseState(() => new UserRegistrationModel());
        var client = UseService<IClientProvider>();
        
        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(user.Value.Name))
            {
                client.Toast($"Registration completed for {user.Value.Name}!");
            }
        }, user);

        var themeOptions = new[] { "Light", "Dark", "Auto" }.ToOptions();
        
        return user.ToForm("Create Account")
            .Builder(m => m.Theme, s => s.ToSelectInput(themeOptions))
            .Builder(m => m.Password, s => s.ToPasswordInput())
            .Builder(m => m.ConfirmPassword, s => s.ToPasswordInput())
            .Validate<string>(m => m.ConfirmPassword, confirmPassword =>
                (confirmPassword == user.Value.Password, "Passwords must match"));
    }
}
```

The `[Display]` attribute supports these properties:
- **Name**: Custom field label (overrides automatic label generation)
- **Description**: Help text shown below the field
- **Order**: Field ordering (lower numbers appear first)
- **GroupName**: Groups related fields together
- **Prompt**: Placeholder text for input fields

## Form Submission

### Basic Form Submission

Forms automatically handle submission when the user presses Enter or clicks the built-in submit button. When a form is submitted successfully, it updates the model state, which triggers any `UseEffect` watching that state.

```csharp demo-tabs
public class FormSubmissionExample : ViewBase
{
    public record ContactModel(string Name, string Email, string Message);

    public override object? Build()
    {
        var contact = UseState(() => new ContactModel("", "", ""));
        var client = UseService<IClientProvider>();
        
        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(contact.Value.Name))
            {
                client.Toast($"Message from {contact.Value.Name} sent successfully!");
            }
        }, contact);
        
        return contact.ToForm()
            .Required(m => m.Name, m => m.Email, m => m.Message);
    }
}
```

### Form Submission with State Updates

React to form submission by watching the model state with `UseEffect`. The form automatically updates the state when submitted successfully.

```csharp demo-tabs
public class FormStatesExample : ViewBase
{
    public record OrderModel(
        string CustomerName,
        string ProductName,
        int Quantity,
        decimal UnitPrice
    );

    public override object? Build()
    {
        var order = UseState(() => new OrderModel("", "", 1, 0.0m));
        var client = UseService<IClientProvider>();
        
        UseEffect(() =>
        {
            if (!string.IsNullOrEmpty(order.Value.CustomerName))
            {
                var total = order.Value.Quantity * order.Value.UnitPrice;
                client.Toast($"Order created! Total: ${total:F2}");
            }
        }, order);
        
        return Layout.Vertical()
            | order.ToForm()
                .Required(m => m.CustomerName, m => m.ProductName, m => m.Quantity, m => m.UnitPrice)
            | Text.Block($"Total: ${order.Value.Quantity * order.Value.UnitPrice:F2}");
    }
}
```

### Form Re-rendering

Demonstrates how to update form state and trigger re-renders:

```csharp demo-tabs
public class SimpleFormWithResetExample : ViewBase
{
    public record MyModel(string Name, string Email, int Age);

    public override object? Build()
    {
        // Create the state for your model
        var model = UseState(() => new MyModel("", "", 0));
        
        // Create a form from the state
        var form = model.ToForm()
            .Label(m => m.Name, "Full Name")
            .Label(m => m.Email, "Email Address")
            .Label(m => m.Age, "Age");
        
        // To update the model and trigger re-render, you MUST use Set()
        UseEffect(async () =>
        {
            // Example: Load data after 2 seconds
            await Task.Delay(2000);
            // CORRECT: This will trigger form re-render
            model.Set(new MyModel("John Doe", "john@example.com", 30));
        });
        
        return Layout.Vertical()
            | form
            | (Layout.Horizontal()
                | new Button("Update Model", _ => {
                    model.Set(new MyModel("Jane Doe", "jane@example.com", 25));
                })
                | new Button("Reset Form", _ => {
                    model.Set(new MyModel("", "", 0));
                }))
            | Text.Block($"Current: {model.Value.Name} ({model.Value.Email}) - Age: {model.Value.Age}");
    }
}
```

<Callout Type="warning">
This example works because it uses the form's internal state management. The form maintains its own copy of the data until submission, so programmatic updates using `.Set()` will be reflected in the form fields.
</Callout>

## Advanced Features

### Conditional Fields

Show or hide fields based on other field values using `.Visible()`.

```csharp demo-tabs
public class ConditionalFieldsExample : ViewBase
{
    public record AccountModel(
        string Email,
        string Password,
        bool HasTwoFactor,
        string PhoneNumber,
        bool IsBusinessAccount,
        string CompanyName,
        string TaxId
    );

    public override object? Build()
    {
        var account = UseState(() => new AccountModel("", "", false, "", false, "", ""));
        
        return account.ToForm()
            .Visible(m => m.PhoneNumber, m => m.HasTwoFactor)
            .Visible(m => m.CompanyName, m => m.IsBusinessAccount)
            .Visible(m => m.TaxId, m => m.IsBusinessAccount)
            .Label(m => m.Email, "Email Address")
            .Label(m => m.Password, "Password")
            .Label(m => m.HasTwoFactor, "Enable Two-Factor Authentication")
            .Label(m => m.PhoneNumber, "Phone Number (for 2FA)")
            .Label(m => m.IsBusinessAccount, "Business Account")
            .Label(m => m.CompanyName, "Company Name")
            .Label(m => m.TaxId, "Tax ID")
            .Required(m => m.Email, m => m.Password);
    }
}
```

### Dynamic Field Configuration

Configure fields based on runtime conditions.

```csharp demo-tabs
public class DynamicConfigurationExample : ViewBase
{
    public record UserModel(
        string Username,
        string Email,
        string Password,
        bool IsAdmin,
        string Role
    );

    public override object? Build()
    {
        var user = UseState(() => new UserModel("", "", "", false, ""));
        var isEditMode = UseState(false);
        var currentUser = UseState(() => new UserModel("admin", "admin@example.com", "", true, "Admin"));
        
        // Build the form with conditional field visibility instead of removal
        var form = user.ToForm()
            .Visible(m => m.Username, m => !isEditMode.Value) // Hide username in edit mode
            .Visible(m => m.IsAdmin, m => currentUser.Value.IsAdmin) // Only show admin field to admins
            .Visible(m => m.Role, m => currentUser.Value.IsAdmin) // Only show role field to admins
            .Required(m => m.Email, m => m.Password);
        
        return Layout.Vertical()
            | (Layout.Horizontal()
                | new Button("New User").HandleClick(_ => isEditMode.Set(false))
                | new Button("Edit User").HandleClick(_ => isEditMode.Set(true)))
            | form
            | (Layout.Horizontal()
                | new Button(isEditMode.Value ? "Update User" : "Create User")
                | new Button("Cancel").Variant(ButtonVariant.Outline));
    }
}
```

## Forms in UI Components

### Sheet Forms

Open forms in slide-out sheets using `.ToSheet()`.

```csharp demo-tabs
public class SheetFormExample : ViewBase
{
    public record ProductModel(
        string Name,
        string Description,
        decimal Price,
        string Category
    );

    public override object? Build()
    {
        var product = UseState(() => new ProductModel("", "", 0.0m, ""));
        var isSheetOpen = UseState(false);
        
        return Layout.Vertical()
            | new Button("Add New Product").HandleClick(_ => isSheetOpen.Set(true))
            | product.ToForm()
                .Required(m => m.Name, m => m.Price, m => m.Category)
                .ToSheet(isSheetOpen, "New Product", "Fill in the product details below");
    }
}
```

### Dialog Forms

Open forms in modal dialogs using `.ToDialog()`.

```csharp demo-tabs
public class DialogFormExample : ViewBase
{
    public record UserModel(
        string FirstName,
        string LastName,
        string Email,
        string Department
    );

    public override object? Build()
    {
        var user = UseState(() => new UserModel("", "", "", ""));
        var isDialogOpen = UseState(false);
        
        return Layout.Vertical()
            | new Button("Create User").HandleClick(_ => isDialogOpen.Set(true))
            | user.ToForm()
                .Required(m => m.FirstName, m => m.LastName, m => m.Email)
                .ToDialog(isDialogOpen, "Create New User", "Please provide user information", 
                         width: Size.Units(125));
    }
}
```

## Migration from Manual Forms

If you're currently using manual form layouts, migrate to `.ToForm()`:

```csharp
// ❌ Don't do this - manual form layout
var form = new Form(
    new TextInput(name),
    new TextInput(email),
    new Button("Submit")
);

// ✅ Do this instead - use .ToForm()
var model = UseState(() => new UserModel("", ""));
return model.ToForm()
    .Required(m => m.Name, m => m.Email);
```

<Callout Type="warning">
Avoid manually creating form layouts. Always use `.ToForm()` on your state objects for better state management, validation, and type safety.
</Callout>

<WidgetDocs Type="Ivy.Form" ExtensionTypes="Ivy.Views.Forms.FormsExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Forms/Form.cs"/>

## Examples

<Details>
<Summary>
Forms with Data Tables
</Summary>
<Body>
Combine forms with data tables for CRUD operations.

```csharp demo-tabs
public class CrudFormExample : ViewBase
{
    public record ProductModel(
        string Name,
        string Description,
        decimal Price,
        string Category
    );

    public override object? Build()
    {
        var products = UseState(() => new ProductModel[0]);
        var selectedProduct = UseState<ProductModel?>(() => null);
        var editingProduct = UseState<ProductModel?>(() => null);
        var isCreateDialogOpen = UseState(false);
        var isEditDialogOpen = UseState(false);
        
        var addProduct = (Event<Button> e) =>
        {
            editingProduct.Set(new ProductModel("", "", 0.0m, ""));
            isCreateDialogOpen.Set(true);
        };
        
        var editProduct = (Event<Button> e) =>
        {
            if (selectedProduct.Value != null)
            {
                editingProduct.Set(selectedProduct.Value);
                isEditDialogOpen.Set(true);
            }
        };
        
        var deleteProduct = (Event<Button> e) =>
        {
            if (selectedProduct.Value != null)
            {
                var updatedProducts = products.Value.Where(p => p != selectedProduct.Value).ToArray();
                products.Set(updatedProducts);
                selectedProduct.Set((ProductModel?)null);
            }
        };
        
        // Create dialog content for new product
        var createDialog = isCreateDialogOpen.Value && editingProduct.Value != null
            ? new Dialog(
                _ => isCreateDialogOpen.Set(false),
                new DialogHeader("Create New Product"),
                new DialogBody(
                    Layout.Vertical()
                        | Text.Block("Fill in the product details")
                        | editingProduct.ToForm()
                            .Required(m => m.Name, m => m.Price, m => m.Category)
                ),
                new DialogFooter(
                    new Button("Cancel", _ => isCreateDialogOpen.Set(false), variant: ButtonVariant.Outline),
                    new Button("Create Product", _ => {
                        if (editingProduct.Value != null)
                        {
                            var updatedProducts = products.Value.Append(editingProduct.Value).ToArray();
                            products.Set(updatedProducts);
                            isCreateDialogOpen.Set(false);
                        }
                    })
                )
            ).Width(Size.Units(125))
            : null;
        
        // Create dialog content for editing product
        var editDialog = isEditDialogOpen.Value && editingProduct.Value != null
            ? new Dialog(
                _ => isEditDialogOpen.Set(false),
                new DialogHeader("Edit Product"),
                new DialogBody(
                    Layout.Vertical()
                        | Text.Block("Update product information")
                        | editingProduct.ToForm()
                            .Required(m => m.Name, m => m.Price, m => m.Category)
                ),
                new DialogFooter(
                    new Button("Cancel", _ => isEditDialogOpen.Set(false), variant: ButtonVariant.Outline),
                    new Button("Update Product", _ => {
                        if (editingProduct.Value != null && selectedProduct.Value != null)
                        {
                            var updatedProducts = products.Value.Select(p => 
                                p == selectedProduct.Value ? editingProduct.Value : p).ToArray();
                            products.Set(updatedProducts);
                            selectedProduct.Set(editingProduct.Value);
                            isEditDialogOpen.Set(false);
                        }
                    })
                )
            ).Width(Size.Units(125))
            : null;
        
        return Layout.Vertical()
            | (Layout.Horizontal()
                | new Button("Add Product", addProduct)
                | new Button("Edit Product", editProduct)
                    .Disabled(selectedProduct.Value == null)
                | new Button("Delete Product", deleteProduct)
                    .Disabled(selectedProduct.Value == null)
                    .Variant(ButtonVariant.Destructive))
            | (Layout.Vertical()
                | Text.Block("Select a product to edit/delete:")
                | new SelectInput<ProductModel?>(
                    selectedProduct.Value,
                    e => selectedProduct.Set(e.Value),
                    products.Value.ToOptions()
                ).Placeholder("Choose a product..."))
            | products.Value.ToTable()
                .Width(Size.Full())
                .Builder(p => p.Name, f => f.Default())
                .Builder(p => p.Description, f => f.Text())
                .Builder(p => p.Price, f => f.Default())
                .Builder(p => p.Category, f => f.Default())
            | (selectedProduct.Value != null ? 
                Text.Block($"Selected: {selectedProduct.Value.Name} (${selectedProduct.Value.Price})")
                : Text.Block("No product selected"))
            | createDialog!
            | editDialog!;
    }
}
```

</Body>
</Details>

<Details>
<Summary>
Forms with Real-time Updates
</Summary>
<Body>
Forms automatically update state, enabling real-time UI updates.

```csharp demo-tabs
public class RealTimeFormExample : ViewBase
{
    public record CalculatorModel(
        decimal Number1,
        decimal Number2,
        string Operation
    );

    public override object? Build()
    {
        var calculator = UseState(() => new CalculatorModel(0, 0, "+"));
        
        decimal CalculateResult()
        {
            return calculator.Value.Operation switch
            {
                "+" => calculator.Value.Number1 + calculator.Value.Number2,
                "-" => calculator.Value.Number1 - calculator.Value.Number2,
                "*" => calculator.Value.Number1 * calculator.Value.Number2,
                "/" => calculator.Value.Number2 != 0 ? calculator.Value.Number1 / calculator.Value.Number2 : 0,
                _ => 0
            };
        }
        
        // Create options for the operation select input
        var operationOptions = new[] { "+", "-", "*", "/" }.ToOptions();
        
        return Layout.Horizontal()
            | new Card(
                calculator.ToForm()
                    .Builder(m => m.Operation, s => s.ToSelectInput(operationOptions))
                    .Label(m => m.Number1, "First Number")
                    .Label(m => m.Number2, "Second Number")
                    .Label(m => m.Operation, "Operation")
            ).Title("Calculator").Width(1/2f)
            | new Card(
                Layout.Vertical()
                    | Text.H3("Result")
                    | Text.Block($"{calculator.Value.Number1} {calculator.Value.Operation} {calculator.Value.Number2} = {CalculateResult()}")
            ).Title("Result").Width(1/2f);
    }
}
```

</Body>
</Details>
