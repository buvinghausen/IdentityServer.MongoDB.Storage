using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer.MongoDB.Abstractions.Stores;
using IdentityServer4.Models;
using IdentityServer4.Stores;

namespace IdentityServer4.MongoDB.Storage.Stores
{
	// This class leverages MongoDB's polymorphic storage capacity to keep all three resource types in one collection
	internal class MongoResourceStore : MongoResourceStoreBase<Resource>, IResourceStore
	{
		public MongoResourceStore(ConfigurationStoreOptions options) : base(options)
		{
		}

		public Task<IEnumerable<IdentityResource>>
			FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames) =>
			FindIdentityResourcesByScopeNameAsync(scopeNames, CancellationToken.None);

		public Task<IEnumerable<IdentityResource>>
			FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames,
				CancellationToken cancellationToken) =>
			FindResourcesByName<IdentityResource>(scopeNames, cancellationToken);

		public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames) =>
			FindApiScopesByNameAsync(scopeNames, CancellationToken.None);

		public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames,
			CancellationToken cancellationToken) =>
			FindResourcesByName<ApiScope>(scopeNames, cancellationToken);

		public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames) =>
			FindApiResourcesByScopeNameAsync(scopeNames, CancellationToken.None);

		public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames,
			CancellationToken cancellationToken) =>
			ToEnumerableAsync<ApiResource>(r => r.Scopes.Any(scopeNames.Contains), cancellationToken);

		public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames) =>
			FindApiResourcesByNameAsync(apiResourceNames, CancellationToken.None);

		public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames,
			CancellationToken cancellationToken) =>
			FindResourcesByName<ApiResource>(apiResourceNames, cancellationToken);

		public Task<Resources> GetAllResourcesAsync() => GetAllResourcesAsync(CancellationToken.None);

		public Task<Resources> GetAllResourcesAsync(CancellationToken cancellationToken) => GetAllResourcesAsync(
			result =>
				new Resources(
					result.OfType<IdentityResource>(),
					result.OfType<ApiResource>(),
					result.OfType<ApiScope>()), cancellationToken);

		private Task<IEnumerable<T>> FindResourcesByName<T>(IEnumerable<string> names,
			CancellationToken cancellationToken = default) where T : Resource =>
			ToEnumerableAsync<T>(r => names.Contains(r.Name), cancellationToken);
	}
}
