using MongoDB.Bson.Serialization.Conventions;

namespace IdentityServer.MongoDB.Abstractions.Configuration
{
	internal static class MongoConventions
	{
		internal static void Register(string conventionName, string defaultNamespace) =>
			ConventionRegistry.Register(conventionName,
				new ConventionPack
				{
					new CamelCaseElementNameConvention(), // <- this is here because it's this author's belief BSON should be camelCase like JSON
					new IgnoreIfNullConvention(false), // <- This has to explicitly be here so properties can be set to null and not get replaced by the defaults
					new IgnoreExtraElementsConvention(true) // <- This has to be explicitly here so the Resource polymorphism works and to make it resilient against possible future schema changes
				}, t => t.Namespace == defaultNamespace);
	}
}
