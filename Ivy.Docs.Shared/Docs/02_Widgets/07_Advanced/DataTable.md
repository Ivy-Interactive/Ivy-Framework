---
prepare: |
  var firstNames = new[] { "John", "Sarah", "Mike", "Emily", "Alex", "Lisa", "David", "Jessica", "Robert", "Amanda", "Kevin", "Michelle", "Christopher", "Jennifer", "Daniel", "Nicole", "Matthew", "Stephanie", "Andrew", "Rachel", "James", "Patricia", "Thomas", "Barbara", "Charles", "Susan", "Joseph", "Linda", "Paul", "Karen", "Mark", "Betty", "Donald", "Helen", "Steven", "Dorothy", "Kenneth", "Sandra", "Brian", "Ashley", "Edward", "Kimberly", "Ronald", "Donna", "Anthony", "Carol", "Ruth", "Jason", "Sharon", "Nancy", "Larry", "Karen", "Frank", "Diane", "Carl", "Janet", "Gerald", "Judith", "Harold", "Teresa", "Dennis", "Pamela", "Eugene", "Gloria", "Arthur", "Doris", "Ralph", "Cheryl", "Russell", "Mildred", "Henry", "Katherine", "Willie", "Joan", "Albert", "Evelyn", "Howard", "Virginia", "Craig", "Melissa", "Alan", "Debra", "Louis", "Rebecca", "Billy", "Laura", "Terry", "Anna", "Sean", "Marie", "Joe", "Frances", "Carl", "Ann" };
  var lastNames = new[] { "Smith", "Johnson", "Brown", "Davis", "Wilson", "Chen", "Miller", "Taylor", "Garcia", "White", "Lee", "Rodriguez", "Martinez", "Lopez", "Anderson", "Thompson", "Jackson", "Harris", "Clark", "Lewis", "Walker", "Hall", "Allen", "Young", "King", "Wright", "Scott", "Green", "Baker", "Adams", "Nelson", "Carter", "Mitchell", "Perez", "Roberts", "Turner", "Phillips", "Campbell", "Parker", "Evans", "Edwards", "Collins", "Stewart", "Sanchez", "Morris", "Rogers", "Reed", "Cook", "Morgan", "Bell", "Murphy", "Bailey", "Rivera", "Cooper", "Richardson", "Cox", "Howard", "Ward", "Torres", "Peterson", "Gray", "Ramirez", "James", "Watson", "Brooks", "Kelly", "Sanders", "Price", "Bennett", "Wood", "Barnes", "Ross", "Henderson", "Coleman", "Jenkins", "Perry", "Powell", "Long", "Patterson", "Hughes", "Flores", "Washington", "Butler", "Simmons", "Foster", "Gonzales", "Bryant", "Alexander", "Russell", "Griffin", "Diaz", "Hayes", "Myers", "Ford", "Hamilton", "Graham", "Sullivan", "Wallace", "Woods", "Cole" };
  var statusIcons = new[] { Icons.Rocket, Icons.Star, Icons.ThumbsUp, Icons.Heart, Icons.Check, Icons.Clock, Icons.X, Icons.Circle };
  var sampleUsers = Enumerable.Range(0, 10000).Select(id =>
  {
      var random = new Random(id * 17 + 42); // Different seed per row
      var firstName = firstNames[random.Next(firstNames.Length)];
      var lastName = lastNames[random.Next(lastNames.Length)];
      var name = $"{firstName} {lastName}";
      var email = $"{firstName.ToLower()}.{lastName.ToLower()}{id}@example.com";
      var salary = random.Next(40000, 70000);
      var status = statusIcons[random.Next(statusIcons.Length)];
      var isActive = random.Next(100) > 25;
      return new { Name = name, Email = email, Salary = salary, Status = status, IsActive = isActive };
  }).AsQueryable();
---

# DataTable

<Ingress>
Display and interact with large datasets using high-performance data tables with sorting, filtering, pagination, and real-time updates powered by Apache Arrow.
</Ingress>

The `DataTable` widget provides a powerful, high-performance solution for displaying tabular data. Built on Apache Arrow for optimal performance with large datasets.

## Basic Usage

Create a DataTable from any `IQueryable<T>` using the `.ToDataTable()` extension method:

```csharp demo-tabs
sampleUsers.ToDataTable()
    .Header(u => u.Name, "Full Name")
    .Header(u => u.Email, "Email Address")
    .Header(u => u.Salary, "Salary")
    .Header(u => u.Status, "Status")
```

## Column Types

DataTable automatically detects column types and provides appropriate labels:

- **Text** - String and character data
- **Number** - Numeric values (int, decimal, double, etc.)
- **Boolean** - True/false values with checkboxes
- **Date/DateTime** - Date and time values
- **Icon** - Icon enum values displayed as icons

<Callout Type="tip">
DataTable provides automatic scaffolding. It detects your model properties and creates appropriate columns automatically. You only need to customize what you want to change.
</Callout>

<WidgetDocs Type="Ivy.DataTable" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/DataTables/DataTable.cs"/>
