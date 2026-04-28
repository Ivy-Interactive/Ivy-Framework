# Data Seeder Generation Guide

Generate a data seeder class using the Bogus library for Entity Framework Core.

## Output Requirements

- Class name: `DataSeeder`
- Plain class (no interface) with two methods:
  - `public async System.Threading.Tasks.Task SeedAsync({ContextClass} context)` - seeds bogus data
  - `public async System.Threading.Tasks.Task ClearAsync({ContextClass} context)` - deletes all non-enum/lookup table data in reverse dependency order
- Uses the Bogus library for fake data generation
- File-scoped namespace
- DO NOT include the namespace in usings
- ALWAYS include `using System;`
- Use `System.Threading.Tasks.Task` as return type (fully qualified to avoid conflicts with entity models)
- Use C# 12+ syntax
- Generate complete, compilable code
- No comments in generated code
- No markdown formatting in output
- All closing braces must be present

## Enum/Lookup Table Rules

- **Do NOT seed** enum/lookup tables that are already seeded via `HasData()` in `OnModelCreating` - these are part of migrations and always present
- Only **query** enum/lookup tables to get FK references: `var statuses = await context.OrderStatuses.ToArrayAsync();`
- In `ClearAsync`, do NOT delete from enum/lookup tables - only delete from tables with bogus data

## ClearAsync Requirements

The `ClearAsync` method must:
1. Delete from tables in **reverse dependency order** (dependent tables first, independent tables last)
2. Skip enum/lookup tables that are seeded via `HasData()` in migrations
3. Use `RemoveRange` + `SaveChangesAsync` for each table
4. Example:
```csharp
public async System.Threading.Tasks.Task ClearAsync(MyContext context)
{
    context.OrderLines.RemoveRange(await context.OrderLines.ToArrayAsync());
    await context.SaveChangesAsync();
    context.Orders.RemoveRange(await context.Orders.ToArrayAsync());
    await context.SaveChangesAsync();
    context.Products.RemoveRange(await context.Products.ToArrayAsync());
    await context.SaveChangesAsync();
    context.Customers.RemoveRange(await context.Customers.ToArrayAsync());
    await context.SaveChangesAsync();
    // Do NOT clear OrderStatuses - seeded via HasData() in migrations
}
```

## Top 5 Critical Rules

1. **No async in RuleFor**: Never use `await` inside `.RuleFor()` lambdas. They are synchronous. Pre-fetch async data BEFORE creating the Faker instance.
2. **Variable naming**: Use `{entityName}ToAdd` or `new{EntityName}` for variables in seeding blocks. Use plain `{entityName}` for queried collections.
3. **Faker<T> vs Faker**: `Faker<T>` is for defining rules via `.RuleFor()`. `Faker` (non-generic) is for generating individual values like `.Internet.Email()`, `.Random.Int()`, etc.
4. **Explicit types in do-while**: Use `string email;` not `var email;` before do-while loops.
5. **No DependentOn**: Bogus does not have a `.DependentOn()` method. Set foreign keys directly: `.RuleFor(p => p.DepartmentId, f => f.PickRandom(departments).Id)`.
6. **Date distribution**: Ensure the majority of seeded date/time values fall within the last 30 days using `f.Date.Recent(30)` instead of `f.Date.Past(1)`. This keeps seeded data relevant and visible in typical date-filtered views.

### Quick Example of Rules 1, 2 & 3

WRONG - Causes CS4034, CS0136 and CS1061 errors:
```csharp
if (!await _context.Users.AnyAsync())
{
    var usedEmails = new HashSet<string>();
    var userFaker = new Faker<User>()
        .RuleFor(u => u.Name, f => f.Person.FullName)
        .RuleFor(u => u.CreatedAt, f => f.Date.Recent(30));

    var users = new List<User>();
    for (int i = 0; i < 100; i++)
    {
        string email;
        do
        {
            email = userFaker.Internet.Email();  // CS1061: Faker<User> has no 'Internet' property!
        } while (!usedEmails.Add(email));

        var user = userFaker.Generate();
        user.Email = email;
        users.Add(user);
    }
    _context.Users.AddRange(users);
    await _context.SaveChangesAsync();
}
var users = await _context.Users.ToArrayAsync();  // CS0136: 'users' already declared above!

if (!await _context.Orders.AnyAsync())
{
    var newOrders = new Faker<Order>()
        .RuleFor(o => o.CustomerName, f => f.Name.FullName())
        .RuleFor(o => o.StatusId, f => f.PickRandom(await _context.OrderStatuses.ToArrayAsync()).Id)  // CS4034: Cannot use 'await' in RuleFor lambda!
        .Generate(100);
    _context.Orders.AddRange(newOrders);
    await _context.SaveChangesAsync();
}
```

