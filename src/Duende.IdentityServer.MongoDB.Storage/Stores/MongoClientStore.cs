using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoClientStore : MongoClientStoreBase<Client>, IClientStore, ICorsPolicyService
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoClientStore(ConfigurationStoreOptions options) : base(options)
		{
		}

		public Task<Client> FindClientByIdAsync(string clientId) =>
			FindClientByIdAsync(clientId, CancellationToken.None);

		public Task<Client> FindClientByIdAsync(string clientId, CancellationToken cancellationToken) =>
			SingleOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

		public Task<bool> IsOriginAllowedAsync(string origin) =>
			IsOriginAllowedAsync(origin, CancellationToken.None);

		public Task<bool> IsOriginAllowedAsync(string origin, CancellationToken cancellationToken) =>
			AnyAsync(c => c.AllowedCorsOrigins.Contains(origin), cancellationToken);
	}
}
