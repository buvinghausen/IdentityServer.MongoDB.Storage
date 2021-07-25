using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Configuration
{
	internal static class DatabaseInitializerBase
	{
		private static readonly CreateCollectionOptions Options = new()
		{
			Collation = new Collation(CultureInfo.CurrentCulture.Name, strength: CollationStrength.Secondary)
		};

		// Sets up the Clients & Resources collections and indexes
		public static async Task InitializeConfigurationStoreAsync<TClient, TResource>(
			Expression<Func<TClient, object>> corsOriginsSelector,
			Expression<Func<TResource, object>> nameSelector,
			IEnumerable<string> additionalCollectionNames,
			ConfigurationStoreOptionsBase configurationStoreOptions,
			CancellationToken cancellationToken = default)
		{
			if (configurationStoreOptions is null)
				return;

			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await CreateCollectionsAsync(configurationStoreOptions.Database,
				new[] { configurationStoreOptions.ClientCollectionName, configurationStoreOptions.ResourceCollectionName }
					.Concat(
						additionalCollectionNames), cancellationToken).ConfigureAwait(false);

			// Step 2 create the unique index on the Resources collection which is a unique composite key of name & the type discriminator
			await Task.WhenAll(
				configurationStoreOptions.Database
					.GetCollection<TClient>(configurationStoreOptions.ResourceCollectionName).Indexes
					.CreateOneAsync(Builders<TClient>.IndexKeys
							.Ascending(corsOriginsSelector),
						new CreateIndexOptions { Background = true, Name = "ix_allowedCorsOrigins", Unique = false },
						cancellationToken),
				configurationStoreOptions.Database
					.GetCollection<TResource>(configurationStoreOptions.ResourceCollectionName).Indexes
					.CreateOneAsync(Builders<TResource>.IndexKeys
							.Ascending(nameSelector)
							.Ascending(new StringFieldDefinition<TResource>("_t")),
						new CreateIndexOptions { Background = true, Name = "ux_name_t", Unique = true },
						cancellationToken)).ConfigureAwait(false);
		}

		// Sets up the DeviceCodes & PersistedGrants collections and indexes
		public static async Task InitializeOperationalStoreAsync<TDeviceCode, TPersistedGrant>(
			Expression<Func<TDeviceCode, object>> deviceCodeSelector,
			Expression<Func<TDeviceCode, object>> deviceCodeExpirationSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantSubjectIdSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantClientIdSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantSessionIdSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantTypeSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantExpirationSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantConsumedSelector,
			OperationalStoreOptionsBase operationalStoreOptions,
			CancellationToken cancellationToken = default)
		{
			if (operationalStoreOptions is null)
				return;

			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await CreateCollectionsAsync(operationalStoreOptions.Database,
				new[]
				{
					operationalStoreOptions.DeviceFlowCollectionName,
					operationalStoreOptions.PersistedGrantCollectionName
				},
				cancellationToken).ConfigureAwait(false);

			// Step 2 create the indexes on both collections
			var deviceIxBuilder = Builders<TDeviceCode>.IndexKeys;
			var grantIxBuilder = Builders<TPersistedGrant>.IndexKeys;

			await Task.WhenAll(
				operationalStoreOptions.Database
					.GetCollection<TDeviceCode>(operationalStoreOptions.DeviceFlowCollectionName).Indexes
					.CreateManyAsync(
						new CreateIndexModel<TDeviceCode>[]
						{
							new(deviceIxBuilder
									.Ascending(deviceCodeSelector),
								new CreateIndexOptions {Background = true, Name = "ux_deviceCode", Unique = true}),
							new(deviceIxBuilder
									.Ascending(deviceCodeExpirationSelector),
								new CreateIndexOptions {Background = true, Name = "ix_expiration", Unique = false})
						}, cancellationToken),
				operationalStoreOptions.Database
					.GetCollection<TPersistedGrant>(operationalStoreOptions.PersistedGrantCollectionName).Indexes
					.CreateManyAsync(new CreateIndexModel<TPersistedGrant>[]
					{
						new(grantIxBuilder
								.Ascending(persistedGrantSubjectIdSelector)
								.Ascending(persistedGrantClientIdSelector)
								.Ascending(persistedGrantTypeSelector),
							new CreateIndexOptions
							{
								Background = true, Name = "ix_subjectId_clientId_type", Unique = false
							}),
						new(grantIxBuilder
								.Ascending(persistedGrantSubjectIdSelector)
								.Ascending(persistedGrantSessionIdSelector)
								.Ascending(persistedGrantTypeSelector),
							new CreateIndexOptions
							{
								Background = true, Name = "ix_subjectId_sessionId_type", Unique = false
							}),
						new(grantIxBuilder
								.Ascending(persistedGrantExpirationSelector),
							new CreateIndexOptions {Background = true, Name = "ix_expiration", Unique = false}),
						new(grantIxBuilder
								.Ascending(persistedGrantConsumedSelector),
							new CreateIndexOptions {Background = true, Name = "ix_consumedTime", Unique = false})
					}, cancellationToken)
			).ConfigureAwait(false);
		}

		private static Task CreateCollectionsAsync(IMongoDatabase database, IEnumerable<string> names,
			CancellationToken cancellationToken) =>
			Task.WhenAll(names.Select(name => database.CreateCollectionAsync(name, Options, cancellationToken)));
	}
}
