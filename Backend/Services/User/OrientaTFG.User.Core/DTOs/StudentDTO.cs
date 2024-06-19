namespace OrientaTFG.User.Core.DTOs;

public class StudentDTO : UserDTO
{
    /// <summary>
    /// Gets or sets if the student has an assigned TFG
    /// </summary>
    public bool TFG { get; set; }
}
