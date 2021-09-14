using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer.MongoDB.Abstractions.Services;
using LinqKit;

namespace IdentityServer.MongoDB.Abstractions.Stores;

internal abstract class MongoPersistedGrantStoreBase<TModel, TFilter> : MongoStoreBase<TModel>, IOperationalStore
{
	protected readonly bool RemoveConsumedTokens;

	protected MongoPersistedGrantStoreBase(OperationalStoreOptionsBase options) :
		base(options.Database, options.PersistedGrantCollectionName)
	{
		RemoveConsumedTokens = options.RemoveConsumedTokens;
	}

	// IPersistedGrantStore implementation
	public Task StoreAsync(TModel grant) => StoreAsync(grant, CancellationToken.None);

	public Task StoreAsync(TModel grant, CancellationToken cancellationToken) =>
		ReplaceOneAsync(PropFilter(KeySelector, GetKey(grant)), grant, cancellationToken);

	public Task<TModel> GetAsync(string key) => GetAsync(key, CancellationToken.None);

	public Task<TModel> GetAsync(string key, CancellationToken cancellationToken) =>
		SingleOrDefaultAsync(PropFilter(KeySelector, key), cancellationToken);

	public Task RemoveAsync(string key) => RemoveAsync(key, CancellationToken.None);

	public Task RemoveAsync(string key, CancellationToken cancellationToken) =>
		DeleteOneAsync(PropFilter(KeySelector, key), cancellationToken);

	public Task<IEnumerable<TModel>> GetAllAsync(TFilter filter) => GetAllAsync(filter, CancellationToken.None);

	public Task<IEnumerable<TModel>> GetAllAsync(TFilter filter, CancellationToken cancellationToken) =>
		ToEnumerableAsync(ToExpression(filter), cancellationToken);

	public Task RemoveAllAsync(TFilter filter) => RemoveAllAsync(filter, CancellationToken.None);

	public Task RemoveAllAsync(TFilter filter, CancellationToken cancellationToken) =>
		DeleteManyAsync(ToExpression(filter), cancellationToken);

	// IOperationalStore implementation
	public Task RemoveTokensAsync(CancellationToken cancellationToken = default) =>
		DeleteManyAsync(TokenCleanupFilter, cancellationToken);

	// Force child class to provide selectors & functions to handle different class assemblies & definitions
	protected abstract (string SubjectId, string ClientId, string SessionId, string Type) GetFilterMetadata(TFilter filter);

	protected abstract void ValidateFilter(TFilter filter);

	protected abstract string GetKey(TModel model);

	protected abstract Expression<Func<TModel, string>> KeySelector { get; }

	protected abstract Expression<Func<TModel, string>> SubjectIdSelector { get; }

	protected abstract Expression<Func<TModel, string>> ClientIdSelector { get; }

	protected abstract Expression<Func<TModel, string>> SessionIdSelector { get; }

	protected abstract Expression<Func<TModel, string>> TypeSelector { get; }

	protected abstract Expression<Func<TModel, bool>> TokenCleanupFilter { get; }

	// Helper function to build Mongo driver compatible predicates
	private Expression<Func<TModel, bool>> ToExpression(TFilter filter)
	{
		// Perform validation on the filter
		ValidateFilter(filter);
		Expression<Func<TModel, bool>> exp = default;
		var builder = PredicateBuilder.New<TModel>();
		var (subjectId, clientId, sessionId, type) = GetFilterMetadata(filter);

		// The way Identity Server built the indexes in EF was first via SubjectId
		if (!string.IsNullOrWhiteSpace(subjectId))
			exp = builder.Start(PropFilter(SubjectIdSelector, subjectId));

		// Then either ClientId or SessionId
		if (!string.IsNullOrWhiteSpace(clientId))
			exp = builder.And(PropFilter(ClientIdSelector, clientId));
		if (!string.IsNullOrWhiteSpace(sessionId))
			exp = builder.And(PropFilter(SessionIdSelector, sessionId));

		// Then last by Type
		if (!string.IsNullOrWhiteSpace(type))
			exp = builder.And(PropFilter(TypeSelector, type));

		// Flatten expression into something the Mongo driver can understand since it can't hang with invocation expressions
		return exp.Expand();
	}

	// Helper function to flatten selector expression & concrete value into predicate expression
	private static Expression<Func<TModel, bool>> PropFilter(Expression<Func<TModel, string>> selector, string value) =>
		Expression.Lambda<Func<TModel, bool>>(Expression.Equal(selector.Body, Expression.Constant(value)), selector.Parameters[0]);
}
