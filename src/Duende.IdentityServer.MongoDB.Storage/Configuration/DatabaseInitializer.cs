using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Configuration;

namespace Duende.IdentityServer.MongoDB.Storage.Configuration
{
	public class DatabaseInitializer : IDatabaseInitializer
	{
		private readonly ConfigurationStoreOptions _configurationStoreOptions;
		private readonly OperationalStoreOptions _operationalStoreOptions;

		// ReSharper disable SuggestBaseTypeForParameter
		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions,
			OperationalStoreOptions operationalStoreOptions)
		{
			_configurationStoreOptions = configurationStoreOptions;
			_operationalStoreOptions = operationalStoreOptions;
		}

		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions) : this(
			configurationStoreOptions, default)
		{
		}

		public DatabaseInitializer(OperationalStoreOptions operationalStoreOptions)
		{
			_operationalStoreOptions = operationalStoreOptions;
		}
		// ReSharper restore SuggestBaseTypeForParameter

		public Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default) =>
			DatabaseInitializerBase.InitializeConfigurationStoreAsync<Client, Resource>(
				c => c.AllowedCorsOrigins,
				r => r.Name,
				new[] { _configurationStoreOptions?.IdentityProviderCollectionName }, _configurationStoreOptions,
				cancellationToken);

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
				new []{ _operationalStoreOptions?.SigningKeyCollectionName },
				_operationalStoreOptions,
				cancellationToken);
	}
}
