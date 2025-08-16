using System.Reflection;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Apps;

public abstract class SampleBase : ViewBase
{
    private readonly Align? _showCodePosition;
    private readonly Thickness? _showCodeOffset;

    protected SampleBase(Align? showCodePosition = Align.BottomRight, Thickness? showCodeOffset = null)
    {
        _showCodePosition = showCodePosition;
        _showCodeOffset = showCodeOffset ?? new Thickness(0, 0, 15, 2);
    }

    protected abstract object? BuildSample();

    public override object? Build()
    {
        var runtimeType = GetType();

        return new Fragment(
            BuildSample(),
            _showCodePosition != null ? new FloatingPanel(new Button("Show Code")
                .Variant(ButtonVariant.Outline)
                .Icon(Icons.Code)
                .Large()
                .BorderRadius(BorderRadius.Full)
                .WithSheet(() => new CodeView(runtimeType), runtimeType.FullName![12..].Replace(".", "/") + ".cs", width: Size.Fraction(1 / 2f)))
                .Align(_showCodePosition.Value)
                .Offset(_showCodeOffset)
                : null
        );
    }
}

public class CodeView(Type type) : ViewBase
{
    public override object? Build()
    {
        var assembly = typeof(SampleBase).Assembly;
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