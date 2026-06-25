namespace Ivy.Plugin.HelloWorld;

public class HelloWorldGreeter(string greeting, bool enthusiastic) : IGreeter
{
    public string Greet(string name) =>
        enthusiastic ? $"{greeting}, {name}!!!" : $"{greeting}, {name}.";
}
