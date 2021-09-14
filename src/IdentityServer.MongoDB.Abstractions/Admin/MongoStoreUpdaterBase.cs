using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Admin;

public abstract class MongoStoreUpdaterBase<T> : IConfigurationStoreUpdater<T>
{
	private readonly IMongoCollection<T> _collection;
	private readonly UpdateDefinitionBuilder<T> _updateBuilder = Builders<T>.Update;
	private readonly ThreadLocal<IList<UpdateDefinition<T>>> _updates = new(() => new List<UpdateDefinition<T>>());

	protected MongoStoreUpdaterBase(IMongoCollection<T> collection)
	{
		_collection = collection;
	}

	public Task<T> FirstOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		_collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

	public Task<TProjection> FirstOrDefaultAsync<TProjection>(Expression<Func<T, bool>> filter, Expression<Func<T, TProjection>> projection, CancellationToken cancellationToken = default) =>
		_collection.Find(filter).Project(projection).FirstOrDefaultAsync(cancellationToken);

	public Task DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		_collection.DeleteManyAsync(filter, cancellationToken);

	public Task DeleteOneAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		_collection.DeleteOneAsync(filter, cancellationToken);

	public Task InsertManyAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
		_collection.InsertManyAsync(entities, cancellationToken: cancellationToken);

	public abstract Task InsertOrUpdateAsync(T entity, CancellationToken cancellationToken = default);

	public Task UpdateManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		UpdateAsync(filter, true, cancellationToken);

	public Task UpdateOneAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
		UpdateAsync(filter, false, cancellationToken);

	public bool HasChanges => _updates.Value.Any();

	public IConfigurationStoreUpdater<T> AddToSet<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value)
	{
		_updates.Value.Add(_updateBuilder.AddToSet(field,
			value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> AddToSetEach<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
	{
		_updates.Value.Add(_updateBuilder.AddToSetEach(field, values ?? throw new ArgumentNullException(nameof(values))));
		return this;
	}

	public IConfigurationStoreUpdater<T> BitwiseAnd<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.BitwiseAnd(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> BitwiseOr<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.BitwiseOr(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> BitwiseXor<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.BitwiseXor(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> Inc<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.Inc(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> Max<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.Max(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> Min<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.Min(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> Mul<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.Mul(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> PopFirst<TField>(Expression<Func<T, TField>> field)
	{
		_updates.Value.Add(_updateBuilder.PopFirst(field.ToObjectLambda()));
		return this;
	}

	public IConfigurationStoreUpdater<T> PopLast<TField>(Expression<Func<T, TField>> field)
	{
		_updates.Value.Add(_updateBuilder.PopLast(field.ToObjectLambda()));
		return this;
	}

	public IConfigurationStoreUpdater<T> Pull<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value)
	{
		_updates.Value.Add(_updateBuilder.Pull(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> PullAll<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
	{
		_updates.Value.Add(_updateBuilder.PullAll(field, values ?? throw new ArgumentNullException(nameof(values))));
		return this;
	}

	public IConfigurationStoreUpdater<T> PullFilter<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, Expression<Func<TItem, bool>> filter)
	{
		_updates.Value.Add(_updateBuilder.PullFilter(field, filter));
		return this;
	}

	public IConfigurationStoreUpdater<T> Push<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value)
	{
		_updates.Value.Add(_updateBuilder.Push(field, value ?? throw new ArgumentNullException(nameof(value))));
		return this;
	}

	public IConfigurationStoreUpdater<T> Set<TField>(Expression<Func<T, TField>> field, TField value)
	{
		_updates.Value.Add(_updateBuilder.Set(field, value));
		return this;
	}

	public IConfigurationStoreUpdater<T> Unset<TField>(Expression<Func<T, TField>> field)
	{
		_updates.Value.Add(_updateBuilder.Unset(field.ToObjectLambda()));
		return this;
	}

	protected Task InsertOrUpdateAsync(Expression<Func<T, bool>> filter, T entity, CancellationToken cancellationToken = default) =>
		_collection.ReplaceOneAsync(filter, entity, new UpdateOptions { IsUpsert = true }, cancellationToken);

	private async Task UpdateAsync(Expression<Func<T, bool>> filter, bool multiple, CancellationToken cancellationToken = default)
	{
		if (!HasChanges) return;
		var updates = _updateBuilder.Combine(_updates.Value);
		if (multiple)
		{
			await _collection.UpdateManyAsync(filter, updates, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}
		else
		{
			await _collection.UpdateOneAsync(filter, updates, cancellationToken: cancellationToken)
				.ConfigureAwait(false);
		}

		_updates.Value.Clear();
	}
}

internal static class MongoExtensions
{
	internal static Expression<Func<TDocument, object>> ToObjectLambda<TDocument, TField>(this Expression<Func<TDocument, TField>> value) =>
		Expression.Lambda<Func<TDocument, object>>(Expression.Convert(value.Body, typeof(object)), value.Parameters);
}
