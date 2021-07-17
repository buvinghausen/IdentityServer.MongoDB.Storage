using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer.MongoDB.Abstractions.Stores;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;

namespace IdentityServer4.MongoDB.Storage.Stores
{
	// This class just provides the IdentityServer4 versions of Client & IClientStore
	internal class MongoClientStore : MongoClientStoreBase<Client>, IClientStore, ICorsPolicyService
	{
		public MongoClientStore(ConfigurationStoreOptions options) : base(options)
		{
		}

		public Task<Client> FindClientByIdAsync(string clientId) =>
			SingleOrDefaultAsync(c => c.ClientId == clientId);

		public Task<bool> IsOriginAllowedAsync(string origin) =>
			AnyAsync(c => c.AllowedCorsOrigins.Contains(origin));
	}
}
