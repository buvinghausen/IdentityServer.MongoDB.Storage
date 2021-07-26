
namespace IdentityServer.MongoDB.Abstractions.Options
{
	public abstract class ConfigurationStoreOptionsBase : OptionsBase
	{
		public string ClientCollectionName { get; set; } = "Clients";

		public string ResourceCollectionName { get; set; } = "Resources";

		public bool AddConfigurationStoreUpdate { get; set; } = false;
	}
}
