﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Stores;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using MongoDB.Driver;

namespace IdentityServer4.MongoDB.Storage.Stores
{
	// This class leverages MongoDB's polymorphic storage capacity to keep all three resource types in one collection
	internal class MongoResourceStore : MongoResourceStoreBase<Resource>, IResourceStore
	{
		public MongoResourceStore(IMongoDatabase database) : base(database)
		{
		}

		public Task<IEnumerable<IdentityResource>>
			FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames) =>
			FindResourcesByName<IdentityResource>(scopeNames);

		public Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames) =>
			FindResourcesByName<ApiScope>(scopeNames);

		public Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames) =>
			ToEnumerableAsync<ApiResource>(r => r.Scopes.Any(scopeNames.Contains));

		public Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames) =>
			FindResourcesByName<ApiResource>(apiResourceNames);

		public Task<Resources> GetAllResourcesAsync() => GetAllResourcesAsync(result =>
			new Resources(
				result.OfType<IdentityResource>(),
				result.OfType<ApiResource>(),
				result.OfType<ApiScope>()));

		private Task<IEnumerable<T>> FindResourcesByName<T>(IEnumerable<string> names) where T : Resource =>
			ToEnumerableAsync<T>(r => names.Contains(r.Name));
	}
}
