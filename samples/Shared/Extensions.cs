using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SequentialGuid;

namespace Shared;

public static class Extensions
{
	// Wire up mongo in a telemetry friendly way so you can see all the commands in the console output
	public static IMongoDatabase GetMongoDatabase(this IConfiguration configuration, string name)
	{
		var url = new MongoUrl(configuration.GetConnectionString(name));
		var settings = MongoClientSettings.FromUrl(url);
		settings.ClusterConfigurator = cb =>
			cb.Subscribe(new DiagnosticsActivityEventSubscriber(new InstrumentationOptions
			{
				CaptureCommandText = true,
				ShouldStartActivity = evt => evt.DatabaseNamespace.DatabaseName == url.DatabaseName
			}));
		return new MongoClient(settings).GetDatabase(url.DatabaseName);
	}

	// Add default OTel configuration and allow for additional sources to be added
	public static IServiceCollection AddOpenTelemetryTracing(this IServiceCollection services, string name, Action<TracerProviderBuilder> configuration = null) =>
		services.AddOpenTelemetryTracing(builder =>
		{
			builder
				.SetResourceBuilder(ResourceBuilder.CreateDefault()
					.AddService(name, serviceInstanceId: SequentialGuidGenerator.Instance.NewGuid().ToString()))
				.AddMongoDBInstrumentation()
				.AddConsoleExporter();
			configuration?.Invoke(builder);
		});
}
