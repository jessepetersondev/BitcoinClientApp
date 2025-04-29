using System.Collections.Generic;
using System.Linq;
using BitcoinClientApp.Services;
using NBitcoin;
using Xunit;

namespace BitcoinClientApp.Tests.Services
{
    public class PsbtServiceTests
    {
        private readonly PsbtService _psbtService;

        public PsbtServiceTests()
        {
            _psbtService = new PsbtService();
        }

        [Fact]
        public void Create_ShouldCreateValidPSBT()
        {
            // Arrange
            // Create a key and address
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            // Create a coin (input)
            var coin = new Coin(
                fromTxHash: uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"),
                fromOutputIndex: 0,
                amount: Money.Coins(1),
                scriptPubKey: address.ScriptPubKey
            );
            
            var coins = new List<Coin> { coin };
            
            // Create an output
            var destKey = new Key();
            var destinationAddress = destKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var txOut = new TxOut(Money.Coins(0.9m), destinationAddress.ScriptPubKey);
            var outs = new List<TxOut> { txOut };
            
            // Set fee rate
            var feeRate = new FeeRate(Money.Satoshis(10000)); // 10,000 sat/vbyte
            
            // Act
            var psbt = _psbtService.Create(coins, outs, feeRate);
            
            // Assert
            Assert.NotNull(psbt);
            Assert.IsAssignableFrom<PSBT>(psbt);
            
            // Verify inputs
            Assert.Single(psbt.Inputs);
            Assert.Equal(coin.Outpoint.Hash, psbt.Inputs[0].PrevOut.Hash);
            Assert.Equal(coin.Outpoint.N, psbt.Inputs[0].PrevOut.N);
            
            // Verify outputs - now expecting 2 outputs (payment + change)
            Assert.Equal(2, psbt.Outputs.Count);
            
            // First output should match our specified output
            Assert.Equal(txOut.Value, psbt.Outputs[0].Value);
            Assert.Equal(txOut.ScriptPubKey, psbt.Outputs[0].ScriptPubKey);
            
            // Second output should be the change address
            Assert.Equal(address.ScriptPubKey, psbt.Outputs[1].ScriptPubKey);
        }

        [Fact]
        public void Parse_ShouldDecodePSBTFromBase64()
        {
            // Arrange - Create a valid PSBT to test parsing
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            var coin = new Coin(
                uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"),
                0,
                Money.Coins(1),
                address.ScriptPubKey
            );
            
            var destKey = new Key();
            var destinationAddress = destKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var txOut = new TxOut(Money.Coins(0.9m), destinationAddress.ScriptPubKey);
            
            // Create a PSBT
            var tx = Transaction.Create(Network.Main);
            tx.Inputs.Add(new TxIn(coin.Outpoint));
            tx.Outputs.Add(txOut);
            
            var psbt = PSBT.FromTransaction(tx, Network.Main);
            psbt.AddCoins(coin);
            
            // Convert to base64
            string psbtBase64 = psbt.ToBase64();
            
            // Act
            var parsedPsbt = _psbtService.Parse(psbtBase64);
            
            // Assert
            Assert.NotNull(parsedPsbt);
            Assert.IsAssignableFrom<PSBT>(parsedPsbt);
            
            // Verify basic PSBT structure
            Assert.Single(parsedPsbt.Inputs);
            Assert.Single(parsedPsbt.Outputs);
        }

        [Fact]
        public void Sign_ShouldSignPSBTWithProvidedKey()
        {
            // Arrange
            // Create a key and derivation path
            var masterKey = new ExtKey();
            
            // Create a coin (input)
            var coin = new Coin(
                fromTxHash: uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"),
                fromOutputIndex: 0,
                amount: Money.Coins(1),
                scriptPubKey: masterKey.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ScriptPubKey
            );
            
            var coins = new List<Coin> { coin };
            
            // Create an output
            var destKey = new Key();
            var destinationAddress = destKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var txOut = new TxOut(Money.Coins(0.9m), destinationAddress.ScriptPubKey);
            var outs = new List<TxOut> { txOut };
            
            // Create PSBT
            var psbt = _psbtService.Create(coins, outs, new FeeRate(Money.Satoshis(10000)));
            
            // Act
            var signedPsbt = _psbtService.Sign(psbt, masterKey);
            
            // Assert
            Assert.NotNull(signedPsbt);
            Assert.True(signedPsbt.Inputs.All(i => i.PartialSigs.Count > 0)); // Each input should have a signature
        }

        [Fact]
        public void Finalize_ShouldCreateFinalTransaction()
        {
            // Arrange
            // Create a key and address
            var key = new Key();
            var address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            // Create a sample transaction to use as a NonWitnessUtxo
            var fundingTx = Transaction.Create(Network.Main);
            fundingTx.Outputs.Add(new TxOut(Money.Coins(1), address.ScriptPubKey));
            
            // Create a coin (input)
            var coin = new Coin(
                fromTxHash: fundingTx.GetHash(),
                fromOutputIndex: 0,
                amount: Money.Coins(1),
                scriptPubKey: address.ScriptPubKey
            );
            
            var coins = new List<Coin> { coin };
            
            // Create an output
            var destKey = new Key();
            var destinationAddress = destKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            var txOut = new TxOut(Money.Coins(0.9m), destinationAddress.ScriptPubKey);
            var outs = new List<TxOut> { txOut };
            
            // Create PSBT
            var psbt = _psbtService.Create(coins, outs, new FeeRate(Money.Satoshis(10000)));
            
            // Add the NonWitnessUtxo to the PSBT
            psbt.Inputs[0].NonWitnessUtxo = fundingTx;
            
            // Sign PSBT
            psbt.SignWithKeys(key);
            
            // Act
            var finalTx = _psbtService.Finalize(psbt);
            
            // Assert
            Assert.NotNull(finalTx);
            Assert.IsType<Transaction>(finalTx);
            
            // Verify inputs
            Assert.Single(finalTx.Inputs);
            Assert.Equal(coin.Outpoint.Hash, finalTx.Inputs[0].PrevOut.Hash);
            Assert.Equal(coin.Outpoint.N, finalTx.Inputs[0].PrevOut.N);
            
            // Verify outputs (payment + change)
            Assert.Equal(2, finalTx.Outputs.Count);
            
            // First output should match our specified output
            Assert.Equal(txOut.Value, finalTx.Outputs[0].Value);
            Assert.Equal(txOut.ScriptPubKey, finalTx.Outputs[0].ScriptPubKey);
            
            // Second output should be the change address
            Assert.Equal(address.ScriptPubKey, finalTx.Outputs[1].ScriptPubKey);
            
            // Verify the inputs have scriptSigs (are finalized)
            Assert.NotEmpty(finalTx.Inputs[0].ScriptSig.ToBytes());
        }
    }
} 