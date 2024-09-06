namespace OrientaTFG.TFG.Core.DTOs;

public class MessageDTO
{
    /// <summary>
    /// Gets or sets the message's text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the comment's creator name
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the datetime the message was created
    /// </summary>
    public DateTime CreatedOn { get; set; }
}
