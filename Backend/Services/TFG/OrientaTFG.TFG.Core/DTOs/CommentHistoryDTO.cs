namespace OrientaTFG.TFG.Core.DTOs;

public class CommentHistoryDTO
{
    /// <summary>
    /// Gets or sets the comments
    /// </summary>
    public List<CommentDTO>? Comments { get; set; }

    /// <summary>
    /// Gets or sets the task's name
    /// </summary>
    public string TaskName { get; set; } = string.Empty;
}
