using Ivy.Core.Hooks;

namespace Ivy.Test;

public class FormSubmitStrategyTests
{
    public record SimpleModel(string Name, int Age);

    [Fact]
    public void FormSubmitStrategy_HasExpectedValues()
    {
        Assert.Equal(0, (int)FormSubmitStrategy.OnSubmit);
        Assert.Equal(1, (int)FormSubmitStrategy.OnBlur);
        Assert.Equal(2, (int)FormSubmitStrategy.OnChange);
    }

    [Fact]
    public void FormBuilder_DefaultSubmitStrategy_IsOnSubmit()
    {
        var state = new State<SimpleModel>(new SimpleModel("", 0));
        var builder = new FormBuilder<SimpleModel>(state);
        Assert.Equal(FormSubmitStrategy.OnSubmit, builder._submitStrategy);
    }

    [Fact]
    public void FormBuilder_SubmitStrategy_SetsStrategy()
    {
        var state = new State<SimpleModel>(new SimpleModel("", 0));
        var builder = new FormBuilder<SimpleModel>(state);

        var result = builder.SubmitStrategy(FormSubmitStrategy.OnBlur);

        Assert.Same(builder, result);
        Assert.Equal(FormSubmitStrategy.OnBlur, builder._submitStrategy);
    }

    [Fact]
    public void FormBuilder_SubmitStrategy_OnChange_SetsStrategy()
    {
        var state = new State<SimpleModel>(new SimpleModel("", 0));
        var builder = new FormBuilder<SimpleModel>(state);

        builder.SubmitStrategy(FormSubmitStrategy.OnChange);

        Assert.Equal(FormSubmitStrategy.OnChange, builder._submitStrategy);
    }

    [Fact]
    public void FormBuilder_SubmitStrategy_FluentChaining()
    {
        var state = new State<SimpleModel>(new SimpleModel("", 0));
        var builder = new FormBuilder<SimpleModel>(state)
            .SubmitStrategy(FormSubmitStrategy.OnBlur)
            .ValidationStrategy(FormValidationStrategy.OnBlur);

        Assert.Equal(FormSubmitStrategy.OnBlur, builder._submitStrategy);
        Assert.Equal(FormValidationStrategy.OnBlur, builder._validationStrategy);
    }
}
