using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.IO.Hashing;

namespace Ivy.Core;

public readonly record struct PathSegment(string Type, string? Key, int Index, bool IsWidget)
{
    public override string ToString()
    {
        return $"{Type}:{Key ?? Index.ToString()}";
    }
}

[DebuggerDisplay("{ToString()}")]
public class TreePath : Stack<PathSegment>
{
    public void Push(IView view, int index)
    {
        Push(new PathSegment(view.GetType().Name!, view.Key, index, false));
    }

    public void Push(IWidget widget, int index)
    {
        Push(new PathSegment(widget.GetType().Name!, widget.Key, index, true));
    }

    public TreePath Clone()
    {
        var clone = new TreePath();
        var segments = ToArray();
        for (int i = segments.Length - 1; i >= 0; i--)
        {
            clone.Push(segments[i]); // PathSegment is a struct, no need to copy fields
        }
        return clone;
    }

    public override string ToString()
    {
        if (Count == 0) return string.Empty;

        // Estimate capacity: avg segment ~15 chars (Type:Key/Index) + separator
        var sb = new StringBuilder(Count * 16);
        bool first = true;
        foreach (var e in this)
        {
            if (!first) sb.Append('>');
            first = false;
            sb.Append(e.Type);
            sb.Append(':');
            if (e.Key is not null) sb.Append(e.Key);
            else sb.Append(e.Index);
        }
        return sb.ToString();
    }

    private static readonly char[] Base32Chars = "abcdefghijklmnopqrstuvwxyz234567".ToCharArray();

    public string GenerateId()
    {
        // Use XxHash64 - extremely fast with excellent distribution
        // 64-bit hash gives collision probability of ~1 in 10^19 for random inputs
        // Even with birthday paradox, 1M items = ~0.000003% collision chance

        var hash = new XxHash64();
        Span<byte> indexBytes = stackalloc byte[4];

        // Hash each segment directly without ToString() allocation
        foreach (var segment in this)
        {
            // Hash the type name
            hash.Append(MemoryMarshal.AsBytes(segment.Type.AsSpan()));

            // Hash separator
            hash.Append([(byte)':']);

            // Hash key or index
            if (segment.Key is not null)
            {
                hash.Append(MemoryMarshal.AsBytes(segment.Key.AsSpan()));
            }
            else
            {
                System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(indexBytes, segment.Index);
                hash.Append(indexBytes);
            }

            // Hash segment separator
            hash.Append([(byte)'>']);
        }

        ulong hashValue = hash.GetCurrentHashAsUInt64();

        // Convert to 10-char base32 string (50 bits of entropy, plenty for uniqueness)
        return string.Create(10, hashValue, static (span, h) =>
        {
            for (int i = 0; i < 10; i++)
            {
                span[i] = Base32Chars[h & 0x1F];
                h >>= 5;
            }
        });
    }
}