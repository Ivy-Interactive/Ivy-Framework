# Entity Framework Core Code First Generation Guide

Convert DBML specifications into EF Core Code First entities with data annotations and Fluent API.

## Output Specifications

* Create well-structured C# POCO classes for each table
* Generate a complete DbContext class with:
  - DbSet properties for each entity
  - OnModelCreating method with Fluent API configurations
* Use appropriate EF Core attributes: [Key], [Required], [MaxLength], [Table], [Column], [ForeignKey]
* Use PascalCase for all C# class names and properties
* Initialize non-nullable string properties with `= null!;`
* Initialize navigation properties with `= null!;`
* Use nullable types only when truly optional

## Required Using Directives

Entity files:
- `using System.ComponentModel.DataAnnotations;` — for [Key], [Required], [MaxLength], [Column], etc.
- `using System.ComponentModel.DataAnnotations.Schema;` — for [Table], [ForeignKey], [DatabaseGenerated]
- `using Microsoft.EntityFrameworkCore;` — only if using EF-specific attributes like [Index]

DbContext file:
- `using Microsoft.EntityFrameworkCore;` — for DbContext, DbSet<T>, ModelBuilder
- `using {Namespace}.Models;` — to reference entity classes from the Models subdirectory

Always include all required usings explicitly. Do not rely on global usings or implicit usings for non-System namespaces.

## File Organization

Generate individual files using the WriteFile tool:
- One `.cs` file per entity class
- One `.cs` file for the DbContext class
- Use file-scoped namespaces

## Relationships

### One-to-Many
For `Ref: Post.UserId > User.Id`:
- User class: `public ICollection<Post> Posts { get; set; } = null!;`
- Post class: `public User User { get; set; } = null!;`
- Fluent API: `.HasOne(e => e.User).WithMany(e => e.Posts).HasForeignKey(e => e.UserId)`

### One-to-One
Use `HasOne...WithOne` with non-collection navigation properties.

### Many-to-Many (explicit junction table)
Navigation properties on principal entities must reference the junction entity type:
```csharp
public class Student { public ICollection<StudentCourse> StudentCourses { get; set; } = null!; }
public class Course { public ICollection<StudentCourse> StudentCourses { get; set; } = null!; }
```

**Composite Primary Keys:** Junction tables with composite primary keys defined in DBML (e.g., `(StudentId, CourseId) [pk]`) MUST have `HasKey` configured in `OnModelCreating`, because data annotations `[Key]` cannot define composite keys:
```csharp
modelBuilder.Entity<StudentCourse>(entity =>
{
    entity.HasKey(e => new { e.StudentId, e.CourseId });
});
```
This is required for ALL join/junction tables with composite primary keys — without it, EF Core will fail with "requires a primary key to be defined".

## Navigation Property Naming

CRITICAL: Navigation property names MUST NOT match their containing class name (causes CS0542).
- Use role-based names: `PickupLocation`, `DropoffLocation`
- For self-referencing: `FollowerUser`, `FolloweeUser`

## Multiple FKs to Same Entity

When an entity has multiple FKs to the same target, explicitly configure ALL relationships in OnModelCreating.

## DbContext Constructor

```csharp
public MyContext(DbContextOptions<MyContext> options) : base(options) { }
```

Do NOT include an OnConfiguring method.

## Notes

* Use C# 12+ syntax with file-scoped namespaces
* Generate COMPLETE code for ALL entities - never truncate
* DO NOT include comments in generated code
* Favor data annotations where possible, Fluent API for complex configs
