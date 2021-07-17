using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;
using MongoDB.Driver;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoSigningKeyStore : MongoStoreBase<SerializedKey>, ISigningKeyStore
	{
		public MongoSigningKeyStore(IMongoDatabase database) : base(database, "SigningKeys")
		{
		}

		public Task<IEnumerable<SerializedKey>> LoadKeysAsync() =>
			ToEnumerableAsync();

		public Task StoreKeyAsync(SerializedKey key) =>
			ReplaceOneAsync(sk => sk.Id == key.Id, key);

		public Task DeleteKeyAsync(string id) =>
			DeleteOneAsync(sk => sk.Id == id);
	}
}
