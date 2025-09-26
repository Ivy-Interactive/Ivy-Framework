using Ivy.Samples.Shared.Apps.Widgets.Inputs;
using Ivy.Shared;
using Ivy.Widgets.Internal;

namespace Ivy.Samples.Shared.Apps.Demos;

[App(icon: Icons.Code)]
public class DbmlEditorApp : ViewBase
{
    public override object? Build()
    {
        var sampleDbml =
            """
            Enum order_status {
                pending
                shipped
                delivered
                cancelled
            }

            Table customer {
                id int [pk, increment, not null]
                name varchar [not null]
                email varchar [not null, unique]
                address text
                created_at timestamp [not null]
                updated_at timestamp [not null]
            }

            Table product_category {
                id int [pk, increment, not null]
                name varchar [not null, unique]
                description text
                created_at timestamp [not null]
                updated_at timestamp [not null]
            }

            Table product {
                id int [pk, increment, not null]
                name varchar [not null]
                description text
                price decimal [not null]
                category_id int [not null]
                created_at timestamp [not null]
                updated_at timestamp [not null]
            }

            Table "order" {
                id int [pk, increment, not null]
                customer_id int [not null]
                status order_status [not null]
                total_amount decimal [not null]
                created_at timestamp [not null]
                updated_at timestamp [not null]
            }

            Table order_line {
                id int [pk, increment, not null]
                order_id int [not null]
                product_id int [not null]
                quantity int [not null]
                unit_price decimal [not null]
                created_at timestamp [not null]
                updated_at timestamp [not null]
            }

            Table review {
                id int [pk, increment, not null]
                product_id int [not null]
                customer_id int [not null]
                rating int [not null]
                comment text
                created_at timestamp [not null]
                updated_at timestamp [not null]
            }

            Ref: product.category_id > product_category.id
            Ref: "order".customer_id > customer.id
            Ref: order_line.order_id > "order".id
            Ref: order_line.product_id > product.id
            Ref: review.product_id > product.id
            Ref: review.customer_id > customer.id
            """;

        var dbml = this.UseState(sampleDbml);
        return Layout.Horizontal().RemoveParentPadding().Height(Size.Full())
               | dbml.ToCodeInput().Width(90).Height(Size.Full()).Language(Languages.Dbml)
               | new DbmlCanvas(dbml.Value).Width(Size.Grow())
            ;
    }
}