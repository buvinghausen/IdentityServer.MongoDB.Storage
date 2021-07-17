using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoIdentityProviderStore : MongoStoreBase<IdentityProvider>, IIdentityProviderStore
	{
		public MongoIdentityProviderStore(IMongoDatabase database) : base(database, "IdentityProviders")
		{
		}

		public Task<IEnumerable<IdentityProviderName>> GetAllSchemeNamesAsync() =>
			ToEnumerableAsync(ip => new IdentityProviderName
			{
				DisplayName = ip.DisplayName, Enabled = ip.Enabled, Scheme = ip.Scheme
			});

		public Task<IdentityProvider> GetBySchemeAsync(string scheme) =>
			SingleOrDefaultAsync(ip => ip.Scheme == scheme);
	}
}
