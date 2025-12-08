using Ivy.Hooks;
using Ivy.Samples.Shared;
using Ivy.Shared;
using Ivy.Views.Alerts;
using Ivy.Views.Builders;
using Ivy.Views.Forms;
using Ivy.Views.Kanban;

namespace Ivy.Samples.Shared.Apps.Tests;

[App(icon: Icons.Kanban, path: ["Tests"], title: "Pipeline Debug")]
public class PipelineDebugApp : ViewBase
{
    public override object? Build()
    {
        // Simulated State
        var deals = UseState<ImmutableArray<DealRecord>>([]);
        var isLoading = UseState(true);
        var (alertView, showAlert) = this.UseAlert();

        // Simulated Service (In-memory)
        var service = UseService<MockPipelineService>();

        // Edit Sheet Trigger
        var (editView, showEdit) = this.UseTrigger((IState<bool> isOpen, Guid linkId)
            => new DealEditSheetDebug(isOpen, linkId, service, deals));

        UseEffect(async () =>
        {
            isLoading.Set(true);
            var fetchedDeals = await service.FetchDeals();
            deals.Set(fetchedDeals);
            isLoading.Set(false);
        }, [EffectTrigger.AfterInit()]);

        if (isLoading.Value) return Text.Muted("Loading...");

        var createBtn = new Button("New Deal")
            .Icon(Icons.Plus)
            .Outline()
            .HandleClick(async () =>
            {
                var newDeal = await service.CreateDeal();
                deals.Set([.. deals.Value, newDeal]);
            });

        var kanban = deals.Value
            .ToKanban(
                groupBySelector: deal => deal.DealState,
                idSelector: deal => deal.Id,
                orderSelector: deal => deal.Order
            )
            .ColumnWidth(Size.Units(35))
            .ColumnOrder(deal => deal.DealStateOrder)
            .CardBuilder(CardBuilder)
            .HandleMove(OnMove)
            ;

        var header = Layout.Horizontal().Gap(2) | createBtn | Text.Muted("Drag items quickly to test race conditions");

        var body = new HeaderLayout(
            header,
            kanban
        ).Scroll(Scroll.None);

        return new Fragment()
               | body
               | editView
               | alertView;

        object CardBuilder(DealRecord deal)
        {
            var dropDown = Icons.Ellipsis
                .ToButton()
                .Ghost()
                .WithDropDown(
                    MenuItem.Default("Delete").Icon(Icons.Trash).HandleSelect(() => OnDelete(deal.Id))
                );

            var details = new
            {
                Amount = deal.AmountFormatted(),
                Contact = deal.ContactName,
                Owner = deal.OwnerName
            };

            var content = details.ToDetails();

            return new Card(content)
                .Small()
                .Title(deal.InvestorName)
                .Icon(dropDown)
                .HandleClick(() => showEdit(deal.Id))
                .Hover(CardHoverVariant.Pointer)
                .Key(deal.Id); // Ensure Key is set
        }

        void OnDelete(Guid cardId)
        {
            // Simulate instant local update then slow server
            deals.Set([.. deals.Value.Where(d => d.Id != cardId)]);
            _ = service.DeleteDeal(cardId);
        }

        void OnMove((object? cardId, string toState, int? targetIndex) moveData)
        {
            if (!Guid.TryParse(moveData.cardId?.ToString(), out var dealId)) return;

            // Optimistic update
            deals.Set(MoveDealLocally(deals.Value, dealId, moveData.toState, moveData.targetIndex ?? 0));

            // Fire and forget with delay to simulate race
            _ = service.MoveDeal(dealId, moveData.toState, moveData.targetIndex ?? 0);
        }
    }