CORRECT - Pre-fetch data, proper variable naming, and Faker usage:
```csharp
if (!await _context.Users.AnyAsync())
{
    var usedEmails = new HashSet<string>();
    var faker = new Faker();  // Plain Faker for data generation!
    var userFaker = new Faker<User>()
        .RuleFor(u => u.Name, f => f.Person.FullName)
        .RuleFor(u => u.CreatedAt, f => f.Date.Recent(30))
        .RuleFor(u => u.UpdatedAt, (f, u) => f.Date.Between(u.CreatedAt, DateTime.UtcNow));

    var usersList = new List<User>();
    for (int i = 0; i < 100; i++)
    {
        string email;
        do
        {
            email = faker.Internet.Email();  // Use plain 'faker' for data generation!
        } while (!usedEmails.Add(email));

        var user = userFaker.Generate();
        user.Email = email;
        usersList.Add(user);
    }
    _context.Users.AddRange(usersList);
    await _context.SaveChangesAsync();
}
var users = await _context.Users.ToArrayAsync();  // Different name: 'users' - no conflict!

// Seeding Orders - pre-fetch lookup data FIRST
if (!await _context.Orders.AnyAsync())
{
    var orderStatuses = await _context.OrderStatuses.ToArrayAsync();  // Pre-fetch BEFORE creating Faker!
    var newOrders = new Faker<Order>()
        .RuleFor(o => o.CustomerName, f => f.Name.FullName())
        .RuleFor(o => o.StatusId, f => f.PickRandom(orderStatuses).Id)  // Use pre-fetched variable - no await!
        .RuleFor(o => o.CreatedAt, f => f.Date.Recent(30))
        .RuleFor(o => o.UpdatedAt, (f, o) => f.Date.Between(o.CreatedAt ?? DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow))
        .Generate(100);
    _context.Orders.AddRange(newOrders);
    await _context.SaveChangesAsync();
}
```

## Seeding Strategy

### Analyze the Data Model

- Identify all entity types and map relationships (one-to-one, one-to-many, many-to-many)
- Note required fields, constraints, and unique indexes
- Identify auto-generated properties (primary keys with ValueGeneratedOnAdd or database-generated identity columns)
- Check for entities already seeded via `.HasData()` in `OnModelCreating` - these should NOT be created in the seeder, only queried
- Identify one-to-one relationships by looking for `HasOne(...).WithOne(...)` configuration and non-collection navigation properties
- DO NOT SEED C# enum types - they don't exist as database tables

### Seeding Order

1. **First Priority - Lookup/Reference Tables:** Check if empty, seed immediately, save changes, query into arrays
2. **Second Priority - Independent Entities:** Seed entities with no foreign key dependencies, save after each
3. **Third Priority - Dependent Entities:** Seed entities referencing lookups/others using `f.PickRandom(lookupArray).Id`
4. Always `SaveChangesAsync()` after each entity type before seeding dependents
5. Check `AnyAsync()` before seeding to avoid duplicates
6. For entities seeded via `HasData()` in OnModelCreating, just query them - don't re-seed

### Data Volume and Time Range Requirements

The seeded data is used to generate dashboards with KPIs and charts. Follow these requirements:

**Time-Based Data (Orders, Transactions, Events, etc.):**
- Generate data spanning at least 90 days to show meaningful trends in line charts
- Distribute data across the entire time range, with emphasis on recent periods
- Use `f.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow)` instead of `f.Date.Recent(30)`
- Create more recent data than older data to show growth (60% in last 30 days, 30% in days 30-60, 10% in days 60-90)

**Data Volume for KPIs:**
- Transactional entities (orders, sales, events): minimum 500-1000 records
- Master data (customers, products): minimum 100-300 records
- Relationship/detail entities (order lines, reviews): multiple per parent entity

**Positive Trending Data for KPIs:**
- **Sales/Revenue**: Increase order values and frequency over time
- **Customer Growth**: More customers created in recent periods than older periods
- **Engagement Metrics**: Higher activity (reviews, orders per customer) in recent periods
- **Quality Metrics**: Skew ratings/scores toward positive values (e.g., 70% of reviews should be 4-5 stars)
- **Fulfillment/Success**: Favor positive status values (e.g., more "Delivered" than "Cancelled")

### Implementation Patterns for Trends

