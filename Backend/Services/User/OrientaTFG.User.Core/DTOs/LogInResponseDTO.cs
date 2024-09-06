using OrientaTFG.Shared.Infrastructure.Enums;

namespace OrientaTFG.User.Core.DTOs;

public class LogInResponseDTO : ErrorMessageDTO
{
    /// <summary>
    /// Gets or sets the token
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the user's profile picture
    /// </summary>
    public string ProfilePicture { get; set; }

    /// <summary>
    /// Gets or sets the user's role
    /// </summary>
    public RoleEnum Role { get; set; }
}
