using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.MongoDB.Abstractions.Admin;
using IdentityServer.MongoDB.Abstractions.Configuration;
using IdentityServer4.Models;
using Microsoft.Extensions.Hosting;

namespace IdentityServer4.AdminConsole
{
	internal class DatabaseInitializer : IHostedService
	{
		private static readonly string[] AllowedScopes =
		{
			IdentityServerConstants.StandardScopes.OpenId, IdentityServerConstants.StandardScopes.Profile,
			IdentityServerConstants.StandardScopes.Email, "resource1.scope1", "resource2.scope1", "transaction"
		};

		private readonly IDatabaseInitializer _databaseInitializer;
		private readonly IConfigurationStoreUpdater<Client> _clientUpdater;
		private readonly IConfigurationStoreUpdater<ApiResource> _apiResourceUpdater;
		private readonly IConfigurationStoreUpdater<ApiScope> _apiScopeUpdater;
		private readonly IConfigurationStoreUpdater<IdentityResource> _identityResourceUpdater;

		// This constructor is here for the operational store only setup
		public DatabaseInitializer(IDatabaseInitializer databaseInitializer)
		{
			_databaseInitializer = databaseInitializer;
		}

		// This constructor will be called when the configuration store has been added
		public DatabaseInitializer(IDatabaseInitializer databaseInitializer,
			IConfigurationStoreUpdater<Client> clientUpdater,
			IConfigurationStoreUpdater<ApiResource> apiResourceUpdater,
			IConfigurationStoreUpdater<ApiScope> apiScopeUpdater,
			IConfigurationStoreUpdater<IdentityResource> identityResourceUpdater)
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

			// Configuration store updater configuration not loaded in service collection so just exit
			if (_clientUpdater is null)
				return;

			// Step 2 - Cleanup the tables to simulate a new configuration (please do not do this in your own app)
			await Task.WhenAll(
				_clientUpdater.DeleteManyAsync(_ => true, cancellationToken),
				_apiResourceUpdater.DeleteManyAsync(_ => true, cancellationToken),
				_apiScopeUpdater.DeleteManyAsync(_ => true, cancellationToken),
				_identityResourceUpdater.DeleteManyAsync(_ => true, cancellationToken));

			// Step 3 - Create configuration store documents of every type in the database
			var profile = new IdentityResources.Profile();
			var email = new IdentityResources.Email();
			await Task.WhenAll(
				_clientUpdater.InsertManyAsync(new Client[]
				{
					///////////////////////////////////////////
					// JS OIDC Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "js_oidc",
						ClientName = "JavaScript OIDC Client",
						ClientUri = "http://identityserver.io",
						AllowedGrantTypes = GrantTypes.Code,
						RequireClientSecret = false,
						RedirectUris =
						{
							"https://localhost:5001/index.html",
							"https://localhost:5001/callback.html",
							"https://localhost:5001/silent.html",
							"https://localhost:5001/popup.html"
						},
						PostLogoutRedirectUris = {"https://localhost:5001/index.html"},
						AllowedCorsOrigins = {"https://localhost:5001"},
						AllowedScopes = AllowedScopes
					},

					///////////////////////////////////////////
					// MVC Automatic Token Management Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "mvc.tokenmanagement",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.Code,
						RequirePkce = true,
						AccessTokenLifetime = 75,
						RedirectUris = {"https://localhost:5001/signin-oidc"},
						FrontChannelLogoutUri = "https://localhost:5001/signout-oidc",
						PostLogoutRedirectUris = {"https://localhost:5001/signout-callback-oidc"},
						AllowOfflineAccess = true,
						AllowedScopes = AllowedScopes
					},

					///////////////////////////////////////////
					// MVC Code Flow Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "mvc.code",
						ClientName = "MVC Code Flow",
						ClientUri = "http://identityserver.io",
						ClientSecrets = {new Secret("secret".Sha256())},
						RequireConsent = true,
						AllowedGrantTypes = GrantTypes.Code,
						RedirectUris = {"https://localhost:5001/signin-oidc"},
						FrontChannelLogoutUri = "https://localhost:5001/signout-oidc",
						PostLogoutRedirectUris = {"https://localhost:5001/signout-callback-oidc"},
						AllowOfflineAccess = true,
						AllowedScopes = AllowedScopes
					},

