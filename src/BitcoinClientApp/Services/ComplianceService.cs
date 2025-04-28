using System.Collections.Generic;
using BtcComplianceToolkit;
using BtcComplianceToolkit.Models;
using NBitcoin;

namespace BitcoinClientApp.Services
{
    public class ComplianceService
    {
        public IEnumerable<Utxo> FindOfac(IEnumerable<Utxo> utxos, HashSet<BitcoinAddress> ofac) =>
            BtcComplianceToolkit.BtcComplianceToolkit.FindOfacHits(utxos, new ComplianceSettings { OfacAddresses = ofac });

        public bool HasRisky(Transaction tx) =>
            BtcComplianceToolkit.BtcComplianceToolkit.HasRiskyScripts(tx, new ComplianceSettings());

        public IDictionary<string, List<Utxo>> Cluster(IEnumerable<Utxo> utxos) =>
            (IDictionary<string, List<Utxo>>)BtcComplianceToolkit.BtcComplianceToolkit.ClusterByScript(utxos);
    }
}
