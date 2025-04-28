using PSBTToolkit;
using NBitcoin;
using System.Collections.Generic;

namespace BitcoinClientApp.Services
{
    public class PsbtService
    {
        public PSBT Create(IEnumerable<Coin> coins, IEnumerable<TxOut> outs, FeeRate fee) =>
            PSBTHelper.CreatePSBT(coins, outs, fee);

        public PSBT Parse(string b64) => PSBTHelper.Parse(b64);

        public PSBT Sign(PSBT psbt, ExtKey key) => PSBTHelper.SignWith(psbt, key);

        public Transaction Finalize(PSBT psbt) => PSBTHelper.Finalize(psbt);
    }
}
