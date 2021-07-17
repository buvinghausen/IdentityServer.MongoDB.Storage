using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Configuration;

namespace Duende.IdentityServer.MongoDB.Storage.Configuration
{
	internal static class MongoConfiguration
	{
		public static void Initialize()
		{
			MongoConfigurationBase.RegisterConventions("Duende.IdentityServer.MongoDB.Storage Conventions",
				typeof(Client).Namespace);

			MongoConfigurationBase.RegisterClassMaps<Client, PersistedGrant, DeviceFlowCode, DeviceCode>(
				client => client.ClientId,
				grant => grant.Key,
				code => code.UserCode,
				code => code.Subject);
		}
	}
}
