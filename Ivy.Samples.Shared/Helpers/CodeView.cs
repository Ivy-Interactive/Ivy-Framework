using Ivy;
using Ivy.Shared;

namespace Ivy.Samples.Shared.Helpers;

public class CodeView(Type type) : ViewBase
{
    public override object? Build()
    {
        var assembly = typeof(CodeView).Assembly;
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