using Duende.Bff;
using Duende.Bff.MongoDB;
using Microsoft.AspNetCore.Builder;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extensions for BffBuilder
/// </summary>
public static class BffBuilderExtensions
{
	/// <summary>
	/// Adds MongoDB support for user session store.
	/// </summary>
	/// <param name="bffBuilder"></param>
	/// <returns></returns>
	public static BffBuilder AddMongoDbServerSideSessions(this BffBuilder bffBuilder)
	{
		_ = bffBuilder.Services.AddTransient<IUserSessionStoreCleanup, UserSessionStore>();
		return bffBuilder.AddServerSideSessions<UserSessionStore>();
	}
}
