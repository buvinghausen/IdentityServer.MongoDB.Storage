using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoIdentityProviderStore : MongoStoreBase<IdentityProvider>, IIdentityProviderStore
	{
		public MongoIdentityProviderStore(ConfigurationStoreOptions options) : base(options.Database, options.IdentityProviderCollectionName)
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
