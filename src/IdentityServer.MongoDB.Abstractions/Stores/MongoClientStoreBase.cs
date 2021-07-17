using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoClientStoreBase<T> : MongoStoreBase<T>
	{
		protected MongoClientStoreBase(IMongoDatabase database) : base(database, CollectionNames.ClientCollectionName)
		{
		}
	}
}
