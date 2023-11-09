using System.Diagnostics;

namespace v10.Snowflakes;

public interface IDateTimeProvider
{
    public DateTimeOffset Now { get; }
}

public class DefaultDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.Now;
}

public class UtcDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset Now => DateTimeOffset.UtcNow;
}

internal static class MonotonicTimer
{
    private static readonly long s_epoch = GetInstanceEpoch();
    private static readonly Stopwatch s_stopwatch = Stopwatch.StartNew();

    internal static DateTimeOffset Epoch => new DateTimeOffset(2015, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
    public static IDateTimeProvider DateTimeProvider => new UtcDateTimeProvider();

    public static long ElapsedMilliseconds => s_epoch + s_stopwatch.ElapsedMilliseconds;

    private static long GetInstanceEpoch()
    {
        var x = DateTimeProvider.Now;
        TimeSpan deltaNow = x - Epoch;

        return (long)deltaNow.TotalMilliseconds;
    }
}
