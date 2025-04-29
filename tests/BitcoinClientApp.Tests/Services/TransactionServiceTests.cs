using System.Collections.Generic;
using BitcoinAddressToolkit.Models;
using BitcoinClientApp.Services;
using NBitcoin;
using Xunit;
using Moq;

namespace BitcoinClientApp.Tests.Services
{
    public class TransactionServiceTests
    {
        private readonly ITransactionService _transactionService;

        public TransactionServiceTests()
        {
            _transactionService = new TransactionService();
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
            
            // Act
            var transaction = _transactionService.BuildAndSign(utxos, destinationAddress, privateKey, fee);
            
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
            
            // Calculate expected output amount (input - fee)
            var expectedOutputAmount = utxo.Amount - fee;
            
            // Verify the output amount is correct (accounting for the fee)
            var totalOutputValue = Money.Zero;
            foreach (var output in transaction.Outputs)
            {
                totalOutputValue += output.Value;
            }
            
            Assert.Equal(expectedOutputAmount, totalOutputValue);
        }
    }
} 