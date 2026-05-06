# Avatar

A user avatar image with fallback initials. In Lovable apps, this is the shadcn/ui Avatar component.

## Lovable

```tsx
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";

<Avatar>
  <AvatarImage src={user.avatarUrl} alt={user.name} />
  <AvatarFallback>{user.name.charAt(0)}</AvatarFallback>
</Avatar>
```

## Ivy

```csharp
new Avatar(user.Name, user.AvatarUrl);

// Fallback is automatic from the name initials
new Avatar("John Doe"); // Shows "JD"
```

## Parameters

| Parameter | Documentation | Ivy |
|-----------|---------------|-----|
| `AvatarImage.src` | Image URL | Second parameter (url) |
| `AvatarImage.alt` | Alt text | First parameter (name) |
| `AvatarFallback` | Fallback content (usually initials) | Automatic from name |
