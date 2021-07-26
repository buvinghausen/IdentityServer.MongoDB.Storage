using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Options;

namespace IdentityServer4.MongoDB.Storage.Configuration
{
	public class DatabaseInitializer : IDatabaseInitializer
	{
		private readonly ConfigurationStoreOptions _configurationStoreOptions;
		private readonly OperationalStoreOptions _operationalStoreOptions;

		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions, OperationalStoreOptions operationalStoreOptions)
		{
			_configurationStoreOptions = configurationStoreOptions;
			_operationalStoreOptions = operationalStoreOptions;
		}

		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions)
		{
			_configurationStoreOptions = configurationStoreOptions;
		}

		public DatabaseInitializer(OperationalStoreOptions operationalStoreOptions)
		{
			_operationalStoreOptions = operationalStoreOptions;
		}

		public Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default) =>
			DatabaseInitializerBase.InitializeConfigurationStoreAsync<Client, Resource>(c => c.AllowedCorsOrigins, r => r.Name,
				new string[0], _configurationStoreOptions, cancellationToken);

		public Task InitializeOperationalStoreAsync(CancellationToken cancellationToken = default) =>
			DatabaseInitializerBase.InitializeOperationalStoreAsync<DeviceFlowCode, PersistedGrant>(
				dc => dc.DeviceCode,
				dc => dc.Expiration,
				pg => pg.SubjectId,
				pg => pg.ClientId,
				pg => pg.SessionId,
				pg => pg.Type,
				pg => pg.Expiration,
				pg => pg.ConsumedTime,
				new string[0],
				_operationalStoreOptions,
				cancellationToken);
	}
}
