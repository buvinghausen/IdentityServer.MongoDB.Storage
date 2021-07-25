using System;

namespace IdentityServer.MongoDB.Abstractions.Options
{
	public abstract class OperationalStoreOptionsBase : OptionsBase
	{
		public bool EnableTokenCleanup { get; set; } = false;

		public bool RemoveConsumedTokens { get; set; } = false;

		public TimeSpan TokenCleanupInterval { get; set; } = TimeSpan.FromHours(1);

		public string DeviceFlowCollectionName { get; set; } = "DeviceCodes";

		public string PersistedGrantCollectionName { get; set; } = "PersistedGrant";
	}
}