**Growing Transaction Volume:**
```csharp
var orders = new List<Order>();
var faker = new Faker();

// Period 1 (60-90 days ago): 100 orders
for (int i = 0; i < 100; i++)
{
    orders.Add(new Order
    {
        OrderDate = faker.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60)),
        TotalAmount = faker.Random.Decimal(50, 300),
        CustomerId = faker.PickRandom(customers).Id,
        StatusId = faker.PickRandom(orderStatuses).Id,
        CreatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow.AddDays(-60)),
        UpdatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-90), DateTime.UtcNow)
    });
}

// Period 2 (30-60 days ago): 200 orders (2x growth)
for (int i = 0; i < 200; i++)
{
    orders.Add(new Order
    {
        OrderDate = faker.Date.Between(DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-30)),
        TotalAmount = faker.Random.Decimal(60, 350),
        CustomerId = faker.PickRandom(customers).Id,
        StatusId = faker.PickRandom(orderStatuses).Id,
        CreatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-60), DateTime.UtcNow.AddDays(-30)),
        UpdatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-60), DateTime.UtcNow)
    });
}

// Period 3 (last 30 days): 400 orders (2x growth again)
for (int i = 0; i < 400; i++)
{
    orders.Add(new Order
    {
        OrderDate = faker.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow),
        TotalAmount = faker.Random.Decimal(70, 400),
        CustomerId = faker.PickRandom(customers).Id,
        StatusId = faker.PickRandom(orderStatuses).Id,
        CreatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow),
        UpdatedAt = faker.Date.Between(DateTime.UtcNow.AddDays(-30), DateTime.UtcNow)
    });
}
```

**Positive-Skewed Ratings:**
```csharp
var rating = faker.Random.Double() < 0.7
    ? faker.Random.Int(4, 5)  // 70% are 4-5 stars
    : faker.Random.Int(1, 3); // 30% are 1-3 stars
```

**Status Distribution with Positive Bias:**
```csharp
var statusId = faker.Random.Double() switch
{
    < 0.70 => deliveredStatus.Id,    // 70% delivered
    < 0.90 => processingStatus.Id,   // 20% processing
    _ => cancelledStatus.Id          // 10% cancelled
};
```

## One-to-One Relationship Seeding

One-to-one relationships require exactly one dependent entity per principal entity. NEVER use `PickRandom()` for one-to-one relationships.

**How to detect:** Look for `HasOne(...).WithOne(...)` in OnModelCreating and non-collection navigation properties.

WRONG - Using PickRandom violates one-to-one constraint:
```csharp
var newTranscripts = new Faker<Transcript>()
    .RuleFor(t => t.StudentId, f => f.PickRandom(students).Id)  // Multiple transcripts per student!
    .Generate(200);
```

CORRECT - Iterate through principal entities:
```csharp
var newTranscripts = new List<Transcript>();
var transcriptFaker = new Faker<Transcript>()
    .RuleFor(t => t.CumulativeGPA, f => f.Random.Decimal(2.0m, 4.0m))
    .RuleFor(t => t.CreatedAt, f => f.Date.Recent(30))
    .RuleFor(t => t.UpdatedAt, (f, t) => f.Date.Between(t.CreatedAt, DateTime.UtcNow));

foreach (var student in students)
{
    var transcript = transcriptFaker.Generate();
    transcript.StudentId = student.Id;
    newTranscripts.Add(transcript);
}
_context.Transcripts.AddRange(newTranscripts);
await _context.SaveChangesAsync();
```

## Common Patterns

```csharp
// Unique fields
var usedEmails = new HashSet<string>();
var emailFaker = new Faker();
string email;
do { email = emailFaker.Internet.Email(); } while (!usedEmails.Add(email));

// Many-to-many junction tables (avoid duplicate composite keys)
var selectedIds = new HashSet<int>();
if (selectedIds.Add(item.Id)) { /* add junction record */ }

// Don't set auto-generated primary keys
// Reference saved entities for foreign keys
var items = await context.Items.ToArrayAsync();
.RuleFor(o => o.ItemId, f => f.PickRandom(items).Id)
```

## Common Mistakes to Avoid

### 1. Async/Await in RuleFor Lambdas

WRONG:
```csharp
.RuleFor(o => o.StatusId, f => f.PickRandom(await _context.OrderStatuses.ToArrayAsync()).Id)  // CS4034!
```
CORRECT: Pre-fetch data before creating Faker (see Quick Example above).

### 2. Variable Naming Conflicts (CS0136)

**Pattern 1: If Blocks (Seeding vs Querying)**
- Inside if blocks (seeding): Use prefix `new` or suffix `ToAdd`
- Outside if blocks (querying): Use simple names

```csharp
if (!await _context.Artists.AnyAsync())
{
    var newArtists = new Faker<Artist>()
        .RuleFor(a => a.Name, f => f.Person.FullName)
        .Generate(50);
    _context.Artists.AddRange(newArtists);
    await _context.SaveChangesAsync();
}
var artists = await _context.Artists.ToArrayAsync();  // No conflict!
```

**Pattern 2: Foreach Loops - THE MOST COMMON CS0136 ERROR**

