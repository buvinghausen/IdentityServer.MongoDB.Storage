using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Admin;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	class MongoSigningKeyUpdater : MongoStoreUpdaterBase<SerializedKey>
	{
		public MongoSigningKeyUpdater(IMongoDatabase database) : base(
			database.GetCollection<SerializedKey>("SigningKeys"))
		{
		}

		public override Task InsertOrUpdateAsync(SerializedKey entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(sk => sk.Id == entity.Id, entity, cancellationToken);
	}
}
