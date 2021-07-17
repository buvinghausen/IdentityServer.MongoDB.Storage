using IdentityServer.MongoDB.Abstractions.Options;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoClientStoreBase<T> : MongoStoreBase<T>
	{
		protected MongoClientStoreBase(ConfigurationStoreOptions options) : base(options.Database, options.ClientCollectionName)
		{
		}
	}
}
