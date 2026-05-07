# Supabase Edge Functions

Serverless backend functions. Lovable apps use Supabase Edge Functions (Deno/TypeScript) for server-side logic, external API calls, and webhook handling.

## Lovable

```typescript
// supabase/functions/send-email/index.ts
import { serve } from "https://deno.land/std@0.168.0/http/server.ts";

const corsHeaders = {
  "Access-Control-Allow-Origin": "*",
  "Access-Control-Allow-Headers": "authorization, x-client-info, apikey, content-type",
};

serve(async (req) => {
  if (req.method === "OPTIONS") {
    return new Response(null, { headers: corsHeaders });
  }

  const { to, subject, body } = await req.json();
  const RESEND_API_KEY = Deno.env.get("RESEND_API_KEY");

  const res = await fetch("https://api.resend.com/emails", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${RESEND_API_KEY}`,
    },
    body: JSON.stringify({ from: "noreply@example.com", to, subject, html: body }),
  });

  const data = await res.json();
  return new Response(JSON.stringify(data), {
    headers: { ...corsHeaders, "Content-Type": "application/json" },
  });
});

// Called from frontend:
const { data, error } = await supabase.functions.invoke("send-email", {
  body: { to: "user@example.com", subject: "Hello", body: "<p>Hi!</p>" },
});
```

## Ivy

Edge functions map to different Ivy patterns depending on their purpose:

### Server Action (for API calls triggered by user actions)

```csharp
// In your App class or a service
async ValueTask SendEmail(string to, string subject, string body)
{
    var apiKey = Configuration["Resend:ApiKey"];
    var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", apiKey);

    await client.PostAsJsonAsync("https://api.resend.com/emails", new
    {
        from = "noreply@example.com",
        to,
        subject,
        html = body,
    });
}
```

### Webhook Handler (for incoming webhooks from external services)

```csharp
// Stripe webhook, payment processing, etc.
// Configured as an Ivy webhook endpoint
```

### Scheduled Job (for periodic tasks)

```csharp
// For edge functions called on a schedule (cron)
// Configured as an Ivy background job
```

## Common Edge Function Patterns

| Pattern | Lovable Edge Function | Ivy Equivalent |
|---------|----------------------|----------------|
| External API call (Stripe, Resend, OpenAI) | `fetch()` with API key from `Deno.env.get()` | `HttpClient` with key from `Configuration` |
| CORS handling | `corsHeaders` boilerplate | Not needed (Ivy handles CORS) |
| JWT verification | `verify_jwt = true` in config | Built-in auth |
| Database access in function | `createClient()` with service role key | Direct connection access |
| Webhook receiver | Edge function + `verify_jwt = false` | Ivy webhook endpoint |
| Scheduled task | External cron trigger | Ivy background job |
| File processing | Edge function with Storage | Ivy file handling |
