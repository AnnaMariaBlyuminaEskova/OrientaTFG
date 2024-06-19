using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace OrientaTFG.Shared.Infrastructure.Model;

[Table("MainTaskStatus", Schema = "Master")]
public class MainTaskStatus
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
    /// Gets or sets the main task status's main tasks
    /// </summary>
    [InverseProperty(nameof(MainTaskStatus))]
    public virtual ICollection<MainTask>? MainTasks { get; set; }
}
