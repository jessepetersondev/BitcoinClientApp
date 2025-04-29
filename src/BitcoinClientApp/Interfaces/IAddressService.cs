using System.Collections.Generic;
using System.Threading.Tasks;
using NBitcoin;

namespace BitcoinClientApp.Interfaces
{
    public interface IAddressService
    {
        Task<(Key privateKey, BitcoinAddress address)> CreateNewWallet(string username);
        Task<IEnumerable<BitcoinAddress>> GetAddressesForUser(string username);
        Task<Key?> GetPrivateKeyForAddress(string address, string username);
    }
}