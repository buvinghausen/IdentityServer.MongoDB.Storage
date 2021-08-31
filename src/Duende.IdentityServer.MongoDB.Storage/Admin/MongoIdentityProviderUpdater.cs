using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	public sealed class MongoIdentityProviderUpdater : MongoStoreUpdaterBase<IdentityProvider>
	{
		public MongoIdentityProviderUpdater(ConfigurationStoreOptions options) : this(options.Database,
			options.IdentityProviderCollectionName)
		{
		}

		public MongoIdentityProviderUpdater(IMongoDatabase database, string collectionName) : base(
			database.GetCollection<IdentityProvider>(collectionName))
		{
		}

		public override Task
			InsertOrUpdateAsync(IdentityProvider entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(ip => ip.Scheme == entity.Scheme, entity, cancellationToken);
	}
}
