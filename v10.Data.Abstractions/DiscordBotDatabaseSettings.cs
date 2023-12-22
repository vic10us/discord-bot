using v10.Data.Abstractions.Interfaces;

namespace v10.Data.Abstractions;

public class DiscordBotDatabaseSettings : IDatabaseSettings
{
    public string CollectionName { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
}
