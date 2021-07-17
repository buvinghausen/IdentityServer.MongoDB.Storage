using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;

namespace Duende.IdentityServer.MongoDB.Storage.Admin
{
	internal class MongoResourceUpdater<T> : MongoStoreUpdaterBase<T> where T : Resource
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoResourceUpdater(ConfigurationStoreOptions options) : base(
			options.Database.GetCollection<Resource>(options.ResourceCollectionName).OfType<T>())
		{
		}

		public override Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default) =>
			InsertOrUpdateAsync(r => r.Name == entity.Name, entity, cancellationToken);
	}
}
