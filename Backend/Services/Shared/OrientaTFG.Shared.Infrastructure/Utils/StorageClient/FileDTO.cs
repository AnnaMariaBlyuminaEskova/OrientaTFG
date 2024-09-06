namespace OrientaTFG.Shared.Infrastructure.Utils.StorageClient;

public class FileDTO
{
    /// <summary>
    /// Gets or sets the the file's name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the the file's content
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the the file's url
    /// </summary>
    public string? URL { get; set; } = string.Empty;
}
