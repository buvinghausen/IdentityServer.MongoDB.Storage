using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	public class MongoClientUpdater : MongoStoreUpdaterBase<Client>
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
