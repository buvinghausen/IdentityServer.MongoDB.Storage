using System;
using System.Linq.Expressions;
using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityServer.MongoDB.Abstractions.Options;
using IdentityServer.MongoDB.Abstractions.Stores;

namespace Duende.IdentityServer.MongoDB.Storage.Stores
{
	internal class MongoDeviceFlowStore : MongoDeviceFlowStoreBase<DeviceCode, DeviceFlowCode>, IDeviceFlowStore
	{
		public MongoDeviceFlowStore(OperationalStoreOptions options) : base(options)
		{
		}

		protected override ClaimsPrincipal GetIdentity(DeviceCode data) => data.Subject;

		protected override (string ClientId, DateTime CreationTime, int Lifetime) GetMetadata(DeviceCode data) =>
			(data.ClientId, data.CreationTime, data.Lifetime);

		protected override Expression<Func<DeviceFlowCode, bool>> TokenCleanupFilter =>
			code => code.Expiration <= DateTime.UtcNow;
	}
}
