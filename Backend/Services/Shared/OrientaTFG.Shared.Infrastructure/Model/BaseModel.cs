using System.ComponentModel.DataAnnotations;

namespace OrientaTFG.Shared.Infrastructure.Model;

public class BaseModel
{
    /// <summary>
    /// Gets or sets the identifier
    /// </summary>
    [Key]
    public int Id { get; set; }
}
