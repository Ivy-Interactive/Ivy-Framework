using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Testing;
using Xunit;
using VerifyCS = Microsoft.CodeAnalysis.CSharp.Testing.XUnit.AnalyzerVerifier<
    Ivy.Analyser.Analyzers.HookUsageAnalyzer>;

namespace Ivy.Analyser.Tests
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
        
        if (state1 > 0)
        {
            var state3 = UseState(true);
        }
        
        for (int i = 0; i < 5; i++)
        {
            var state4 = UseState(i);
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
    }
}