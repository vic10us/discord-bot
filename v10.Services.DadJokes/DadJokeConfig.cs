namespace v10.Services.DadJokes;

public class DadJokeConfig : IDadJokeServiceConfiguration
{
    public string BaseUrl { get; set; } = "";
    public string ConfigurationKey { get; set; } = "DadJokes";
}
