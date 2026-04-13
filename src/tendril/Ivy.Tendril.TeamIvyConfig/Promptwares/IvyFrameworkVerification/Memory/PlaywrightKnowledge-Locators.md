# Locator Strategies

## General Locator Patterns

- Prefer `page.getByText()` for visible text content
- Use `page.getByRole("button", { name: /pattern/i })` for buttons
- Use `page.locator("code")` for inline code elements
- Use `.first()` when multiple matches are possible
- Use `waitFor({ state: "visible", timeout: 5000 })` for async content

## Flexible Text Matching

- When matching formatted values (currency, percentages), use flexible regex patterns like `/Value:.*50.*200/` instead of strict patterns like `/Value: \$50\.00 - \$200\.00/` to handle formatting variations

## Heading Role Conflicts

- `getByRole('heading', { name: 'Text' })` can match multiple headings if names overlap (e.g., "Percent Format" matches both "Currency & Percent Formatting" H1 and "Percent Format" H3). Always use `{ exact: true }` when heading names are substrings of other headings

## Scoping Locators to Containers

- **Scope element locators to widget containers** — `page.locator('ul')` or `page.locator('ol')` can match Ivy shell elements (e.g., toast viewport `<ol>`). Scope to the widget container: `.markdown-widget ul`, `.markdown-widget ol`, etc.

## Role-Based Locators

- `state.ToSelectInput().Variant(SelectInputVariants.Toggle)` renders as radio buttons — use `getByRole("radio", { name: "OptionName" })` to click them, NOT `getByText()` (the option text may appear in headings/descriptions too)
- `IClientProvider.Toast()` renders both a visible `<div>` and an `aria-live` status `<span>` — use `getByText("message", { exact: true })` to avoid strict mode violations
- Clicking `ListItem` in Ivy lists: extract the visible item title text from the page (e.g., via regex on body text) and use `getByText(name, { exact: true }).dispatchEvent("click")`. Filtering parent divs by `hasText` matches too many ancestors
- `data-testid="list-item"` does NOT exist in Ivy list rendering — click list items by visible text content using `getByText(name, { exact: true }).first().dispatchEvent("click")`

## Text-Based Locator Pitfalls

- **Toast DOM locator**: Radix Toast renders individual toasts as `<li>` elements with `data-state="open"`. Use `page.locator('li[data-state="open"]')` to find all visible toast elements. Do NOT use `[role="status"]` — that matches the aria-live region, not individual toasts. The toast viewport is an `<ol>` element with `gap-2` for spacing.
- **`Html` component inline styles don't render as expected** — `new Html(...)` with inline CSS properties like `background-color`, `border`, `color`, `width`, `height` are NOT applied to the actual DOM. Do NOT use `page.locator('div[style*="..."]')` selectors to target Html content. Instead use text-based assertions (`page.content().includes(...)`) or `getByText()` locators. Also do NOT check for hex color codes in `page.content()` — they won't appear in the rendered HTML.
- **Sidebar nav button name conflicts**: Chrome sidebar renders app names as `role="button"` elements. A button with text "C" will conflict with "Calculator" sidebar button when using `getByRole("button", { name: "C" })` — always use `{ exact: true }` for single-character button names
- **Confetti Click trigger wrapper uses `role="button"`** — `.WithConfetti(AnimationTrigger.Click)` wraps children in `<div role="button" tabindex="0">`. This means `getByRole("button", { name: "X" })` matches both the wrapper div (containing all child text) AND the actual `<button>` inside. Always use `{ exact: true }` when targeting buttons inside a Click-trigger Confetti wrapper.
- **Ivy layout doesn't render widgets as direct DOM siblings of headings** — `h2.nextElementSibling` won't contain the widget that follows the heading in the Ivy tree. Use full-page `body` text assertions or specific locators instead of DOM sibling traversal.

## Chrome Navigation Locators

- `UseChrome()` renders a hidden sidebar search `<input type="search" data-testid="sidebar-search">` that is the first `input` in the DOM but outside the viewport — `page.locator("input").first()` will target it instead of app inputs. Use `input[type='text']` or label-based locators to target app inputs
- **Single-app Chrome auto-selection**: When `UseChrome().UseTabs()` is enabled and there's only ONE app registered, Chrome automatically opens that app's tab on page load — no need to click the sidebar nav item. Clicking it may cause a re-navigation that times out.
- **Chrome URL routing**: When `UseChrome()` is active, direct URL navigation (e.g., `/basic-app` or `/basic-app?shell=false`) shows "App Not Found". Apps must be accessed by navigating to `/` first, then clicking sidebar items. In Playwright tests, always navigate to `http://localhost:PORT/` and use sidebar clicks.
- **Chrome tabs start with NO tab open** — `UseChrome(new ChromeSettings().UseTabs())` renders a sidebar but the content area is blank on initial load. Tests MUST click a sidebar item (e.g., `page.getByText("Dashboard").first().click()`) before asserting content
- Navigation is via sidebar nav items, NOT `role="tab"` — use `page.getByText("AppName").first().click()`. **BUT** sidebar items may be outside the viewport in headless mode — `click()` and `click({ force: true })` both fail with "Element is outside of the viewport". Use `dispatchEvent("click")` instead, or use sidebar search first to filter then `dispatchEvent("click")`
- `Icons.Plus.ToButton().Ghost().Tooltip("Create X").ToTrigger(...)` — the tooltip text does NOT become the button's accessible name in Playwright. `getByRole("button", { name: /Create X/i })` fails. Use `page.locator("button").filter({ has: page.locator("svg") }).first()` instead
- Icon-only buttons (`.Destructive()` trash) have no accessible name — working approach: iterate all `page.locator("button")` elements, check `innerText().trim() === ""`, then `dispatchEvent("click")` on the match

## Network and Navigation

- `page.goto()` with `waitUntil: "networkidle"` hangs on Ivy apps because WebSocket connections keep the network active — use `waitUntil: "domcontentloaded"` instead.
- `page.goBack()` in Ivy SPA may not reliably restore blade state — prefer re-navigating or keeping blade context
- `waitForServer` must use `http`/`https` module, not `fetch` — `fetch` may not be available in all Node.js versions used by Playwright. Use polling loop instead. **On macOS, Ivy defaults to HTTPS** — use `https.get(url, { rejectUnauthorized: false }, ...)` and add `ignoreHTTPSErrors: true` to Playwright config (both `use` and `projects[0].use`). On Windows, Ivy uses HTTP by default.
- Without `UseChrome()`, Ivy SPA doesn't support URL-based app routing — `/app-name` shows the default app. Must enable Chrome for multi-app navigation.
- `baseURL` in `playwright.config.ts` is evaluated at import time, before `beforeAll` runs. When using dynamic ports, use full URLs in `page.goto()` calls instead of relative paths

## Input Widget TestId Wrapping

- **Input widget TestId wraps Field container**: `.TestId()` on input widgets (BoolInput, SelectInput, etc.) is applied to the outer Field wrapper, not the inner interactive element. Using `getByTestId('x').locator('button[role="switch"]')` times out. Instead use `page.locator('button[role="switch"]').first()` or `page.locator('button[role="checkbox"]').nth(index)`.
- **`data-testid` on Box/Card widgets is NOT reliably found by `[data-testid="..."]` selectors** in Playwright. Use text-based locators (`getByText`, `getByRole`) and `page.evaluate()` with DOM tree walking to inspect computed CSS properties instead
