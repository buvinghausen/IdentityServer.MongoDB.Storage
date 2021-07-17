using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoClientStore : MongoClientStoreBase<Client>, IClientStore, ICorsPolicyService
	{
		public MongoClientStore(IMongoDatabase database) : base(database)
		{
		}

		public Task<Client> FindClientByIdAsync(string clientId) =>
			SingleOrDefaultAsync(c => c.ClientId == clientId);

		public Task<bool> IsOriginAllowedAsync(string origin) =>
			AnyAsync(c => c.AllowedCorsOrigins.Contains(origin));
	}
}
