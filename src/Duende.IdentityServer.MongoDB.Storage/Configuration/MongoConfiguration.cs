using Duende.IdentityServer.Models;
using IdentityServer.MongoDB.Abstractions.Configuration;
using MongoDB.Bson.Serialization;

namespace Duende.IdentityServer.MongoDB.Storage.Configuration
{
	internal static class MongoConfiguration
	{
		public static void Initialize()
		{
			// Register shared configuration
			MongoConfigurationBase.RegisterConventions("Duende.IdentityServer.MongoDB.Storage Conventions",
				typeof(Client).Namespace);

			MongoConfigurationBase.RegisterClassMaps<Client, PersistedGrant, DeviceFlowCode, DeviceCode>(
				client => client.ClientId,
				grant => grant.Key,
				code => code.UserCode,
				code => code.Subject);

			// Register specific configuration
			// Tell Mongo to register the scheme to the _id property
			BsonClassMap.RegisterClassMap<IdentityProvider>(cm =>
			{
				cm.AutoMap();
				cm.SetIdMember(cm.GetMemberMap(ip => ip.Scheme));
			});
		}
	}
}
