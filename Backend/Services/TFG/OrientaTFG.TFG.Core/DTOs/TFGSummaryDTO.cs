namespace OrientaTFG.TFG.Core.DTOs;

public class TFGSummaryDTO
{
    /// <summary>
    /// Gets or sets the tfg's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tfg's main task summaries
    /// </summary>
    public List<MainTaskSummaryDTO> MainTaskSummaryDTOList { get; set; } = [];
}
