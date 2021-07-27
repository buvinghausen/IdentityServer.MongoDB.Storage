using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;
using Shared;

namespace Duende.IdentityServer.Web
{
	internal class Program
	{
		private static Task Main(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
					webBuilder.ConfigureServices((context, services) =>
					{
						// The expectation here is you've already run the AdminConsole and set up the database collections
						// With case insensitive collation and set all the necessary indexes
						// You can mix IDatabaseInitializer in your running application as it is replay safe
						// but it's this author's opinion you should run it out of band and avoid the startup costs of one time configuration
						// every single time you boot up your app
						// Also the configuration store only requires read access, the operational store needs read & write access
						var database = context.Configuration.GetMongoDatabase("Identity");
						services
							.AddOpenTelemetryTracing(context.HostingEnvironment.ApplicationName,
								builder => builder.AddAspNetCoreInstrumentation())
							.AddIdentityServer(options =>
								options.EmitStaticAudienceClaim =
									true) // https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
							.AddDeveloperSigningCredential()
							.AddMongoConfigurationStore(options => options.Database = database)
							.AddMongoOperationalStore(options =>
							{
								options.Database = database;
								options.EnableTokenCleanup =
									true; // <-- turn on IHostedService which will periodically wipe PersistedGrants & DeviceCodes (defaults to false)
								options.RemoveConsumedTokens =
									true; // <-- cleanup consumed tokens in addition to expired tokens from PersistedGrants (defaults to false)
							});
						services.AddControllers();
					}).Configure((context, app) =>
					{
						if (context.HostingEnvironment.IsDevelopment()) app.UseDeveloperExceptionPage();
						app
							.UseHttpsRedirection()
							.UseRouting()
							.UseAuthorization()
							.UseIdentityServer()
							.UseEndpoints(endpoints => endpoints.MapControllers());
					}))
				.RunConsoleAsync();
	}
}
