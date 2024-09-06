using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("SubTasks", Schema = "TFG")]
public class SubTask : BaseModel
{
    /// <summary>
    /// Gets or sets the sub task's name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the sub task's estimated hours
    /// </summary>
    [Required]
    public int EstimatedHours { get; set; }

    /// <summary>
    /// Gets or sets the sub task's total hours
    /// </summary>
    public int? TotalHours { get; set; }

    /// <summary>
    /// Gets or sets the main task's id
    /// </summary>
    [Required]
    public int MainTaskId { get; set; }

    /// <summary>
    /// Gets or sets the status's id
    /// </summary>
    [Required]
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the sub task's order between all the main task's sub tasks
    /// </summary>
    [Required]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the sub task's creator name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the main task
    /// </summary>
    [ForeignKey(nameof(MainTaskId))]
    [InverseProperty(nameof(Model.MainTask.SubTasks))]
    public virtual MainTask MainTask { get; set; } = null!;

    /// <summary>
    /// Gets or sets the status
    /// </summary>
    [ForeignKey(nameof(StatusId))]
    [InverseProperty(nameof(Model.SubTaskStatus.SubTasks))]
    public virtual SubTaskStatus SubTaskStatus { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sub task's comments
    /// </summary>
    [InverseProperty(nameof(SubTask))]
    public virtual ICollection<Comment>? Comments { get; set; }
}
