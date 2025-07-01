using System.Reflection;
using Ivy.Shared;

namespace Ivy.Samples.Apps;

public abstract class SampleBase : ViewBase
{
    private readonly Align? _showCodePosition;

    protected SampleBase(Align? showCodePosition = Align.BottomLeft)
    {
        _showCodePosition = showCodePosition;
    }

    protected abstract object? BuildSample();

    public override object? Build()
    {
        var runtimeType = GetType();

        return new Fragment(
            BuildSample(),
            _showCodePosition != null ? FloatingLayerExtensions.Align(new FloatingPanel(new Button("Show Code")
                .Variant(ButtonVariant.Outline)
                .Icon(Icons.Code)
                .Large()
                .BorderRadius(BorderRadius.Full)
                .WithSheet(() => new CodeView(runtimeType), runtimeType.FullName![12..].Replace(".", "/") + ".cs", width: Size.Fraction(1 / 2f))), _showCodePosition.Value) : null
        );
    }
}

public class CodeView(Type type) : ViewBase
{
    public override object? Build()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = type.FullName + ".cs";

        string code;
        using (Stream? stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                return new Exception("Resource not found.");
            }

            using (StreamReader reader = new StreamReader(stream))
            {
                code = reader.ReadToEnd();
            }
        }

        return new Code(code, Languages.Csharp);
    }
}