using System;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Admin;
using Duende.IdentityServer.MongoDB.Storage.Configuration;
using Duende.IdentityServer.MongoDB.Storage.Options;
using Duende.IdentityServer.MongoDB.Storage.Stores;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer.MongoDB.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceConfigurationExtensions
	{
		private static bool _initialized;

		public static IServiceCollection AddIdentityServerConfigurationStoreAdmin(this IServiceCollection services,
			Action<ConfigurationStoreOptions> config)
		{
			Initialize();
			var options = new ConfigurationStoreOptions();
			config(options);
			services
				.AddSingleton(options)
				.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();
			return  services.AddTransient<IConfigurationStoreUpdater<Client>, MongoClientUpdater>()
				.AddTransient<IConfigurationStoreUpdater<ApiResource>, MongoResourceUpdater<ApiResource>>()
				.AddTransient<IConfigurationStoreUpdater<ApiScope>, MongoResourceUpdater<ApiScope>>()
				.AddTransient<IConfigurationStoreUpdater<IdentityResource>, MongoResourceUpdater<IdentityResource>>()
				.AddTransient<IConfigurationStoreUpdater<IdentityProvider>, MongoIdentityProviderUpdater>();
		}

		public static IServiceCollection AddIdentityServerOperationalStoreAdmin(this IServiceCollection services,
			Action<OperationalStoreOptions> config)
		{
			Initialize();
			var options = new OperationalStoreOptions();
			config(options);
			services
				.AddSingleton(options)
				.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();
			return services;
		}

		public static IIdentityServerBuilder AddConfigurationStore(this IIdentityServerBuilder builder, Action<ConfigurationStoreOptions> config)
		{
			Initialize();
			var options = new ConfigurationStoreOptions();
			config(options);
			builder.Services
				.AddSingleton(options)
				.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();

			builder
				.AddClientStore<MongoClientStore>()
				.AddResourceStore<MongoResourceStore>()
				.AddIdentityProviderStore<MongoIdentityProviderStore>()
				.AddCorsPolicyService<MongoClientStore>();

			if (!options.AddConfigurationStoreUpdaters)
				return builder;

			// Only add IConfigurationStoreUpdater to the service collection if explicitly requested
			builder.Services
				.AddTransient<IConfigurationStoreUpdater<Client>, MongoClientUpdater>()
				.AddTransient<IConfigurationStoreUpdater<ApiResource>, MongoResourceUpdater<ApiResource>>()
				.AddTransient<IConfigurationStoreUpdater<ApiScope>, MongoResourceUpdater<ApiScope>>()
				.AddTransient<IConfigurationStoreUpdater<IdentityResource>, MongoResourceUpdater<IdentityResource>>();

			return builder;
		}

		public static IIdentityServerBuilder AddOperationalStore(this IIdentityServerBuilder builder, Action<OperationalStoreOptions> config)
		{
			Initialize();
			var options = new OperationalStoreOptions();
			config(options);
			builder.Services
				.AddSingleton(options)
				.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();

			builder
				.AddPersistedGrantStore<MongoPersistedGrantStore>()
				.AddDeviceFlowStore<MongoDeviceFlowStore>()
				.AddSigningKeyStore<MongoSigningKeyStore>();

			if (!options.EnableTokenCleanup) return builder;

			// Only add TokenCleanupService if explicitly requested
			builder.Services
				.AddHostedService(_ => new TokenCleanupService(options, new IOperationalStore[]
				{
					new MongoDeviceFlowStore(options),
					new MongoPersistedGrantStore(options)
				}));
			return builder;
		}

		private static void Initialize()
		{
			if (_initialized) return;
			MongoConfiguration.Initialize();
			_initialized = true;
		}
	}
}