WRONG:
```csharp
foreach (var game in games)
{
    var achievements = new Faker<Achievement>()  // declared here
        .RuleFor(a => a.GameId, _ => game.Id)
        .Generate(5);
    newAchievements.AddRange(achievements);
}
var achievements = await _context.Achievements.ToArrayAsync(); // CS0136!
```

CORRECT - Use descriptive prefix:
```csharp
var newAchievements = new List<Achievement>();
foreach (var game in games)
{
    var gameAchievements = new Faker<Achievement>()  // Prefixed with 'game'
        .RuleFor(a => a.GameId, _ => game.Id)
        .Generate(5);
    newAchievements.AddRange(gameAchievements);
}
_context.Achievements.AddRange(newAchievements);
await _context.SaveChangesAsync();
var achievements = await _context.Achievements.ToArrayAsync(); // No conflict!
```

### 3. Never Call Random Methods Multiple Times in the Same RuleFor

Each call generates a DIFFERENT random value, causing index-out-of-range errors.

WRONG:
```csharp
.RuleFor(s => s.Title, f => f.Lorem.Sentence(4).Substring(0, Math.Min(255, f.Lorem.Sentence(4).Length)))
```

CORRECT:
```csharp
.RuleFor(s => s.Title, f => {
    var sentence = f.Lorem.Sentence(4);
    return sentence.Substring(0, Math.Min(255, sentence.Length));
})
```

### 4. Faker<T> vs Faker Scope Confusion

`Faker<T>` does NOT have `.Internet`, `.Person`, `.Random`, etc. properties.

WRONG:
```csharp
var userFaker = new Faker<User>().RuleFor(u => u.Name, f => f.Person.FullName);
email = userFaker.Internet.Email();  // CS1061: Faker<User> has no 'Internet'!
```

CORRECT:
```csharp
var faker = new Faker();  // Non-generic for data generation
var userFaker = new Faker<User>().RuleFor(u => u.Name, f => f.Person.FullName);
email = faker.Internet.Email();  // Use plain Faker
```

### 5. Not Assigning the Result of Faker.Generate()

WRONG:
```csharp
var journalists = new List<Journalist>();
var journalistFaker = new Faker<Journalist>()
    .RuleFor(j => j.Name, f => f.Name.FullName())
    .Generate(50);  // Result not assigned! List stays empty!
```

CORRECT:
```csharp
var journalistFaker = new Faker<Journalist>()
    .RuleFor(j => j.Name, f => f.Name.FullName());
var journalists = journalistFaker.Generate(50);  // Assign the result!
```

### 6. Setting Auto-Generated Primary Keys

WRONG:
```csharp
.RuleFor(l => l.Id, f => f.UniqueIndex)  // Id is auto-generated!
```

CORRECT: Don't set Id - let the database generate it. Only set Id when entity uses `ValueGeneratedNever()`.

### 7. Hardcoding Foreign Keys Without Ensuring Reference Data Exists

WRONG:
```csharp
.RuleFor(l => l.TypeId, f => f.PickRandom(1, 2))  // These IDs might not exist!
```

CORRECT: Create/save reference data first, then use `f.PickRandom(logTypes).Id`.

### 8. Re-seeding Entities Already Seeded via HasData()

WRONG:
```csharp
context.OutreachStatuses.AddRange(outreachStatuses);  // UNIQUE constraint violation!
```

CORRECT: Just query existing records: `var outreachStatuses = await context.OutreachStatuses.ToArrayAsync();`

Check `OnModelCreating` for `.HasData()` calls before creating lookup/reference data.

### 9. Lambda Parameter Shadowing

WRONG:
```csharp
.RuleFor(f => f.CreatedAt, f => f.Date.Recent(30))
.RuleFor(f => f.UpdatedAt, f => f.Date.Between(f.CreatedAt, DateTime.UtcNow))
// f.CreatedAt tries to access CreatedAt on Faker, not the entity!
```

CORRECT - Use two-parameter lambda with descriptive entity name:
```csharp
.RuleFor(f => f.CreatedAt, f => f.Date.Recent(30))
.RuleFor(f => f.UpdatedAt, (f, file) => f.Date.Between(file.CreatedAt, DateTime.UtcNow))
```

Rules:
- Use `f =>` when you only need Faker to generate random data
- Use `(f, entity) =>` when you need to access properties already set on the entity
- ALWAYS use descriptive names like `course`, `professor`, `student` instead of `c`, `p`, `s`
- CRITICAL: When CreatedAt is nullable (DateTime?), use null-coalescing: `entity.CreatedAt ?? DateTime.UtcNow.AddMonths(-2)`

### 10. Using PickRandom for One-to-One Relationships

See dedicated section above. Iterate through principal entities instead.

### 11. Duplicate Composite Keys in Many-to-Many

