using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Stores;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Options;
using IdentityServer4.Stores;

namespace IdentityServer4.MongoDB.Storage.Stores
{
	// This class leverages MongoDB's polymorphic storage capacity to keep all three resource types in one collection
	internal class MongoResourceStore : MongoResourceStoreBase<Resource, IdentityResource, ApiResource, ApiScope, Resources>, IResourceStore
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoResourceStore(ConfigurationStoreOptions options) : base(options)
		{
		}

		public override Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames,
			CancellationToken cancellationToken) =>
			ToEnumerableAsync<ApiResource>(r => r.Scopes.Any(scopeNames.Contains), cancellationToken);

		protected override Task<IEnumerable<T>> FindResourcesByName<T>(IEnumerable<string> names,
			CancellationToken cancellationToken = default) =>
			ToEnumerableAsync<T>(r => names.Contains(r.Name), cancellationToken);

		protected override Resources GetResources(IList<Resource> resources) =>
			new(resources.OfType<IdentityResource>(),
				resources.OfType<ApiResource>(),
				resources.OfType<ApiScope>());
	}
}
