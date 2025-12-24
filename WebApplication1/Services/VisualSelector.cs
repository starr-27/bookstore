using System.Security.Cryptography;
using System.Text;

namespace WebApplication1.Services;

public interface IVisualSelector
{
    string? PickStable(IReadOnlyList<string> candidates, string key);
    string? PickStableByLong(IReadOnlyList<string> candidates, long key);
}

public sealed class VisualSelector : IVisualSelector
{
    public string? PickStable(IReadOnlyList<string> candidates, string key)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var h = HashToUInt(key);
        var idx = (int)(h % (uint)candidates.Count);
        return candidates[idx];
    }

    public string? PickStableByLong(IReadOnlyList<string> candidates, long key)
    {
        if (candidates.Count == 0)
        {
            return null;
        }

        var idx = (int)((ulong)key % (ulong)candidates.Count);
        return candidates[idx];
    }

    private static uint HashToUInt(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return BitConverter.ToUInt32(hash, 0);
    }
}
