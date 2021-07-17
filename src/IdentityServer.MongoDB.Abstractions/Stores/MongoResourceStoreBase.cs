using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoResourceStoreBase<T> : MongoStoreBase<T>
	{
		protected MongoResourceStoreBase(ConfigurationStoreOptions options) : base(options.Database,
			options.ResourceCollectionName)
		{
		}

		protected async Task<TResult> GetAllResourcesAsync<TResult>(Func<IList<T>, TResult> resourceFunc) =>
			resourceFunc(await ToListAsync().ConfigureAwait(false));
	}
}
