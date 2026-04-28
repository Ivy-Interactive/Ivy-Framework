# DBML Generation Guide

You are generating DBML (Database Markup Language) code as specified by https://dbml.dbdiagram.io/docs/.

## DBML Output Requirements

* Generate valid DBML code according to the official specification
* Use PascalCase for all table and column names (C# convention)
* Table names must be singular (e.g., `User` not `Users`)
* Use .NET-friendly data types: `int`, `string`, `DateTime`, `Guid`, `decimal`, `bool`, `byte[]`, `DateOnly`, `TimeOnly`, `DateTimeOffset`, `double`, `float`, `long`
* Use descriptive names that reflect the purpose of each element
* Implement proper relationships using:
  - ref: syntax for foreign keys
  - Appropriate relationship types (>, <, -, <>)
* Include essential fields in all tables:
  - Primary key (typically Id)
  - Timestamps (CreatedAt, UpdatedAt) unless specified otherwise
* **CRITICAL**: DO NOT create empty enums. Only define enums that have at least one value. If you cannot determine the enum values, use a `string` field instead.

## Entity Ordering Requirements

**CRITICAL**: Tables MUST be defined in DAG (Directed Acyclic Graph) order based on their dependencies:

* Tables with NO foreign key dependencies must be listed FIRST
* Tables that reference other tables must be listed AFTER the tables they reference
* All `Ref:` declarations must appear at the end, after all table definitions

Example:
```dbml
// 1. Independent tables first
Table User {
  Id int [pk, increment]
  Name string [not null]
  Email string [not null, unique]
  CreatedAt DateTime [not null]
}

// 2. Dependent tables
Table Post {
  Id int [pk, increment]
  UserId int [not null]
  Title string [not null]
  Content string [not null]
  CreatedAt DateTime [not null]
}

// 3. All refs at the end
Ref: Post.UserId > User.Id
```

## Composite Primary Keys

```dbml
Table PostTag {
  PostId int [not null]
  TagId int [not null]
  indexes {
    (PostId, TagId) [pk]
  }
}
```

## Common Mistakes to Avoid

1. Each field can only have ONE settings bracket: `name string [not null, unique]` (NOT `name string [not null] [unique]`)
2. Refs must be top-level, NOT inside table definitions
3. Enums must be top-level, NOT inside table definitions
4. DO NOT specify [nullable] - use [null] or [not null]
5. DO NOT specify any indexes other than composite primary keys
6. DO NOT include comments in the DBML code

## Enum Syntax

```dbml
Enum OrderStatus {
  Pending
  Processing
  Shipped
  Delivered
  Cancelled
}

Table Order {
  Id int [pk, increment]
  Status OrderStatus [not null]
}
```

If enum values contain spaces, use double quotes: `"Not Yet Set"`

## After Generation

Use the **PresentPlan** tool with `planType: "Dbml"` to display the schema to the user for review.
