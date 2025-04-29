namespace BitcoinClientApp.Interfaces
{
    public interface ICryptoService
    {
        string GenerateSalt();
        string EncryptPrivateKey(string privateKey, string salt);
        string DecryptPrivateKey(string encryptedPrivateKey, string salt);
    }
}