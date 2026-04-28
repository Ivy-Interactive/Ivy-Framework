# Dialog

A modal dialog overlay for confirmations, forms, or important content. In Lovable apps, this is the shadcn/ui Dialog component.

## Lovable

```tsx
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";

const [open, setOpen] = useState(false);

<Dialog open={open} onOpenChange={setOpen}>
  <DialogTrigger asChild>
    <Button>Open Dialog</Button>
  </DialogTrigger>
  <DialogContent>
    <DialogHeader>
      <DialogTitle>Edit Profile</DialogTitle>
      <DialogDescription>Make changes to your profile.</DialogDescription>
    </DialogHeader>
    <Input placeholder="Name" />
    <DialogFooter>
      <Button variant="outline" onClick={() => setOpen(false)}>Cancel</Button>
      <Button onClick={handleSave}>Save</Button>
    </DialogFooter>
  </DialogContent>
</Dialog>
```

## Ivy

```csharp
var (dialog, openDialog) = UseDialog("Edit Profile", DialogContent);

new Button("Open Dialog", onClick: e => openDialog());
dialog; // render the dialog view

IEnumerable<object> DialogContent()
{
    yield return new Text("Make changes to your profile.");
    var name = UseState("");
    yield return name.ToTextInput().Placeholder("Name");
}

// Simple confirmation dialog
var (alert, showAlert) = UseAlert();
showAlert("Are you sure?", result =>
{
    if (result == AlertResult.Yes)
        DoSomething();
}, "Confirm", AlertButtonSet.YesNo);
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `open` | Controlled open state | Managed by `UseDialog` hook |
| `onOpenChange` | Callback when open state changes | Managed by `UseDialog` hook |
| `DialogTitle` | Dialog header title | First parameter of `UseDialog` |
| `DialogDescription` | Description text below title | Rendered as `Text` in dialog content |
| `DialogContent` | Dialog body content | Content callback function |
| `DialogFooter` | Footer with action buttons | Handled by Ivy dialog system |
| `DialogTrigger` | Element that opens the dialog | Call `openDialog()` from any event handler |