WRONG:
```csharp
for (int i = 0; i < count; i++)
{
    startupFounders.Add(new StartupFounder
    {
        StartupId = startup.Id,
        FounderId = faker.PickRandom(founders).Id  // Same founder may be picked twice!
    });
}
```

CORRECT:
```csharp
var selectedFounders = new HashSet<int>();
for (int i = 0; i < count && selectedFounders.Count < founders.Count; i++)
{
    var founder = faker.PickRandom(founders);
    if (selectedFounders.Add(founder.Id))
    {
        startupFounders.Add(new StartupFounder { StartupId = startup.Id, FounderId = founder.Id });
    }
}
```

### 12. Unique Constraint Violations

For fields with unique constraints, use `HashSet` to track used values:
```csharp
var usedTagNames = new HashSet<string>();
var wordFaker = new Faker();
while (tags.Count < 30)
{
    var tagName = wordFaker.Lorem.Word();
    if (usedTagNames.Add(tagName))
    {
        var tag = tagFaker.Generate();
        tag.Name = tagName;
        tags.Add(tag);
    }
}
```

### 13. Random.Int() Returns an Integer, Not a Collection

WRONG:
```csharp
var items = new Faker().Random.Int(1, 3).Select(_ => new Item { ... });  // Can't Select on int!
```

CORRECT: Use the integer to control a loop.

### 14. Count Property vs Count() Method

Use `.Count()` LINQ method (not `.Count` property) on collections generated by Bogus `.Generate()`.

### 15. No Join Method on string[]

WRONG: `f.Lorem.Words(2).Join(" ")`
CORRECT: `string.Join(" ", f.Lorem.Words(2))`

### 16. Commerce.Price() Returns a String

WRONG: `.RuleFor(p => p.Price, f => f.Commerce.Price(1, 1000, 2))`
CORRECT: `.RuleFor(p => p.Price, f => f.Random.Decimal(1, 1000))`

### 17. Non-Existent Methods

These DO NOT exist in Bogus:
- `f.Education` (entire dataset) - Create custom arrays and use `f.PickRandom()`
- `f.Commerce.Industry` - Create your own industry list
- `f.Date.FutureRefDate` - Use `f.Date.Future(int yearsToGoForward, DateTime? refDate)`
- `.DependentOn()` - Set properties directly
- `.Approximately()` - Use conditional logic

**Fallback Strategy:** When Bogus doesn't have a dataset, create a custom array and use `f.PickRandom()`:
```csharp
var degreeNames = new[] {
    "Computer Science", "Mathematics", "Physics", "Chemistry", "Biology",
    "Engineering", "Business Administration", "Economics", "Psychology"
};
var name = f.PickRandom(degreeNames);
```

### 18. Using 'f' Parameter Outside RuleFor

WRONG:
```csharp
foreach (var customer in customers)
{
    var customerDeals = new Faker<Deal>()
        .RuleFor(d => d.CustomerId, _ => customer.Id)
        .Generate(f.Random.Int(1, 5)); // 'f' is not in scope here!
}
```

CORRECT:
```csharp
var faker = new Faker();
foreach (var customer in customers)
{
    var customerDeals = new Faker<Deal>()
        .RuleFor(d => d.CustomerId, _ => customer.Id)
        .Generate(faker.Random.Int(1, 5));
}
```

## Bogus API Reference

The following is the COMPLETE list of available Bogus datasets. There is NO Education dataset or any other datasets not listed here.

