using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.MongoDB.Abstractions.Options;
using Microsoft.Extensions.Hosting;

namespace IdentityServer.MongoDB.Abstractions.Services
{
	internal sealed class TokenCleanupService : BackgroundService
	{
		private readonly TimeSpan _interval;
		private readonly IOperationalStore[] _operationalStores;

		public TokenCleanupService(OperationalStoreOptionsBase options, IEnumerable<IOperationalStore> operationalStores)
		{
			_interval = options.TokenCleanupInterval;
			_operationalStores = operationalStores.ToArray();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				await Task.WhenAll(_operationalStores.Select(store => store.RemoveTokensAsync(stoppingToken))).ConfigureAwait(false);
				// wait specified interval
				await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
			}
		}
	}
}
