using Microsoft.Extensions.Configuration;

namespace v10.Services.DadJokes;

public class DadJokeConfig : IDadJokeServiceConfiguration
{
  public string BaseUrl { get; set; } = "";
  public string ConfigurationKey { get; set; } = "DadJokes";

  public DadJokeConfig(IConfiguration config)
  {
    config?.Bind($"{ConfigurationKey}", this);
  }

  public Task Validate()
  {
    return Task.CompletedTask;
  }
}
