using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Ivy.Analyser.Analyzers.HookUsageAnalyzer>;

namespace Ivy.Analyser.Test
{
    public class HookUsageAnalyzerTests
    {
        [Fact]
        public async Task ValidHookInBuildMethod()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state = UseState(false);
        UseEffect(() => { });
        var memo = UseMemo(() => 42);
        var reference = UseRef<string>();
        var context = UseContext<MyContext>();
        var callback = UseCallback(() => { });
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
    protected void UseEffect(Action effect) { }
    protected T UseMemo<T>(Func<T> factory) => default!;
    protected Ref<T> UseRef<T>() => default!;
    protected T UseContext<T>() => default!;
    protected Action UseCallback(Action callback) => default!;
}

public class Button { }
public class MyContext { }
public class Ref<T> { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInLambdaShouldFail()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var handler = (Event<Button> e) =>
        {
            var s = {|IVYHOOK001:UseState(false)|};
        };
        return new Button().OnClick(handler);
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button 
{
    public Button OnClick(Action<Event<Button>> handler) => this;
}
public class Event<T> { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInLocalFunctionShouldFail()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        void LocalFunction()
        {
            var s = {|IVYHOOK001:UseState(false)|};
        }
        
        LocalFunction();
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInAnotherMethodShouldFail()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        Initialize();
        return new Button();
    }
    
