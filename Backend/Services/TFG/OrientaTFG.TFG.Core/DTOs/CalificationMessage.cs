namespace OrientaTFG.TFG.Core.DTOs;

public class CalificationMessage : Message
{
    /// <summary>
    /// Gets or sets the calification's datetime
    /// </summary>
    public DateTime DateTime { get; set; }

    /// <summary>
    /// Gets or sets the main task's maximum points
    /// </summary>
    public int MaximumPoints { get; set; }

    /// <summary>
    /// Gets or sets the main task's obtained points
    /// </summary>
    public int ObtainedPoints { get; set; }
}
