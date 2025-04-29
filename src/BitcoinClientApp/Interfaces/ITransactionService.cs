
using System.Collections.Generic;
using BitcoinAddressToolkit.Models;
using NBitcoin;

namespace BitcoinClientApp.Interfaces
{
    public interface ITransactionService
    {
        Transaction BuildAndSign(IEnumerable<Utxo> utxos, BitcoinAddress dest, Key key, Money fee);
    }
}