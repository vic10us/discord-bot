namespace v10.Data.Abstractions.Interfaces;

public interface IDatabaseSettings
{
    string CollectionName { get; set; }
    string ConnectionString { get; set; }
    string DatabaseName { get; set; }
}
