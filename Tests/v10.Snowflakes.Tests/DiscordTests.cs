using Newtonsoft.Json;
using Xunit.Abstractions;

namespace v10.Snowflakes.Tests;

public class DiscordTests(ITestOutputHelper output)
{
    private readonly ITestOutputHelper _output = output;

    [Fact]
    public void DiscordSnowFlake_FromId()
    {
        _ = Id.TryParse(1152378737694875699, out Id parsed);
        Discord snowflake = parsed.ToDiscordId();
        _output.WriteLine(JsonConvert.SerializeObject(snowflake));
        snowflake.Increment++;
        Assert.Equal(1152378737694875700, snowflake.ToId());
        _output.WriteLine(JsonConvert.SerializeObject(snowflake));
    }
}
