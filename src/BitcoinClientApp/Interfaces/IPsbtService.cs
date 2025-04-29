using System.Collections.Generic;
using NBitcoin;

namespace BitcoinClientApp.Interfaces
{
    public interface IPsbtService
    {
        PSBT Create(IEnumerable<Coin> coins, IEnumerable<TxOut> outs, FeeRate fee);
        PSBT Parse(string b64);
        PSBT Sign(PSBT psbt, ExtKey key);
        Transaction Finalize(PSBT psbt);
    }
}