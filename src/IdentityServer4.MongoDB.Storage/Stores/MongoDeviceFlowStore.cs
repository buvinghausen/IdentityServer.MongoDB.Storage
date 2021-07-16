﻿using System;
using System.Security.Claims;
using IdentityServer.MongoDB.Abstractions.Stores;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Storage.Stores
{
	internal class MongoDeviceFlowStore : MongoDeviceFlowStoreBase<DeviceCode, DeviceFlowCode>, IDeviceFlowStore
	{
		public MongoDeviceFlowStore(IMongoDatabase database) : base(database)
		{
		}

		protected override ClaimsPrincipal GetIdentity(DeviceCode data) => data.Subject;

		protected override (string ClientId, DateTime CreationTime, int Lifetime) GetMetadata(DeviceCode data) =>
			(data.ClientId, data.CreationTime, data.Lifetime);
	}
}
