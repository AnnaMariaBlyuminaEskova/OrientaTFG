using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Comments", Schema = "TFG")]
public class Comment : BaseModel
{
    /// <summary>
    /// Gets or sets the comment's text
    /// </summary>
    [Required]
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the the comment's files
    /// </summary>
    public string? Files { get; set; }

    /// <summary>
    /// Gets or sets the comment's creator name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the datetime the comment was created
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the comment's main task's id
    /// </summary>
    public int? MainTaskId { get; set; }

    /// <summary>
    /// Gets or sets the main task
    /// </summary>
    [ForeignKey(nameof(MainTaskId))]
    [InverseProperty(nameof(Model.MainTask.Comments))]
    public virtual MainTask? MainTask { get; set; }

    /// <summary>
    /// Gets or sets the comment's sub task's id
    /// </summary>
    public int? SubTaskId { get; set; }

    /// <summary>
    /// Gets or sets the sub task
    /// </summary>
    [ForeignKey(nameof(SubTaskId))]
    [InverseProperty(nameof(Model.SubTask.Comments))]
    public virtual SubTask? SubTask { get; set; }
}
