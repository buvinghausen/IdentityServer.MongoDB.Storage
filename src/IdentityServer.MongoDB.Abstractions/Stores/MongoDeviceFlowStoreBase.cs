using System.Linq.Expressions;
using System.Security.Claims;
using IdentityModel;
using IdentityServer.MongoDB.Abstractions.Entities;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer.MongoDB.Abstractions.Services;
using MongoDB.Driver;

namespace IdentityServer.MongoDB.Abstractions.Stores;

internal abstract class MongoDeviceFlowStoreBase<TModel, TEntity> : MongoStoreBase<TEntity>, IOperationalStore where TModel : class where TEntity : DeviceFlowCodeBase<TModel>, new()
{
	protected MongoDeviceFlowStoreBase(OperationalStoreOptionsBase options) :
		base(options.Database, options.DeviceFlowCollectionName)
	{
	}

	// IDeviceFlowStore implementation
	public Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, TModel data) =>
		StoreDeviceAuthorizationAsync(deviceCode, userCode, data, CancellationToken.None);

	public Task StoreDeviceAuthorizationAsync(string deviceCode, string userCode, TModel data, CancellationToken cancellationToken)
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
			}, cancellationToken);
	}

	public Task<TModel> FindByUserCodeAsync(string userCode) =>
		FindByUserCodeAsync(userCode, CancellationToken.None);

	public Task<TModel> FindByUserCodeAsync(string userCode, CancellationToken cancellationToken) =>
		FindAsync(dc => dc.UserCode == userCode, cancellationToken);

	public Task<TModel> FindByDeviceCodeAsync(string deviceCode) =>
		FindByDeviceCodeAsync(deviceCode, CancellationToken.None);

	public Task<TModel> FindByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken) =>
		FindAsync(dc => dc.DeviceCode == deviceCode, cancellationToken);

	public Task RemoveByDeviceCodeAsync(string deviceCode) =>
		RemoveByDeviceCodeAsync(deviceCode, CancellationToken.None);

	public Task RemoveByDeviceCodeAsync(string deviceCode, CancellationToken cancellationToken) =>
		DeleteOneAsync(dc => dc.DeviceCode == deviceCode, cancellationToken);

	public Task UpdateByUserCodeAsync(string userCode, TModel data) =>
		UpdateByUserCodeAsync(userCode, data, CancellationToken.None);

	public Task UpdateByUserCodeAsync(string userCode, TModel data, CancellationToken cancellationToken) =>
		UpdateOneAsync(dc => dc.UserCode == userCode, Builders<TEntity>.Update
			.Set(dc => dc.Data, data)
			.Set(dc => dc.SubjectId, GetIdentity(data)?.FindFirst(JwtClaimTypes.Subject)?.Value), cancellationToken);

	// IOperationalStore implementation
	public Task RemoveTokensAsync(CancellationToken cancellationToken = default) =>
		DeleteManyAsync(TokenCleanupFilter, cancellationToken);

	// Force child class to provide selectors & functions to handle different class assemblies & definitions
	protected abstract ClaimsPrincipal GetIdentity(TModel data);

	protected abstract (string ClientId, DateTime CreationTime, int Lifetime) GetMetadata(TModel data);

	protected abstract Expression<Func<TEntity, bool>> TokenCleanupFilter { get; }

	// Helper function to unwrap the model from the entity
	private async Task<TModel> FindAsync(Expression<Func<TEntity, bool>> filter, CancellationToken cancellationToken)
	{
		var document = await SingleOrDefaultAsync(filter, cancellationToken);
		return document?.Data;
	}
}
