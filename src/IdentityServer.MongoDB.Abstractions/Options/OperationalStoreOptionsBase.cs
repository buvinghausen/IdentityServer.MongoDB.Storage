namespace IdentityServer.MongoDB.Abstractions.Options;

public abstract class OperationalStoreOptionsBase : OptionsBase
{
	public bool EnableTokenCleanup { get; set; }

	public bool RemoveConsumedTokens { get; set; }

	public TimeSpan TokenCleanupInterval { get; set; } = TimeSpan.FromHours(1);

	public string DeviceFlowCollectionName { get; set; } = "DeviceCodes";

	public string PersistedGrantCollectionName { get; set; } = "PersistedGrants";
}
