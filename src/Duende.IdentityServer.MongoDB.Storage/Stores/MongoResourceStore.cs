using System.Collections.Generic;
using System.Linq;
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
			FindResourcesByName<IdentityResource>(scopeNames);

		public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames) =>
			FindResourcesByName<ApiScope>(scopeNames);

		public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames) =>
			ToEnumerableAsync<ApiResource>(r => r.Scopes.Any(scopeNames.Contains));

		public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames) =>
			FindResourcesByName<ApiResource>(apiResourceNames);

		public Task<Resources> GetAllResourcesAsync() => GetAllResourcesAsync(result =>
			new Resources(
				result.OfType<IdentityResource>(),
			result.OfType<ApiResource>(),
			result.OfType<ApiScope>()));

		private Task<IEnumerable<T>> FindResourcesByName<T>(IEnumerable<string> names) where T : Resource =>
			ToEnumerableAsync<T>(r => names.Contains(r.Name));
	}
}
