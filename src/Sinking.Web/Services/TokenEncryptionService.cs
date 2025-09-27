using System.Security.Cryptography;
using System.Text;

namespace Sinking.Web.Services;

/// <summary>
/// Service for encrypting and decrypting Personal Access Tokens using AES encryption
/// </summary>
public class TokenEncryptionService : ITokenEncryptionService
{
    private readonly byte[] _key;
    private readonly byte[] _iv;
    private readonly ILogger<TokenEncryptionService> _logger;

    public TokenEncryptionService(IConfiguration configuration, ILogger<TokenEncryptionService> logger)
    {
        _logger = logger;
        
        // In production, these should be stored securely (Azure Key Vault, etc.)
        var keyString = configuration["Encryption:Key"];
        var ivString = configuration["Encryption:IV"];

        if (string.IsNullOrEmpty(keyString) || string.IsNullOrEmpty(ivString))
        {
            _logger.LogWarning("Encryption keys not found in configuration. Using default keys. This is not secure for production use.");
            keyString = "DefaultKey32BytesLongForSinkingApp";
            ivString = "DefaultIV16Bytes";
        }

        // Ensure key is 32 bytes for AES-256
        _key = PadOrTruncate(Encoding.UTF8.GetBytes(keyString), 32);
        // Ensure IV is 16 bytes for AES
        _iv = PadOrTruncate(Encoding.UTF8.GetBytes(ivString), 16);
    }

    public string EncryptToken(string plainTextToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainTextToken);

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var encryptor = aes.CreateEncryptor();
            using var msEncrypt = new MemoryStream();
            using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainTextToken);
            }

            return Convert.ToBase64String(msEncrypt.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to encrypt token");
            throw new InvalidOperationException("Token encryption failed", ex);
        }
    }

    public string DecryptToken(string encryptedToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(encryptedToken);

        try
        {
            var cipherBytes = Convert.FromBase64String(encryptedToken);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;

            using var decryptor = aes.CreateDecryptor();
            using var msDecrypt = new MemoryStream(cipherBytes);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid encrypted token format");
            return string.Empty;
        }
        catch (CryptographicException ex)
        {
            _logger.LogError(ex, "Failed to decrypt token - invalid key or corrupted data");
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token decryption");
            return string.Empty;
        }
    }

    private static byte[] PadOrTruncate(byte[] input, int targetLength)
    {
        if (input.Length == targetLength)
            return input;

        var result = new byte[targetLength];
        if (input.Length > targetLength)
        {
            Array.Copy(input, result, targetLength);
        }
        else
        {
            Array.Copy(input, result, input.Length);
            // Remaining bytes stay as zeros
        }
        return result;
    }
}