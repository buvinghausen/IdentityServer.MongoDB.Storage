using IdentityServer.MongoDB.Abstractions.Options;

namespace Duende.IdentityServer.MongoDB.Storage.Options
{
	public sealed class OperationalStoreOptions : OperationalStoreOptionsBase
	{
		public string SigningKeyCollectionName { get; set; } = "SigningKeys";
	}
}
