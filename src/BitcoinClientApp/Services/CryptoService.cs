using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BitcoinClientApp.Services
{
    public interface ICryptoService
    {
        string GenerateSalt();
        string EncryptPrivateKey(string privateKey, string salt);
        string DecryptPrivateKey(string encryptedPrivateKey, string salt);
    }

    public class CryptoService : ICryptoService
    {
        private static readonly string DefaultPassword = "BitcoinWallet2023!"; // In a real app, this would be provided by the user

        // Generate a random salt
        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        // Encrypt a private key
        public string EncryptPrivateKey(string privateKey, string salt)
        {
            // For a real application, the password should be provided by the user
            // and not hardcoded or stored in the application
            string password = DefaultPassword;

            // Generate key from password and salt
            using var deriveBytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
            byte[] key = deriveBytes.GetBytes(32);
            byte[] iv = deriveBytes.GetBytes(16);

            // Encrypt the private key
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                byte[] plainBytes = Encoding.UTF8.GetBytes(privateKey);
                cs.Write(plainBytes, 0, plainBytes.Length);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        // Decrypt a private key
        public string DecryptPrivateKey(string encryptedPrivateKey, string salt)
        {
            string password = DefaultPassword;

            // Generate key from password and salt
            using var deriveBytes = new Rfc2898DeriveBytes(password, Convert.FromBase64String(salt), 10000, HashAlgorithmName.SHA256);
            byte[] key = deriveBytes.GetBytes(32);
            byte[] iv = deriveBytes.GetBytes(16);

            // Decrypt the private key
            using var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(encryptedPrivateKey));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            
            return reader.ReadToEnd();
        }
    }
} 