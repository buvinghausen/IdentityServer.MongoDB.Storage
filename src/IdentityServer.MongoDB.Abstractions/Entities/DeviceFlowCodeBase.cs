using System;

namespace IdentityServer.MongoDB.Abstractions.Entities;

// This class is here as a wrapper to match what the EntityFramework storage provider has
// Rather than serialize the device code to JSON and put it in a string we run the native object in
internal abstract class DeviceFlowCodeBase<T>
{
	public string DeviceCode { get; set; }

	public string UserCode { get; set; }

	public string SubjectId { get; set; }

	public string SessionId { get; set; }

	public string ClientId { get; set; }

	public string Description { get; set; }

	public DateTime CreationTime { get; set; }

	public DateTime? Expiration { get; set; }

	public T Data { get; set; }
}
