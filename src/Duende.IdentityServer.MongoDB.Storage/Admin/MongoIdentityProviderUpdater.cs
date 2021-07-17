using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Admin;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	class MongoIdentityProviderUpdater : MongoStoreUpdaterBase<IdentityProvider>
	{
		public MongoIdentityProviderUpdater(IMongoDatabase database) : base(database.GetCollection<IdentityProvider>("IdentityProviders"))
		{
		}

		public override Task InsertOrUpdateAsync(IdentityProvider entity,
			CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(ip => ip.Scheme == entity.Scheme, entity, cancellationToken);
	}
}
