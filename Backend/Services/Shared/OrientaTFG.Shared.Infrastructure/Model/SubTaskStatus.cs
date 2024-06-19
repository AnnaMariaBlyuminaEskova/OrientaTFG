using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("SubTaskStatus", Schema = "Master")]
public class SubTaskStatus 
{
    /// <summary>
    /// Gets or sets the identifier
    /// </summary>
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the status's name
    /// </summary>
    [StringLength(50)]
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the sub task status's sub tasks
    /// </summary>
    [InverseProperty(nameof(SubTaskStatus))]
    public virtual ICollection<SubTask>? SubTasks { get; set; }
}
