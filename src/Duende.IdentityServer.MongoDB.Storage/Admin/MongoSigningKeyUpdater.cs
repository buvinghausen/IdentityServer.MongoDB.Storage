using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	internal class MongoSigningKeyUpdater : MongoStoreUpdaterBase<SerializedKey>
	{
		public MongoSigningKeyUpdater(ConfigurationStoreOptions options) : base(
			options.Database.GetCollection<SerializedKey>(options.SigningKeyCollectionName))
		{
		}

		public override Task InsertOrUpdateAsync(SerializedKey entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(sk => sk.Id == entity.Id, entity, cancellationToken);
	}
}
