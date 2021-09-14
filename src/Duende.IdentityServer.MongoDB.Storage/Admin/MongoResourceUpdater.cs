using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin;

public sealed class MongoResourceUpdater<T> : MongoStoreUpdaterBase<T> where T : Resource
{
	// ReSharper disable once SuggestBaseTypeForParameter
	public MongoResourceUpdater(ConfigurationStoreOptions options) : this(options.Database,
		options.ResourceCollectionName)
	{
	}

	public MongoResourceUpdater(IMongoDatabase database, string collectionName) :
		base(database.GetCollection<Resource>(collectionName).OfType<T>())
	{
	}

	public override Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default) =>
		InsertOrUpdateAsync(r => r.Name == entity.Name, entity, cancellationToken);
}
