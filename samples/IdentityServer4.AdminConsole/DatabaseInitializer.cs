using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer4.Models;
using Microsoft.Extensions.Hosting;

namespace IdentityServer4.AdminConsole
{
	internal class DatabaseInitializer : IHostedService
	{
		private readonly IDatabaseInitializer _databaseInitializer;
		private readonly IConfigurationStoreUpdater<Client> _clientUpdater;
		private readonly IConfigurationStoreUpdater<ApiResource> _apiResourceUpdater;
		private readonly IConfigurationStoreUpdater<ApiScope> _apiScopeUpdater;
		private readonly IConfigurationStoreUpdater<IdentityResource> _identityResourceUpdater;

		public DatabaseInitializer(IDatabaseInitializer databaseInitializer, IConfigurationStoreUpdater<Client> clientUpdater, IConfigurationStoreUpdater<ApiResource> apiResourceUpdater, IConfigurationStoreUpdater<ApiScope> apiScopeUpdater, IConfigurationStoreUpdater<IdentityResource> identityResourceUpdater)
		{
			_databaseInitializer = databaseInitializer;
			_clientUpdater = clientUpdater;
			_apiResourceUpdater = apiResourceUpdater;
			_apiScopeUpdater = apiScopeUpdater;
			_identityResourceUpdater = identityResourceUpdater;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			// Step 1 - Call database initialize to create collections & indexes (note: this only needs to be run once per database)
			// In this example I'm creating both the configuration store and operational store collections and indexes
			// Also it is replay safe so you can always run it multiple times
			await Task.WhenAll(
				_databaseInitializer.InitializeConfigurationStoreAsync(cancellationToken),
				_databaseInitializer.InitializeOperationalStoreAsync(cancellationToken));

			// Step 2 - Create configuration store documents of every type in the database


			// Step 3 - Show how updates work in the event you need to change the whole document or just part(s)


			// Step 4 - Delete document example
		}

		public Task StopAsync(CancellationToken cancellationToken) =>
			Task.CompletedTask;
	}
}
