using System;
using System.Linq.Expressions;
using System.Security.Claims;
using IdentityServer.MongoDB.Abstractions.Stores;
using IdentityServer4.Models;
using IdentityServer4.MongoDB.Storage.Options;
using IdentityServer4.Stores;

namespace IdentityServer4.MongoDB.Storage.Stores
{
	internal sealed class MongoDeviceFlowStore : MongoDeviceFlowStoreBase<DeviceCode, DeviceFlowCode>, IDeviceFlowStore
	{
		// ReSharper disable once SuggestBaseTypeForParameter
		public MongoDeviceFlowStore(OperationalStoreOptions options) : base(options)
		{
		}

		protected override ClaimsPrincipal GetIdentity(DeviceCode data) => data.Subject;

		protected override (string ClientId, DateTime CreationTime, int Lifetime) GetMetadata(DeviceCode data) =>
			(data.ClientId, data.CreationTime, data.Lifetime);

		protected override Expression<Func<DeviceFlowCode, bool>> TokenCleanupFilter =>
			code => code.Expiration < DateTime.UtcNow;
	}
}
