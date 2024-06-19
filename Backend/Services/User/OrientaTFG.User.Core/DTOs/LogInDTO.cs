namespace OrientaTFG.User.Core.DTOs;

public class LogInDTO
{
    /// <summary>
    /// Gets or sets the user's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's password
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
