﻿using ConfigurationStoreOptionsBase = IdentityServer.MongoDB.Abstractions.Options.ConfigurationStoreOptions;

namespace Duende.IdentityServer.MongoDB.Storage.Options
{
	class ConfigurationStoreOptions : ConfigurationStoreOptionsBase
	{
		public string IdentityProviderCollectionName { get; set; } = "IdentityProviders";
		public string SigningKeyCollectionName { get; set; } = "SigningKeys";
	}
}