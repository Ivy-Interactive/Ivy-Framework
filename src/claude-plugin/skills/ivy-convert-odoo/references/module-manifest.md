# Module Manifest

Odoo's `__manifest__.py` file defines module metadata, dependencies, data files, and assets. Every Odoo module requires this file to declare what models, views, security rules, and demo data the module provides.

## Odoo

```python
# __manifest__.py
{
    'name': 'Sales',
    'version': '17.0.1.0.0',
    'category': 'Sales/Sales',
    'summary': 'From quotations to invoices',
    'description': """
        Manage your sales pipeline, quotations, and orders.
    """,
    'depends': ['base', 'mail', 'account', 'product'],
    'data': [
        # Security
        'security/sale_security.xml',
        'security/ir.model.access.csv',

        # Views
        'views/sale_order_views.xml',
        'views/sale_order_line_views.xml',
        'views/res_partner_views.xml',

        # Menu
        'views/sale_menus.xml',

        # Reports
        'report/sale_report_templates.xml',
        'report/sale_report.xml',

        # Data
        'data/sale_data.xml',
        'data/mail_template_data.xml',

        # Wizards
        'wizard/sale_make_invoice_advance_views.xml',
    ],
    'demo': [
        'demo/sale_demo.xml',
    ],
    'assets': {
        'web.assets_backend': [
            'sale/static/src/js/**/*',
            'sale/static/src/css/**/*',
            'sale/static/src/xml/**/*',
        ],
    },
    'installable': True,
    'application': True,
    'auto_install': False,
    'license': 'LGPL-3',
    'sequence': 5,
}
```

```
# Typical module directory structure
sale/
├── __init__.py
├── __manifest__.py
├── models/
│   ├── __init__.py
│   ├── sale_order.py
│   └── sale_order_line.py
├── views/
│   ├── sale_order_views.xml
│   └── sale_menus.xml
├── security/
│   ├── sale_security.xml
│   └── ir.model.access.csv
├── report/
│   ├── sale_report.xml
│   └── sale_report_templates.xml
├── wizard/
│   └── sale_make_invoice_advance_views.xml
├── data/
│   └── sale_data.xml
├── demo/
│   └── sale_demo.xml
└── static/
    └── description/
        └── icon.png
```

## Ivy

```csharp
// Module manifest → Ivy project structure
// Odoo modules map to pages/features within an Ivy app

// Ivy project structure equivalent:
// MyApp/
// ├── MyApp.csproj            ← project file (dependencies)
// ├── Program.cs              ← app entry point
// ├── Pages/
// │   ├── SalesPage.cs        ← views + menus
// │   ├── QuotationsPage.cs
// │   └── CustomersPage.cs
// ├── Models/
// │   ├── SaleOrder.cs        ← data models
// │   └── SaleOrderLine.cs
// ├── Services/
// │   ├── SaleService.cs      ← business logic
// │   └── ReportService.cs
// └── Data/
//     └── AppDbContext.cs      ← database context

// Dependencies (equivalent to 'depends' in manifest)
// In .csproj file:
// <PackageReference Include="Ivy.Framework" Version="1.0.0" />
// <ProjectReference Include="../Shared/Shared.csproj" />

// Data loading (equivalent to 'data' files)
// Seed data in DbContext or migration:
// protected override void OnModelCreating(ModelBuilder modelBuilder)
// {
//     modelBuilder.Entity<MailTemplate>().HasData(
//         new MailTemplate { Id = 1, Name = "Sale Reminder", ... }
//     );
// }

// Security definitions (equivalent to security/ files)
// Role-based authorization in service configuration:
// services.AddAuthorization(options =>
// {
//     options.AddPolicy("SalesManager",
//         policy => policy.RequireRole("SalesManager"));
//     options.AddPolicy("Salesperson",
//         policy => policy.RequireRole("Salesperson"));
// });

// Module registration (equivalent to 'application': True)
// Pages are registered in the app configuration
// and appear in the sidebar navigation automatically.
```

## Parameters

| Odoo Element | Description | Ivy Equivalent |
|---|---|---|
| `__manifest__.py` | Module definition file | `.csproj` project file |
| `name` | Module display name | Project/assembly name |
| `version` | Module version | Assembly version in `.csproj` |
| `depends` | Module dependencies | `PackageReference` / `ProjectReference` in `.csproj` |
| `data` | XML/CSV data files to load | EF migrations, seed data, or config files |
| `demo` | Demo/sample data | Seed data for development |
| `assets` | JavaScript/CSS/XML frontend assets | Static files or bundled frontend resources |
| `installable` | Can be installed | Project is buildable and deployable |
| `application` | Top-level app (shows in app drawer) | Standalone page or app section |
| `auto_install` | Install when dependencies met | Implicit via project references |
| `category` | Module categorization | Project folder organization |
| `license` | Software license | License field in `.csproj` |
| `models/` directory | Python model definitions | `Models/` directory with C# entity classes |
| `views/` directory | XML view definitions | `Pages/` directory with Ivy page classes |
| `security/` directory | ACL and record rules | Authorization policies in service config |
| `report/` directory | QWeb report templates | `Services/ReportService.cs` or report pages |
