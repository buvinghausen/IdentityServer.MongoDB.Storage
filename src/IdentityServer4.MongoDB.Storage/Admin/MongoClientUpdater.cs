using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Options;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Storage.Admin
{
	public sealed class MongoClientUpdater : MongoStoreUpdaterBase<Client>
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoClientUpdater(ConfigurationStoreOptions options) : this(options.Database,
			options.ClientCollectionName)
		{
		}

		public MongoClientUpdater(IMongoDatabase database, string collectionName) : base(
			database.GetCollection<Client>(collectionName))
		{
		}

		public override Task InsertOrUpdateAsync(Client entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(c => c.ClientId == entity.ClientId, entity, cancellationToken);
	}
}
