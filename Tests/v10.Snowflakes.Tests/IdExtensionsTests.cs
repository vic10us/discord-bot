using Xunit.Abstractions;

namespace v10.Snowflakes.Tests;

public class IdExtensionsTests
{
    private readonly ITestOutputHelper _output;

    public IdExtensionsTests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Fact]
    public void Id_ToDateTimeOffset()
    {
        Id id = Id.Create();
        DateTimeOffset timeStamp = id.ToDateTimeOffset();
        DateTimeOffset now = DateTimeOffset.Now;
        TimeSpan delta = now - timeStamp;

        Assert.True(delta.Seconds <= 1);
    }

    [Fact]
    public void Id_ToUnixTimeMilliseconds()
    {
        Id id = Id.Create();
        long timestamp = id.ToUnixTimeMilliseconds();
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        Assert.True(now - timestamp < 100);
    }

    [Fact]
    public void Id_IsValid()
    {
        Id id = Id.Create();
        bool isValid = id.IsSnowflake();

        Assert.True(isValid);
    }

    [Fact]
    public void Id_ToStringIdentifier_ProducesValidId()
    {
        Id id = Id.Create();
        string s = id.ToStringIdentifier();

        Assert.NotEqual(default, s);
    }
}
