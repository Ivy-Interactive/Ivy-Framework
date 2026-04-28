# Supabase Auth

Authentication patterns using Supabase Auth. Lovable apps use `@supabase/supabase-js` for user authentication with email/password, OAuth, and session management.

## Lovable

```tsx
import { supabase } from "@/integrations/supabase/client";
import { useNavigate } from "react-router-dom";

// Sign up
const { data, error } = await supabase.auth.signUp({
  email: "user@example.com",
  password: "password123",
});

// Sign in
const { data, error } = await supabase.auth.signInWithPassword({
  email: "user@example.com",
  password: "password123",
});

// OAuth (Google, GitHub, etc.)
const { error } = await supabase.auth.signInWithOAuth({
  provider: "google",
  options: { redirectTo: window.location.origin + "/dashboard" },
});

// Sign out
await supabase.auth.signOut();

// Get current user
const { data: { user } } = await supabase.auth.getUser();

// Auth state listener (common in AuthProvider)
useEffect(() => {
  const { data: { subscription } } = supabase.auth.onAuthStateChange(
    (event, session) => {
      if (session) setUser(session.user);
      else setUser(null);
    }
  );
  return () => subscription.unsubscribe();
}, []);

// Protected route pattern
const ProtectedRoute = ({ children }) => {
  const { user, loading } = useAuth();
  if (loading) return <Spinner />;
  if (!user) return <Navigate to="/auth" />;
  return children;
};

// RLS (Row Level Security) - enforced at database level
// Policies reference auth.uid() to restrict data access per user
```

## Ivy

In Ivy, authentication is handled by the built-in auth system. No manual auth implementation is needed.

```csharp
// Auth is configured at the project level, not per-app
// Ivy handles login, signup, OAuth, and session management automatically

// Access current user in any App
var user = UseCurrentUser();

// User properties
var email = user.Email;
var name = user.Name;
var id = user.Id;

// Authorization is handled through Ivy's role-based access control
// RLS policies from Supabase map to Ivy's server-side authorization
```

## Parameters

| Pattern | Lovable | Ivy |
|---------|---------|-----|
| Sign up (email/password) | `supabase.auth.signUp()` | Built-in auth UI |
| Sign in (email/password) | `supabase.auth.signInWithPassword()` | Built-in auth UI |
| OAuth providers | `supabase.auth.signInWithOAuth()` | Configured in project settings |
| Sign out | `supabase.auth.signOut()` | Built-in UI / `client.SignOut()` |
| Get current user | `supabase.auth.getUser()` | `UseCurrentUser()` |
| Auth state listener | `onAuthStateChange()` | Automatic (Ivy manages sessions) |
| Protected routes | `ProtectedRoute` component | Ivy apps are protected by default |
| Auth page | Custom `src/pages/Auth.tsx` | Built-in (skip during conversion) |
| RLS policies | `CREATE POLICY ... USING (auth.uid() = user_id)` | Ivy server-side authorization |
| Password reset | `supabase.auth.resetPasswordForEmail()` | Built-in auth flow |
