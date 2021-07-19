using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoResourceStoreBase<TResource, TIdentityResource, TApiResource, TApiScope, TResources> : MongoStoreBase<TResource>
		where TIdentityResource : TResource
		where TApiResource : TResource
		where TApiScope : TResource
	{
		protected MongoResourceStoreBase(ConfigurationStoreOptions options) : base(options.Database,
			options.ResourceCollectionName)
		{
		}

		public Task<IEnumerable<TIdentityResource>>
			FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames) =>
			FindIdentityResourcesByScopeNameAsync(scopeNames, CancellationToken.None);

		public Task<IEnumerable<TIdentityResource>>
			FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames,
				CancellationToken cancellationToken) =>
			FindResourcesByName<TIdentityResource>(scopeNames, cancellationToken);

		public Task<IEnumerable<TApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames) =>
			FindApiScopesByNameAsync(scopeNames, CancellationToken.None);

		public Task<IEnumerable<TApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames,
			CancellationToken cancellationToken) =>
			FindResourcesByName<TApiScope>(scopeNames, cancellationToken);

		public Task<IEnumerable<TApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames) =>
			FindApiResourcesByScopeNameAsync(scopeNames, CancellationToken.None);

		public Task<IEnumerable<TApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames) =>
			FindApiResourcesByNameAsync(apiResourceNames, CancellationToken.None);

		public Task<IEnumerable<TApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames,
			CancellationToken cancellationToken) =>
			FindResourcesByName<TApiResource>(apiResourceNames, cancellationToken);

		public Task<TResources> GetAllResourcesAsync() => GetAllResourcesAsync(CancellationToken.None);

		public async Task<TResources> GetAllResourcesAsync(CancellationToken cancellationToken) =>
			GetResources(await ToListAsync(cancellationToken).ConfigureAwait(false));

		// Abstract methods that the child class must define to conform
		public abstract Task<IEnumerable<TApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames,
			CancellationToken cancellationToken);

		protected abstract Task<IEnumerable<T>> FindResourcesByName<T>(IEnumerable<string> names,
			CancellationToken cancellationToken = default) where T : TResource;

		protected abstract TResources GetResources(IList<TResource> resources);
	}
}
