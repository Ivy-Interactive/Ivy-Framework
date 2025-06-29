using Ivy.Samples.Apps.Widgets.Inputs;
using Ivy.Shared;
using Ivy.Widgets.Internal;

namespace Ivy.Samples.Apps.Demos;

[App(icon: Icons.Code)]
public class DbmlEditorApp : ViewBase
{
    public override object? Build()
    {
        var sampleDbml =
            """
            Table users {
              id integer
              username varchar
              role varchar
              created_at timestamp
            }
            
            Table posts {
              id integer [primary key]
              title varchar
              body text [note: 'Content of the post']
              user_id integer
              created_at timestamp
            }
            
            Ref: posts.user_id > users.id // many-to-one
            """;

        var dbml = this.UseState(sampleDbml);
        return Layout.Horizontal().RemoveParentPadding().Height(Size.Screen())
               | dbml.ToCodeInput().Width(90).Height(Size.Full()).Language(Languages.Dbml)
               | new DbmlCanvas(dbml.Value).Width(Size.Grow())
            ;
    }
}