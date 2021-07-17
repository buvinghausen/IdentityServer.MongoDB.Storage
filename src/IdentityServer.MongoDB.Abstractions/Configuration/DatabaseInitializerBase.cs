﻿using System;
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
	internal abstract class DatabaseInitializerBase
	{
		private readonly ConfigurationStoreOptions _configurationStoreOptions;
		private readonly OperationalStoreOptions _operationalStoreOptions;

		private readonly CreateCollectionOptions _options = new()
		{
			Collation = new Collation(CultureInfo.CurrentCulture.Name, strength: CollationStrength.Secondary)
		};

		protected DatabaseInitializerBase(ConfigurationStoreOptions configurationStoreOptions = default, OperationalStoreOptions operationalStoreOptions = default)
		{
			_configurationStoreOptions = configurationStoreOptions;
			_operationalStoreOptions = operationalStoreOptions;
		}

		// Sets up the Clients & Resources collections and indexes
		public abstract Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default);

		protected async Task InitializeConfigurationStoreAsync<TClient, TResource>(
			Expression<Func<TClient, object>> corsOriginsSelector,
			Expression<Func<TResource, object>> nameSelector,
			IEnumerable<string> additionalCollectionNames,
			CancellationToken cancellationToken = default)
		{
			if (_configurationStoreOptions is null)
				return;

			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await CreateCollectionsAsync(_configurationStoreOptions.Database,
				new[] { _configurationStoreOptions.ClientCollectionName, _configurationStoreOptions.ResourceCollectionName }.Concat(
					additionalCollectionNames), cancellationToken).ConfigureAwait(false);

			// Step 2 create the unique index on the Resources collection which is a unique composite key of name & the type discriminator
			await Task.WhenAll(
				_configurationStoreOptions.Database
					.GetCollection<TClient>(_configurationStoreOptions.ResourceCollectionName).Indexes
					.CreateOneAsync(Builders<TClient>.IndexKeys
							.Ascending(corsOriginsSelector),
						new CreateIndexOptions { Background = true, Name = "ix_allowedCorsOrigins", Unique = false },
						cancellationToken),
				_configurationStoreOptions.Database
					.GetCollection<TResource>(_configurationStoreOptions.ResourceCollectionName).Indexes
					.CreateOneAsync(Builders<TResource>.IndexKeys
							.Ascending(nameSelector)
							.Ascending(new StringFieldDefinition<TResource>("_t")),
						new CreateIndexOptions { Background = true, Name = "ux_name_t", Unique = true },
						cancellationToken)).ConfigureAwait(false);
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
			Expression<Func<TPersistedGrant, object>> persistedGrantConsumedSelector,
			CancellationToken cancellationToken = default)
		{
			if (_operationalStoreOptions is null)
				return;

			// Step 1 create the collections with case insensitive collation to match SQL Server defaults
			await CreateCollectionsAsync(_operationalStoreOptions.Database,
				new[] { _operationalStoreOptions.DeviceFlowCollectionName, _operationalStoreOptions.PersistedGrantCollectionName },
				cancellationToken).ConfigureAwait(false);

			// Step 2 create the indexes on both collections
			var deviceIxBuilder = Builders<TDeviceCode>.IndexKeys;
			var grantIxBuilder = Builders<TPersistedGrant>.IndexKeys;

			await Task.WhenAll(
				_operationalStoreOptions.Database
					.GetCollection<TDeviceCode>(_operationalStoreOptions.DeviceFlowCollectionName).Indexes.CreateManyAsync(
						new CreateIndexModel<TDeviceCode>[]
						{
							new(deviceIxBuilder
									.Ascending(deviceCodeSelector),
								new CreateIndexOptions {Background = true, Name = "ux_deviceCode", Unique = true}),
							new(deviceIxBuilder
									.Ascending(deviceCodeExpirationSelector),
								new CreateIndexOptions {Background = true, Name = "ix_expiration", Unique = false})
						}, cancellationToken),
				_operationalStoreOptions.Database
					.GetCollection<TPersistedGrant>(_operationalStoreOptions.PersistedGrantCollectionName).Indexes
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

		private Task CreateCollectionsAsync(IMongoDatabase database, IEnumerable<string> names, CancellationToken cancellationToken) =>
			Task.WhenAll(names.Select(name => database.CreateCollectionAsync(name, _options, cancellationToken)));
	}
}