```
Bogus.Faker [Class]
  Address: Bogus.DataSets.Address
  Commerce: Bogus.DataSets.Commerce
  Company: Bogus.DataSets.Company
  Database: Bogus.DataSets.Database
  Date: Bogus.DataSets.Date
  Finance: Bogus.DataSets.Finance
  Hacker: Bogus.DataSets.Hacker
  Image: Bogus.DataSets.Images
  Internet: Bogus.DataSets.Internet
  Lorem: Bogus.DataSets.Lorem
  Music: Bogus.DataSets.Music
  Name: Bogus.DataSets.Name
  Person: Bogus.Person
  Phone: Bogus.DataSets.PhoneNumbers
  Random: Bogus.Randomizer
  Rant: Bogus.DataSets.Rant
  System: Bogus.DataSets.System
  Vehicle: Bogus.DataSets.Vehicle
  PickRandom(IEnumerable<T> arg0)
  PickRandom(IList<T> arg0)
  PickRandom(T[] arg0)

Bogus.DataSets.Date [Class]
  Between(DateTime start, DateTime end): DateTime
  BetweenDateOnly(DateOnly start, DateOnly end): DateOnly
  BetweenOffset(DateTimeOffset start, DateTimeOffset end): DateTimeOffset
  BetweenTimeOnly(TimeOnly start, TimeOnly end): TimeOnly
  Future(int yearsToGoForward, DateTime? refDate)
  FutureDateOnly(int yearsToGoForward, DateOnly? refDate)
  FutureOffset(int yearsToGoForward, DateTimeOffset? refDate)
  Month(bool abbreviation, bool useContext): string
  Past(int yearsToGoBack, DateTime? refDate)
  PastDateOnly(int yearsToGoBack, DateOnly? refDate)
  PastOffset(int yearsToGoBack, DateTimeOffset? refDate)
  Recent(int days, DateTime? refDate)
  RecentDateOnly(int days, DateOnly? refDate)
  RecentOffset(int days, DateTimeOffset? refDate)
  Soon(int days, DateTime? refDate)
  SoonDateOnly(int days, DateOnly? refDate)
  SoonOffset(int days, DateTimeOffset? refDate)
  Timespan(TimeSpan? maxSpan)
  TimeZoneString(): string
  Weekday(bool abbreviation, bool useContext): string

Bogus.DataSets.Address
  BuildingNumber(): string
  CardinalDirection(bool useAbbreviation = false): string
  City(): string
  CityPrefix(): string
  CitySuffix(): string
  Country(): string
  CountryCode(Iso3166Format format = Alpha2): string
  County(): string
  Direction(bool useAbbreviation = false): string
  FullAddress(): string
  Latitude(double min = -90, double max = 90): double
  Longitude(double min = -180, double max = 180): double
  OrdinalDirection(bool useAbbreviation = false): string
  SecondaryAddress(): string
  State(): string
  StateAbbr(): string
  StreetAddress(bool useFullAddress = false): string
  StreetName(): string
  StreetSuffix(): string
  ZipCode(string format = null): string

Bogus.DataSets.Commerce
  Categories(int num): string[]
  Color(): string
  Department(int max = 3, bool returnMax = false): string
  Ean13(): string
  Ean8(): string
  Price(decimal min = 1, decimal max = 1000, int decimals = 2, string symbol = ""): string
  Product(): string
  ProductAdjective(): string
  ProductDescription(): string
  ProductMaterial(): string
  ProductName(): string

// NOTE: There is no Industry() method in the Company dataset.
// NOTE: There is NO Education dataset in Bogus at all.

Bogus.DataSets.Company
  Bs(): string
  CatchPhrase(): string
  CompanyName(int? formatIndex)
  CompanyName(string format): string
  CompanySuffix(): string

Bogus.DataSets.Finance
  Account(int length = 8): string
  AccountName(): string
  Amount(decimal min = 0, decimal max = 1000, int decimals = 2): decimal
  Bic(): string
  BitcoinAddress(): string
  CreditCardCvv(): string
  CreditCardNumber(CardType provider = null): string
  Currency(bool includeFundCodes): Currency
  EthereumAddress(): string
  Iban(bool formatted = false, string countryCode = null): string
  LitecoinAddress(): string
  RoutingNumber(): string
  TransactionType(): string

Bogus.DataSets.Hacker
  Abbreviation(): string
  Adjective(): string
  IngVerb(): string
  Noun(): string
  Phrase(): string
  Verb(): string

Bogus.DataSets.Images
  DataUri(int width, int height, string htmlColor = "grey"): string
  PicsumUrl(int width, int height, bool grayscale, bool blur, int? imageId)
  PlaceholderUrl(int width, int height, string text, string format, string backColor, string textColor): string

Bogus.DataSets.Internet
  Avatar(): string
  Color(byte baseRed, byte baseGreen, byte baseBlue, bool grayscale, ColorFormat format): string
  DomainName(): string
  DomainSuffix(): string
  DomainWord(): string
  Email(string firstName, string lastName, string provider, string uniqueSuffix): string
  ExampleEmail(string firstName, string lastName): string
  Ip(): string
  IpAddress(): IPAddress
  Ipv6(): string
  Ipv6Address(): IPAddress
  Mac(string separator = ":"): string
  Password(int length = 10, bool memorable = false, string regexPattern = "\\w", string prefix = ""): string
  Port(): int
  Protocol(): string
  Url(): string
  UrlRootedPath(string fileExt): string
  UrlWithPath(string protocol, string domain, string fileExt): string
  UserAgent(): string
  UserName(string firstName, string lastName): string

Bogus.DataSets.Lorem
  Letter(int num = 1): string
  Lines(int? lineCount, string separator)
  Paragraph(int min = 3): string
  Paragraphs(int count = 3, string separator = "\n\n"): string
  Paragraphs(int min, int max, string separator = "\n\n"): string
  Sentence(int? wordCount, int? range)
  Sentences(int? sentenceCount, string separator)
  Slug(int wordcount = 3): string
  Text(): string
  Word(): string
  Words(int num = 3): string[]

Bogus.DataSets.Music
  Genre(): string

Bogus.DataSets.Name
  FirstName(Gender? gender)
  FullName(Gender? gender)
  JobArea(): string
  JobDescriptor(): string
  JobTitle(): string
  JobType(): string
  LastName(Gender? gender)
  Prefix(Gender? gender)
  Suffix(): string

Bogus.DataSets.PhoneNumbers
  PhoneNumber(string format = null): string
  PhoneNumberFormat(int phoneFormatsArrayIndex = 0): string

Bogus.DataSets.Rant
  Review(string product = "product"): string
  Reviews(string product = "product", int lines = 2): string[]

Bogus.DataSets.System
  AndroidId(): string
  ApplePushToken(): string
  BlackBerryPin(): string
  CommonFileExt(): string
  CommonFileName(string ext): string
  CommonFileType(): string
  DirectoryPath(): string
  Exception(): Exception
  FileExt(string mimeType): string
  FileName(string ext): string
  FilePath(): string
  FileType(): string
  MimeType(): string
  Semver(): string
  Version(): Version

Bogus.DataSets.Vehicle
  Fuel(): string
  Manufacturer(): string
  Model(): string
  Type(): string
  Vin(bool strict = false): string

Bogus.Randomizer [Class]
  AlphaNumeric(int length): string
  ArrayElement(T[] arg0)
  ArrayElements(T[] array, int? count)
  Bool(): bool
  Bool(float weight): bool
  Byte(byte min = 0, byte max = 255): byte
  Bytes(int count): byte[]
  Char(char min, char max): char
  Chars(char min, char max, int count = 5): char[]
  ClampString(string str, int? min, int? max)
  CollectionItem(ICollection<T> arg0)
  Decimal(decimal min = 0, decimal max = 1): decimal
  Digits(int count, int minDigit = 0, int maxDigit = 9): int[]
  Double(double min = 0, double max = 1): double
  Enum(T[] exclude)
  EnumValues(int? count, T[] exclude)
  Even(int min = 0, int max = 1): int
  Float(float min = 0, float max = 1): float
  Guid(): Guid
  Hash(int length = 40, bool upperCase = false): string
  Hexadecimal(int length, string prefix): string
  Int(int min = -2147483648, int max = 2147483647): int
  ListItem(List<T> arg0)
  ListItem(IList<T> arg0)
  ListItems(IList<T> items, int? count)
  ListItems(List<T> items, int? count)
  Long(long min, long max): long
  Number(int max): int
  Number(int min = 0, int max = 1): int
  Odd(int min = 0, int max = 1): int
  Replace(string format): string
  ReplaceNumbers(string format, char symbol = '#'): string
  Short(short min, short max): short
  Shuffle(IEnumerable<T> items)
  String(int? length, char minChar, char maxChar)
  String(int minLength, int maxLength, char minChar, char maxChar): string
  String2(int length, string chars = "abcdefghijklmnopqrstuvwxyz"): string
  String2(int minLength, int maxLength, string chars): string
  UInt(uint min = 0, uint max = 4294967295): uint
  ULong(ulong min = 0, ulong max = 18446744073709551615): ulong
  UShort(ushort min = 0, ushort max = 65535): ushort
  Uuid(): Guid
  WeightedRandom(T[] items, float[] weights)
  Word(): string
  Words(int? count)
  WordsArray(int min, int max): string[]
  WordsArray(int count): string[]
```

