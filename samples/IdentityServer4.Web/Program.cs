using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
var database = builder.Configuration.GetMongoDatabase("Identity");
builder.Services
	.AddOpenTelemetryTracing(builder.Environment.ApplicationName, trace => trace.AddAspNetCoreInstrumentation())
	// https://docs.duendesoftware.com/identityserver/v5/fundamentals/resources/
	.AddIdentityServer(options => options.EmitStaticAudienceClaim = true)
	.AddDeveloperSigningCredential()
	.AddMongoConfigurationStore(options => options.Database = database)
	.AddMongoOperationalStore(options =>
	{
		options.Database = database;
		options.EnableTokenCleanup = true; // <-- turn on IHostedService which will periodically wipe PersistedGrants & DeviceCodes (defaults to false)
		options.RemoveConsumedTokens = true; // <-- cleanup consumed tokens in addition to expired tokens (defaults to false)
	});
builder.Services
	.AddControllers();
var app = builder.Build();
if (builder.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();
app
	.UseHttpsRedirection()
	.UseRouting()
	.UseAuthorization()
	.UseIdentityServer()
	.UseEndpoints(endpoints => endpoints.MapControllers());
await app.RunAsync()
	.ConfigureAwait(false);
