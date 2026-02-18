using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class LearningTelemetryTests
    {
        private LearningTelemetryManagerV2 _telemetry;

        [SetUp]
        public void SetUp()
        {
            _telemetry = new LearningTelemetryManagerV2();
        }

        [Test]
        public void EvaluateAlerts_PassRateTooHigh_ReturnsTooEasyAlert()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.9f,
                RecoverySuccessRate = 0.8f,
                ActiveRecallAccuracy = 0.8f,
                DecayRegressionRate = 0.1f,
                GateStallDays = 2f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("GATE_TOO_EASY", result.Value[0].Code);
        }

        [Test]
        public void EvaluateAlerts_PassRateTooLow_ReturnsTooHardAlert()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.3f,
                RecoverySuccessRate = 0.8f,
                ActiveRecallAccuracy = 0.8f,
                DecayRegressionRate = 0.2f,
                GateStallDays = 4f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("GATE_TOO_HARD", result.Value[0].Code);
        }

        [Test]
        public void EvaluateAlerts_RecoveryTooLow_ReturnsRecoveryAlert()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.6f,
                RecoverySuccessRate = 0.4f,
                ActiveRecallAccuracy = 0.8f,
                DecayRegressionRate = 0.2f,
                GateStallDays = 5f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Value.Count);
            Assert.AreEqual("RECOVERY_FAILING", result.Value[0].Code);
        }

        [Test]
        public void EvaluateAlerts_InTargetRange_ReturnsNoAlert()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 0.6f,
                RecoverySuccessRate = 0.65f,
                ActiveRecallAccuracy = 0.82f,
                DecayRegressionRate = 0.2f,
                GateStallDays = 3f
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Value.Count);
        }

        [Test]
        public void EvaluateAlerts_InvalidInput_ReturnsError()
        {
            var result = _telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = 1.2f,
                RecoverySuccessRate = 0.65f,
                ActiveRecallAccuracy = 0.82f,
                DecayRegressionRate = 0.2f,
                GateStallDays = 3f
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }
    }
}

