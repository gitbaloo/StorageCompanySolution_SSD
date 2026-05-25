using StorageCompany.Core.Interfaces.Services;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;

namespace StorageCompany.Core.Services;

public class EncryptionService(IOptionsMonitor<AppOptions> optionsMonitor) : IEncryptionService
{
    public string Encrypt(string plainText)
    {
        // Load Aes-Gcm with the encryption key and the tag size
        using var aesGcm = new AesGcm(GetEncryptionKey(), 16);

        // Create the 3 parts that make up the cipher text and convert the plain text string to a byte array
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherTextBytes = new byte[plainTextBytes.Length];

        // Encrypt the different parts and combine them into one byte array whereafter it is converted into a base64 string
        aesGcm.Encrypt(nonce, plainTextBytes, cipherTextBytes, tag);
        var combinedBytes = nonce.Concat(cipherTextBytes).Concat(tag).ToArray();
        var encryptedPayload = Convert.ToBase64String(combinedBytes);
       
        return encryptedPayload;
    }

    public string Decrypt(string encryptedPayload)
    {
        // Load Aes-Gcm with the encryption key and the tag size
        using var aesGcm = new AesGcm(GetEncryptionKey(), 16);

        // Convert the base64 string back into byte array
        var encryptedPayloadBytes = Convert.FromBase64String(encryptedPayload);
        
        // Create the 3 parts that the encryptedPayload gets split into and create the plain text byte array that matches the length of the cipher
        // Just like in encryption nonce is defined with 12 bytes and tag with 16 bites. Decryption must also match this pattern
        var nonce = encryptedPayloadBytes[..12];
        var tag = encryptedPayloadBytes[^16..];
        var cipherTextBytes = encryptedPayloadBytes[12..^16];
        var plainTextBytes = new byte[cipherTextBytes.Length];

        // Decrypt the parts so we can get the decrypted plain text byte array
        aesGcm.Decrypt(nonce, cipherTextBytes, tag, plainTextBytes);

        // Decode the plain text byte array to a readible string and return it
        string plainText = Encoding.UTF8.GetString(plainTextBytes);

        return plainText;
    }

    private byte[] GetEncryptionKey()
    {
        // Load the encryption key from AppOptions.cs
        var encryptionKey = optionsMonitor.CurrentValue.EncryptionKey;
        
        // Loads the SHA256 that is used for hashing
        using var sha256 = SHA256.Create();

        // Encryption key is encoded from a string into a byte array which are then hashed and returned
        var bytes = Encoding.UTF8.GetBytes(encryptionKey);
        var hashedBytes = sha256.ComputeHash(bytes);

        return hashedBytes;
    }
}