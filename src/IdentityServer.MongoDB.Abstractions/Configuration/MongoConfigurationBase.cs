using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Claims;
using IdentityModel;
using IdentityServer.MongoDB.Abstractions.Entities;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace IdentityServer.MongoDB.Abstractions.Configuration;

internal static class MongoConfigurationBase
{
	internal static void RegisterConventions(string conventionName, string defaultNamespace) =>
		ConventionRegistry.Register(conventionName,
			new ConventionPack
			{
				new CamelCaseElementNameConvention(), // <- this is here because it's this author's belief BSON should be camelCase like JSON
				new IgnoreIfNullConvention(false), // <- This has to explicitly be here so properties can be set to null and not get replaced by the defaults
				new IgnoreExtraElementsConvention(true) // <- This has to be explicitly here so the Resource polymorphism works and to make it resilient against possible future schema changes
			}, t => t.Namespace == defaultNamespace || t.Namespace == "IdentityServer.MongoDB.Abstractions.Entities");

	internal static void RegisterClassMaps<TClient, TPersistedGrant, TDeviceCode, TApiResource, TApiScope, TIdentityResource>(Expression<Func<TClient, string>> clientIdSelector, Expression<Func<TPersistedGrant, string>> keySelector, Expression<Func<TDeviceCode, ClaimsPrincipal>> subjectSelector)
	{
		// These must be mapped so you can run GetAllResources() as the first call on IResourceStore
		BsonClassMap.RegisterClassMap<TApiResource>();
		BsonClassMap.RegisterClassMap<TApiScope>();
		BsonClassMap.RegisterClassMap<TIdentityResource>();

		// Set ClientId to be Mongo's primary key
		BsonClassMap.RegisterClassMap<TClient>(cm =>
		{
			cm.AutoMap();
			cm.SetIdMember(cm.GetMemberMap(clientIdSelector));
		});

		// Set Key to be Mongo's primary key
		BsonClassMap.RegisterClassMap<TPersistedGrant>(cm =>
		{
			cm.AutoMap();
			cm.SetIdMember(cm.GetMemberMap(keySelector));
		});

		// This ClassMap is registered to configure the serializer for ClaimsPrincipal which is not a POCO friendly class
		BsonClassMap.RegisterClassMap<TDeviceCode>(cm =>
		{
			cm.AutoMap();
			cm.GetMemberMap(subjectSelector)
				.SetSerializer(new ClaimsPrincipalSerializer());
		});

		// Set UserCode to be Mongo's primary key
		BsonClassMap.RegisterClassMap<DeviceFlowCodeBase<TDeviceCode>>(cm =>
		{
			cm.AutoMap();
			cm.SetIdMember(cm.GetMemberMap(dc => dc.UserCode));
		});
	}

	private class ClaimsPrincipalSerializer : SerializerBase<ClaimsPrincipal>
	{
		public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, ClaimsPrincipal value)
		{
			var writer = context.Writer;
			writer.WriteStartDocument();
			writer.WriteName("authenticationType");
			writer.WriteString(value.Identity?.AuthenticationType);
			writer.WriteName("claims");
			writer.WriteStartArray();
			foreach (var claim in value.Claims)
			{
				writer.WriteStartDocument();
				writer.WriteName("type");
				writer.WriteString(claim.Type);
				writer.WriteName("value");
				writer.WriteString(claim.Value);
				writer.WriteName("valueType");
				writer.WriteString(claim.ValueType);
				writer.WriteEndDocument();
			}
			writer.WriteEndArray();
			writer.WriteEndDocument();
		}

		public override ClaimsPrincipal Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var reader = context.Reader;
			reader.ReadStartDocument();
			var authenticationType = reader.ReadString();
			reader.ReadStartArray();
			var claims = new List<Claim>();
			while (reader.State != BsonReaderState.Type ||
					reader.ReadBsonType() != BsonType.EndOfDocument)
			{
				reader.ReadStartDocument();
				claims.Add(new Claim(
					reader.ReadString(),
					reader.ReadString(),
					reader.ReadString()));
				reader.ReadEndDocument();
			}
			reader.ReadEndArray();
			reader.ReadEndDocument();
			return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType, JwtClaimTypes.Name, JwtClaimTypes.Role));
		}
	}
}
