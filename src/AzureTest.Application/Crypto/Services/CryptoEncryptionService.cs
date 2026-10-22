using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace AzureTest.Application.Crypto.Services;

public class CryptoEncryptionService : ICryptoEncryptionService
{
    private readonly byte[] _key;

    public CryptoEncryptionService(IConfiguration? config = null)
    {
        var secret = config?["Crypto:MasterEncryptionKey"] 
            ?? "AntigravityCryptoVault2026MasterKey_CipherGrade!";
        using var sha = SHA256.Create();
        _key = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
    }

    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText)) return string.Empty;

        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        byte[] tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes
        byte[] cipherBytes = new byte[plainBytes.Length];

        using var aesGcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Encrypt(nonce, plainBytes, cipherBytes, tag);

        // Pack format: Nonce (12) + Tag (16) + CipherText (N)
        byte[] result = new byte[nonce.Length + tag.Length + cipherBytes.Length];
        Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
        Buffer.BlockCopy(tag, 0, result, nonce.Length, tag.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, nonce.Length + tag.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherTextBase64)
    {
        if (string.IsNullOrEmpty(cipherTextBase64)) return string.Empty;

        byte[] payload = Convert.FromBase64String(cipherTextBase64);
        int nonceSize = AesGcm.NonceByteSizes.MaxSize; // 12
        int tagSize = AesGcm.TagByteSizes.MaxSize; // 16

        if (payload.Length < nonceSize + tagSize)
            throw new CryptographicException("Invalid ciphertext payload size for AES-GCM-256.");

        byte[] nonce = new byte[nonceSize];
        byte[] tag = new byte[tagSize];
        int cipherSize = payload.Length - nonceSize - tagSize;
        byte[] cipherBytes = new byte[cipherSize];
        byte[] plainBytes = new byte[cipherSize];

        Buffer.BlockCopy(payload, 0, nonce, 0, nonceSize);
        Buffer.BlockCopy(payload, nonceSize, tag, 0, tagSize);
        Buffer.BlockCopy(payload, nonceSize + tagSize, cipherBytes, 0, cipherSize);

        using var aesGcm = new AesGcm(_key, AesGcm.TagByteSizes.MaxSize);
        aesGcm.Decrypt(nonce, cipherBytes, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    public string MaskKey(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return string.Empty;
        if (apiKey.Length <= 8) return "••••••••";
        return apiKey[..4] + "••••••••" + apiKey[^4..];
    }
}
