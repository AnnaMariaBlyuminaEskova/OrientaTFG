namespace OrientaTFG.Shared.Infrastructure.Utils.StorageClient;

public interface IStorageClient
{
    /// <summary>
    /// Gets the file content 
    /// </summary>
    /// <param name="fileName">The file name</param>
    /// <returns>string</returns>
    Task<string> GetFileContent(string fileName);

    /// <summary>
    /// Uploads a file
    /// </summary>
    /// <param name="base64File">The base64 string file</param>
    /// <param name="fileName">The file name</param>
    Task UploadFile(string base64File, string fileName);

    /// <summary>
    /// Uploads a comment's files
    /// </summary>
    /// <param name="files">The files to upload</param>
    /// <param name="directoryName">The directory name where the comment's files are going to be uploaded</param>
    Task UploadCommentFiles(List<FileDTO> files, string directoryName);

    /// <summary>
    /// Deletes all files within a specified directory
    /// </summary>
    /// <param name="directoryName">The directory name where the comment's files are located</param>
    Task DeleteCommentFiles(string directoryName);
}
