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
public class TreePath
{
    public PathSegment Segment { get; }
    public TreePath? Parent { get; }
    public int Count { get; }

    public TreePath()
    {
        Count = 0;
    }

    private TreePath(PathSegment segment, TreePath? parent)
    {
        Segment = segment;
        Parent = parent;
        Count = (parent?.Count ?? 0) + 1;
    }

    public TreePath Push(IView view, int index)
    {
        return new TreePath(new PathSegment(view.GetType().Name!, view.Key, index, false), this);
    }

    public TreePath Push(IWidget widget, int index)
    {
        return new TreePath(new PathSegment(widget.GetType().Name!, widget.Key, index, true), this);
    }

    public TreePath Clone()
    {
        return this; // Immutable linked list shares state natively!
    }

    public TreePath? Pop()
    {
        return Parent;
    }

    public override string ToString()
    {
        if (Count == 0) return string.Empty;

        var segments = new PathSegment[Count];
        var curr = this;
        for (int i = Count - 1; i >= 0; i--)
        {
            segments[i] = curr.Segment;
            curr = curr.Parent!;
        }

        var sb = new StringBuilder(Count * 16);
        bool first = true;
        foreach (var e in segments)
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
    private string? _cachedId;

    public string GenerateId()
    {
        if (_cachedId != null) return _cachedId;

        // Use XxHash64 - extremely fast with excellent distribution
        // 64-bit hash gives collision probability of ~1 in 10^19 for random inputs
        // Even with birthday paradox, 1M items = ~0.000003% collision chance

        var hash = new XxHash64();
        Span<byte> indexBytes = stackalloc byte[4];

        // We must hash from root to leaf to maintain identical sequence with `Stack`.
        // Since we are a linked list (leaf to root), we extract sequentially.
        var segments = new PathSegment[Count];
        var curr = this;
        for (int i = Count - 1; i >= 0; i--)
        {
            segments[i] = curr.Segment;
            curr = curr.Parent!;
        }

        foreach (var segment in segments)
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
        _cachedId = string.Create(10, hashValue, static (span, h) =>
        {
            for (int i = 0; i < 10; i++)
            {
                span[i] = Base32Chars[h & 0x1F];
                h >>= 5;
            }
        });

        return _cachedId;
    }
}