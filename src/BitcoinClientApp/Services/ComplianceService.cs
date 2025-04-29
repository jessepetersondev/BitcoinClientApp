using System.Collections.Generic;
using System.Linq;
using BtcComplianceToolkit;
using BtcComplianceToolkit.Models;
using NBitcoin;

namespace BitcoinClientApp.Services
{
    public interface IComplianceService
    {
        IEnumerable<Utxo> FindOfac(IEnumerable<Utxo> utxos, HashSet<BitcoinAddress> ofac);
        bool HasRisky(Transaction tx);
        IDictionary<Script, List<Utxo>> Cluster(IEnumerable<Utxo> utxos);
    }
    
    public class ComplianceService : IComplianceService
    {
        public IEnumerable<Utxo> FindOfac(IEnumerable<Utxo> utxos, HashSet<BitcoinAddress> ofac) =>
            BtcComplianceToolkit.BtcComplianceToolkit.FindOfacHits(utxos, new ComplianceSettings { OfacAddresses = ofac });

        public bool HasRisky(Transaction tx) =>
            BtcComplianceToolkit.BtcComplianceToolkit.HasRiskyScripts(tx, new ComplianceSettings());

        public IDictionary<Script, List<Utxo>> Cluster(IEnumerable<Utxo> utxos) =>
            BtcComplianceToolkit.BtcComplianceToolkit.ClusterByScript(utxos);
    }
}
