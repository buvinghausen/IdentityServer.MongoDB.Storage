namespace Duende.Bff.MongoDB;

/// <summary>
/// Entity class for a user session
/// </summary>
public class UserSessionEntity : UserSession
{
	/// <summary>
	/// Discriminator to allow multiple applications to share the user session table.
	/// </summary>
	public string? ApplicationName { get; set; }
}
