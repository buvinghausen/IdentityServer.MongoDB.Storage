using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace Duende.Bff.MongoDB;

internal sealed class UserSessionStore : IUserSessionStore, IUserSessionStoreCleanup
{
	private readonly string? _applicationDiscriminator;
	private readonly IMongoCollection<UserSessionEntity> _collection;
	private readonly IMongoQueryable<UserSessionEntity> _queryable;
	private readonly ILogger<UserSessionStore> _logger;

	public UserSessionStore(IOptions<DataProtectionOptions> options, IMongoDatabase database, ILogger<UserSessionStore> logger) : this(options, database.GetCollection<UserSessionEntity>("UserSessions"), logger)
	{
	}

	private UserSessionStore(IOptions<DataProtectionOptions> options, IMongoCollection<UserSessionEntity> collection, ILogger<UserSessionStore> logger)
	{
		_applicationDiscriminator = options.Value.ApplicationDiscriminator;
		_collection = collection;
		_queryable = collection.AsQueryable();
		_logger = logger;
	}

	/// <inheritdoc/>
	public async Task<UserSession?> GetUserSessionAsync(string key, CancellationToken cancellationToken = default)
	{
		var item = await _queryable
			.SingleOrDefaultAsync(us => us.Key == key && us.ApplicationName == _applicationDiscriminator,
				cancellationToken).ConfigureAwait(false);
		UserSession? result = null;
		if (item != null)
		{
			result = new UserSession();
			item.CopyTo(result);
		}
		else
		{
			_logger.NoUserSessionFound(key);
		}
		return result;
	}


	/// <inheritdoc/>
	public Task CreateUserSessionAsync(UserSession session, CancellationToken cancellationToken = default)
	{
		var item = new UserSessionEntity
		{
			ApplicationName = _applicationDiscriminator
		};
		session.CopyTo(item);
		return _collection.InsertOneAsync(item, new InsertOneOptions(), cancellationToken);
	}

	/// <inheritdoc/>
	public async Task UpdateUserSessionAsync(string key, UserSessionUpdate session,
		CancellationToken cancellationToken = default)
	{
		var item = await _queryable.SingleOrDefaultAsync(us => us.Key == key && us.ApplicationName == _applicationDiscriminator, cancellationToken).ConfigureAwait(false);
		if (item != null)
		{
			session.CopyTo(item);
			await _collection.ReplaceOneAsync(us => us.Key == key && us.ApplicationName == _applicationDiscriminator, item, new UpdateOptions { IsUpsert = false }, cancellationToken).ConfigureAwait(false);
		}
		else
		{
			_logger.NoUserSessionFound(key);
		}
	}

	/// <inheritdoc/>
	public Task DeleteUserSessionAsync(string key, CancellationToken cancellationToken = default) =>
		_collection.DeleteOneAsync(us => us.Key == key && us.ApplicationName == _applicationDiscriminator,
			cancellationToken);

	/// <inheritdoc/>
	public Task<IReadOnlyCollection<UserSession>> GetUserSessionsAsync(UserSessionsFilter filter, CancellationToken cancellationToken = default)
	{
		filter.Validate();
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task DeleteUserSessionsAsync(UserSessionsFilter filter, CancellationToken cancellationToken = default)
	{
		filter.Validate();
		
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task DeleteExpiredSessionsAsync(CancellationToken cancellationToken = default) =>
		_collection.DeleteManyAsync(us => us.Expires < DateTime.UtcNow, cancellationToken);
}
