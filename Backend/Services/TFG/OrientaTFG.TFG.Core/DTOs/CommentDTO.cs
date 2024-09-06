namespace OrientaTFG.TFG.Core.DTOs;

public class CommentDTO
{
    /// <summary>
    /// Gets or sets the comment's text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the the comment's files
    /// </summary>
    public List<FileCommentHistoryDTO>? Files { get; set; }

    /// <summary>
    /// Gets or sets the comment's creator name
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the datetime the comment was created
    /// </summary>
    public DateTime CreatedOn { get; set; }
}
