using IdentityServer.MongoDB.Abstractions.Stores;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Options;
using IdentityServer4.Services;
using IdentityServer4.Stores;

namespace IdentityServer4.MongoDB.Storage.Stores;

// This class just provides the IdentityServer4 versions of Client, IClientStore, & ICorsPolicyService
internal sealed class MongoClientStore : MongoClientStoreBase<Client>, IClientStore, ICorsPolicyService
{
	// ReSharper disable once SuggestBaseTypeForParameter
	public MongoClientStore(ConfigurationStoreOptions options) : base(options)
	{
	}

	public override Task<Client> FindClientByIdAsync(string clientId, CancellationToken cancellationToken) =>
		SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

	public override Task<bool> IsOriginAllowedAsync(string origin, CancellationToken cancellationToken) =>
		AnyAsync(c => c.AllowedCorsOrigins.Contains(origin), cancellationToken);
}
