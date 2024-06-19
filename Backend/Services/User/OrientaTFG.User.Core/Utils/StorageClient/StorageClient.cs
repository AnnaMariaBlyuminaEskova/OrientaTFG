using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace OrientaTFG.User.Core.Utils.StorageClient;

public class StorageClient : IStorageClient
{
    /// <summary>
    /// The configuration
    /// </summary>
    private readonly IConfiguration configuration;

    /// <summary>
    /// The profile's pictures container name
    /// </summary>
    private const string ProfilePictureContainerName = "profile-pictures";

    /// <summary>
    /// Initializes a new instance of the <see cref="StorageClient"/> class
    /// </summary>
    /// <param name="configuration">The configuration</param>
    public StorageClient(IConfiguration configuration)
    {
        this.configuration = configuration;
    }

    /// <summary>
    /// Gets the file content 
    /// </summary>
    /// <param name="fileName">The profile picture file name</param>
    /// <returns>byte[]</returns>
    public async Task<byte[]> GetFileContent(string fileName)
    {
        // Create a StorageClient object to connect to the Blob service
        var blobServiceClient = new BlobServiceClient(configuration["StorageConnectionString"]);

        // Get the container client object to interact with the container
        var containerClient = blobServiceClient.GetBlobContainerClient(ProfilePictureContainerName);

        // Get the blob client object to interact with the specific blob (file)
        var blobClient = containerClient.GetBlobClient(fileName);

        // Download the blob's content and store it in a memory stream
        using (var memoryStream = new MemoryStream())
        {
            await blobClient.DownloadToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
