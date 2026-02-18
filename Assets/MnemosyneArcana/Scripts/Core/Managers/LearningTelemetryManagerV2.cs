using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class LearningTelemetryManagerV2 : ILearningTelemetryService
    {
        public ServiceResult<IReadOnlyList<TelemetryAlert>> EvaluateAlerts(LearningTelemetrySnapshot snapshot)
        {
            if (snapshot == null ||
                snapshot.PassRateByGate < 0f || snapshot.PassRateByGate > 1f ||
                snapshot.RecoverySuccessRate < 0f || snapshot.RecoverySuccessRate > 1f ||
                snapshot.ActiveRecallAccuracy < 0f || snapshot.ActiveRecallAccuracy > 1f ||
                snapshot.DecayRegressionRate < 0f || snapshot.DecayRegressionRate > 1f ||
                snapshot.GateStallDays < 0f)
            {
                return ServiceResult<IReadOnlyList<TelemetryAlert>>.Fail(ErrorCode.InvalidInput);
            }

            var alerts = new List<TelemetryAlert>();

            if (snapshot.PassRateByGate > 0.85f)
            {
                alerts.Add(new TelemetryAlert
                {
                    Code = "GATE_TOO_EASY",
                    Message = "PassRateByGate > 85%，關卡壓力不足。"
                });
            }
            else if (snapshot.PassRateByGate < 0.35f)
            {
                alerts.Add(new TelemetryAlert
                {
                    Code = "GATE_TOO_HARD",
                    Message = "PassRateByGate < 35%，關卡挫折過高。"
                });
            }

            if (snapshot.RecoverySuccessRate < 0.50f)
            {
                alerts.Add(new TelemetryAlert
                {
                    Code = "RECOVERY_FAILING",
                    Message = "RecoverySuccessRate < 50%，回補關設計可能失效。"
                });
            }

            return ServiceResult<IReadOnlyList<TelemetryAlert>>.Ok(alerts);
        }
    }
}

