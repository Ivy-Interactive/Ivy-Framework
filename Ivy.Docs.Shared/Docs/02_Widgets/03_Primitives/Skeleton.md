# Skeleton

<Ingress>
Create elegant loading placeholders that mimic your content structure to improve perceived performance during data loading.
</Ingress>

The `Skeleton` widget creates placeholder loading indicators that mimic the shape of your content. It improves perceived performance by showing users the layout of the page while data is loading.

```csharp demo-tabs 
public class ProductCardView : ViewBase
{
    public record ProductData(
        string Name, 
        string Description, 
        decimal Price, 
        string ImageUrl, 
        double Rating, 
        int Stock);
    
    public override object? Build()
    {
        var isLoading = UseState(true);
        var product = UseState<ProductData?>();
        
        // Simulate loading data
        UseEffect(() => {
            var timer = new System.Threading.Timer(_ => {
                product.Set(new ProductData(
                    "Premium Wireless Headphones",
                    "Experience crystal-clear sound with our premium noise-cancelling wireless headphones. Features 30-hour battery life and memory foam ear cushions for all-day comfort.",
                    149.99m,
                    "https://example.com/headphones.jpg",
                    4.7,
                    12
                ));
                isLoading.Set(false);
            }, null, 2000, Timeout.Infinite);
            return timer;
        }, []);
        
        return Layout.Vertical().Width(Size.Units(300)).Padding(4)
            | (isLoading.Value
                ? Layout.Vertical().Gap(3)
                    | new Skeleton().Height(Size.Units(200)).Width(Size.Full())
                    | new Skeleton().Height(Size.Units(24)).Width(Size.Units(200))
                    | new Skeleton().Height(Size.Units(16)).Width(Size.Full())
                    | new Skeleton().Height(Size.Units(16)).Width(Size.Full())
                    | new Skeleton().Height(Size.Units(16)).Width(Size.Units(150))
                    | Layout.Horizontal().Gap(2)
                        | new Skeleton().Height(Size.Units(24)).Width(Size.Units(80))
                        | new Skeleton().Height(Size.Units(36)).Width(Size.Units(100))
                : Layout.Vertical().Gap(3)
                    | new Image("https://example.com/headphones.jpg")
                        .Height(Size.Units(200))
                        .Width(Size.Full())
                    | Text.H3(product.Value?.Name)
                    | Text.P(product.Value?.Description)
                    | Text.Strong($"Rating: {product.Value?.Rating}/5")
                    | Layout.Horizontal().Gap(2)
                        | Text.H4($"${product.Value?.Price}")
                        | new Button("Add to Cart"));
    }
}
```

<WidgetDocs Type="Ivy.Skeleton" ExtensionTypes="Ivy.SkeletonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Skeleton.cs"/> 