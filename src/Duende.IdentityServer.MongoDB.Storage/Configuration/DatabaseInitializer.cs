using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Configuration;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Configuration
{
	internal class DatabaseInitializer : DatabaseInitializerBase
	{
		public DatabaseInitializer(IMongoDatabase database) : base(database)
		{
		}

		public override Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default) =>
			InitializeConfigurationStoreAsync<Resource>(r => r.Name, cancellationToken);

		public override Task InitializeOperationalStoreAsync(CancellationToken cancellationToken = default) =>
			InitializeOperationalStoreAsync<DeviceFlowCode, PersistedGrant>(
				dc => dc.UserCode,
				dc => dc.Expiration,
				pg => pg.SubjectId,
				pg => pg.ClientId,
				pg => pg.SessionId,
				pg => pg.Type,
				pg => pg.Expiration,
				cancellationToken);
	}
}
