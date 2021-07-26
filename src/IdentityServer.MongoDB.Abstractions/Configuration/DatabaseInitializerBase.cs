using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;
using MongoDB.Bson;
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
						additionalCollectionNames).ToList(), cancellationToken).ConfigureAwait(false);

			// Step 2 create the unique index on the Resources collection which is a unique composite key of name & the type discriminator
			await Task.WhenAll(
				configurationStoreOptions.Database
					.GetCollection<TClient>(configurationStoreOptions.ClientCollectionName).Indexes
					.CreateOneAsync(Builders<TClient>.IndexKeys
							.Ascending(corsOriginsSelector),
						new CreateIndexOptions { Background = true, Name = "ix_allowedCorsOrigins", Sparse = true },
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
								new CreateIndexOptions {Background = true, Name = "ix_expiration", Sparse = true})
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
								Background = true, Name = "ix_subjectId_clientId_type", Sparse = true
							}),
						new(grantIxBuilder
								.Ascending(persistedGrantSubjectIdSelector)
								.Ascending(persistedGrantSessionIdSelector)
								.Ascending(persistedGrantTypeSelector),
							new CreateIndexOptions
							{
								Background = true, Name = "ix_subjectId_sessionId_type", Sparse = true
							}),
						new(grantIxBuilder
								.Ascending(persistedGrantExpirationSelector),
							new CreateIndexOptions {Background = true, Name = "ix_expiration", Sparse = true}),
						new(grantIxBuilder
								.Ascending(persistedGrantConsumedSelector),
							new CreateIndexOptions {Background = true, Name = "ix_consumedTime", Sparse = true})
					}, cancellationToken)
			).ConfigureAwait(false);
		}

		private static async Task CreateCollectionsAsync(IMongoDatabase database, IList<string> names,
			CancellationToken cancellationToken)
		{
			// Mongo really should have a better way of doing this but this is a framework component
			// So the pain is hidden here this is the list of desired collections that are not present
			names = (await (await database
					.ListCollectionsAsync(
						new ListCollectionsOptions
						{
							Filter = new BsonDocument("name", new BsonDocument("$in", new BsonArray(names)))
						}, cancellationToken)
					.ConfigureAwait(false)).ToListAsync(cancellationToken).ConfigureAwait(false))
				.Select(bd => bd["name"].AsString).Except(names).ToList();

			// If all the collections are present simply return
			if (!names.Any()) return;

			// If we got here create the missing collections
			await Task.WhenAll(names.Select(name => database.CreateCollectionAsync(name, Options, cancellationToken)))
				.ConfigureAwait(false);
		}
	}
}
