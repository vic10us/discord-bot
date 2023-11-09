using System.Diagnostics;
using Xunit.Abstractions;

namespace v10.Snowflakes.Tests;

public class ParseTests
{
    private readonly ITestOutputHelper _output;

    public ParseTests(ITestOutputHelper testOutputHelper)
    {
        _output = testOutputHelper;
    }

    [Fact]
    public void Id_Parse_Invalid()
    {
        const long value = 10;

        Assert.Throws<FormatException>(() => Id.Parse(value));
    }

    [Fact]
    public void Id_Parse()
    {
        long id = Id.Create();

        Id.Parse(id);
    }

    [Fact]
    public void Id_TryParse_Invalid()
    {
        const long value = 10;

        bool parse = Id.TryParse(value, out _);

        Assert.False(parse);
    }

    [Fact]
    public void Id_TryParse()
    {
        long id = Id.Create();

        bool parse = Id.TryParse(id, out Id parsed);

        Assert.True(parse);
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void Id_TryParseString()
    {
        long id = Id.Create();

        bool parse = Id.TryParse(id.ToString(), out Id parsed);

        Assert.True(parse);
        Assert.Equal(id, parsed);
    }

    [Fact]
    public void Id_TryParse_Many()
    {
        List<Id> ids = Enumerable.Range(0, 100_000).Select(_ => Id.Create()).ToList();
        List<Id> problematic = [];

        bool failed = false;
        foreach (var id in ids)
        {
            if (!Id.TryParse((long)id, out Id parsed))
            {
                Debug.WriteLine(id);
                problematic.Add(id);
                failed = true;
            }
        }

        Assert.False(failed);
    }

    [Theory]
    [InlineData("1152378737694875699")]
    public void Id_TryParse_String(string id)
    {
        var _ = Id.TryParse(id, out Id parsed);
        _output.WriteLine(parsed.ToString());
        Assert.Equal(id, parsed.ToString());
    }

    [Fact]
    public void Id_TryParse_Problematic()
    {
        _ = Id.Parse(1108047973760811023);
    }
}
