using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Stores;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Storage.Admin
{
	public class MongoClientUpdater : MongoStoreUpdaterBase<Client>
	{
		public MongoClientUpdater(IMongoDatabase database) : base(
			database.GetCollection<Client>(MongoClientStore.CollectionName))
		{
		}

		public override Task InsertOrUpdateAsync(Client entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(c => c.ClientId == entity.ClientId, entity, cancellationToken);
	}
}
