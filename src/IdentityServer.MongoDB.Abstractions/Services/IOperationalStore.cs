namespace IdentityServer.MongoDB.Abstractions.Services;

interface IOperationalStore
{
	Task RemoveTokensAsync(CancellationToken cancellationToken = default);
}
