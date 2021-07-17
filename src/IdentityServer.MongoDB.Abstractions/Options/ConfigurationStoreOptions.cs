using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Options
{
	class ConfigurationStoreOptions
	{
		public IMongoDatabase Database { get; set; }

		public string ClientCollectionName { get; set; } = "Clients";
		public string ResourceCollectionName { get; set; } = "Resources";
	}
}
