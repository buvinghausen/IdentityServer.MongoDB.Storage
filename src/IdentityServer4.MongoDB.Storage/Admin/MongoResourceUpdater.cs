using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer4.Models;

namespace IdentityServer4.MongoDB.Storage.Admin
{
	internal class MongoResourceUpdater<T> : MongoStoreUpdaterBase<T> where T : Resource
	{
		public MongoResourceUpdater(ConfigurationStoreOptions options) : base(
			options.Database.GetCollection<Resource>(options.ResourceCollectionName).OfType<T>())
		{
		}

		public override Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(r => r.Name == entity.Name, entity, cancellationToken);
	}
}
