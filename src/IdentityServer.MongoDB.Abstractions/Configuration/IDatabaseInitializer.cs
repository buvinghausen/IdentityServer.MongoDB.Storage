namespace IdentityServer.MongoDB.Abstractions.Configuration;

public interface IDatabaseInitializer
{
	Task InitializeConfigurationStoreAsync(CancellationToken cancellationToken = default);
	Task InitializeOperationalStoreAsync(CancellationToken cancellationToken = default);
}
