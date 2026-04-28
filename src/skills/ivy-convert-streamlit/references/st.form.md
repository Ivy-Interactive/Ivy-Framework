# st.form

Creates a container that groups input widgets together with a Submit button. When submitted, all widget values inside the form are sent as a batch rather than individually.

## Streamlit

```python
with st.form("my_form"):
    st.write("Inside the form")
    slider_val = st.slider("Form slider")
    checkbox_val = st.checkbox("Form checkbox")
    submitted = st.form_submit_button("Submit")
    if submitted:
        st.write("slider", slider_val, "checkbox", checkbox_val)
```

## Ivy

```csharp
// Forms are created from state objects using .ToForm()
var user = UseState(() => new UserModel("", "", false, 25));

UseEffect(() =>
{
    if (!string.IsNullOrEmpty(user.Value.Name))
    {
        client.Toast($"User {user.Value.Name} created!");
    }
}, user);

return user.ToForm("Submit");
```

## Parameters

| Parameter        | Documentation                                                                 | Ivy                                                                                          |
|------------------|-------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| clear_on_submit  | If True, all widgets inside the form are reset to their default values after submission. | Not supported (reset the state object manually to clear after submit)                        |
| enter_to_submit  | If True, pressing Enter inside a widget submits the form. Defaults to True.   | Supported (forms submit on Enter by default)                                                 |
| border           | If True, shows a border around the form container. Defaults to True.          | Not supported                                                                                |
