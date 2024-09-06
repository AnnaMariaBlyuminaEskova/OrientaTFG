namespace OrientaTFG.TFG.Core.DTOs;

public class UpdateSubTaskDTO
{
    /// <summary>
    /// Gets or sets the sub task's id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the sub task's total hours
    /// </summary>
    public int? TotalHours { get; set; }

    /// <summary>
    /// Gets or sets the sub task's status id
    /// </summary>
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the sub task's order
    /// </summary>
    public int Order { get; set; }
}
