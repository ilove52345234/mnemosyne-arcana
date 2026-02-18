using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class GateProgressionTests
    {
        private GateProgressionManagerV2 _gate;

        [SetUp]
        public void SetUp()
        {
            _gate = new GateProgressionManagerV2();
        }

        [Test]
        public void EvaluateProgress_Model0_ZeroCanPass()
        {
            var result = _gate.EvaluateProgress(0, 1f, 1f, 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Value.CurrentRequiredVocab);
            Assert.IsTrue(result.Value.CanPassCurrentGate);
            Assert.AreEqual(0, result.Value.HighestUnlockedRequiredVocab);
        }

        [Test]
        public void EvaluateProgress_Model1_Uses2000Requirement_Not1000()
        {
            var result = _gate.EvaluateProgress(2000, 1f, 1f, 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2000, result.Value.CurrentRequiredVocab);
            Assert.IsTrue(result.Value.CanPassCurrentGate);
        }

        [Test]
        public void EvaluateProgress_EffectiveVocabBelowRequirement_FailsCurrentGate()
        {
            var result = _gate.EvaluateProgress(4000, 0.7f, 0.7f, 2); // 1960 < 3000

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.CanPassCurrentGate);
            Assert.AreEqual(0, result.Value.HighestUnlockedRequiredVocab);
        }

        [Test]
        public void EvaluateProgress_EffectiveVocabReachesFinal_Unlocks10000Model()
        {
            var result = _gate.EvaluateProgress(12000, 0.95f, 0.9f, 9); // 10260

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.CanPassCurrentGate);
            Assert.AreEqual(9, result.Value.HighestUnlockedModelIndex);
            Assert.AreEqual(10000, result.Value.HighestUnlockedRequiredVocab);
        }

        [Test]
        public void EvaluateProgress_InvalidInput_ReturnsError()
        {
            var result = _gate.EvaluateProgress(-1, 1.2f, 1f, 0);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }

        [Test]
        public void EvaluateRecoveryGate_AboveCoverage_NoRecoveryNeeded()
        {
            var result = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.9f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 0,
                daysSinceLastDemotion: 100);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.NeedsRecoveryGate);
            Assert.IsFalse(result.Value.ShouldDemote);
            Assert.IsFalse(result.Value.DemotionBlockedByProtection);
            Assert.AreEqual(0, result.Value.ProtectionDaysRemaining);
        }

        [Test]
        public void EvaluateRecoveryGate_BelowCoverage_FirstCycle_EnterRecoveryOnly()
        {
            var result = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.7f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 1,
                daysSinceLastDemotion: 100);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.NeedsRecoveryGate);
            Assert.IsFalse(result.Value.ShouldDemote);
            Assert.IsFalse(result.Value.DemotionBlockedByProtection);
        }

        [Test]
        public void EvaluateRecoveryGate_BelowCoverage_SecondCycleWithinProtection_Blocked()
        {
            var result = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.7f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 2,
                daysSinceLastDemotion: 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.NeedsRecoveryGate);
            Assert.IsFalse(result.Value.ShouldDemote);
            Assert.IsTrue(result.Value.DemotionBlockedByProtection);
            Assert.AreEqual(4, result.Value.ProtectionDaysRemaining);
        }

        [Test]
        public void EvaluateRecoveryGate_BelowCoverage_SecondCycleAfterProtection_Demote()
        {
            var result = _gate.EvaluateRecoveryGate(
                coreCoverageRate: 0.7f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 2,
                daysSinceLastDemotion: 7);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.NeedsRecoveryGate);
            Assert.IsTrue(result.Value.ShouldDemote);
            Assert.IsFalse(result.Value.DemotionBlockedByProtection);
            Assert.AreEqual(0, result.Value.ProtectionDaysRemaining);
        }

        [Test]
        public void EvaluateRecoveryGate_InvalidInput_ReturnsError()
        {
            var result = _gate.EvaluateRecoveryGate(
                coreCoverageRate: -0.1f,
                requiredCoverageRate: 0.85f,
                consecutiveRecoveryCycleFailures: 0,
                daysSinceLastDemotion: 0);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }

        [Test]
        public void EvaluateBossRecallGate_MeetsRatioAndAccuracy_Passes()
        {
            var result = _gate.EvaluateBossRecallGate(
                activeRecallQuestionRatio: 0.45f,
                activeRecallAccuracy: 0.82f,
                requiredRecallRatio: 0.40f,
                requiredRecallAccuracy: 0.80f);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.MeetsRecallRatio);
            Assert.IsTrue(result.Value.MeetsRecallAccuracy);
            Assert.IsTrue(result.Value.CanPassBossGate);
        }

        [Test]
        public void EvaluateBossRecallGate_RatioInsufficient_Fails()
        {
            var result = _gate.EvaluateBossRecallGate(
                activeRecallQuestionRatio: 0.35f,
                activeRecallAccuracy: 0.90f,
                requiredRecallRatio: 0.40f,
                requiredRecallAccuracy: 0.80f);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.MeetsRecallRatio);
            Assert.IsTrue(result.Value.MeetsRecallAccuracy);
            Assert.IsFalse(result.Value.CanPassBossGate);
        }

        [Test]
        public void EvaluateBossRecallGate_AccuracyInsufficient_Fails()
        {
            var result = _gate.EvaluateBossRecallGate(
                activeRecallQuestionRatio: 0.50f,
                activeRecallAccuracy: 0.72f,
                requiredRecallRatio: 0.40f,
                requiredRecallAccuracy: 0.80f);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.MeetsRecallRatio);
            Assert.IsFalse(result.Value.MeetsRecallAccuracy);
            Assert.IsFalse(result.Value.CanPassBossGate);
        }

        [Test]
        public void EvaluateBossRecallGate_InvalidInput_ReturnsError()
        {
            var result = _gate.EvaluateBossRecallGate(
                activeRecallQuestionRatio: 1.2f,
                activeRecallAccuracy: 0.8f,
                requiredRecallRatio: 0.4f,
                requiredRecallAccuracy: 0.8f);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }

        [Test]
        public void EvaluateFinalMasteryGate_Above95Below100_MainClearOnly()
        {
            var result = _gate.EvaluateFinalMasteryGate(
                masteryCoverageRate: 0.97f,
                stableDaysAtHundredPercent: 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsMainClearEligible);
            Assert.IsFalse(result.Value.IsTrueClearEligible);
        }

        [Test]
        public void EvaluateFinalMasteryGate_At100ButStableDaysNotEnough_NoTrueClear()
        {
            var result = _gate.EvaluateFinalMasteryGate(
                masteryCoverageRate: 1.0f,
                stableDaysAtHundredPercent: 5);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsMainClearEligible);
            Assert.IsFalse(result.Value.IsTrueClearEligible);
        }

        [Test]
        public void EvaluateFinalMasteryGate_At100AndStableFor7Days_TrueClear()
        {
            var result = _gate.EvaluateFinalMasteryGate(
                masteryCoverageRate: 1.0f,
                stableDaysAtHundredPercent: 7);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsMainClearEligible);
            Assert.IsTrue(result.Value.IsTrueClearEligible);
        }

        [Test]
        public void EvaluateFinalMasteryGate_InvalidInput_ReturnsError()
        {
            var result = _gate.EvaluateFinalMasteryGate(
                masteryCoverageRate: 1.2f,
                stableDaysAtHundredPercent: -1);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }
    }
}
