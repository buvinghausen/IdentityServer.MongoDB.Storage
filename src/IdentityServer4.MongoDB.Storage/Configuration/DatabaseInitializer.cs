using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Stores;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Storage.Configuration
{
	public class DatabaseInitializer
	{
		private readonly IMongoDatabase _database;
		private readonly CreateCollectionOptions _options = new()
		{
			Collation = new Collation(CultureInfo.CurrentCulture.Name, strength: CollationStrength.Secondary)
		};

		public DatabaseInitializer(IMongoDatabase database)
		{
			_database = database;
		}

		// Sets up the Clients & Resources collections and indexes
		public async Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default)
		{
			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await Task.WhenAll(
				_database.CreateCollectionAsync(MongoClientStore.CollectionName, _options, cancellationToken),
				_database.CreateCollectionAsync(MongoResourceStore.CollectionName, _options, cancellationToken))
				.ConfigureAwait(false);

			// Step 2 create the unique index on the Resources collection which is a unique composite key of name & the type discriminator
			await _database
				.GetCollection<Resource>(MongoResourceStore.CollectionName).Indexes
				.CreateOneAsync(Builders<Resource>.IndexKeys
						.Ascending(r => r.Name)
						.Ascending(new StringFieldDefinition<Resource>("_t")),
					new CreateIndexOptions { Background = true, Name = "ux_name_t", Unique = true },
					cancellationToken).ConfigureAwait(false);
		}

		// Sets up the DeviceCodes & PersistedGrants collections and indexes
		public async Task InitializeOperationalStoreAsync(CancellationToken cancellationToken = default)
		{
			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await Task.WhenAll(
					_database.CreateCollectionAsync(MongoDeviceFlowStore.CollectionName, _options, cancellationToken),
					_database.CreateCollectionAsync(MongoPersistedGrantStore.CollectionName, _options, cancellationToken))
				.ConfigureAwait(false);

			// Step 2 create the indexes on both collections
			var deviceIxBuilder = Builders<DeviceFlowCode>.IndexKeys;
			var grantIxBuilder = Builders<PersistedGrant>.IndexKeys;

			await Task.WhenAll(
				_database
					.GetCollection<DeviceFlowCode>(MongoDeviceFlowStore.CollectionName).Indexes.CreateManyAsync(new CreateIndexModel<DeviceFlowCode>[]
					{
						new (deviceIxBuilder
								.Ascending(dc => dc.DeviceCode),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ux_deviceCode",
								Unique = true
							}),
						new (deviceIxBuilder
								.Ascending(dc => dc.Expiration),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ix_expiration",
								Unique = false
							})
					}, cancellationToken),
				_database
					.GetCollection<PersistedGrant>(MongoPersistedGrantStore.CollectionName).Indexes.CreateManyAsync(new CreateIndexModel<PersistedGrant>[]
					{
						new (grantIxBuilder
								.Ascending(dc => dc.SubjectId)
								.Ascending(dc => dc.ClientId)
								.Ascending(dc => dc.Type),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ix_subjectId_clientId_type",
								Unique = false
							}),
						new (grantIxBuilder
								.Ascending(dc => dc.SubjectId)
								.Ascending(dc => dc.SessionId)
								.Ascending(dc => dc.Type),
							new CreateIndexOptions
							{
								Background = true,
								Name = "ix_subjectId_sessionId_type",
								Unique = false
							}),
						new (grantIxBuilder
								.Ascending(dc => dc.Expiration),
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
