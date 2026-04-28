# st.tabs

Insert containers separated into tabs. Tabs allow users to navigate between groups of related content. All tab content is sent to the frontend regardless of which tab is selected.

## Streamlit

```python
import streamlit as st

tab1, tab2, tab3 = st.tabs(["Cat", "Dog", "Owl"])

with tab1:
    st.header("A cat")
    st.image("https://static.streamlit.io/examples/cat.jpg", width=200)
with tab2:
    st.header("A dog")
    st.image("https://static.streamlit.io/examples/dog.jpg", width=200)
with tab3:
    st.header("An owl")
    st.image("https://static.streamlit.io/examples/owl.jpg", width=200)
```

## Ivy

```csharp
Layout.Tabs(
    new Tab("Cat", Layout.Column(Header("A cat"), Image("cat.jpg"))),
    new Tab("Dog", Layout.Column(Header("A dog"), Image("dog.jpg"))),
    new Tab("Owl", Layout.Column(Header("An owl"), Image("owl.jpg")))
)
```

## Parameters

| Parameter | Streamlit | Ivy |
|-----------|-----------|-----|
| tabs | `tabs` (list of str) &mdash; Labels for each tab. Supports GitHub-flavored Markdown including bold, italics, strikethroughs, inline code, links, and images. | `tabs` (Tab[]) &mdash; Array of `Tab` objects, each with a label and content. Supports icons via `.Icon()` and badges via `.Badge()`. |
| default | `default` (str or None) &mdash; Which tab is displayed initially by matching label string. Defaults to the first tab. | `SelectedIndex` (int?) &mdash; Initially selected tab by index rather than label. |
| variant | Not supported | `Variant` (TabsVariant) &mdash; Display mode: `Tabs` (button-style with underline) or `Content` (emphasizes content area). |
| closable tabs | Not supported | `OnClose` event &mdash; Enables closable tabs by handling the close event. |
| reordering | Not supported | `OnReorder` event &mdash; Enables drag-and-drop tab reordering. |
| refresh | Not supported | `OnRefresh` event &mdash; Adds a refresh button per tab. |
| add button | Not supported | `AddButtonText` (string) &mdash; Shows an add-tab button with the given label. |
| icons | Supported via Markdown image syntax in tab labels. | `.Icon()` fluent method on each `Tab` object. |
| badges | Not supported | `.Badge()` fluent method on each `Tab` object. |
