using IdentityServer.MongoDB.Abstractions.Options;

namespace Duende.IdentityServer.MongoDB.Storage.Options
{
	public sealed class ConfigurationStoreOptions : ConfigurationStoreOptionsBase
	{
		public string IdentityProviderCollectionName { get; set; } = "IdentityProviders";
	}
}
