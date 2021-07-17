using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Stores;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	public class MongoClientUpdater : MongoStoreUpdaterBase<Client>
	{
		public MongoClientUpdater(IMongoDatabase database) : base(
			database.GetCollection<Client>(CollectionNames.ClientCollectionName))
		{
		}

		public override Task InsertOrUpdateAsync(Client entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(c => c.ClientId == entity.ClientId, entity, cancellationToken);
	}
}
