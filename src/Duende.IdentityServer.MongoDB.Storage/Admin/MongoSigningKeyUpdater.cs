using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	public class MongoSigningKeyUpdater : MongoStoreUpdaterBase<SerializedKey>
	{
		public MongoSigningKeyUpdater(ConfigurationStoreOptions options) : this(options.Database,
			options.SigningKeyCollectionName)
		{
		}

		public MongoSigningKeyUpdater(IMongoDatabase database, string collectionName) : base(
			database.GetCollection<SerializedKey>(collectionName))
		{
		}

		public override Task InsertOrUpdateAsync(SerializedKey entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(sk => sk.Id == entity.Id, entity, cancellationToken);
	}
}
