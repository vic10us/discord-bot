namespace bot.Configuration.Models;

public class DiscordBotDatabaseSettings : IDatabaseSettings
{
    public string CollectionName { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}
