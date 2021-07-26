using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SequentialGuid;

namespace IdentityServer4.AdminConsole
{
	internal class Program
	{
		private static Task Main(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					// Wire up mongo in a telemetry friendly way so you can see all the commands in the console output
					var url = new MongoUrl(hostContext.Configuration.GetConnectionString("Identity"));
					var settings = MongoClientSettings.FromUrl(url);
					settings.ClusterConfigurator = cb =>
						cb.Subscribe(new DiagnosticsActivityEventSubscriber(new InstrumentationOptions
						{
							CaptureCommandText = true,
							ShouldStartActivity = evt => evt.DatabaseNamespace.DatabaseName == url.DatabaseName
						}));
					var database = new MongoClient(settings).GetDatabase(url.DatabaseName);
					// Wire up otel (not required) and point identityserver admin to the desired database
					// Note: operational store and configuration stores can be in separate databases
					// You only need read access for the configuration store but the operational store requires write access
					services
						.AddOpenTelemetryTracing(builder => builder
							.SetResourceBuilder(ResourceBuilder.CreateDefault()
								.AddService(hostContext.HostingEnvironment.ApplicationName,
									serviceInstanceId: SequentialGuidGenerator.Instance.NewGuid().ToString()))
							.AddMongoDBInstrumentation()
							.AddConsoleExporter())
						.AddIdentityServerConfigurationStoreAdmin(options => options.Database = database)
						.AddIdentityServerOperationalStoreAdmin(options => options.Database = database)
						.AddHostedService<DatabaseInitializer>();
				})
				.RunConsoleAsync();
	}
}
