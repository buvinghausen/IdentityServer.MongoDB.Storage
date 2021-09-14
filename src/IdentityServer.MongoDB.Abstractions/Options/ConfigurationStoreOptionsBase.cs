namespace IdentityServer.MongoDB.Abstractions.Options;

public abstract class ConfigurationStoreOptionsBase : OptionsBase
{
	public string ClientCollectionName { get; set; } = "Clients";

	public string ResourceCollectionName { get; set; } = "Resources";

	public bool AddConfigurationStoreUpdaters { get; set; } = false;
}
