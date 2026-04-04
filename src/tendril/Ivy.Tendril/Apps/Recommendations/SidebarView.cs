using Ivy;
using Ivy.Tendril.Services;

namespace Ivy.Tendril.Apps.Recommendations;

public class SidebarView(
    List<Recommendation> recommendations,
    IState<Recommendation?> selectedState) : ViewBase
{
    private readonly List<Recommendation> _recommendations = recommendations;
    private readonly IState<Recommendation?> _selectedState = selectedState;

    public override object Build()
    {
        return new List(_recommendations.Select(rec =>
        {
            var clickableRec = rec;

            return new ListItem(rec.Title)
                .Content(Layout.Horizontal().Gap(1)
                    | new Badge($"#{rec.PlanId}").Variant(BadgeVariant.Outline).Small()
                )
                .OnClick(() => _selectedState.Set(clickableRec));
        }));
    }
}
