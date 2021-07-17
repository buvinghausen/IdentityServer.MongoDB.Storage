using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoResourceStoreBase<T> : MongoStoreBase<T>
	{
		protected MongoResourceStoreBase(IMongoDatabase database) : base(database,
			CollectionNames.ResourceCollectionName)
		{
		}

		protected async Task<TResult> GetAllResourcesAsync<TResult>(Func<IList<T>, TResult> resourceFunc) =>
			resourceFunc(await ToListAsync().ConfigureAwait(false));
	}
}
