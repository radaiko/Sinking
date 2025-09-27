namespace Sinking.Web.Services;

/// <summary>
/// Service for encrypting and decrypting Personal Access Tokens
/// </summary>
public interface ITokenEncryptionService
{
    /// <summary>
    /// Encrypts a plain text token for secure storage
    /// </summary>
    /// <param name="plainTextToken">The token to encrypt</param>
    /// <returns>The encrypted token</returns>
    string EncryptToken(string plainTextToken);
    
    /// <summary>
    /// Decrypts an encrypted token back to plain text
    /// </summary>
    /// <param name="encryptedToken">The encrypted token</param>
    /// <returns>The decrypted token</returns>
    string DecryptToken(string encryptedToken);
}