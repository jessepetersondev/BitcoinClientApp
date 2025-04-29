using WatchOnlyWalletToolkit.Models;
using NBitcoin;
using System.Collections.Generic;

namespace BitcoinClientApp.Services
{
    public interface IWatchOnlyService
    {
        ExtPubKey Parse(string xpub);
        IEnumerable<BitcoinAddress> Derive(ExtPubKey x, int change, int start, int count);
        IEnumerable<Utxo> Filter(ExtPubKey x, IEnumerable<Utxo> utxos, int change, int start, int count);
    }

    public class WatchOnlyService : IWatchOnlyService
    {
        public ExtPubKey Parse(string xpub) => WatchOnlyWalletToolkit.WatchOnlyWalletToolkit.ParseXpub(xpub);

        public IEnumerable<BitcoinAddress> Derive(ExtPubKey x, int change, int start, int count) =>
            WatchOnlyWalletToolkit.WatchOnlyWalletToolkit.DeriveAddresses(x, 0, change, start, count, Network.Main);

        public IEnumerable<Utxo> Filter(ExtPubKey x, IEnumerable<Utxo> utxos, int change, int start, int count) =>
            WatchOnlyWalletToolkit.WatchOnlyWalletToolkit.FilterUtxos(x, utxos, 0, change, start, count, Network.Main);
    }
}
