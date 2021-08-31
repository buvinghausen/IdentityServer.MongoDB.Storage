using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal sealed class MongoIdentityProviderStore : MongoStoreBase<IdentityProvider>, IIdentityProviderStore
	{
		public MongoIdentityProviderStore(ConfigurationStoreOptions options) : base(options.Database, options.IdentityProviderCollectionName)
		{
		}

		public Task<IEnumerable<IdentityProviderName>> GetAllSchemeNamesAsync() =>
			GetAllSchemeNamesAsync(CancellationToken.None);

		public Task<IEnumerable<IdentityProviderName>> GetAllSchemeNamesAsync(CancellationToken cancellationToken) =>
			ToEnumerableAsync(ip => new IdentityProviderName
			{
				DisplayName = ip.DisplayName, Enabled = ip.Enabled, Scheme = ip.Scheme
			}, cancellationToken);

		public Task<IdentityProvider> GetBySchemeAsync(string scheme) =>
			GetBySchemeAsync(scheme, CancellationToken.None);

		public Task<IdentityProvider> GetBySchemeAsync(string scheme, CancellationToken cancellationToken) =>
			SingleOrDefaultAsync(ip => ip.Scheme == scheme, cancellationToken);
	}
}
