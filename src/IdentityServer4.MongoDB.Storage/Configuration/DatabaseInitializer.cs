using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer4.Models;

namespace IdentityServer4.MongoDB.Storage.Configuration
{
	internal class DatabaseInitializer : DatabaseInitializerBase
	{
		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions, OperationalStoreOptions operationalStoreOptions) : base(configurationStoreOptions, operationalStoreOptions)
		{
		}

		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions) : base(configurationStoreOptions)
		{
		}

		public DatabaseInitializer(OperationalStoreOptions operationalStoreOptions) : base(operationalStoreOptions: operationalStoreOptions)
		{
		}

		public override Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default) =>
			InitializeConfigurationStoreAsync<Client, Resource>(c => c.AllowedCorsOrigins, r => r.Name, new string[0], cancellationToken);

		public override Task InitializeOperationalStoreAsync(CancellationToken cancellationToken = default) =>
			InitializeOperationalStoreAsync<DeviceFlowCode, PersistedGrant>(
				dc => dc.UserCode,
				dc => dc.Expiration,
				pg => pg.SubjectId,
				pg => pg.ClientId,
				pg => pg.SessionId,
				pg => pg.Type,
				pg => pg.Expiration,
				pg => pg.ConsumedTime,
				cancellationToken);
	}
}
