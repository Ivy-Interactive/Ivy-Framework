using System.Collections;
using System.Diagnostics;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Ivy;

public static class Utils
{
    public static string? NullIfEmpty(this string? input) => string.IsNullOrWhiteSpace(input) ? null : input;

    public static Type? GetCollectionTypeParameter(this Type type)
    {
        if (type == null) return null;

        // Handle arrays
        if (type.IsArray)
        {
            return type.GetElementType();
        }

        // Handle Dictionary separately if you want both key and value types
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
        {
            return typeof(KeyValuePair<,>).MakeGenericType(type.GetGenericArguments());
        }

        // Handle generic collections
        if (type.IsGenericType)
        {
            var genericArgs = type.GetGenericArguments();
            if (genericArgs.Length == 1)
            {
                return genericArgs[0];
            }
        }

        // Try to infer from IEnumerable<T>
        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableInterface?.GetGenericArguments()[0];
    }

    public static bool IsCollectionType(this Type? type)
    {
        if (type == null) return false;

        // Handle arrays
        if (type.IsArray) return true;

        // Handle common generic collection types
        if (type.IsGenericType)
        {
            var genericTypeDef = type.GetGenericTypeDefinition();
            if (genericTypeDef == typeof(List<>) ||
                genericTypeDef == typeof(IList<>) ||
                genericTypeDef == typeof(IEnumerable<>) ||
                genericTypeDef == typeof(ICollection<>))
            {
                return true;
            }
        }

        // Handle non-generic collections like ArrayList, Hashtable, etc.
        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            return true;
        }

