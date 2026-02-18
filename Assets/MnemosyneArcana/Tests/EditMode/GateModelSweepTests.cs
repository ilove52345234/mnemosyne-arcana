using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class GateModelSweepTests
    {
        private static readonly int[] ModelLearnedCounts = { 0, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };

        [Test]
        public void Sweep_IdealMemory_MapsOneToOneModelUnlock()
        {
            var gate = new GateProgressionManagerV2();
            var expectedUnlockedModels = new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            for (var i = 0; i < ModelLearnedCounts.Length; i++)
            {
                var result = gate.EvaluateProgress(
                    learnedCount: ModelLearnedCounts[i],
                    retentionRate: 1f,
                    retrievalRate: 1f,
                    currentModelIndex: i);

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(expectedUnlockedModels[i], result.Value.HighestUnlockedModelIndex);
                Assert.IsTrue(result.Value.CanPassCurrentGate);
            }
        }

        [Test]
        public void Sweep_RealisticMemory_ShowsExpectedChokeDistribution()
        {
            var gate = new GateProgressionManagerV2();
            const float retentionRate = 0.85f;
            const float retrievalRate = 0.8f;

            // 以目前 10 段模型與 EffectiveVocab 公式固定基線：
            // effective = learned * 0.85 * 0.8 = learned * 0.68
            var expectedUnlockedModels = new[] { 0, 0, 1, 1, 2, 3, 3, 4, 5, 5 };

            for (var i = 0; i < ModelLearnedCounts.Length; i++)
            {
                var result = gate.EvaluateProgress(
                    learnedCount: ModelLearnedCounts[i],
                    retentionRate: retentionRate,
                    retrievalRate: retrievalRate,
                    currentModelIndex: i);

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(expectedUnlockedModels[i], result.Value.HighestUnlockedModelIndex);
            }
        }
    }
}

