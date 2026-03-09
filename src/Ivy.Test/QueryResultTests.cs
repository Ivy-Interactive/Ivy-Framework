using Ivy.Core.Hooks;

namespace Ivy.Test;

public class QueryResultTests
{
    private static readonly QueryMutator<string> EmptyMutator = new(
        static (_, _) => { },
        static () => { },
        static () => { });

    [Fact]
    public void ToTrigger_WithState_ReturnsAfterChangeTrigger()
    {
        var state = new State<QueryResult<string>>(
            new QueryResult<string>("test", Loading: false, Validating: false, Previous: false, EmptyMutator));

        var result = new QueryResult<string>("test", Loading: false, Validating: false, Previous: false, EmptyMutator)
        {
            State = state
        };

        var trigger = result.ToTrigger();

        Assert.NotNull(trigger);
        Assert.Equal(EffectTriggerType.AfterChange, trigger.Type);
        Assert.Same(state, trigger.State);
    }

    [Fact]
    public void ToTrigger_WithoutState_ThrowsInvalidOperationException()
    {
        var result = new QueryResult<string>("test", Loading: false, Validating: false, Previous: false, EmptyMutator);

        var ex = Assert.Throws<InvalidOperationException>(() => result.ToTrigger());
        Assert.Contains("idle", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