## Full Example

```csharp
using Bogus;
using Microsoft.EntityFrameworkCore;
using System;

namespace ProjectNamespace;

public class DataSeeder
{
    public async System.Threading.Tasks.Task SeedAsync(DataContext context)
    {
        if (!await context.DealStates.AnyAsync())
        {
            var dealStateNames = new[] { "Sourced", "Due Diligence", "Investment Committee", "Term Sheet", "Closing", "Portfolio", "Exit", "Passed" };
            var newDealStates = dealStateNames.Select(name => new DealState { Name = name }).ToList();
            context.DealStates.AddRange(newDealStates);
            await context.SaveChangesAsync();
        }

        if (!await context.Genders.AnyAsync())
        {
            var genderNames = new[] { "Male", "Female", "Other" };
            var newGenders = genderNames.Select(name => new Gender { Name = name }).ToList();
            context.Genders.AddRange(newGenders);
            await context.SaveChangesAsync();
        }

        if (!await context.Industries.AnyAsync())
        {
            var industryNames = new[] { "Technology", "Healthcare", "Finance", "Consumer", "Ecommerce", "SaaS", "Biotech", "Robotics", "AI", "Blockchain" };
            var newIndustries = industryNames.Select(name => new Industry { Name = name }).ToList();
            context.Industries.AddRange(newIndustries);
            await context.SaveChangesAsync();
        }

        var dealStates = await context.DealStates.ToArrayAsync();
        var genders = await context.Genders.ToArrayAsync();
        var industries = await context.Industries.ToArrayAsync();

        if (!await context.Startups.AnyAsync())
        {
            var newStartups = new Faker<Startup>()
                .RuleFor(s => s.Name, f => {
                    var companyName = f.Company.CompanyName();
                    return companyName.Substring(0, Math.Min(100, companyName.Length));
                })
                .RuleFor(s => s.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow))
                .RuleFor(s => s.UpdatedAt, (f, x) => f.Date.Between(x.CreatedAt ?? DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow))
                .RuleFor(s => s.IndustryId, f => f.PickRandom(industries).Id)
                .Generate(200);
            context.Startups.AddRange(newStartups);
            await context.SaveChangesAsync();
        }

        var startups = await context.Startups.ToArrayAsync();

        if (!await context.Partners.AnyAsync())
        {
            var newPartners = new Faker<Partner>()
                .RuleFor(p => p.Name, f => f.Person.FullName)
                .RuleFor(p => p.Email, f => f.Internet.Email())
                .RuleFor(p => p.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow))
                .RuleFor(p => p.UpdatedAt, (f, x) => f.Date.Between(x.CreatedAt ?? DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow))
                .Generate(15);
            context.Partners.AddRange(newPartners);
            await context.SaveChangesAsync();
        }

        var partners = await context.Partners.ToArrayAsync();

        var faker = new Faker();

        if (!await context.Founders.AnyAsync())
        {
            var newFounders = new List<Founder>();
            foreach (var startup in startups)
            {
                var count = faker.Random.Int(1, 3);
                var startupFounders = new Faker<Founder>()
                    .RuleFor(f => f.Name, x => x.Name.FullName())
                    .RuleFor(f => f.Email, x => x.Internet.Email())
                    .RuleFor(f => f.GenderId, x => x.PickRandom(genders).Id)
                    .RuleFor(f => f.Age, x => x.Random.Int(20, 60))
                    .RuleFor(f => f.CreatedAt, f => f.Date.Between(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow))
                    .RuleFor(f => f.UpdatedAt, (f, x) => f.Date.Between(x.CreatedAt ?? DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow))
                    .RuleFor(f => f.StartupId, _ => startup.Id)
                    .Generate(count);
                newFounders.AddRange(startupFounders);
            }
            context.Founders.AddRange(newFounders);
            await context.SaveChangesAsync();
        }

        var founders = await context.Founders.ToArrayAsync();

        if (!await context.Deals.AnyAsync())
        {
            var newDeals = new List<Deal>();
            for (int i = 0; i < 200; i++)
            {
                newDeals.Add(new Deal
                {
                    CreatedAt = faker.Date.Between(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow),
                    UpdatedAt = faker.Date.Between(DateTime.UtcNow.AddMonths(-2), DateTime.UtcNow),
                    StartupId = faker.PickRandom(startups).Id,
                    PartnerId = faker.PickRandom(partners).Id,
                    DealAmount = faker.Random.Int(100_000, 10_000_000),
                    DealStateId = faker.PickRandom(dealStates).Id
                });
            }
            context.Deals.AddRange(newDeals);
            await context.SaveChangesAsync();
        }

        if (!await context.StartupPartners.AnyAsync())
        {
            var newStartupPartners = new List<StartupPartner>();
            foreach (var startup in startups)
            {
                var numPartners = faker.Random.Int(1, 3);
                var selectedPartners = new HashSet<int>();
                for (int i = 0; i < numPartners && selectedPartners.Count < partners.Count; i++)
                {
                    var partner = faker.PickRandom(partners);
                    if (selectedPartners.Add(partner.Id))
                    {
                        newStartupPartners.Add(new StartupPartner
                        {
                            StartupId = startup.Id,
                            PartnerId = partner.Id
                        });
                    }
                }
            }
            context.StartupPartners.AddRange(newStartupPartners);
            await context.SaveChangesAsync();
        }
    }

    public async System.Threading.Tasks.Task ClearAsync(DataContext context)
    {
        context.StartupPartners.RemoveRange(await context.StartupPartners.ToArrayAsync());
        await context.SaveChangesAsync();
        context.Deals.RemoveRange(await context.Deals.ToArrayAsync());
        await context.SaveChangesAsync();
        context.Founders.RemoveRange(await context.Founders.ToArrayAsync());
        await context.SaveChangesAsync();
        context.Partners.RemoveRange(await context.Partners.ToArrayAsync());
        await context.SaveChangesAsync();
        context.Startups.RemoveRange(await context.Startups.ToArrayAsync());
        await context.SaveChangesAsync();
        // Do NOT clear DealStates, Genders, Industries - seeded via HasData()
    }
}
```
