using IdentityServer4.AdminConsole;

await Host
	.CreateDefaultBuilder(args)
	.ConfigureServices((context, services) =>
	{
		// Wire up otel (not required) and point identityserver admin to the desired database
		// Note: operational store and configuration stores can be in separate databases
		// You will need write access to both stores from the admin console
		var database = context.Configuration.GetMongoDatabase("Identity");
		services
			.AddOpenTelemetryTracing(context.HostingEnvironment.ApplicationName)
			.AddIdentityServerConfigurationStoreAdmin(options => options.Database = database)
			.AddIdentityServerOperationalStoreAdmin(options => options.Database = database)
			.AddHostedService<DatabaseInitializer>();
	})
	.RunConsoleAsync()
	.ConfigureAwait(false);
