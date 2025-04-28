using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BitcoinAddressToolkit;
using BitcoinAddressToolkit.Shared.Enums;
using NBitcoin;
using BitcoinClientApp.Data;
using BitcoinClientApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BitcoinClientApp.Services
{
    public class AddressService
    {
        private readonly ApplicationDbContext _context;
        private readonly CryptoService _cryptoService;

        public AddressService(ApplicationDbContext context, CryptoService cryptoService)
        {
            _context = context;
            _cryptoService = cryptoService;
        }

        public async Task<(Key privateKey, BitcoinAddress address)> CreateNewWallet(string username)
        {
            // Generate a new private key
            var privateKey = new Key();
            var publicKey = privateKey.PubKey;
            var address = publicKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);

            // Generate a salt for encryption
            var salt = _cryptoService.GenerateSalt();
            
            // Encrypt the private key
            var privateKeyWif = privateKey.GetWif(Network.Main).ToString();
            var encryptedPrivateKey = _cryptoService.EncryptPrivateKey(privateKeyWif, salt);

            // Store the wallet info in the database
            var wallet = new Wallet
            {
                Username = username,
                EncryptedPrivateKey = encryptedPrivateKey,
                Salt = salt,
                PublicKey = publicKey.ToString(),
                Address = address.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            _context.Wallets.Add(wallet);
            await _context.SaveChangesAsync();

            return (privateKey, address);
        }

        public async Task<IEnumerable<BitcoinAddress>> GetAddressesForUser(string username)
        {
            var wallets = await _context.Wallets
                .Where(w => w.Username == username)
                .ToListAsync();

            var addresses = new List<BitcoinAddress>();
            
            foreach (var wallet in wallets)
            {
                try
                {
                    // Try to create a BitcoinAddress from the stored address string
                    if (!string.IsNullOrEmpty(wallet.Address))
                    {
                        var address = BitcoinAddress.Create(wallet.Address, Network.Main);
                        addresses.Add(address);
                    }
                    // Fallback: if we don't have an address string, try to derive it from the public key
                    else if (!string.IsNullOrEmpty(wallet.PublicKey))
                    {
                        try
                        {
                            var pubKey = new PubKey(wallet.PublicKey);
                            var address = pubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
                            addresses.Add(address);
                        }
                        catch
                        {
                            // Skip this wallet if we can't parse the public key
                        }
                    }
                }
                catch
                {
                    // Skip invalid addresses
                }
            }

            return addresses;
        }

        public async Task<Key?> GetPrivateKeyForAddress(string address, string username)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.Address == address && w.Username == username);

            if (wallet == null)
                return null;

            try
            {
                // Decrypt the private key
                var privateKeyWif = _cryptoService.DecryptPrivateKey(wallet.EncryptedPrivateKey, wallet.Salt);
                
                // Convert the WIF string back to a Key object
                var bitcoinSecret = new BitcoinSecret(privateKeyWif, Network.Main);
                return bitcoinSecret.PrivateKey;
            }
            catch
            {
                // Return null if decryption fails
                return null;
            }
        }
    }
}
