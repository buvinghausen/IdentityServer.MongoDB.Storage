using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace IdentityServer.MongoDB.Abstractions.Stores;

internal abstract class MongoStoreBase<T>
{
	private readonly IMongoCollection<T> _collection;
	private readonly IMongoQueryable<T> _query;

	protected MongoStoreBase(IMongoDatabase database, string collectionName)
	{
		_collection = database.GetCollection<T>(collectionName);
		_query = _collection.AsQueryable();
	}

	protected Task<bool> AnyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		_query.AnyAsync(filter, cancellationToken);

	protected Task DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		_collection.DeleteManyAsync(filter, cancellationToken);

	protected Task DeleteOneAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		_collection.DeleteOneAsync(filter, cancellationToken);

	protected Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T entity, CancellationToken cancellationToken = default) =>
		_collection.ReplaceOneAsync(filter, entity, new UpdateOptions { IsUpsert = true }, cancellationToken);

	protected Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		_query.SingleOrDefaultAsync(filter, cancellationToken);

	protected async Task<IEnumerable<T>> ToEnumerableAsync(CancellationToken cancellationToken = default) =>
		await _query.ToListAsync(cancellationToken).ConfigureAwait(false);

	protected async Task<IEnumerable<T>> ToEnumerableAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		await _query.Where(filter).ToListAsync(cancellationToken).ConfigureAwait(false);

	protected async Task<IEnumerable<TChild>> ToEnumerableAsync<TChild>(Expression<Func<TChild, bool>> filter, CancellationToken cancellationToken = default) where TChild : T =>
		await _query.OfType<TChild>().Where(filter).ToListAsync(cancellationToken).ConfigureAwait(false);

	protected async Task<IEnumerable<TProjection>> ToEnumerableAsync<TProjection>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjection>> projection, CancellationToken cancellationToken = default) =>
		await _query.Where(filter).Select(projection).ToListAsync(cancellationToken).ConfigureAwait(false);

	protected async Task<IEnumerable<TProjection>> ToEnumerableAsync<TProjection>(Expression<Func<T, TProjection>> projection, CancellationToken cancellationToken = default) =>
		await _query.Select(projection).ToListAsync(cancellationToken).ConfigureAwait(false);

	protected Task<List<T>> ToListAsync(CancellationToken cancellationToken = default) =>
		_query.ToListAsync(cancellationToken);

	protected Task UpdateOneAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> updates, CancellationToken cancellationToken = default) =>
		_collection.UpdateOneAsync(filter, updates, cancellationToken: cancellationToken);
}
