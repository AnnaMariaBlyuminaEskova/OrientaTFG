namespace OrientaTFG.TFG.Core.DTOs;

public class CreateMainTaskDTO
{
    /// <summary>
    /// Gets or sets the tfg's id
    /// </summary>
    public int TFGId { get; set; }

    /// <summary>
    /// Gets or sets the main task's id
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// Gets or sets the main task's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main task's deadline
    /// </summary>
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Gets or sets the main task's maximum points
    /// </summary>
    public int MaximumPoints { get; set; }

    /// <summary>
    /// Gets or sets the main task's description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
