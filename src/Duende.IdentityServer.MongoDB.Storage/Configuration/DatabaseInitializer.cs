using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer.MongoDB.Abstractions.Options;
using ConfigurationStoreOptions = Duende.IdentityServer.MongoDB.Storage.Options.ConfigurationStoreOptions;

namespace Duende.IdentityServer.MongoDB.Storage.Configuration
{
	internal class DatabaseInitializer : DatabaseInitializerBase
	{
		private readonly string[] _additionalNames;

		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions, OperationalStoreOptions operationalStoreOptions) : base(configurationStoreOptions, operationalStoreOptions)
		{
			_additionalNames = new[]
			{
				configurationStoreOptions.IdentityProviderCollectionName,
				configurationStoreOptions.SigningKeyCollectionName
			};
		}

		public DatabaseInitializer(ConfigurationStoreOptions configurationStoreOptions) : this(configurationStoreOptions, default)
		{
		}

		public DatabaseInitializer(OperationalStoreOptions operationalStoreOptions) : base(operationalStoreOptions: operationalStoreOptions)
		{
			_additionalNames = new string[0];
		}

		public override Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default) =>
			InitializeConfigurationStoreAsync<Client, Resource>(c => c.AllowedCorsOrigins, r => r.Name, _additionalNames, cancellationToken);

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
