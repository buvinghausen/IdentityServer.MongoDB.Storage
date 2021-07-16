﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoStoreBase<T>
	{
		private readonly IMongoCollection<T> _collection;

		protected MongoStoreBase(IMongoDatabase database, string collectionName)
		{
			_collection = database.GetCollection<T>(collectionName);
		}

		protected Task<T> SingleOrDefaultAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
			_collection
				.Find(filter)
				.SingleOrDefaultAsync(cancellationToken);

		protected Task DeleteManyAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
			_collection.DeleteManyAsync(filter, cancellationToken);

		protected Task DeleteOneAsync(Expression<Func<T, bool>> filter, CancellationToken cancellationToken = default) =>
			_collection.DeleteOneAsync(filter, cancellationToken);

		protected Task ReplaceOneAsync(Expression<Func<T, bool>> filter, T entity, CancellationToken cancellationToken = default) =>
			_collection.ReplaceOneAsync(filter, entity, new UpdateOptions { IsUpsert = true }, cancellationToken);

		protected Task UpdateOneAsync(Expression<Func<T, bool>> filter, UpdateDefinition<T> updates,
			CancellationToken cancellationToken = default) =>
			_collection.UpdateOneAsync(filter, updates, cancellationToken: cancellationToken);

		protected async Task<IEnumerable<T>> ToEnumerableAsync(Expression<Func<T, bool>> filter,
			CancellationToken cancellationToken = default) =>
			await _collection.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);

		protected async Task<IEnumerable<TChild>> ToEnumerableAsync<TChild>(Expression<Func<TChild, bool>> filter, CancellationToken cancellationToken = default) where TChild : T =>
			await _collection.OfType<TChild>().Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);

		protected Task<List<T>> ToListAsync(CancellationToken cancellationToken = default) =>
			_collection.Find(_ => true).ToListAsync(cancellationToken);
	}
}
