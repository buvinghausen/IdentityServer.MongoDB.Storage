using System.Collections.Generic;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoSigningKeyStore : MongoStoreBase<SerializedKey>, ISigningKeyStore
	{
		public MongoSigningKeyStore(ConfigurationStoreOptions options) : base(options.Database, options.SigningKeyCollectionName)
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
