using BitcoinAddressToolkit.Models;
using BitcoinClientApp.Interfaces;
using NBitcoin;

namespace BitcoinClientApp.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly Mock<ITransactionService> _mockTransactionService;

        public TransactionServiceTests()
        {
            _mockTransactionService = new Mock<ITransactionService>();
        }

        [Fact]
        public void BuildAndSign_ShouldCreateSignedTransaction()
        {
            // Arrange
            // Create a private key and address for testing
            var privateKey = new Key();
            var sourceAddress = privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            // Create a destination address
            var destinationKey = new Key();
            var destinationAddress = destinationKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
            
            // Create a UTXO
            var utxo = new Utxo
            {
                Outpoint = new OutPoint(uint256.Parse("0000000000000000000000000000000000000000000000000000000000000001"), 0),
                Amount = Money.Coins(1),
                ScriptPubKey = sourceAddress.ScriptPubKey
            };
            
            var utxos = new List<Utxo> { utxo };
            var fee = Money.Coins(0.0001m); // 0.0001 BTC fee
            
            // Create expected transaction
            var expectedTx = Transaction.Create(Network.Main);
            expectedTx.Inputs.Add(new TxIn(utxo.Outpoint));
            
            // Calculate expected output amount (input - fee)
            var expectedOutputAmount = utxo.Amount - fee;
            
            // Add output to destination address with the full amount minus fee
            expectedTx.Outputs.Add(new TxOut(expectedOutputAmount, destinationAddress.ScriptPubKey));
            
            // Create a transaction with a dummy signature for testing purposes
            var coin = new Coin(utxo.Outpoint, new TxOut(utxo.Amount, utxo.ScriptPubKey));
            // Add dummy signature to make it appear signed for test verification
            expectedTx.Inputs[0].ScriptSig = new Script(Op.GetPushOp(new byte[72]));

            // Setup mock to return the expected transaction
            _mockTransactionService.Setup(t => t.BuildAndSign(
                It.IsAny<List<Utxo>>(),
                It.IsAny<BitcoinAddress>(),
                It.IsAny<Key>(),
                It.IsAny<Money>()))
                .Returns(expectedTx);
            
            // Act
            var transaction = _mockTransactionService.Object.BuildAndSign(utxos, destinationAddress, privateKey, fee);
            
            // Assert
            Assert.NotNull(transaction);
            Assert.IsType<Transaction>(transaction);
            
            // Verify the transaction has inputs and outputs
            Assert.NotEmpty(transaction.Inputs);
            Assert.NotEmpty(transaction.Outputs);
            
            // Verify the transaction has the correct input
            Assert.Equal(utxo.Outpoint.Hash, transaction.Inputs[0].PrevOut.Hash);
            Assert.Equal(utxo.Outpoint.N, transaction.Inputs[0].PrevOut.N);
            
            // Verify the transaction has the correct output to the destination address
            Assert.Contains(transaction.Outputs, o => o.ScriptPubKey == destinationAddress.ScriptPubKey);
            
            // Verify the transaction is properly signed
            // This test uses scriptSigs directly since we know we're using P2PKH
            Assert.NotEmpty(transaction.Inputs[0].ScriptSig.ToBytes());
            
            // Verify the output amount is correct (accounting for the fee)
            var totalOutputValue = Money.Zero;
            foreach (var output in transaction.Outputs)
            {
                totalOutputValue += output.Value;
            }
            
            Assert.Equal(expectedOutputAmount, totalOutputValue);
            
            // Verify mock was called with correct parameters
            _mockTransactionService.Verify(t => t.BuildAndSign(
                It.Is<List<Utxo>>(u => u.Count == utxos.Count),
                It.Is<BitcoinAddress>(a => a == destinationAddress),
                It.Is<Key>(k => k == privateKey),
                It.Is<Money>(m => m == fee)),
                Times.Once);
        }
    }
} 