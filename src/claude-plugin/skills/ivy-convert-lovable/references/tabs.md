# Tabs

A tabbed interface for organizing content into switchable panels. In Lovable apps, this is the shadcn/ui Tabs component.

## Lovable

```tsx
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";

<Tabs defaultValue="overview">
  <TabsList>
    <TabsTrigger value="overview">Overview</TabsTrigger>
    <TabsTrigger value="analytics">Analytics</TabsTrigger>
    <TabsTrigger value="settings">Settings</TabsTrigger>
  </TabsList>
  <TabsContent value="overview">
    <p>Overview content here</p>
  </TabsContent>
  <TabsContent value="analytics">
    <p>Analytics content here</p>
  </TabsContent>
  <TabsContent value="settings">
    <p>Settings content here</p>
  </TabsContent>
</Tabs>
```

## Ivy

```csharp
new Tabs(
    new Tab("Overview",
        new Text("Overview content here")
    ),
    new Tab("Analytics",
        new Text("Analytics content here")
    ),
    new Tab("Settings",
        new Text("Settings content here")
    )
);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `defaultValue` | Initially active tab | First tab is default |
| `value` | Controlled active tab | Not supported (use `DefaultTab`) |
| `onValueChange` | Callback when tab changes | Not supported |
| `TabsTrigger.value` | Tab identifier | Implicit from `Tab` order |
| `TabsTrigger.children` | Tab label text | First parameter of `Tab` |
| `TabsContent` | Tab panel content | Children of `Tab` |
