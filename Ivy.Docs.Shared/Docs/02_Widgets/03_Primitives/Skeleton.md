---
searchHints:
  - loading
  - placeholder
  - shimmer
  - skeleton
  - loading-state
  - pending
---

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
        var isLoading = UseState(false);
        var product = UseState<ProductData?>();
        var loadTrigger = UseState(0);
        
        // Handle loading with proper timer cleanup
        UseEffect(() => {
            if (loadTrigger.Value > 0)
            {
                isLoading.Set(true);
                product.Set((ProductData?)null);
                
                // Create timer with proper disposal
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
                
                // Return cleanup function to dispose timer
                return timer;
            }
            return null;
        }, [loadTrigger]);
        
        void LoadProduct()
        {
            loadTrigger.Set(t => t + 1);
        }
        
        return Layout.Vertical().Gap(4).Padding(4)
            | new Button(
                isLoading.Value ? "Loading..." : "Load Product Data", 
                onClick: _ => LoadProduct()
            ).Disabled(isLoading.Value)
            | (product.Value != null || isLoading.Value
                ? (isLoading.Value
                    ? Layout.Vertical().Gap(3)
                        | new Skeleton().Height(Size.Units(40)).Width(Size.Full())
                        | new Skeleton().Height(Size.Units(24)).Width(Size.Units(40))
                        | new Skeleton().Height(Size.Units(16)).Width(Size.Full())
                        | new Skeleton().Height(Size.Units(16)).Width(Size.Full())
                        | new Skeleton().Height(Size.Units(16)).Width(Size.Units(40))
                        | (Layout.Horizontal().Gap(2)
                            | new Skeleton().Height(Size.Units(24)).Width(Size.Units(40))
                            | new Skeleton().Height(Size.Units(36)).Width(Size.Units(40)))
                    : Layout.Vertical().Gap(3)
                        | new Image("https://example.com/headphones.jpg")
                            .Height(Size.Units(40))
                            .Width(Size.Full())
                        | Text.H3(product.Value?.Name)
                        | Text.P(product.Value?.Description)
                        | Text.Strong($"Rating: {product.Value?.Rating}/5")
                        | (Layout.Horizontal().Gap(2)
                            | Text.H4($"${product.Value?.Price}")
                            | new Spacer().Width(Size.Grow())
                            | new Button("Add to Cart")))
                : Text.P("Click the button above to load product data and see the skeleton loading state."));
    }
}
```

<WidgetDocs Type="Ivy.Skeleton" ExtensionTypes="Ivy.SkeletonExtensions" SourceUrl="https://github.com/Ivy-Interactive/Ivy-Framework/blob/main/Ivy/Widgets/Primitives/Skeleton.cs"/>
