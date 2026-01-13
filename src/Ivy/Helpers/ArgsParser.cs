namespace Ivy.Helpers;

public class ArgsParser
{
    /*
     * ["--option", "value"] => { "option": "value" }
     * ["-o", "value"] => { "o": "value" }
     * [--foo --bar baz] => { "foo": true, "bar": "baz" }
     */

    public Dictionary<string, object> Parse(string[] args)
    {
        var result = new Dictionary<string, object>();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg.StartsWith("--") || arg.StartsWith("-"))
            {
                string key = arg.TrimStart('-');

                // Check if there's a next item and it's not an option
                if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    result[key] = args[i + 1];
                    i++; // Skip the value in next iteration
                }
                else
                {
                    // Boolean flag
                    result[key] = true;
                }
            }
        }

        return result;
    }

    public T GetValue<T>(Dictionary<string, object> parsedArgs, string key, T defaultValue = default!)
    {
        if (parsedArgs.TryGetValue(key, out var value))
        {
            if (value is T typedValue)
            {
                return typedValue;
            }

            try
            {
                // Try to convert the value to the requested type
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        return defaultValue;
    }
}


