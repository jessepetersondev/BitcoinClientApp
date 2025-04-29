using PSBTToolkit;
using NBitcoin;
using System.Collections.Generic;
using System.Linq;
using BitcoinClientApp.Interfaces;

namespace BitcoinClientApp.Services
{
    
    public class PsbtService : IPsbtService
    {
        public PSBT Create(IEnumerable<Coin> coins, IEnumerable<TxOut> outs, FeeRate fee)
        {
            // Use the first input's address as the change address
            var firstCoin = coins.First();
            var changeScript = firstCoin.ScriptPubKey;
            
            // Create a builder
            var builder = Network.Main.CreateTransactionBuilder();
            builder.AddCoins(coins);
            
            foreach (var txOut in outs)
            {
                builder.Send(txOut.ScriptPubKey, txOut.Value);
            }
            
            builder.SetChange(changeScript);
            builder.SendEstimatedFees(fee);
            
            // Build PSBT
            var psbt = builder.BuildPSBT(false);
            return psbt;
        }

        public PSBT Parse(string b64) => PSBTHelper.Parse(b64);

        public PSBT Sign(PSBT psbt, ExtKey key) => PSBTHelper.SignWith(psbt, key);

        public Transaction Finalize(PSBT psbt) => PSBTHelper.Finalize(psbt);
    }
}
