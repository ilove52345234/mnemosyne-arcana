using System;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class S4PriorityValidationTests
    {
        private GateProgressionManagerV2 _gate;
        private DecayManagerV2 _decay;

        [SetUp]
        public void SetUp()
        {
            _gate = new GateProgressionManagerV2();
            _decay = new DecayManagerV2();
        }

        [Test]
        public void RecoveryGate_ThreeModelProfiles_MatchPassCriteria()
        {
            // S4-M1 (Low): below required coverage, repeated failure, still inside 7-day protection.
            var low = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.72f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 2,
                daysSinceLastDemotion: 3);

            Assert.IsTrue(low.IsSuccess);
            Assert.IsTrue(low.Value.NeedsRecoveryGate);
            Assert.IsFalse(low.Value.ShouldDemote);
            Assert.IsTrue(low.Value.DemotionBlockedByProtection);
            Assert.Greater(low.Value.ProtectionDaysRemaining, 0);

            // S4-M2 (Mid): below required coverage but not yet over failure threshold.
            var mid = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.82f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 1,
                daysSinceLastDemotion: 10);

            Assert.IsTrue(mid.IsSuccess);
            Assert.IsTrue(mid.Value.NeedsRecoveryGate);
            Assert.IsFalse(mid.Value.ShouldDemote);
            Assert.IsFalse(mid.Value.DemotionBlockedByProtection);

            // S4-M3 (High): above coverage target should pass without recovery.
            var high = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.91f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 0,
                daysSinceLastDemotion: 10);

            Assert.IsTrue(high.IsSuccess);
            Assert.IsFalse(high.Value.NeedsRecoveryGate);
            Assert.IsFalse(high.Value.ShouldDemote);
            Assert.IsFalse(high.Value.DemotionBlockedByProtection);
        }

        [Test]
        public void RecoveryGate_DemotionTriggersAfterProtectionWindow()
        {
            var result = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.70f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 2,
                daysSinceLastDemotion: 8);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.NeedsRecoveryGate);
            Assert.IsTrue(result.Value.ShouldDemote);
            Assert.IsFalse(result.Value.DemotionBlockedByProtection);
            Assert.AreEqual(0, result.Value.ProtectionDaysRemaining);
        }

        [Test]
        public void Decay_LongCycle_SevenFourteenThirtyDays_DegradesStepwise()
        {
            var now = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc);
            var word = new WordProgress
            {
                WordId = "s4-long-cycle",
                Level = LearningLevel.Lv4,
                Pool = WordPool.Mastered,
                LastPracticed = now
            };

            ApplyDecayAt(word, now.AddDays(7));
            Assert.AreEqual(LearningLevel.Lv3, word.Level);
            Assert.AreEqual(WordPool.Learning, word.Pool);

            ApplyDecayAt(word, now.AddDays(14));
            Assert.AreEqual(LearningLevel.Lv2, word.Level);
            Assert.AreEqual(WordPool.Decayed, word.Pool);

            ApplyDecayAt(word, now.AddDays(30));
            Assert.AreEqual(LearningLevel.Lv0, word.Level);
            Assert.AreEqual(WordPool.Decayed, word.Pool);
        }

        private void ApplyDecayAt(WordProgress word, DateTime checkpoint)
        {
            // Simulate a long idle window by consuming each level's decay interval stepwise.
            var simulatedLastPracticed = word.LastPracticed;
            for (var i = 0; i < 8; i++)
            {
                var result = _decay.EvaluateDecay(word, checkpoint);
                if (!result.Decayed)
                {
                    return;
                }

                word.Level = result.NewLevel;
                word.Pool = result.NewPool;

                var consumedDays = GetDecayDays(result.PreviousLevel);
                if (consumedDays <= 0)
                {
                    return;
                }

                simulatedLastPracticed = simulatedLastPracticed.AddDays(consumedDays);
                if (simulatedLastPracticed > checkpoint)
                {
                    simulatedLastPracticed = checkpoint;
                }

                word.LastPracticed = simulatedLastPracticed;
            }
        }

        private static int GetDecayDays(LearningLevel level)
        {
            return level switch
            {
                LearningLevel.Lv1 => 1,
                LearningLevel.Lv2 => 3,
                LearningLevel.Lv3 => 7,
                LearningLevel.Lv4 => 7,
                _ => -1
            };
        }
    }
}
