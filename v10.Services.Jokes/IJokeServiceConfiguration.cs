namespace v10.Services.Jokes;

public interface IJokeServiceConfiguration
{
  string ConfigurationKey { get; set; }
  Task Validate();
}