namespace OrientaTFG.User.Core.Utils.StorageClient;

public interface IStorageClient
{
    /// <summary>
    /// Gets the file content 
    /// </summary>
    /// <param name="fileName">The profile picture file name</param>
    /// <returns>byte[]</returns>
    Task<byte[]> GetFileContent(string fileName);
}
