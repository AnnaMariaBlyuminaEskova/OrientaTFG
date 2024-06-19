using System.Security.Cryptography;
using System.Text;

namespace OrientaTFG.User.Core.Utils.PasswordEncrypter;

public class PasswordEncrypter : IPasswordEncrypter
{
    /// <summary>
    /// Encrypts the password
    /// </summary>
    /// <param name="password">The user's password</param>
    /// <returns>The encrypted password</returns>
    public string Encrypt(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }
}
