using System;
using System.Globalization;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Stores;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Configuration
{
	internal abstract class DatabaseInitializerBase
	{
		private readonly IMongoDatabase _database;
		private readonly CreateCollectionOptions _options = new()
		{
			Collation = new Collation(CultureInfo.CurrentCulture.Name, strength: CollationStrength.Secondary)
		};

		protected DatabaseInitializerBase(IMongoDatabase database)
		{
			_database = database;
		}

		// Sets up the Clients & Resources collections and indexes
		public abstract Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default);

		protected async Task InitializeConfigurationStoreAsync<TResource>(Expression<Func<TResource, object>> nameSelector, CancellationToken cancellationToken = default)
		{
			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await Task.WhenAll(
				_database.CreateCollectionAsync(CollectionNames.ClientCollectionName, _options, cancellationToken),
				_database.CreateCollectionAsync(CollectionNames.ResourceCollectionName, _options, cancellationToken))
				.ConfigureAwait(false);

			// Step 2 create the unique index on the Resources collection which is a unique composite key of name & the type discriminator
			await _database
				.GetCollection<TResource>(CollectionNames.ResourceCollectionName).Indexes
				.CreateOneAsync(Builders<TResource>.IndexKeys
						.Ascending(nameSelector)
						.Ascending(new StringFieldDefinition<TResource>("_t")),
					new CreateIndexOptions { Background = true, Name = "ux_name_t", Unique = true },
					cancellationToken).ConfigureAwait(false);
		}

		// Sets up the DeviceCodes & PersistedGrants collections and indexes
		public abstract Task InitializeOperationalStoreAsync(CancellationToken cancellationToken = default);

		protected async Task InitializeOperationalStoreAsync<TDeviceCode, TPersistedGrant>(
			Expression<Func<TDeviceCode, object>> deviceCodeSelector,
			Expression<Func<TDeviceCode, object>> deviceCodeExpirationSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantSubjectIdSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantClientIdSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantSessionIdSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantTypeSelector,
			Expression<Func<TPersistedGrant, object>> persistedGrantExpirationSelector,
			CancellationToken cancellationToken = default)
		{
			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await Task.WhenAll(
					_database.CreateCollectionAsync(CollectionNames.DeviceCodeCollectionName, _options, cancellationToken),
					_database.CreateCollectionAsync(CollectionNames.PersistedGrantCollectionName, _options, cancellationToken))
				.ConfigureAwait(false);

			// Step 2 create the indexes on both collections
			var deviceIxBuilder = Builders<TDeviceCode>.IndexKeys;
			var grantIxBuilder = Builders<TPersistedGrant>.IndexKeys;

			await Task.WhenAll(
				_database
					.GetCollection<TDeviceCode>(CollectionNames.DeviceCodeCollectionName).Indexes.CreateManyAsync(new CreateIndexModel<TDeviceCode>[]
					{
						new (deviceIxBuilder
								.Ascending(deviceCodeSelector),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ux_deviceCode",
								Unique = true
							}),
						new (deviceIxBuilder
								.Ascending(deviceCodeExpirationSelector),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ix_expiration",
								Unique = false
							})
					}, cancellationToken),
				_database
					.GetCollection<TPersistedGrant>(CollectionNames.PersistedGrantCollectionName).Indexes.CreateManyAsync(new CreateIndexModel<TPersistedGrant>[]
					{
						new (grantIxBuilder
								.Ascending(persistedGrantSubjectIdSelector)
								.Ascending(persistedGrantClientIdSelector)
								.Ascending(persistedGrantTypeSelector),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ix_subjectId_clientId_type",
								Unique = false
							}),
						new (grantIxBuilder
								.Ascending(persistedGrantSubjectIdSelector)
								.Ascending(persistedGrantSessionIdSelector)
								.Ascending(persistedGrantTypeSelector),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ix_subjectId_sessionId_type",
								Unique = false
							}),
						new (grantIxBuilder
								.Ascending(persistedGrantExpirationSelector),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ix_expiration",
								Unique = false
							})
					}, cancellationToken)
			).ConfigureAwait(false);
		}
	}
}
