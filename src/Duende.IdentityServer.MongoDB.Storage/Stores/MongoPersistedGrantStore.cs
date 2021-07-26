using System;
using System.Linq.Expressions;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.MongoDB.Storage.Options;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoPersistedGrantStore : MongoPersistedGrantStoreBase<PersistedGrant, PersistedGrantFilter>,
		IPersistedGrantStore
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoPersistedGrantStore(OperationalStoreOptions options) : base(options)
		{
		}

		protected override (string SubjectId, string ClientId, string SessionId, string Type) GetFilterMetadata(
			PersistedGrantFilter filter) =>
			(filter.SubjectId, filter.ClientId, filter.SessionId, filter.Type);

		protected override void ValidateFilter(PersistedGrantFilter filter) =>
			filter.Validate();

		protected override string GetKey(PersistedGrant model) => model.Key;

		protected override Expression<Func<PersistedGrant, string>> KeySelector => grant => grant.Key;

		protected override Expression<Func<PersistedGrant, string>> SubjectIdSelector => grant => grant.SubjectId;

		protected override Expression<Func<PersistedGrant, string>> ClientIdSelector => grant => grant.ClientId;

		protected override Expression<Func<PersistedGrant, string>> SessionIdSelector => grant => grant.SessionId;

		protected override Expression<Func<PersistedGrant, string>> TypeSelector => grant => grant.Type;

		// Yield back the predicate expression on both sides of the ternary for Mongo to decipher correctly
		protected override Expression<Func<PersistedGrant, bool>> TokenCleanupFilter => RemoveConsumedTokens
			? grant =>
				grant.Expiration < DateTime.UtcNow || grant.ConsumedTime < DateTime.UtcNow
			: grant =>
				grant.Expiration < DateTime.UtcNow;
	}
}
