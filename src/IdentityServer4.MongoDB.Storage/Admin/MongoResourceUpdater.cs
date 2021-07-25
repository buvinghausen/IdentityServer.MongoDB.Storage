using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Options;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Storage.Admin
{
	public class MongoResourceUpdater<T> : MongoStoreUpdaterBase<T> where T : Resource
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoResourceUpdater(ConfigurationStoreOptions options) : this(options.Database,
			options.ResourceCollectionName)
		{
		}

		public MongoResourceUpdater(IMongoDatabase database, string collectionName) : base(database
			.GetCollection<Resource>(collectionName).OfType<T>())
		{
		}

		public override Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(r => r.Name == entity.Name, entity, cancellationToken);
	}
}
