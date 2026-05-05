# st.dialog

A decorator that turns a function into a modal dialog. When the decorated function is called, a modal window appears overlaying the app. Widgets rendered inside the function appear within the modal. Dialog functions behave like fragments — widget interactions rerun only the dialog, not the full script.

## Streamlit

```python
import streamlit as st

@st.dialog("Cast your vote")
def vote(item):
    st.write(f"Why is {item} your favorite?")
    reason = st.text_input("Because...")
    if st.button("Submit"):
        st.session_state.vote = {"item": item, "reason": reason}
        st.rerun()

if "vote" not in st.session_state:
    st.write("Vote for your favorite")
    if st.button("A"):
        vote("A")
    if st.button("B"):
        vote("B")
else:
    st.write(f"You voted for {st.session_state.vote['item']} "
             f"because {st.session_state.vote['reason']}")
```

## Ivy

```csharp
public class VoteDialogExample : ViewBase
{
    public override object? Build()
    {
        var isOpen = UseState(false);
        var selectedItem = UseState("");
        var vote = UseState<(string Item, string Reason)?>(null);
        var client = UseService<IClientProvider>();

        return Layout.Vertical(
            vote.Value == null
                ? Layout.Vertical(
                    Text.P("Vote for your favorite"),
                    new Button("A", _ => { selectedItem.Set("A"); isOpen.Set(true); }),
                    new Button("B", _ => { selectedItem.Set("B"); isOpen.Set(true); })
                )
                : Text.P($"You voted for {vote.Value?.Item} because {vote.Value?.Reason}"),
            isOpen.Value
                ? new Dialog(
                    _ => isOpen.Set(false),
                    new DialogHeader("Cast your vote"),
                    new DialogBody(
                        Text.P($"Why is {selectedItem.Value} your favorite?"),
                        UseState("").ToTextInput().Placeholder("Because...").OnChange(reason =>
                        {
                            // store reason in state
                        })
                    ),
                    new DialogFooter(
                        new Button("Cancel", _ => isOpen.Set(false)).Outline(),
                        new Button("Submit", _ =>
                        {
                            vote.Set((selectedItem.Value, "..."));
                            isOpen.Set(false);
                        })
                    )
                )
                : null
        );
    }
}
```

## Parameters

| Parameter   | Documentation                                                                                                  | Ivy                                                                                          |
|-------------|----------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| title       | The title displayed at the top of the modal dialog. Supports markdown formatting (bold, italics, links, etc.). | `DialogHeader(string title)` — passed as the header section of the `Dialog` constructor.     |
| dismissible | Whether the user can close the dialog by clicking outside, pressing ESC, or clicking X. Default `True`.        | Controlled via the `onClose` callback and `Visible` property. No built-in dismissible flag.  |
| icon        | An emoji, Material Symbols icon, or spinner displayed next to the title.                                       | Not supported                                                                                |
| on_dismiss  | Behavior on dismiss: `"ignore"` (no rerun), `"rerun"`, or a callable executed before rerun.                    | `OnClose` event handler (`Func<Event<Dialog>, ValueTask>` or `Action<Event<Dialog>>`).       |
