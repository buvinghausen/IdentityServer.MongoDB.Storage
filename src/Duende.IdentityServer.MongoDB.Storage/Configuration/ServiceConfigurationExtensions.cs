using System;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Admin;
using Duende.IdentityServer.MongoDB.Storage.Configuration;
using Duende.IdentityServer.MongoDB.Storage.Options;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
	public static class ServiceConfigurationExtensions
	{
		private static bool _initialized = false;

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
				.AddTransient<IConfigurationStoreUpdater<IdentityProvider>, MongoIdentityProviderUpdater>()
				.AddTransient<IConfigurationStoreUpdater<SerializedKey>, MongoSigningKeyUpdater>(); ;
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

		private static void Initialize()
		{
			if (_initialized) return;
			MongoConfiguration.Initialize();
			_initialized = true;
		}
	}
}
