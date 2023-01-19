// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging;

internal static partial class LoggingExtensions
{
	[LoggerMessage(0, LogLevel.Debug,
		"No record found in user session store when trying to get user session for key {Key}")]
	internal static partial void NoUserSessionFound(this ILogger logger, string key);
}
