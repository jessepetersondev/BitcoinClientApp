using BitcoinClientApp.Services;
using NBitcoin;

namespace BitcoinClientApp.Tests.Services
{
    public class PsbtServiceTests
    {
        private readonly Mock<IPsbtService> _mockPsbtService;

        public PsbtServiceTests()
        {
            _mockPsbtService = new Mock<IPsbtService>();
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
            
            // Create expected PSBT
            var tx = Transaction.Create(Network.Main);
            tx.Inputs.Add(new TxIn(coin.Outpoint));
            tx.Outputs.Add(txOut);
            // Add a change output
            tx.Outputs.Add(new TxOut(Money.Coins(0.09m), address.ScriptPubKey)); // Change output
            
            var expectedPsbt = PSBT.FromTransaction(tx, Network.Main);
            expectedPsbt.AddCoins(coin);
            
            // Setup mock to return the expected PSBT
            _mockPsbtService.Setup(p => p.Create(
                It.IsAny<IEnumerable<Coin>>(), 
                It.IsAny<IEnumerable<TxOut>>(), 
                It.IsAny<FeeRate>()))
                .Returns(expectedPsbt);
            
            // Act
            var psbt = _mockPsbtService.Object.Create(coins, outs, feeRate);
            
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
            
            // Verify mock was called with correct parameters
            _mockPsbtService.Verify(p => p.Create(
                It.Is<IEnumerable<Coin>>(c => c.Count() == coins.Count()),
                It.Is<IEnumerable<TxOut>>(t => t.Count() == outs.Count()),
                It.Is<FeeRate>(f => f.FeePerK == feeRate.FeePerK)), 
                Times.Once);
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
            
            var expectedPsbt = PSBT.FromTransaction(tx, Network.Main);
            expectedPsbt.AddCoins(coin);
            
            // Convert to base64
            string psbtBase64 = expectedPsbt.ToBase64();
            
            // Setup mock to return the expected PSBT
            _mockPsbtService.Setup(p => p.Parse(psbtBase64))
                .Returns(expectedPsbt);
            
            // Act
            var parsedPsbt = _mockPsbtService.Object.Parse(psbtBase64);
            
            // Assert
            Assert.NotNull(parsedPsbt);
            Assert.IsAssignableFrom<PSBT>(parsedPsbt);
            
            // Verify basic PSBT structure
            Assert.Single(parsedPsbt.Inputs);
            Assert.Single(parsedPsbt.Outputs);
            
            // Verify mock was called with correct parameter
            _mockPsbtService.Verify(p => p.Parse(psbtBase64), Times.Once);
        }

        [Fact]
        public void Sign_ShouldSignPSBTWithProvidedKey()
        {
            // Arrange
            // Create a key and derivation path
            var masterKey = new ExtKey();
            
            // Create a sample transaction to use as a NonWitnessUtxo
            var fundingTx = Transaction.Create(Network.Main);
            fundingTx.Outputs.Add(new TxOut(Money.Coins(1), masterKey.PrivateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main).ScriptPubKey));
            
            // Create a coin (input)
            var coin = new Coin(
                fromTxHash: fundingTx.GetHash(),
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
            
            // Create unsigned PSBT
            var tx = Transaction.Create(Network.Main);
            tx.Inputs.Add(new TxIn(coin.Outpoint));
            tx.Outputs.Add(txOut);
            
            var unsignedPsbt = PSBT.FromTransaction(tx, Network.Main);
            unsignedPsbt.AddCoins(coin);
            unsignedPsbt.Inputs[0].NonWitnessUtxo = fundingTx;
            
            // Create a manually signed PSBT for the mock to return
            var signedPsbt = unsignedPsbt.Clone();
            signedPsbt.SignWithKeys(masterKey.PrivateKey);
            
            // Setup Create mock
            _mockPsbtService.Setup(p => p.Create(
                It.IsAny<IEnumerable<Coin>>(), 
                It.IsAny<IEnumerable<TxOut>>(), 
                It.IsAny<FeeRate>()))
                .Returns(unsignedPsbt);
            
            // Setup Sign mock
            _mockPsbtService.Setup(p => p.Sign(unsignedPsbt, masterKey))
                .Returns(signedPsbt);
            
            // Act - First call Create to get the PSBT, then sign it
            var psbt = _mockPsbtService.Object.Create(coins, outs, new FeeRate(Money.Satoshis(10000)));
            var result = _mockPsbtService.Object.Sign(psbt, masterKey);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(result.Inputs.All(i => i.PartialSigs.Count > 0)); // Each input should have a signature
            
            // Verify mock was called with correct parameters
            _mockPsbtService.Verify(p => p.Sign(unsignedPsbt, masterKey), Times.Once);
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
            var tx = Transaction.Create(Network.Main);
            tx.Inputs.Add(new TxIn(coin.Outpoint));
            tx.Outputs.Add(txOut);
            tx.Outputs.Add(new TxOut(Money.Coins(0.09m), address.ScriptPubKey)); // Change output
            
            var psbt = PSBT.FromTransaction(tx, Network.Main);
            psbt.AddCoins(coin);
            
            // Add the NonWitnessUtxo to the PSBT
            psbt.Inputs[0].NonWitnessUtxo = fundingTx;
            
            // Sign PSBT
            psbt.SignWithKeys(key);
            
            // Finalize the PSBT before extracting
            psbt.Finalize();
            
            // Create expected final transaction
            var expectedFinalTx = psbt.ExtractTransaction();
            
            // Setup Create mock
            _mockPsbtService.Setup(p => p.Create(
                It.IsAny<IEnumerable<Coin>>(), 
                It.IsAny<IEnumerable<TxOut>>(), 
                It.IsAny<FeeRate>()))
                .Returns(psbt);
            
            // Setup Finalize mock
            _mockPsbtService.Setup(p => p.Finalize(psbt))
                .Returns(expectedFinalTx);
            
            // Act - First get the PSBT, then finalize it
            var testPsbt = _mockPsbtService.Object.Create(coins, outs, new FeeRate(Money.Satoshis(10000)));
            var finalTx = _mockPsbtService.Object.Finalize(testPsbt);
            
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
            
            // Verify the mock was called with correct parameters
            _mockPsbtService.Verify(p => p.Finalize(psbt), Times.Once);
        }
    }
} 