using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	internal class MongoIdentityProviderUpdater : MongoStoreUpdaterBase<IdentityProvider>
	{
		public MongoIdentityProviderUpdater(ConfigurationStoreOptions options) : base(
			options.Database.GetCollection<IdentityProvider>(options.IdentityProviderCollectionName))
		{
		}

		public override Task InsertOrUpdateAsync(IdentityProvider entity,
			CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(ip => ip.Scheme == entity.Scheme, entity, cancellationToken);
	}
}
