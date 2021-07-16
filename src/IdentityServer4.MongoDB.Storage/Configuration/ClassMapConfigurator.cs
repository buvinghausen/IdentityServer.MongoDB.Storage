using IdentityServer4.Models;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace IdentityServer4.MongoDB.Storage.Configuration
{
	/// <summary>
	/// This class only Configures all the MongoDB conventions, class maps, & serializers needed.  It must be initialized before you start using the app
	/// </summary>
	public static class ClassMapConfigurator
	{
		public static void Configure()
		{
			// Register conventions needed to work with IS4 Models
			ConventionRegistry.Register("IdentityServer4 Mongo Storage Conventions",
				new ConventionPack
				{
					new CamelCaseElementNameConvention(), // <- this is here because it's this author's belief BSON should be camelCase like JSON
					new IgnoreIfNullConvention(false), // <- This has to explicitly be here so properties can be set to null and not get replaced by the defaults
					new IgnoreExtraElementsConvention(true) // <- This has to be explicitly here so the Resource polymorphism works and to make it resilient against possible future schema changes
				}, t => t.Namespace == "IdentityServer4.Models");
			
			// Set ClientId to be Mongo's primary key
			BsonClassMap.RegisterClassMap<Client>(cm =>
			{
				cm.AutoMap();
				cm.SetIdMember(cm.GetMemberMap(c => c.ClientId));
			});

			// Set Key to be Mongo's primary key
			BsonClassMap.RegisterClassMap<PersistedGrant>(cm =>
			{
				cm.AutoMap();
				cm.SetIdMember(cm.GetMemberMap(pg => pg.Key));
			});

			// Set UserCode to be Mongo's primary key
			BsonClassMap.RegisterClassMap<DeviceFlowCode>(cm =>
			{
				cm.AutoMap();
				cm.SetIdMember(cm.GetMemberMap(dc => dc.UserCode));
			});

			// This ClassMap is registered to configure the serializer for ClaimsPrincipal which is not a POCO friendly class
			BsonClassMap.RegisterClassMap<DeviceCode>(cm =>
			{
				cm.AutoMap();
				cm.GetMemberMap(dc => dc.Subject)
					.SetSerializer(new ClaimsPrincipalSerializer());
			});
		}
	}
}
