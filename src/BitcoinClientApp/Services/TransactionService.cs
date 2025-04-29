using System.Collections.Generic;
using BitcoinAddressToolkit;
using BitcoinAddressToolkit.Models;
using NBitcoin;
using TransactionsToolkit;

namespace BitcoinClientApp.Services
{
    public interface ITransactionService
    {
        Transaction BuildAndSign(IEnumerable<Utxo> utxos, BitcoinAddress dest, Key key, Money fee);
    }
    
    public class TransactionService : ITransactionService
    {
        public Transaction BuildAndSign(IEnumerable<Utxo> utxos, BitcoinAddress dest, Key key, Money fee) =>
            TransactionBuilderHelper.BuildAndSign(utxos, dest, key, fee);
    }
}
