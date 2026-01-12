---
prepare: |
  var client = UseService<IClientProvider>();
searchHints:
  - paging
  - navigation
  - pages
  - next
  - previous
  - page-numbers
---

# Pagination

<Ingress>
Display page [navigation](../../01_Onboarding/02_Concepts/14_Navigation.md) controls for traversing large sets of data, with customizable appearance and dynamic updates.
</Ingress>

The `Pagination` [widget](../../01_Onboarding/02_Concepts/03_Widgets.md) is used to provide navigation between multiple pages of content, such as [lists](07_List.md) or [tables](08_Table.md). It displays a set of page links, previous/next buttons, and optional ellipsis for skipped ranges. The widget can be customized for appearance and supports binding to [state](../../03_Hooks/Core/03_State.md) for dynamic updates.

## Basic Usage

Here's a simple example of a pagination control initialized at page 5 of 10.

```csharp demo-below
public class BasicPaginationApp : ViewBase
{
    public override object? Build() {
        var page = UseState(5);

        return new Pagination(page.Value, 10, newPage => page.Set(newPage.Value));
    }
}
```

## Configuration

You can configure the number of visible pages adjacent to the current page (siblings) and at the start/end of the range (boundaries).

```csharp demo-tabs
public class PaginationConfigurationApp : ViewBase
{
    public override object? Build() {
        var page = UseState(5);

        return Layout.Vertical().Gap(4)
            | Text.Large("Siblings")
            | (Layout.Vertical()
                | new Pagination(page.Value, 20, newPage => page.Set(newPage.Value)).Siblings(1)
                | new Pagination(page.Value, 20, newPage => page.Set(newPage.Value)).Siblings(2))
            | Text.Large("Boundaries")
            | (Layout.Vertical()
                | new Pagination(page.Value, 20, newPage => page.Set(newPage.Value)).Boundaries(1)
                | new Pagination(page.Value, 20, newPage => page.Set(newPage.Value)).Boundaries(2));
    }
}
```

<WidgetDocs Type="Ivy.Pagination" ExtensionTypes="Ivy.PaginationExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Pagination.cs"/>
