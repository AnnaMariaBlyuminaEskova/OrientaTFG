using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("MainTasks", Schema = "TFG")]
public class MainTask : BaseModel
{
    /// <summary>
    /// Gets or sets the main task's name
    /// </summary>
    [StringLength(100)]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the main task's description
    /// </summary>
    [StringLength(1000)]
    [Required]
    public string Description { get; set; }

    /// <summary>
    /// Gets or sets the main task's maximum points
    /// </summary>
    [Required]
    public int MaximumPoints { get; set; }

    /// <summary>
    /// Gets or sets the main task's obtained points
    /// </summary>
    public int? ObtainedPoints { get; set; }

    /// <summary>
    /// Gets or sets the main task's deadline
    /// </summary>
    [Required]
    public DateTime Deadline { get; set; }

    /// <summary>
    /// Gets or sets the TFG's id
    /// </summary>
    [Required]
    public int TFGId { get; set; }

    /// <summary>
    /// Gets or sets the status's id
    /// </summary>
    [Required]
    public int StatusId { get; set; }

    /// <summary>
    /// Gets or sets the main task's order between all the tfg's tasks
    /// </summary>
    [Required]
    public int Order { get; set; }

    /// <summary>
    /// Gets or sets the main task's creator name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the TFG
    /// </summary>
    [ForeignKey(nameof(TFGId))]
    [InverseProperty(nameof(Model.TFG.MainTasks))]
    public virtual TFG TFG { get; set; } = null!;

    /// <summary>
    /// Gets or sets the main task's sub tasks
    /// </summary>
    [InverseProperty(nameof(MainTask))]
    public virtual ICollection<SubTask>? SubTasks { get; set; }

    /// <summary>
    /// Gets or sets the status
    /// </summary>
    [ForeignKey(nameof(StatusId))]
    [InverseProperty(nameof(Model.MainTaskStatus.MainTasks))]
    public virtual MainTaskStatus MainTaskStatus { get; set; } = null!;
}
