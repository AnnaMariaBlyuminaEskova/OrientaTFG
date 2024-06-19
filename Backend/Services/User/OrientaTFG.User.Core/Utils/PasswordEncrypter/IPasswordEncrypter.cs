namespace OrientaTFG.User.Core.Utils.PasswordEncrypter;

public interface IPasswordEncrypter
{
    /// <summary>
    /// Encrypts the password
    /// </summary>
    /// <param name="password">The user's password</param>
    /// <returns>The encrypted password</returns>
    string Encrypt(string password);
}
