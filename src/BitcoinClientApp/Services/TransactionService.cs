using System.Collections.Generic;
using BitcoinAddressToolkit;
using BitcoinAddressToolkit.Models;
using BitcoinClientApp.Interfaces;
using NBitcoin;

namespace BitcoinClientApp.Services
{
    
    public class TransactionService : ITransactionService
    {
        public Transaction BuildAndSign(IEnumerable<Utxo> utxos, BitcoinAddress dest, Key key, Money fee) =>
            TransactionBuilderHelper.BuildAndSign(utxos, dest, key, fee);
    }
}
