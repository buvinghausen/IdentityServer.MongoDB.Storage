using System.Linq.Expressions;

namespace IdentityServer.MongoDB.Abstractions.Admin;

public interface IConfigurationStoreUpdater<T>
{
	Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

	Task<TProjection> FirstOrDefaultAsync<TProjection>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjection>> projection, CancellationToken cancellationToken = default);

	Task DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

	Task DeleteOneAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

	Task InsertManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

	Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default);

	Task UpdateManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

	Task UpdateOneAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default);

	bool HasChanges { get; }

	IConfigurationStoreUpdater<T> AddToSet<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value);

	IConfigurationStoreUpdater<T> AddToSetEach<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values);

	IConfigurationStoreUpdater<T> BitwiseAnd<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> BitwiseOr<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> BitwiseXor<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> Inc<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> Max<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> Min<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> Mul<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> PopFirst<TField>(Expression<Func<T, TField>> field);

	IConfigurationStoreUpdater<T> PopLast<TField>(Expression<Func<T, TField>> field);

	IConfigurationStoreUpdater<T> Pull<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value);

	IConfigurationStoreUpdater<T> PullAll<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values);

	IConfigurationStoreUpdater<T> PullFilter<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, Expression<Func<TItem, bool>> filter);

	IConfigurationStoreUpdater<T> Push<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value);

	IConfigurationStoreUpdater<T> Set<TField>(Expression<Func<T, TField>> field, TField value);

	IConfigurationStoreUpdater<T> Unset<TField>(Expression<Func<T, TField>> field);
}
