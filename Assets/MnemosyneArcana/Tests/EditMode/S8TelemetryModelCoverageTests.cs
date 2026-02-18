using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class S8TelemetryModelCoverageTests
    {
        private LearningTelemetryManagerV2 _telemetry;

        [SetUp]
        public void SetUp()
        {
            _telemetry = new LearningTelemetryManagerV2();
        }

        [Test]
        public void S8_M1_LowProfile_TriggersGateTooHard()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.32f,
                RecoverySuccessRate = 0.62f,
                ActiveRecallAccuracy = 0.72f,
                DecayRegressionRate = 0.24f,
                GateStallDays = 6f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.That(result.Value, Has.Some.Matches<TelemetryAlert>(a => a.Code == "GATE_TOO_HARD"));
        }

        [Test]
        public void S8_M2_MidProfile_RemainsWithinTarget_NoAlert()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.60f,
                RecoverySuccessRate = 0.66f,
                ActiveRecallAccuracy = 0.82f,
                DecayRegressionRate = 0.18f,
                GateStallDays = 3f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Value.Count);
        }

        [Test]
        public void S8_M3_HighProfile_TriggersGateTooEasy()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.90f,
                RecoverySuccessRate = 0.78f,
                ActiveRecallAccuracy = 0.90f,
                DecayRegressionRate = 0.12f,
                GateStallDays = 1f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.That(result.Value, Has.Some.Matches<TelemetryAlert>(a => a.Code == "GATE_TOO_EASY"));
        }

        [Test]
        public void S8_FP_HighPassButLowRecall_DoesNotTriggerTooEasy()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.88f,
                RecoverySuccessRate = 0.70f,
                ActiveRecallAccuracy = 0.60f,
                DecayRegressionRate = 0.18f,
                GateStallDays = 2f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.That(result.Value, Has.None.Matches<TelemetryAlert>(a => a.Code == "GATE_TOO_EASY"));
        }

        [Test]
        public void S8_FN_BorderlinePassWithLongStall_TriggersTooHard()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.40f,
                RecoverySuccessRate = 0.60f,
                ActiveRecallAccuracy = 0.74f,
                DecayRegressionRate = 0.22f,
                GateStallDays = 8f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.That(result.Value, Has.Some.Matches<TelemetryAlert>(a => a.Code == "GATE_TOO_HARD"));
        }
    }
}
