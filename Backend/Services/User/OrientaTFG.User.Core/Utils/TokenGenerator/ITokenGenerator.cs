namespace OrientaTFG.User.Core.Utils.TokenGenerator;

public interface ITokenGenerator
{
    /// <summary>
    /// Generates the token
    /// </summary>
    /// <param name="userId">The user's id</param>
    /// <param name="userRole">The user's role</param>
    /// <param name="secretKey">The secret key</param>
    /// <param name="expiryMinutes">The token expiration minutes</param>
    /// <returns>The generated token</returns>
    string Generate(int userId, string userRole, string secretKey, int expiryMinutes = 60);
}
