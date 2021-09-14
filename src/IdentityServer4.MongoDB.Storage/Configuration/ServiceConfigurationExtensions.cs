using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer.MongoDB.Abstractions.Services;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Admin;
using IdentityServer4.MongoDB.Storage.Configuration;
using IdentityServer4.MongoDB.Storage.Options;
using IdentityServer4.MongoDB.Storage.Stores;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MongoDB.Driver;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceConfigurationExtensions
{
	private static bool _initialized;

	public static IServiceCollection AddIdentityServerConfigurationStoreAdmin(this IServiceCollection services, Action<ConfigurationStoreOptions> config)
	{
		var options = new ConfigurationStoreOptions();
		config(options);
		return services.AddIdentityServerConfigurationStoreAdmin(options);
	}

	public static IServiceCollection AddIdentityServerConfigurationStoreAdmin(this IServiceCollection services, ConfigurationStoreOptions options)
	{
		Initialize(options);
		services
			.AddSingleton(options)
			.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();
		return services.AddTransient<IConfigurationStoreUpdater<Client>, MongoClientUpdater>()
			.AddTransient<IConfigurationStoreUpdater<ApiResource>, MongoResourceUpdater<ApiResource>>()
			.AddTransient<IConfigurationStoreUpdater<ApiScope>, MongoResourceUpdater<ApiScope>>()
			.AddTransient<IConfigurationStoreUpdater<IdentityResource>, MongoResourceUpdater<IdentityResource>>();
	}

	public static IServiceCollection AddIdentityServerConfigurationStoreAdmin(this IServiceCollection services, IMongoDatabase database) =>
		services.AddIdentityServerConfigurationStoreAdmin(new ConfigurationStoreOptions { Database = database });

	public static IServiceCollection AddIdentityServerOperationalStoreAdmin(this IServiceCollection services, Action<OperationalStoreOptions> config)
	{
		var options = new OperationalStoreOptions();
		config(options);
		return services.AddIdentityServerOperationalStoreAdmin(options);
	}

	public static IServiceCollection AddIdentityServerOperationalStoreAdmin(this IServiceCollection services, OperationalStoreOptions options)
	{
		Initialize(options);
		services
			.AddSingleton(options)
			.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();
		return services;
	}

	public static IServiceCollection AddIdentityServerOperationalStoreAdmin(this IServiceCollection services, IMongoDatabase database) =>
		services.AddIdentityServerOperationalStoreAdmin(new OperationalStoreOptions { Database = database });

	public static IIdentityServerBuilder AddMongoConfigurationStore(this IIdentityServerBuilder builder, Action<ConfigurationStoreOptions> config)
	{
		var options = new ConfigurationStoreOptions();
		config(options);
		return builder.AddMongoConfigurationStore(options);
	}

	public static IIdentityServerBuilder AddMongoConfigurationStore(this IIdentityServerBuilder builder, ConfigurationStoreOptions options)
	{
		Initialize(options);
		builder.Services
			.AddSingleton(options)
			.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();
		builder
			.AddClientStore<MongoClientStore>()
			.AddResourceStore<MongoResourceStore>()
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

	public static IIdentityServerBuilder AddMongoConfigurationStore(this IIdentityServerBuilder builder, IMongoDatabase database) =>
		builder.AddMongoConfigurationStore(new ConfigurationStoreOptions { Database = database });

	public static IIdentityServerBuilder AddMongoOperationalStore(this IIdentityServerBuilder builder,
		Action<OperationalStoreOptions> config)
	{
		var options = new OperationalStoreOptions();
		config(options);
		return builder.AddMongoOperationalStore(options);
	}

	public static IIdentityServerBuilder AddMongoOperationalStore(this IIdentityServerBuilder builder, OperationalStoreOptions options)
	{
		Initialize(options);
		builder.Services
			.AddSingleton(options)
			.TryAddTransient<IDatabaseInitializer, DatabaseInitializer>();

		builder
			.AddPersistedGrantStore<MongoPersistedGrantStore>()
			.AddDeviceFlowStore<MongoDeviceFlowStore>();

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

	public static IIdentityServerBuilder AddMongoOperationalStore(this IIdentityServerBuilder builder, IMongoDatabase database) =>
		builder.AddMongoOperationalStore(new OperationalStoreOptions { Database = database });

	private static void Initialize(OptionsBase options)
	{
		if (options is null) throw new ArgumentNullException(nameof(options));
		if (options.Database is null)
			throw new ArgumentNullException(nameof(options.Database), "You must provide the database");
		if (_initialized) return;
		MongoConfiguration.Initialize();
		_initialized = true;
	}
}
