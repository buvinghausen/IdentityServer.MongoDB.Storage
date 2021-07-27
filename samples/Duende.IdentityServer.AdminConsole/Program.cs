using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

namespace Duende.IdentityServer.AdminConsole
{
	internal class Program
	{
		private static Task Main(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureServices((hostContext, services) =>
				{
					// Wire up otel (not required) and point identityserver admin to the desired database
					// Note: operational store and configuration stores can be in separate databases
					// You will need write access to both stores from the admin console
					var database = hostContext.Configuration.GetMongoDatabase("Identity");
					services
						.AddOpenTelemetryTracing(hostContext.HostingEnvironment.ApplicationName)
						.AddIdentityServerConfigurationStoreAdmin(options => options.Database = database)
						.AddIdentityServerOperationalStoreAdmin(options => options.Database = database)
						.AddHostedService<DatabaseInitializer>();
				})
				.RunConsoleAsync();
	}
}
