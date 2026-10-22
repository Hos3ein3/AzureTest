using System;

namespace AzureTest.Application.Crypto.Services;

public interface ICryptoEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherTextBase64);
    string MaskKey(string apiKey);
}