    private void Initialize()
    {
        var s = {|IVYHOOK001:UseState(false)|};
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInAnonymousMethodShouldFail()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        Action action = delegate()
        {
            var s = {|IVYHOOK001:UseState(false)|};
        };
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookOutsideClassShouldFail()
        {
            var test = @"
using System;

public class TestClass
{
    public void SomeMethod()
    {
        {|IVYHOOK001:UseState(false)|};
    }
    
    protected T UseState<T>(T initialValue) => default!;
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInIfStatementShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(0);
        
        if (state1 > 0)
        {
            var state3 = {|IVYHOOK002:UseState(true)|};
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInTernaryOperatorShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var condition = true;
        var result = condition ? {|IVYHOOK002:UseState(0)|} : {|IVYHOOK002:UseState(1)|};
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInForLoopShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        for (int i = 0; i < 5; i++)
        {
            var state = {|IVYHOOK003:UseState(i)|};
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInForeachLoopShouldWarn()
        {
            var test = @"
using System;
using System.Collections.Generic;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var items = new List<int> { 1, 2, 3 };
        
        foreach (var item in items)
        {
            var state = {|IVYHOOK003:UseState(item)|};
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInWhileLoopShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var condition = true;
        
        while (condition)
        {
            var state = {|IVYHOOK003:UseState(false)|};
            condition = false;
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInDoWhileLoopShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var condition = true;
        
        do
        {
            var state = {|IVYHOOK003:UseState(false)|};
            condition = false;
        } while (condition);
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInSwitchStatementShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var value = 1;
        
        switch (value)
        {
            case 1:
                var state = {|IVYHOOK004:UseState(false)|};
                break;
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task MultipleHooksInValidPositions()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(0);
        var state2 = UseState(""hello"");
        var state3 = UseState(true);
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookWithMemberAccess()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state = this.UseState(false);
        base.UseEffect(() => { });
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
    protected void UseEffect(Action effect) { }
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        // Happy Path Tests

        [Fact]
        public async Task AllHookTypesUsedCorrectly()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state = UseState(0);
        UseEffect(() => { });
        var memo = UseMemo(() => 42);
        var reducer = UseReducer((s, a) => s, 0);
        var refVal = UseRef(100);
        var signal = UseSignal<MySignal, int, int>();
        var trigger = UseTrigger<int>((open, value) => null);
        var service = UseService<MyService>();
        var args = UseArgs<MyArgs>();
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
    protected void UseEffect(Action effect) { }
    protected T UseMemo<T>(Func<T> factory) => default!;
    protected (T, Func<string, T>) UseReducer<T>(Func<T, string, T> reducer, T initialState) => default!;
    protected IState<T> UseRef<T>(T initialValue) => default!;
    protected ISignal<TInput, TOutput> UseSignal<TSignal, TInput, TOutput>() => default!;
    protected (object?, Action<T>) UseTrigger<T>(Func<IState<bool>, T, object?> factory) => default!;
    protected T UseService<T>() => default!;
    protected T? UseArgs<T>() where T : class => default!;
}

public class Button { }
public class MySignal { }
public class MyService { }
public class MyArgs { }
public interface ISignal<TInput, TOutput> { }
public interface IState<T> { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HooksWithFactoryFunctions()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(() => new ExpensiveObject());
        var state2 = UseState(() => 42);
        var memo = UseMemo(() => ComputeValue());
        return new Button();
    }

    private int ComputeValue() => 42;
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(Func<T> factory) => default!;
    protected T UseMemo<T>(Func<T> factory) => default!;
}

public class Button { }
public class ExpensiveObject { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HooksInCorrectOrderWithConditionals()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(0);
        var state2 = UseState(""hello"");
        var state3 = UseState(true);
        
        if (state1 > 0)
        {
            // Using state values is fine, just not calling hooks
            var value = state1;
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HooksInCorrectOrderWithLoops()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(0);
        var state2 = UseState(""done"");
        var items = new[] { 1, 2, 3 };
        
        foreach (var item in items)
        {
            // Using state values is fine, just not calling hooks
            var value = item;
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HooksUsedInExpressionsAtTopLevel()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(0);
        var state2 = UseState(10);
        UseEffect(() => { });
        var value = state1.Value;
        var count = state2.Value + 5;
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected IState<T> UseState<T>(T initialValue) => default!;
    protected void UseEffect(Action effect) { }
}

public interface IState<T> { T Value { get; } }
public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        // Sad Path Tests

        [Fact]
        public async Task HookInElseClauseShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        if (true)
        {
            var state1 = {|IVYHOOK002:UseState(0)|};
        }
        else
        {
            var state2 = {|IVYHOOK002:UseState(1)|};
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInElseIfClauseShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        if (true)
        {
            var state1 = {|IVYHOOK002:UseState(0)|};
        }
        else if (false)
        {
            var state2 = {|IVYHOOK002:UseState(1)|};
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInNestedIfStatementShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        if (true)
        {
            if (false)
            {
                var state = {|IVYHOOK002:UseState(0)|};
            }
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInNestedLoopShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                var state = {|IVYHOOK003:UseState(i + j)|};
            }
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInSwitchWithMultipleCasesShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var value = 1;
        
        switch (value)
        {
            case 1:
                var state1 = {|IVYHOOK004:UseState(false)|};
                break;
            case 2:
                var state2 = {|IVYHOOK004:UseState(true)|};
                break;
            default:
                var state3 = {|IVYHOOK004:UseState(0)|};
                break;
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task MultipleViolationsInSameMethod()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        if (true)
        {
            var state1 = {|IVYHOOK002:UseState(0)|};
        }
        
        for (int i = 0; i < 5; i++)
        {
            var state2 = {|IVYHOOK003:UseState(i)|};
        }
        
        switch (1)
        {
            case 1:
                var state3 = {|IVYHOOK004:UseState(false)|};
                break;
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInTryCatchBlockShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        try
        {
            var state = {|IVYHOOK002:UseState(0)|};
        }
        catch
        {
            var state2 = {|IVYHOOK002:UseState(1)|};
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInUsingStatementShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        using (var disposable = new MyDisposable())
        {
            var state = {|IVYHOOK002:UseState(0)|};
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
public class MyDisposable : IDisposable
{
    public void Dispose() { }
}
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookInLambdaInsideConditionalShouldFail()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        if (true)
        {
            Action action = () =>
            {
                var state = {|IVYHOOK001:UseState(0)|};
            };
        }
        
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookAfterNonHookStatementShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var x = SomeMethod();
        var state = {|IVYHOOK005:UseState(0)|};
        return new Button();
    }

    private int SomeMethod() => 42;
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookAfterReturnStatementShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        if (true)
        {
            return new Button();
        }
        var state = {|IVYHOOK005:UseState(0)|};
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task MultipleHooksAtTopShouldPass()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var state1 = UseState(0);
        var state2 = UseState(""hello"");
        UseEffect(() => { });
        var memo = UseMemo(() => 42);
        
        var x = SomeMethod();
        return new Button();
    }

    private int SomeMethod() => 42;
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
    protected void UseEffect(Action effect) { }
    protected T UseMemo<T>(Func<T> factory) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookStoredInFieldShouldError()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    private object? _count;

    public override object? Build()
    {
        _count = {|IVYHOOK006:UseState(0)|};
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookStoredInFieldWithThisShouldError()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    private object? _count;

    public override object? Build()
    {
        this._count = {|IVYHOOK006:UseState(0)|};
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookStoredInPropertyShouldError()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public object? Count { get; set; }

    public override object? Build()
    {
        Count = {|IVYHOOK006:UseState(0)|};
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookStoredInFieldWithCoalesceAssignmentShouldError()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    private object? _count;

    public override object? Build()
    {
        _count ??= {|IVYHOOK006:UseState(0)|};
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookStoredInLocalVariableShouldNotError()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        var local = UseState(0);
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookDiscardedShouldNotError()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        UseState(0);
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }

        [Fact]
        public async Task HookAfterVariableAssignmentShouldWarn()
        {
            var test = @"
using System;

public class TestView : ViewBase
{
    public override object? Build()
    {
        int x = 10;
        var state = {|IVYHOOK005:UseState(0)|};
        return new Button();
    }
}

public abstract class ViewBase
{
    public abstract object? Build();
    protected T UseState<T>(T initialValue) => default!;
}

public class Button { }
";

            await VerifyCS.VerifyAnalyzerAsync(test);
        }
    }
}