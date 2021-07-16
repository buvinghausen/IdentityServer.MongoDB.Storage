using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Stores;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Storage.Admin
{
	public class MongoResourceUpdater<T> : MongoStoreUpdaterBase<T> where T : Resource
	{
		public MongoResourceUpdater(IMongoDatabase database) : base(
			database.GetCollection<Resource>(MongoResourceStore.CollectionName).OfType<T>())
		{
		}

		public override Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(r => r.Name == entity.Name, entity, cancellationToken);
	}
}
