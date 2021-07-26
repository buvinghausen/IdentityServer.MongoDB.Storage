using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SequentialGuid;

namespace Duende.IdentityServer.AdminConsole
{
	public class Program
	{
		public static Task Main(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					var url = new MongoUrl(hostContext.Configuration.GetConnectionString("Identity"));
					var settings = MongoClientSettings.FromUrl(url);
					settings.ClusterConfigurator = cb =>
						cb.Subscribe(new DiagnosticsActivityEventSubscriber(new InstrumentationOptions
						{
							CaptureCommandText = true,
							ShouldStartActivity = evt => evt.DatabaseNamespace.DatabaseName == url.DatabaseName
						}));
					var database = new MongoClient(settings).GetDatabase(url.DatabaseName);
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
