using System;
using MongoDB.Driver;

namespace bot.Features.Database;

public static class MonoDatabaseExtensions
{
    public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase database)
    {
        return database.GetCollection<T>(typeof(T).Name);
    }

    public static IMongoCollection<T> GetCollection<T>(this IMongoDatabase database, string name)
    {
        return database.GetCollection<T>(name);
    }

    public static IMongoCollection<T> GetCollectionWithExpiry<T>(this IMongoDatabase database, string name)
    {
        var indexKeysDefinition = Builders<T>.IndexKeys.Ascending("expiry");
        var indexOptions = new CreateIndexOptions { ExpireAfter = new TimeSpan(0, 0, 60) };
        var indexModel = new CreateIndexModel<T>(indexKeysDefinition, indexOptions);
        var r = database.GetCollection<T>(name);
        r.Indexes.CreateOne(indexModel);
        return r;
    }
}