					///////////////////////////////////////////
					// MVC Hybrid Flow Sample (Back Channel logout)
					//////////////////////////////////////////
					new()
					{
						ClientId = "mvc.hybrid.backchannel",
						ClientName = "MVC Hybrid (with BackChannel logout)",
						ClientUri = "http://identityserver.io",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.Hybrid,
						RequirePkce = false,
						RedirectUris = {"https://localhost:5001/signin-oidc"},
						BackChannelLogoutUri = "https://localhost:5001/logout",
						PostLogoutRedirectUris = {"https://localhost:5001/signout-callback-oidc"},
						AllowOfflineAccess = true,
						AllowedScopes = AllowedScopes
					},

					///////////////////////////////////////////
					// MVC Code Flow with JAR/JWT Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "mvc.jar.jwt",
						ClientName = "MVC Code Flow with JAR/JWT",
						ClientSecrets =
						{
							new Secret
							{
								Type = IdentityServerConstants.SecretTypes.JsonWebKey,
								Value =
									"{'e':'AQAB','kid':'ZzAjSnraU3bkWGnnAqLapYGpTyNfLbjbzgAPbbW2GEA','kty':'RSA','n':'wWwQFtSzeRjjerpEM5Rmqz_DsNaZ9S1Bw6UbZkDLowuuTCjBWUax0vBMMxdy6XjEEK4Oq9lKMvx9JzjmeJf1knoqSNrox3Ka0rnxXpNAz6sATvme8p9mTXyp0cX4lF4U2J54xa2_S9NF5QWvpXvBeC4GAJx7QaSw4zrUkrc6XyaAiFnLhQEwKJCwUw4NOqIuYvYp_IXhw-5Ti_icDlZS-282PcccnBeOcX7vc21pozibIdmZJKqXNsL1Ibx5Nkx1F1jLnekJAmdaACDjYRLL_6n3W4wUp19UvzB1lGtXcJKLLkqB6YDiZNu16OSiSprfmrRXvYmvD8m6Fnl5aetgKw'}"
							}
						},
						AllowedGrantTypes = GrantTypes.Code,
						RequireRequestObject = true,
						RedirectUris = {"https://localhost:5001/signin-oidc"},
						FrontChannelLogoutUri = "https://localhost:5001/signout-oidc",
						PostLogoutRedirectUris = {"https://localhost:5001/signout-callback-oidc"},
						AllowOfflineAccess = true,
						AllowedScopes = AllowedScopes
					},
					///////////////////////////////////////////
					// Console Client Credentials Flow Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "client",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.ClientCredentials,
						AllowedScopes =
						{
							"resource1.scope1", "resource2.scope1", IdentityServerConstants.LocalApi.ScopeName
						}
					},
					new()
					{
						ClientId = "client.reference",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.ClientCredentials,
						AllowedScopes =
						{
							"resource1.scope1", "resource2.scope1", IdentityServerConstants.LocalApi.ScopeName
						},
						AccessTokenType = AccessTokenType.Reference
					},

					///////////////////////////////////////////
					// Console Structured Scope Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "parameterized.client",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.ClientCredentials,
						AllowedScopes = {"transaction"}
					},

					///////////////////////////////////////////
					// Console Resources and Scopes Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "console.resource.scope",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.ClientCredentials,
						AllowedScopes =
						{
							"resource1.scope1",
							"resource1.scope2",
							"resource2.scope1",
							"resource2.scope2",
							"resource3.scope1",
							"resource3.scope2",
							"shared.scope",
							"transaction",
							"scope3",
							"scope4",
							IdentityServerConstants.LocalApi.ScopeName
						}
					},

					///////////////////////////////////////////
					// X509 mTLS Client
					//////////////////////////////////////////
					new()
					{
						ClientId = "mtls",
						ClientSecrets =
						{
							// new Secret(@"CN=mtls.test, OU=ROO\ballen@roo, O=mkcert development certificate", "mtls.test")
							// {
							//     Type = SecretTypes.X509CertificateName
							// },
							new Secret("5D9E9B6B333CD42C99D1DE6175CC0F3EF99DDF68", "mtls.test")
							{
								Type = IdentityServerConstants.SecretTypes.X509CertificateThumbprint
							},
						},
						AccessTokenType = AccessTokenType.Jwt,
						AllowedGrantTypes = GrantTypes.ClientCredentials,
						AllowedScopes = {"resource1.scope1", "resource2.scope1"}
					},

					///////////////////////////////////////////
					// Console Client Credentials Flow with client JWT assertion
					//////////////////////////////////////////
					new()
					{
						ClientId = "client.jwt",
						ClientSecrets =
						{
							new Secret
							{
								Type = IdentityServerConstants.SecretTypes.X509CertificateBase64,
								Value =
									"MIIEgTCCAumgAwIBAgIQDMMu7l/umJhfEbzJMpcttzANBgkqhkiG9w0BAQsFADCBkzEeMBwGA1UEChMVbWtjZXJ0IGRldmVsb3BtZW50IENBMTQwMgYDVQQLDCtkb21pbmlja0Bkb21icDE2LmZyaXR6LmJveCAoRG9taW5pY2sgQmFpZXIpMTswOQYDVQQDDDJta2NlcnQgZG9taW5pY2tAZG9tYnAxNi5mcml0ei5ib3ggKERvbWluaWNrIEJhaWVyKTAeFw0xOTA2MDEwMDAwMDBaFw0zMDAxMDMxMjM0MDdaMHAxJzAlBgNVBAoTHm1rY2VydCBkZXZlbG9wbWVudCBjZXJ0aWZpY2F0ZTE0MDIGA1UECwwrZG9taW5pY2tAZG9tYnAxNi5mcml0ei5ib3ggKERvbWluaWNrIEJhaWVyKTEPMA0GA1UEAxMGY2xpZW50MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvNtpipaS8k1zA6w0Aoy8U4l+8zM4jHhhblExf3PULrMR6RauxniTki8p+P8CsZT4V8A4qo+JwsgpLIHrVQrbt9DEhHfBKzxwHqt+GoHt7byTfTtp8A/5nLhYc/5CW4HiR194gVx5+HAlvt+BriMTb1czvTf+H20dj41yUPsN7nMdyRLF+uXapQYMLYnq2BJIDq83mqGwojHk7d+N6GwoO95jlyas7KSoj8/FvfbaqkRNx0446hqPOzFHKc8er8K5VrLp6tVjh8ZJyY0F0dKgx6yWITsL54ctbj/cCyfuGjWEMbS2XXgc+x/xQMnmpfhK1qQAUn9jg5EzF9n6mQomOwIDAQABo3MwcTAOBgNVHQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMBMAwGA1UdEwEB/wQCMAAwHwYDVR0jBBgwFoAUEMUlw41YsKZQVls3pEG6CrJk4O8wEQYDVR0RBAowCIIGY2xpZW50MA0GCSqGSIb3DQEBCwUAA4IBgQC0TjNY4Q3Wmw7ggamDImV6HUng3WbYGLYbbL2e3myBrjIxGd1Bi8ZyOu8qeUMIRAbZt2YsSX5S8kx0biaVg2zC+aO5eHhEWMwKB66huInXFjI4wtxZ22r+33fg1R0cLuEUePhftOWrbL0MS4YXVyn9HUMWO4WptG9PJdxNw1UbEB8nw3FkVOdAC9RGqiqalSK+E2UT/kUbTIQ1gPSdQ3nh52mre0H/T9+IRqiozJtNK/CQg4NuEV7rUXHnp7Fmigp6RIJ4TCozglspL341y0rV8M7npU1FYZC2UKNr4ed+GOO1n/sF3LbXDlPXwne99CVVn85wjDaevoR7Md0y2KwE9EggLYcViXNehx4YVv/BjfgqxW8NxiKAxP6kPOZE0XdBrZj2rmcDcGOXCzzYpcduKhFyTOpA0K5RNGC3j1KOUjPVlOtLvjASP7udBEYNfH3mgqXAgqNDOEKi2jG9LITv2IyGUsXhTAsKNJ6A6qiDBzDrvPAYDvsfabPq6tRTwjA="
							},
							new Secret
							{
								Type = IdentityServerConstants.SecretTypes.JsonWebKey,
								Value =
									"{'e':'AQAB','kid':'ZzAjSnraU3bkWGnnAqLapYGpTyNfLbjbzgAPbbW2GEA','kty':'RSA','n':'wWwQFtSzeRjjerpEM5Rmqz_DsNaZ9S1Bw6UbZkDLowuuTCjBWUax0vBMMxdy6XjEEK4Oq9lKMvx9JzjmeJf1knoqSNrox3Ka0rnxXpNAz6sATvme8p9mTXyp0cX4lF4U2J54xa2_S9NF5QWvpXvBeC4GAJx7QaSw4zrUkrc6XyaAiFnLhQEwKJCwUw4NOqIuYvYp_IXhw-5Ti_icDlZS-282PcccnBeOcX7vc21pozibIdmZJKqXNsL1Ibx5Nkx1F1jLnekJAmdaACDjYRLL_6n3W4wUp19UvzB1lGtXcJKLLkqB6YDiZNu16OSiSprfmrRXvYmvD8m6Fnl5aetgKw'}"
							}
						},
						AllowedGrantTypes = GrantTypes.ClientCredentials,
						AllowedScopes = {"resource1.scope1", "resource2.scope1"}
					},

					///////////////////////////////////////////
					// Custom Grant Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "client.custom",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = {"custom", "custom.nosubject"},
						AllowedScopes = {"resource1.scope1", "resource2.scope1"}
					},

					///////////////////////////////////////////
					// Console Resource Owner Flow Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "roclient",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
						AllowOfflineAccess = true,
						AllowedScopes =
						{
							IdentityServerConstants.StandardScopes.OpenId,
							"custom.profile",
							"resource1.scope1",
							"resource2.scope1"
						},
						RefreshTokenUsage = TokenUsage.OneTimeOnly,
						AbsoluteRefreshTokenLifetime = 3600 * 24,
						SlidingRefreshTokenLifetime = 10,
						RefreshTokenExpiration = TokenExpiration.Sliding
					},

					///////////////////////////////////////////
					// Console Public Resource Owner Flow Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "roclient.public",
						RequireClientSecret = false,
						AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
						AllowOfflineAccess = true,
						AllowedScopes =
						{
							IdentityServerConstants.StandardScopes.OpenId,
							IdentityServerConstants.StandardScopes.Email,
							"resource1.scope1",
							"resource2.scope1"
						}
					},

					///////////////////////////////////////////
					// Console with PKCE Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "console.pkce",
						ClientName = "Console with PKCE Sample",
						RequireClientSecret = false,
						AllowedGrantTypes = GrantTypes.Code,
						RequirePkce = true,
						RedirectUris = {"http://127.0.0.1"},
						AllowOfflineAccess = true,
						AllowedScopes =
						{
							IdentityServerConstants.StandardScopes.OpenId,
							IdentityServerConstants.StandardScopes.Profile,
							IdentityServerConstants.StandardScopes.Email,
							"resource1.scope1",
							"resource2.scope1"
						}
					},

					///////////////////////////////////////////
					// Console Resource Indicators Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "console.resource.indicators",
						ClientName = "Console Resource Indicators Sample",
						RequireClientSecret = false,
						AllowedGrantTypes = GrantTypes.Code,
						RequirePkce = true,
						RedirectUris = {"http://127.0.0.1"},
						AllowOfflineAccess = true,
						RefreshTokenUsage = TokenUsage.ReUse,
						AllowedScopes =
						{
							IdentityServerConstants.StandardScopes.OpenId,
							"resource1.scope1",
							"resource1.scope2",
							"resource2.scope1",
							"resource2.scope2",
							"resource3.scope1",
							"resource3.scope2",
							"shared.scope",
							"transaction",
							"scope3",
							"scope4",
						}
					},

					///////////////////////////////////////////
					// WinConsole with PKCE Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "winconsole",
						ClientName = "Windows Console with PKCE Sample",
						RequireClientSecret = false,
						AllowedGrantTypes = GrantTypes.Code,
						RequirePkce = true,
						RedirectUris = {"sample-windows-client://callback"},
						RequireConsent = false,
						AllowOfflineAccess = true,
						AllowedIdentityTokenSigningAlgorithms = {"ES256"},
						AllowedScopes =
						{
							IdentityServerConstants.StandardScopes.OpenId,
							IdentityServerConstants.StandardScopes.Profile,
							IdentityServerConstants.StandardScopes.Email,
							"resource1.scope1",
							"resource2.scope1"
						}
					},


					///////////////////////////////////////////
					// Introspection Client Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "roclient.reference",
						ClientSecrets = {new Secret("secret".Sha256())},
						AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
						AllowedScopes = {"resource1.scope1", "resource2.scope1", "scope3"},
						AccessTokenType = AccessTokenType.Reference
					},

					///////////////////////////////////////////
					// Device Flow Sample
					//////////////////////////////////////////
					new()
					{
						ClientId = "device",
						ClientName = "Device Flow Client",
						AllowedGrantTypes = GrantTypes.DeviceFlow,
						RequireClientSecret = false,
						AllowOfflineAccess = true,
						AllowedScopes =
						{
							IdentityServerConstants.StandardScopes.OpenId,
							IdentityServerConstants.StandardScopes.Profile,
							IdentityServerConstants.StandardScopes.Email,
							"resource1.scope1",
							"resource2.scope1"
						}
					}
				}, cancellationToken),
				// Insert the standard identity resources
				// Because of the way Mongo handles polymorphism it's best not to insert classes inherited
				// from ApiResource, ApiScope, & IdentityResource
				_apiResourceUpdater.InsertManyAsync(new ApiResource[]
				{
					new("urn:resource1", "Resource 1")
					{
						Description = "Something very long and descriptive",
						ApiSecrets = {new Secret("secret".Sha256())},
						Scopes = {"resource1.scope1", "resource1.scope2", "shared.scope"}
					},
					new("urn:resource2", "Resource 2")
					{
						Description = "Something very long and descriptive",
						ApiSecrets = {new Secret("secret".Sha256())},

						// additional claims to put into access token
						UserClaims = {JwtClaimTypes.Name, JwtClaimTypes.Email},
						Scopes = {"resource2.scope1", "resource2.scope2", "shared.scope"}
					},
					new("urn:resource3", "Resource 3 (isolated)")
					{
						ApiSecrets = {new Secret("secret".Sha256())},
						//RequireResourceIndicator = true,
						Scopes = {"resource3.scope1", "resource3.scope2", "shared.scope"}
					}
				}, cancellationToken),
				_apiScopeUpdater.InsertManyAsync(new ApiScope[]
				{
					// local API scope
					new(IdentityServerConstants.LocalApi.ScopeName),

					// resource specific scopes
					new("resource1.scope1"), new("resource1.scope2"), new("resource2.scope1"), new("resource2.scope2"),
					new("resource3.scope1"), new("resource3.scope2"),

					// a scope without resource association
					new("scope3"), new("scope4"),

					// a scope shared by multiple resources
					new("shared.scope"),

					// a parameterized scope
					new("transaction", "Transaction") {Description = "Some Transaction"}
				}, cancellationToken),
				_identityResourceUpdater.InsertManyAsync(new IdentityResource[]
				{
					// some standard scopes from the OIDC spec
					new()
					{
						Name = IdentityServerConstants.StandardScopes.OpenId,
						DisplayName = "Your user identifier",
						Required = true,
						UserClaims = new[] {JwtClaimTypes.Subject}
					},
					new()
					{
						Name = profile.Name,
						DisplayName = profile.DisplayName,
						Description = profile.Description,
						Emphasize = profile.Emphasize,
						UserClaims = profile.UserClaims
					},
					new()
					{
						Name = email.Name,
						DisplayName = email.DisplayName,
						Emphasize = email.Emphasize,
						UserClaims = email.UserClaims
					},
					// custom identity resource with some consolidated claims
					new("custom.profile", new[]
					{
						JwtClaimTypes.Name, JwtClaimTypes.Email, "location", JwtClaimTypes.Address
					})
				}, cancellationToken));
		}

		public Task StopAsync(CancellationToken cancellationToken) =>
			Task.CompletedTask;
	}
}
