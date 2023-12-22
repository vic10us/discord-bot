using Xunit.Abstractions;

namespace v10.Snowflakes.Tests;

public class Base62Tests
{
    private readonly ITestOutputHelper _output;

    public Base62Tests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Theory]
    [InlineData("BXH4N4a92Ft", 1152378737694875699)]
    [InlineData("B9", 123L)]
    [InlineData("Idgn", 2020123L)]
    [InlineData("A", 0L)]
    public void Base62Encode(string expected, long value)
    {
        string encoded = value.Base62Encode();
        _output.WriteLine($"{value}: {encoded}");
        Assert.Equal(expected, encoded);
    }

    [Theory]
    [InlineData("BXH4N4a92Ft", 1152378737694875699)]
    [InlineData("B9", 123L)]
    [InlineData("Idgn", 2020123L)]
    [InlineData("A", 0L)]
    public void Base62Decode(string encoded, long expected)
    {
        long decoded = encoded.Base62Decode();
        _output.WriteLine($"{encoded}: {decoded}");
        Assert.Equal(expected, decoded);
    }
}
