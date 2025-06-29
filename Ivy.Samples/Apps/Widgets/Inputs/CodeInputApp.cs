using Ivy.Shared;

namespace Ivy.Samples.Apps.Widgets.Inputs;

[App(icon: Icons.Code)]
public class CodeInputApp : SampleBase
{
    protected override object? BuildSample()
    {
        var sampleCode =
            """
            Project ecommerce {
              database_type: "PostgreSQL"
            }
            
            Table users {
              id integer [pk, increment]
              username varchar(50) [not null, unique]
              email varchar(100) [not null, unique]
              password_hash varchar(255) [not null]
              created_at timestamp [default: `now()`]
            }
            
            Table products {
              id integer [pk, increment]
              name varchar(100) [not null]
              description text
              price decimal(10,2) [not null]
              stock integer [not null, default: 0]
              created_at timestamp [default: `now()`]
            }
            
            Table orders {
              id integer [pk, increment]
              user_id integer [not null, ref: > users.id]
              status varchar(50) [not null, default: 'pending']
              total_amount decimal(10,2) [not null]
              created_at timestamp [default: `now()`]
            }
            
            Table order_items {
              id integer [pk, increment]
              order_id integer [not null, ref: > orders.id]
              product_id integer [not null, ref: > products.id]
              quantity integer [not null]
              price decimal(10,2) [not null]
            }
            
            Ref: orders.user_id > users.id
            Ref: order_items.order_id > orders.id
            Ref: order_items.product_id > products.id
            """;

        var code = UseState(sampleCode);

        return Layout.Vertical().Width(100)
               | code.ToCodeInput();
    }
}