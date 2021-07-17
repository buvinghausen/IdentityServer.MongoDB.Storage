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
			SingleOrDefaultAsync(c => c.ClientId == clientId);

		public Task<bool> IsOriginAllowedAsync(string origin) =>
			AnyAsync(c => c.AllowedCorsOrigins.Contains(origin));
	}
}
