using System.Collections.Generic;
using BitcoinAddressToolkit;
using BitcoinAddressToolkit.Models;
using NBitcoin;
using TransactionsToolkit;

namespace BitcoinClientApp.Services
{
    public class TransactionService
    {
        public Transaction BuildAndSign(IEnumerable<Utxo> utxos, BitcoinAddress dest, Key key, Money fee) =>
            TransactionBuilderHelper.BuildAndSign(utxos, dest, key, fee);
    }
}
