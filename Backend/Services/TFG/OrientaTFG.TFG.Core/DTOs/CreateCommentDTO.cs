using OrientaTFG.Shared.Infrastructure.Utils.StorageClient;

namespace OrientaTFG.TFG.Core.DTOs;

public class CreateCommentDTO
{
    /// <summary>
    /// Gets or sets the comment's text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the the comment's files
    /// </summary>
    public List<FileDTO>? Files { get; set; }

    /// <summary>
    /// Gets or sets the comment's main task's id
    /// </summary>
    public int? MainTaskId { get; set; }

    /// <summary>
    /// Gets or sets the comment's sub task's id
    /// </summary>
    public int? SubTaskId { get; set; }
}
