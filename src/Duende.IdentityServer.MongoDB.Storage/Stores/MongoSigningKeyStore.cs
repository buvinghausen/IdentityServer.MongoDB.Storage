using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores;

internal sealed class MongoSigningKeyStore : MongoStoreBase<SerializedKey>, ISigningKeyStore
{
	public MongoSigningKeyStore(OperationalStoreOptions options) :
		base(options.Database, options.SigningKeyCollectionName)
	{
	}

	public Task<IEnumerable<SerializedKey>> LoadKeysAsync() =>
		LoadKeysAsync(CancellationToken.None);

	public Task<IEnumerable<SerializedKey>> LoadKeysAsync(CancellationToken cancellationToken) =>
		ToEnumerableAsync(cancellationToken);

	public Task StoreKeyAsync(SerializedKey key) =>
		StoreKeyAsync(key, CancellationToken.None);

	public Task StoreKeyAsync(SerializedKey key, CancellationToken cancellationToken) =>
		ReplaceOneAsync(sk => sk.Id == key.Id, key, cancellationToken);

	public Task DeleteKeyAsync(string id) =>
		DeleteKeyAsync(id, CancellationToken.None);

	public Task DeleteKeyAsync(string id, CancellationToken cancellationToken) =>
		DeleteOneAsync(sk => sk.Id == id, cancellationToken);
}
