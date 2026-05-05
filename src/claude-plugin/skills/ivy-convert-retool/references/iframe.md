# IFrame

A content area to embed an external web page. Supports permissions for downloads, form submissions, microphone/camera access, and popups. Common use cases include embedding dashboards (Grafana, Kibana), third-party tools, documentation sites, media content, and legacy web applications.

## Retool

```js
// Set the source URL
iFrame.src = "https://example.com";

// Reload the iframe content
iFrame.reload();

// Change the URL dynamically
iFrame.setPageUrl("https://example.com/new-page");

// Toggle visibility
iFrame.setHidden(true);
iFrame.setHidden(false);

// Scroll into view
iFrame.scrollIntoView({ behavior: "smooth", block: "center" });
```

## Ivy

```csharp
// Basic iframe
new Iframe("https://example.com");

// With refresh support via token
long refreshToken = 0;
new Iframe("https://example.com", refreshToken: refreshToken);

// Reload by changing the refresh token
refreshToken++;

// Sized iframe
new Iframe("https://example.com")
{
    Width = Size.Full(),
    Height = Size.Units(600)
};

// Toggle visibility
new Iframe("https://example.com")
{
    Visible = false
};
```

## Parameters

| Parameter     | Retool Documentation                                                     | Ivy                                                                  |
|---------------|--------------------------------------------------------------------------|----------------------------------------------------------------------|
| `src`         | `string` - The URL of the page to embed. Default: `null`.               | `Src` (`string`) - URL of content to embed. Set via constructor.     |
| `title`       | `string` - The title text to display. Default: `null`.                   | Not supported                                                        |
| `showTopBar`  | `boolean` - Whether to display a toolbar. Default: `false`.              | Not supported                                                        |
| `style`       | `object` - Custom style options. Default: `null`.                        | Not supported (use `Height`, `Width`, `Scale` instead)               |
| `hidden`      | `boolean` - Whether the component is hidden. Default: `false`.           | `Visible` (`bool`) - Controls visibility.                            |
| `reload()`    | Method - Reloads the iframe content.                                     | `RefreshToken` (`long?`) - Change value to force reload.             |
| `setPageUrl()` | Method - Dynamically sets the iframe URL.                               | Reassign `Src` property.                                             |
| `setHidden()` | Method - Toggles component visibility.                                   | Set `Visible` property.                                              |
| `scrollIntoView()` | Method - Scrolls the container to bring the component into view.   | Not supported                                                        |
| N/A           | N/A                                                                      | `Height` (`Size`) - Vertical dimension of the iframe.                |
| N/A           | N/A                                                                      | `Width` (`Size`) - Horizontal dimension of the iframe.               |
| N/A           | N/A                                                                      | `Scale` (`Scale?`) - Scaling options for the iframe content.         |
