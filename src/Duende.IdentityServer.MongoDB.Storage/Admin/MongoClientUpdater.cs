using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	internal class MongoClientUpdater : MongoStoreUpdaterBase<Client>
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoClientUpdater(ConfigurationStoreOptions options) : base(
			options.Database.GetCollection<Client>(options.ClientCollectionName))
		{
		}

		public override Task InsertOrUpdateAsync(Client entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(c => c.ClientId == entity.ClientId, entity, cancellationToken);
	}
}
