using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace IdentityServer4.MongoDB.Storage.Configuration
{
	// Rather than mess with AutoMapper, sub classes, and all that noise just tell Mongo how to write and read ClaimsPrincipal to disk
	internal class ClaimsPrincipalSerializer : SerializerBase<ClaimsPrincipal>
	{
		public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args,
			ClaimsPrincipal value)
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
			return new ClaimsPrincipal(new ClaimsIdentity(claims, authenticationType, JwtClaimTypes.Name,
				JwtClaimTypes.Role));
		}
	}
}
