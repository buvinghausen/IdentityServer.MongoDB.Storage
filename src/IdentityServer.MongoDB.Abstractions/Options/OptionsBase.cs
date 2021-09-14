using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Options;

public abstract class OptionsBase
{
	public IMongoDatabase Database { get; set; }
}
