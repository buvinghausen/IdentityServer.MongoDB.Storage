using System;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Options
{
	class OperationalStoreOptions
	{
		public IMongoDatabase Database { get; set; }

		public bool EnableTokenCleanup { get; set; } = false;

		public bool RemoveConsumedTokens { get; set; } = false;

		public TimeSpan TokenCleanupInterval { get; set; } = TimeSpan.FromHours(1);
	}
}
