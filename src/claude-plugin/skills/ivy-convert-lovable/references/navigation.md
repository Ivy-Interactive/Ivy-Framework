# Navigation (react-router-dom)

Routing and navigation patterns. Lovable apps use `react-router-dom` for client-side routing.

## Lovable

```tsx
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";

// App.tsx route definitions
<BrowserRouter>
  <Routes>
    <Route path="/" element={<Index />} />
    <Route path="/dashboard" element={<Dashboard />} />
    <Route path="/customers" element={<Customers />} />
    <Route path="/customers/:id" element={<CustomerDetail />} />
    <Route path="/settings" element={<Settings />} />
    <Route path="/auth" element={<Auth />} />
    <Route path="*" element={<NotFound />} />
  </Routes>
</BrowserRouter>

// Programmatic navigation
const navigate = useNavigate();
navigate("/dashboard");
navigate(`/customers/${id}`);

// URL parameters
const { id } = useParams();
const [searchParams] = useSearchParams();
const tab = searchParams.get("tab");

// Navigation links
import { Link } from "react-router-dom";
<Link to="/dashboard">Go to Dashboard</Link>
```

## Ivy

In Ivy, each page becomes a separate App class. Navigation between apps is handled by the Ivy framework.

```csharp
// Each route/page becomes an IApp class
// /dashboard -> DashboardApp
// /customers -> CustomersApp
// /customers/:id -> CustomerDetailApp (or handled within CustomersApp)
// /settings -> SettingsApp
// /auth -> Skip (Ivy handles auth automatically)

// Navigation between apps
client.Navigate<DashboardApp>();
client.Navigate<CustomerDetailApp>(new { Id = customerId });

// Sidebar/navigation is configured at the project level, not per-app
```

## Parameters

| Pattern | Lovable | Ivy |
|---------|---------|-----|
| Route definition | `<Route path="/page" element={<Page />} />` | `IApp` class registration |
| Navigate programmatically | `navigate("/path")` | `client.Navigate<AppClass>()` |
| URL parameters | `useParams()` | App constructor parameters |
| Search params | `useSearchParams()` | App state |
| Link component | `<Link to="/path">` | `new Button().Link()` or sidebar nav |
| Protected routes | `ProtectedRoute` wrapper | Built-in (all apps protected by default) |
| Auth route (`/auth`) | Custom auth page | Skip (Ivy built-in auth) |
| Landing page (`/`) | Marketing/landing page | Skip or convert to first app |
| Not Found (`*`) | 404 page | Built-in |
