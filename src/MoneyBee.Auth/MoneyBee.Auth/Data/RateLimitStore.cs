using System.Collections.Concurrent;

namespace MoneyBee.Auth.Data;

public class RateLimitStore
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly int _limit;
    private readonly TimeSpan _window;

    public RateLimitStore(int limit = 100, int windowSeconds = 60)
    {
        _limit = limit;
        _window = TimeSpan.FromSeconds(windowSeconds);
    }

    public bool IsAllowed(string key, out int remaining, out DateTime resetTime)
    {
        var now = DateTime.UtcNow;
        var entry = _entries.AddOrUpdate(
            key,
            _ => new RateLimitEntry { Count = 1, WindowStart = now },
            (_, existing) =>
            {
                if (now - existing.WindowStart > _window)
                {
                    return new RateLimitEntry { Count = 1, WindowStart = now };
                }
                existing.Count++;
                return existing;
            });

        resetTime = entry.WindowStart.Add(_window);
        remaining = Math.Max(0, _limit - entry.Count);
        return entry.Count <= _limit;
    }

    private class RateLimitEntry
    {
        public int Count { get; set; }
        public DateTime WindowStart { get; set; }
    }
}