    // Helper to move locally (unchanged)
    ImmutableArray<DealRecord> MoveDealLocally(
        ImmutableArray<DealRecord> items,
        Guid dealId,
        string toState,
        int targetIndex)
    {
        var newDealStateOrder = items
            .Where(i => i.DealState == toState)
            .Select(i => i.DealStateOrder)
            .FirstOrDefault();

        if (newDealStateOrder == 0 && !items.Any(i => i.DealState == toState))
        {
            newDealStateOrder = toState switch
            {
                "New" => 0,
                "Qualified" => 1,
                "Proposition" => 2,
                "Won" => 3,
                _ => 0
            };
        }

        var updatedList = items
            .Select(i => i.Id == dealId ? i with { DealState = toState, DealStateOrder = newDealStateOrder } : i)
            .ToList();

        var movedItem = updatedList.FirstOrDefault(i => i.Id == dealId);
        if (movedItem is null)
            return items;

        var group = updatedList
            .Where(i => i.DealState == movedItem.DealState)
            .OrderBy(i => i.Order)
            .ToList();

        group.RemoveAll(i => i.Id == dealId);
        targetIndex = Math.Clamp(targetIndex, 0, group.Count);
        group.Insert(targetIndex, movedItem);

        for (int k = 0; k < group.Count; k++)
            group[k] = group[k] with { Order = k };

        var byId = group.ToDictionary(g => g.Id);
        for (int i = 0; i < updatedList.Count; i++)
        {
            if (byId.TryGetValue(updatedList[i].Id, out var gitem))
                updatedList[i] = gitem;
        }

        return [
            ..updatedList
                .OrderBy(i => i.DealStateOrder)
                .ThenBy(i => i.Order)
        ];
    }
}

public class DealEditSheetDebug(
    IState<bool> isOpen,
    Guid dealId,
    MockPipelineService service,
    IState<ImmutableArray<DealRecord>> deals
    ) : ViewBase
{
    public override object? Build()
    {
        var details = UseState<DealRecord?>();
        var loading = UseState(true);

        UseEffect(async () =>
        {
            var found = await service.GetDeal(dealId);
            details.Set(found);
            loading.Set(false);
        });

        if (loading.Value) return new Loading();

        // Simple form
        return details
            .ToForm()
            .Builder(e => e.InvestorName, e => e.ToTextInput())
            .HandleSubmit(OnSubmit)
            .ToSheet(isOpen, "Edit Deal");

        async Task OnSubmit(DealRecord? modifiedDeal)
        {
            await Task.Delay(500); // Simulate save
        }
    }
}

public class MockPipelineService
{
    private List<DealRecord> _db = new();

    public MockPipelineService()
    {
        // Seed data
        var states = new[] { "New", "Qualified", "Proposition", "Won" };
        var rng = new Random();
        for (int i = 0; i < 20; i++)
        {
            var state = states[rng.Next(states.Length)];
            _db.Add(new DealRecord
            {
                Id = Guid.NewGuid(),
                InvestorName = $"Investor {i}",
                ContactName = $"Contact {i}",
                DealState = state,
                DealStateOrder = Array.IndexOf(states, state),
                Order = i,
                AmountFrom = 1000 * i,
                OwnerName = "Me"
            });
        }
    }

    public async Task<ImmutableArray<DealRecord>> FetchDeals()
    {
        await Task.Delay(2000);
        return [.. _db.OrderBy(d => d.DealStateOrder).ThenBy(d => d.Order)];
    }

    public async Task<DealRecord?> GetDeal(Guid id)
    {
        await Task.Delay(2000);
        return _db.FirstOrDefault(d => d.Id == id);
    }

    public async Task<DealRecord> CreateDeal()
    {
        await Task.Delay(2000);
        var d = new DealRecord
        {
            Id = Guid.NewGuid(),
            InvestorName = $"New Deal {DateTime.Now.Ticks}",
            ContactName = "New Contact",
            DealState = "New",
            DealStateOrder = 0,
            Order = _db.Count(x => x.DealState == "New"),
            OwnerName = "Me"
        };
        _db.Add(d);
        return d;
    }

    public async Task DeleteDeal(Guid id)
    {
        await Task.Delay(2000); // Slow delete
        _db.RemoveAll(d => d.Id == id);
    }

    public async Task MoveDeal(Guid id, string toState, int index)
    {
        await Task.Delay(2000); // Slow move
        var item = _db.FirstOrDefault(d => d.Id == id);
        if (item == null) return;

        // no actual db move needed for race reproduction as long as UI is out of sync or service is slow
    }
}

public record DealRecord
{
    public Guid Id { get; init; }
    public string InvestorName { get; init; } = "";
    public string OwnerName { get; init; } = "";
    public string ContactName { get; init; } = "";
    public string DealState { get; init; } = "";
    public int DealStateOrder { get; init; }
    public float Order { get; init; }
    public int? AmountFrom { get; init; }
    public int? AmountTo { get; init; }

    public string AmountFormatted() => AmountFrom?.ToString("C0") ?? "";
}
