using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PokeNet.Application.Settings;
using PokeNet.Domain.Entities;

namespace PokeNet.Infrastructure.Context
{
    public class MongoDbContext
    {
        public IMongoDatabase Database { get; }

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            Database = client.GetDatabase(settings.Value.DatabaseName);

        }


        public IMongoCollection<Usuario> Usuarios => Database.GetCollection<Usuario>("usuarios");


        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }

    }
}
