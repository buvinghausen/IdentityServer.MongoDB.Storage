using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SequentialGuid;

namespace IdentityServer4.Web
{
	internal class Program
	{
		private static Task Main(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder =>
					webBuilder.ConfigureServices((context, services) =>
					{
						services.AddControllers();
						// Wire up mongo in a telemetry friendly way so you can see all the commands in the console output
						// Normally I would have an 
						var url = new MongoUrl(context.Configuration.GetConnectionString("Identity"));
						var settings = MongoClientSettings.FromUrl(url);
						settings.ClusterConfigurator = cb =>
							cb.Subscribe(new DiagnosticsActivityEventSubscriber(new InstrumentationOptions
							{
								CaptureCommandText = true,
								ShouldStartActivity = evt => evt.DatabaseNamespace.DatabaseName == url.DatabaseName
							}));
						var database = new MongoClient(settings).GetDatabase(url.DatabaseName);
						// The expectation here is you've already run the AdminConsole and set up the database collections
						// With case insensitive collation and set all the necessary indexes
						// You can mix IDatabaseInitializer in your running application as it is replay safe
						// but it's this author's opinion you should run it out of band and avoid the startup costs of one time configuration
						// every single time you boot up your app
						services
							.AddOpenTelemetryTracing(builder => builder
								.SetResourceBuilder(ResourceBuilder.CreateDefault()
									.AddService(context.HostingEnvironment.ApplicationName,
										serviceInstanceId: SequentialGuidGenerator.Instance.NewGuid().ToString()))
								.AddAspNetCoreInstrumentation()
								.AddMongoDBInstrumentation()
								.AddConsoleExporter())
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
									true; // <-- cleanup consumed tokens in addition to expired tokens (defaults to false)
							});
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
