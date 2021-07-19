using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoClientStoreBase<T> : MongoStoreBase<T>
	{
		protected MongoClientStoreBase(ConfigurationStoreOptions options) : base(options.Database, options.ClientCollectionName)
		{
		}

		public Task<T> FindClientByIdAsync(string clientId) =>
			FindClientByIdAsync(clientId, CancellationToken.None);

		public abstract Task<T> FindClientByIdAsync(string clientId, CancellationToken cancellationToken);

		public Task<bool> IsOriginAllowedAsync(string origin) =>
			IsOriginAllowedAsync(origin, CancellationToken.None);

		public abstract Task<bool> IsOriginAllowedAsync(string origin, CancellationToken cancellationToken);
	}
}
