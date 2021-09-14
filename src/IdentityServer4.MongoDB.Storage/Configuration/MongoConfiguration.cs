using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer4.Models;

namespace IdentityServer4.MongoDB.Storage.Configuration;

public static class MongoConfiguration
{
	public static void Initialize()
	{
		MongoConfigurationBase.RegisterConventions("IdentityServer4.MongoDB.Storage Conventions", typeof(Client).Namespace);

		MongoConfigurationBase.RegisterClassMaps<Client, PersistedGrant, DeviceCode, ApiResource, ApiScope, IdentityResource>(client => client.ClientId, grant => grant.Key, code => code.Subject);
	}
}
