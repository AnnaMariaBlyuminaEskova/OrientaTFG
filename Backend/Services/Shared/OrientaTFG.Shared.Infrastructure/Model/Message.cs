using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("Messages", Schema = "TFG")]
public class Message : BaseModel
{
    /// <summary>
    /// Gets or sets the TFG's id
    /// </summary>
    [Required]
    public int TFGId { get; set; }

    /// <summary>
    /// Gets or sets the message's text
    /// </summary>
    [Required]
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the comment's creator name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the datetime the message was created
    /// </summary>
    [Required]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// Gets or sets the TFG
    /// </summary>
    [ForeignKey(nameof(TFGId))]
    [InverseProperty(nameof(Model.TFG.Messages))]
    public virtual TFG TFG { get; set; } = null!;
}
