namespace OrientaTFG.TFG.Core.DTOs;

public class NewSubTaskDTO
{
    /// <summary>
    /// Gets or sets the main task's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the sub task's estimated hours
    /// </summary>
    public int EstimatedHours { get; set; }

    /// <summary>
    /// Gets or sets the sub task's status id
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the sub task's order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the sub task's total hours
    /// </summary>
    public int? TotalHours { get; set; }
}
