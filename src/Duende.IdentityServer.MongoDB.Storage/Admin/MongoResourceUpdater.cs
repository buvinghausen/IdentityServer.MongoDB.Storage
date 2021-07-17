using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Stores;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	public class MongoResourceUpdater<T> : MongoStoreUpdaterBase<T> where T : Resource
	{
		public MongoResourceUpdater(IMongoDatabase database) : base(
			database.GetCollection<Resource>(CollectionNames.ResourceCollectionName).OfType<T>())
		{
		}

		public override Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(r => r.Name == entity.Name, entity, cancellationToken);
	}
}
