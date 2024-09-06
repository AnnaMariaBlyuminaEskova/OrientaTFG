using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace OrientaTFG.Shared.Infrastructure.Utils.StorageClient;

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
    /// The comment's attachments container name
    /// </summary>
    private const string CommentAttachmentsContainerName = "comment-attachments";

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
    /// <param name="fileName">The file name</param>
    /// <returns>string</returns>
    public async Task<string> GetFileContent(string fileName)
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
            byte[] bytes = memoryStream.ToArray();
            string base64String = Convert.ToBase64String(bytes);

            // Determine the file extension based on the fileName
            string extension = Path.GetExtension(fileName).TrimStart('.');
            string mimeType = $"image/{extension}";

            // Construct the data URL with the appropriate MIME type
            string image = $"data:{mimeType};base64,{base64String}";

            return image;
        }
    }

    /// <summary>
    /// Uploads a file
    /// </summary>
    /// <param name="base64File">The base64 string file</param>
    /// <param name="fileName">The file name</param>
    public async Task UploadFile(string base64File, string fileName)
    {
        // Create a StorageClient object to connect to the Blob service
        var blobServiceClient = new BlobServiceClient(configuration["StorageConnectionString"]);

        // Get the container client object to interact with the container
        var containerClient = blobServiceClient.GetBlobContainerClient(ProfilePictureContainerName);

        // Ensure the container exists
        await containerClient.CreateIfNotExistsAsync();

        var base64DataStartIndex = base64File.IndexOf(";base64,") + ";base64,".Length;

        // Get the blob client object to interact with the specific blob (file)
        var blobClient = containerClient.GetBlobClient(fileName);

        // Decode the base64 string
        var base64Data = base64File.Substring(base64DataStartIndex);
        var fileData = Convert.FromBase64String(base64Data);

        // Upload the blob's content
        using (var memoryStream = new MemoryStream(fileData))
        {
            await blobClient.UploadAsync(memoryStream, overwrite: true);
        }

    }

    /// <summary>
    /// Uploads a comment's files
    /// </summary>
    /// <param name="files">The files to upload</param>
    /// <param name="directoryName">The directory name where the comment's files are going to be uploaded</param>
    public async Task UploadCommentFiles(List<FileDTO> files, string directoryName)
    {
        var blobServiceClient = new BlobServiceClient(configuration["StorageConnectionString"]);

        var containerClient = blobServiceClient.GetBlobContainerClient(CommentAttachmentsContainerName);

        await containerClient.CreateIfNotExistsAsync();

        foreach (var file in files)
        {
            string blobName = $"{directoryName}/{file.Name}".Replace("\\", "/");
            var blobClient = containerClient.GetBlobClient(blobName);

            var base64DataStartIndex = file.Content.IndexOf(";base64,") + ";base64,".Length;
            var base64Data = file.Content.Substring(base64DataStartIndex);
            var fileData = Convert.FromBase64String(base64Data);

            using (var memoryStream = new MemoryStream(fileData))
            {
                await blobClient.UploadAsync(memoryStream, overwrite: true);
            }

            var blobUri = blobClient.Uri.ToString();
            file.URL = blobUri;
        }
    }

    /// <summary>
    /// Deletes all files within a specified directory
    /// </summary>
    /// <param name="directoryName">The directory name where the comment's files are located</param>
    public async Task DeleteCommentFiles(string directoryName)
    {
        var blobServiceClient = new BlobServiceClient(configuration["StorageConnectionString"]);

        var containerClient = blobServiceClient.GetBlobContainerClient(CommentAttachmentsContainerName);

        var blobItems = containerClient.GetBlobsAsync(prefix: directoryName);

        await foreach (var blobItem in blobItems)
        {
            var blobClient = containerClient.GetBlobClient(blobItem.Name);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
