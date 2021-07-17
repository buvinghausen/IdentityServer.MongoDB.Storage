using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer4.Models;

namespace IdentityServer4.MongoDB.Storage.Admin
{
	internal class MongoClientUpdater : MongoStoreUpdaterBase<Client>
	{
		public MongoClientUpdater(ConfigurationStoreOptions options) : base(
			options.Database.GetCollection<Client>(options.ClientCollectionName))
		{
		}

		public override Task InsertOrUpdateAsync(Client entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(c => c.ClientId == entity.ClientId, entity, cancellationToken);
	}
}
