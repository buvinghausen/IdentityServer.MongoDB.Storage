using System;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.MongoDB.Abstractions.Entities;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer.MongoDB.Abstractions.Services;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Stores
{
	internal abstract class MongoDeviceFlowStoreBase<TModel, TEntity> : MongoStoreBase<TEntity>, IOperationalStore
		where TModel : class
		where TEntity : DeviceFlowCodeBase<TModel>, new()
	{
		protected MongoDeviceFlowStoreBase(OperationalStoreOptions options) : base(options.Database,
			CollectionNames.DeviceCodeCollectionName)
		{
		}

		// IDeviceFlowStore implementation
		public Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, TModel data)
		{
			var (clientId, creationTime, lifetime) = GetMetadata(data);
			return ReplaceOneAsync(dc => dc.UserCode == userCode,
				new TEntity
				{
					DeviceCode = deviceCode,
					UserCode = userCode,
					ClientId = clientId,
					SubjectId = GetIdentity(data)?.FindFirst(JwtClaimTypes.Subject)?.Value,
					CreationTime = creationTime,
					Expiration = creationTime.AddSeconds(lifetime),
					Data = data
				});
		}

		public Task<TModel> FindByUserCodeAsync(string userCode) =>
			FindAsync(dc => dc.UserCode == userCode);

		public Task<TModel> FindByDeviceCodeAsync(string deviceCode) =>
			FindAsync(dc => dc.DeviceCode == deviceCode);

		public Task RemoveByDeviceCodeAsync(string deviceCode) =>
			DeleteOneAsync(dc => dc.DeviceCode == deviceCode);

		public Task UpdateByUserCodeAsync(string userCode, TModel data) =>
			UpdateOneAsync(dc => dc.UserCode == userCode, Builders<TEntity>.Update
				.Set(dc => dc.Data, data)
				.Set(dc => dc.SubjectId, GetIdentity(data)?.FindFirst(JwtClaimTypes.Subject)?.Value));

		// IOperationalStore implementation
		public Task RemoveTokensAsync(CancellationToken cancellationToken = default) =>
			DeleteManyAsync(TokenCleanupFilter, cancellationToken);

		// Force child class to provide selectors & functions to handle different class assemblies & definitions
		protected abstract ClaimsPrincipal GetIdentity(TModel data);

		protected abstract (string ClientId, DateTime CreationTime, int Lifetime) GetMetadata(TModel data);

		protected abstract Expression<Func<TEntity, bool>> TokenCleanupFilter { get; }

		// Helper function to unwrap the model from the entity
		private async Task<TModel> FindAsync(Expression<Func<TEntity, bool>> filter)
		{
			var document = await SingleOrDefaultAsync(filter);
			return document?.Data;
		}
	}
}
