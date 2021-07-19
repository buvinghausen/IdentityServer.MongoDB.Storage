using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoResourceStore : MongoResourceStoreBase<Resource>, IResourceStore
	{
		// ReSharper disable once SuggestBaseTypeForParameter
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
