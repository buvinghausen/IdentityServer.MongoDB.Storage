using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoClientStoreBase<T> : MongoStoreBase<T>
	{
		internal const string CollectionName = "Clients";

		protected MongoClientStoreBase(IMongoDatabase database) : base(database, CollectionName)
		{
		}
	}
}
