using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Configuration;

namespace Duende.IdentityServer.MongoDB.Storage.Configuration
{
	public class DatabaseInitializer
	{
		private readonly ConfigurationStoreOptions _configurationStoreOptions;
		private readonly OperationalStoreOptions _operationalStoreOptions;
		private readonly string[] _additionalNames;

		// ReSharper disable SuggestBaseTypeForParameter
		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions,
			OperationalStoreOptions operationalStoreOptions)
		{
			_additionalNames = new[]
			{
				configurationStoreOptions.IdentityProviderCollectionName,
				configurationStoreOptions.SigningKeyCollectionName
			};
			_configurationStoreOptions = configurationStoreOptions;
			_operationalStoreOptions = operationalStoreOptions;
		}

		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions) : this(
			configurationStoreOptions, default)
		{
		}

		public DatabaseInitializer(OperationalStoreOptions operationalStoreOptions)
		{
			_additionalNames = new string[0];
			_operationalStoreOptions = operationalStoreOptions;
		}
		// ReSharper restore SuggestBaseTypeForParameter

		public Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default) =>
			DatabaseInitializerBase.InitializeConfigurationStoreAsync<Client, Resource>(
				c => c.AllowedCorsOrigins,
				r => r.Name,
				_additionalNames, _configurationStoreOptions, cancellationToken);

		public Task InitializeOperationalStoreAsync(CancellationToken cancellationToken = default) =>
			DatabaseInitializerBase.InitializeOperationalStoreAsync<DeviceFlowCode, PersistedGrant>(
				dc => dc.UserCode,
				dc => dc.Expiration,
				pg => pg.SubjectId,
				pg => pg.ClientId,
				pg => pg.SessionId,
				pg => pg.Type,
				pg => pg.Expiration,
				pg => pg.ConsumedTime,
				_operationalStoreOptions,
				cancellationToken);
	}
}