        return false;
    }

    public static void PrintDetailedException(Exception? ex)
    {
        while (ex != null)
        {
            Console.WriteLine($"Exception Type:\n{ex.GetType().FullName}");
            Console.WriteLine($"Message:\n{ex.Message}");
            Console.WriteLine($"Source:\n{ex.Source}");
            Console.WriteLine($"Target Site:\n {ex.TargetSite}");
            Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
            Console.WriteLine(new string('-', 80));
            ex = ex.InnerException;
        }
    }

    public static bool IsPortInUse(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return false;
        }
        catch (SocketException)
        {
            return true;
        }
    }

    public static ExpandoObject[] ToExpando(this IEnumerable<IDictionary<string, object>> records) =>
        records.Select(r => r.ToExpando()).ToArray();

    public static ExpandoObject ToExpando(this IDictionary<string, object> dictionary)
    {
        var expando = new ExpandoObject();
        IDictionary<string, object> expandoDict = expando as IDictionary<string, object>;
        foreach (var kvp in dictionary)
        {
            if (kvp.Value is IDictionary<string, object> nested)
                expandoDict[kvp.Key] = nested.ToExpando();
            else if (kvp.Value is IEnumerable<object> list)
                expandoDict[kvp.Key] = list.Select(item => item is IDictionary<string, object> d ? d.ToExpando() : item).ToList();
            else
                expandoDict[kvp.Key] = kvp.Value;
        }
        return expando;
    }

    //todo: this needs a rename
    public static async Task<List<T>> ToListAsync2<T>(
        this IQueryable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        if (source is IAsyncEnumerable<T> asyncEnumerable)
        {
            var list = new List<T>();
            await foreach (var item in asyncEnumerable.WithCancellation(cancellationToken))
            {
                list.Add(item);
            }
            return list;
        }

        return Task.FromResult(source.ToList()).Result;
    }

    public static async Task<T[]> ToArrayAsync<T>(
        this IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var list = new List<T>();
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            list.Add(item);
        }
        return list.ToArray();
    }

    public static Action Before(this Action action, Action before)
    {
        return () =>
        {
            before();
            action();
        };
    }

    public static Action After(this Action action, Action after)
    {
        return () =>
        {
            action();
            after();
        };
    }

    public static string TitleCaseToCamelCase(string titleCase)
    {
        if (string.IsNullOrWhiteSpace(titleCase))
            return string.Empty;

        string camelCase = char.ToLower(titleCase[0]) + titleCase[1..];

        return camelCase;
    }

    public static string CamelCaseToTitleCase(string camelCase)
    {
        if (string.IsNullOrWhiteSpace(camelCase))
            return string.Empty;

        string titleCase = char.ToUpper(camelCase[0]) + camelCase[1..];

        return titleCase;
    }

    /// <summary>
    /// FooBar => Foo Bar
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string? SplitPascalCase(string? input)
    {
        if (input == null) return null;
        //string[] words = Regex.Matches(input, "([A-Z]+(?![a-z])|[A-Z][a-z]+|[0-9]+|[a-z]+)")
        string[] words = Regex.Matches(input, "([A-Z]+[a-z]+|[0-9]+|[a-z]+|[A-Z]+)")
            //.OfType<Match>()
            .Select(m => m.Value)
            .ToArray();
        return string.Join(" ", words);
    }

    /// <summary>
    /// FooBar => foo-bar
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string TitleCaseToFriendlyUrl(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // if (input.EndsWith("app", StringComparison.InvariantCultureIgnoreCase))
        // {
        //     input = input[..^3];
        // }

        bool hadUnderscore = input.StartsWith("_");
        if (hadUnderscore)
        {
            input = input[1..];
        }

        StringBuilder sb = new();

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
            {
                sb.Append('-');
            }

            sb.Append(char.ToLower(input[i]));
        }

        if (hadUnderscore)
        {
            sb.Insert(0, '_');
        }

        return sb.ToString();
    }

    /// <summary>
    /// FooBarApp => Foo Bar
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string TitleCaseToReadable(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        if (input.EndsWith("app", StringComparison.InvariantCultureIgnoreCase))
        {
            input = input[..^3];
        }

        StringBuilder sb = new();

        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
            {
                sb.Append(' ');
            }

            sb.Append(input[i]);
        }

        return sb.ToString();
    }

    public static string TrimEnd(this string source, string value)
    {
        if (!source.EndsWith(value))
            return source;
        return source.Remove(source.LastIndexOf(value));
    }

    public static bool IsNullable(this Type type)
    {
        if (type == null) { return false; }
        return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    public static int SuggestPrecision(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return 4;
            default:
                return 0;
        }
    }

    public static double SuggestStep(this Type type)
    {
        return 1.0;
    }

    public static double SuggestMin(this Type type)
    {
        return 0.0;
    }

    public static double SuggestMax(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Decimal:
                return Convert.ToDouble(decimal.MaxValue);
            case TypeCode.Double:
                return double.MaxValue;
            case TypeCode.Single:
                return float.MaxValue;
            default:
                return int.MaxValue;
        }
    }

    public static bool IsDate(this Type type)
    {
        if (type == null) { return false; }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        return type == typeof(DateTime) || type == typeof(DateTimeOffset);
    }

    public static bool IsNumeric(this Type type)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            type = type.GetGenericArguments()[0];
        }

        switch (Type.GetTypeCode(type))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }

    public static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
               || type.IsEnum
               || type == typeof(string)
               || IsNumeric(type)
               || type == typeof(DateTime)
               || type == typeof(DateTimeOffset)
               || type == typeof(TimeSpan)
               || type == typeof(Guid);
    }

    public static bool IsObservable(object observable)
    {
        return observable
            .GetType()
            .GetInterfaces()
            .Any(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IObservable<>)
            );
    }

    public static string GetNameFromMemberExpression(Expression expression)
    {
        while (true)
        {
            switch (expression)
            {
                case MemberExpression memberExpression:
                    return memberExpression.Member.Name;
                case UnaryExpression unaryExpression:
                    expression = unaryExpression.Operand;
                    continue;
            }

            throw new ArgumentException("Invalid expression.");
        }
    }

    public static bool IsNullableType(this Type type)
    {
        return Nullable.GetUnderlyingType(type) != null;
    }

    public static bool IsValidRequired(object? e)
    {
        return e switch
        {
            null => false,
            Guid guid => guid != Guid.Empty,
            DateTime dt => dt != DateTime.MinValue,
            string str => !string.IsNullOrWhiteSpace(str),
            int i => i != 0,
            double i => i != 0.0,
            _ => true
        };
    }

    public static string EatRight(this string input, char food)
    {
        return EatRight(input, c => c == food);
    }

    public static string EatRight(this string input, Func<char, bool> foodType)
    {
        if (string.IsNullOrEmpty(input)) return input;
        int i = input.Length - 1;
        while (i >= 0)
        {
            if (foodType(input[i]))
            {
                i--;
            }
            else
            {
                break;
            }
        }
        return input.Substring(0, i + 1);
    }

    public static string EatRight(this string input, string food, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(food)) return input;

        int cursor = input.Length;
        while (true)
        {
            if (cursor - food.Length >= 0)
            {
                if (input.Substring(cursor - food.Length, food.Length).Equals(food, stringComparison))
                {
                    cursor = cursor - food.Length;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return input.Substring(0, cursor);
    }

    public static string EatLeft(this string input, char food)
    {
        return EatLeft(input, c => c == food);
    }

    public static string EatLeft(this string input, Func<char, bool> foodType)
    {
        if (string.IsNullOrEmpty(input)) return input;
        int i = 0;
        while (i < input.Length)
        {
            if (foodType(input[i]))
            {
                i++;
            }
            else
            {
                break;
            }
        }
        return input.Substring(i);
    }

    public static string EatLeft(this string input, string food, StringComparison stringComparison = StringComparison.CurrentCulture)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(food)) return input;

        int cursor = 0;
        int n = input.Length;
        while (true)
        {
            if (cursor + food.Length < n)
            {
                if (input.Substring(cursor, food.Length).Equals(food, stringComparison))
                {
                    cursor = cursor + food.Length;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        return input.Substring(cursor, n - cursor);
    }

    public static Exception GetInnerMostException(Exception exception)
    {
        while (exception.InnerException != null)
        {
            exception = exception.InnerException;
        }

        return exception;
    }

    public static void KillProcessUsingPort(int port)
    {
        //Console.WriteLine($"Trying to kill the process using port {port}...");

        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            throw new NotSupportedException("This method is only supported on Windows.");

        var netstat = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "netstat",
                Arguments = "-ano",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
        netstat.Start();
        string output = netstat.StandardOutput.ReadToEnd();
        netstat.WaitForExit();

        var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
        var regex = new Regex(@"\s+");

        foreach (var line in lines)
        {
            if (!line.Trim().StartsWith("TCP"))
                continue;
            var parts = regex.Split(line.Trim());
            if (parts.Length < 5)
                continue;
            string localAddress = parts[1];
            string pidStr = parts[4];
            int colonIndex = localAddress.LastIndexOf(':');
            if (colonIndex == -1)
                continue;
            if (!int.TryParse(localAddress[(colonIndex + 1)..], out int linePort) || linePort != port) continue;
            if (!int.TryParse(pidStr, out int pid)) continue;
            if (pid == 0) continue;
            try
            {
                Process.GetProcessById(pid).Kill();
                //Console.WriteLine($"Killed process {pid} using port {port}");
            }
            catch (Exception)
            {
                //ignore
            }
        }
    }

    public static void OpenBrowser(string localUrl)
    {
        if (string.IsNullOrWhiteSpace(localUrl))
            throw new ArgumentNullException(nameof(localUrl));

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {localUrl}") { CreateNoWindow = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", localUrl);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", localUrl);
        }
    }

    public static bool IsEmptyContent(object? obj)
    {
        if (obj == null)
        {
            return true;
        }

        if (obj is string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }

        if (obj is bool b)
        {
            return !b;
        }

        return false;
    }
}