using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores;

internal sealed class MongoResourceStore : MongoResourceStoreBase<Resource, IdentityResource, ApiResource, ApiScope, Resources>, IResourceStore
{
	// ReSharper disable once SuggestBaseTypeForParameter
	public MongoResourceStore(ConfigurationStoreOptions options) : base(options)
	{
	}

	public override Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames, CancellationToken cancellationToken) =>
		// ReSharper disable once ConvertClosureToMethodGroup (mongo doesn't like ConstantExpressions)
		ToEnumerableAsync<ApiResource>(r => r.Scopes.Any(s => scopeNames.Contains(s)), cancellationToken);

	protected override Task<IEnumerable<T>> FindResourcesByName<T>(IEnumerable<string> names, CancellationToken cancellationToken = default) =>
		ToEnumerableAsync<T>(r => names.Contains(r.Name), cancellationToken);

	protected override Resources GetResources(IList<Resource> resources) =>
		new(resources.OfType<IdentityResource>(), resources.OfType<ApiResource>(), resources.OfType<ApiScope>());
}
