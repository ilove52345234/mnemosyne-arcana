using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class GateProgressionManagerV2 : IGateProgressionService
    {
        // 10 models: 0, 2000, 3000 ... 10000
        private static readonly int[] ModelRequirements = { 0, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000, 10000 };
        private const int DemotionFailureThreshold = 2;
        private const int DemotionProtectionDays = 7;
        private const float MainClearCoverageRate = 0.95f;
        private const float TrueClearCoverageRate = 1.0f;
        private const int TrueClearStableDays = 7;

        public ServiceResult<GateProgressionEvaluation> EvaluateProgress(
            int learnedCount,
            float retentionRate,
            float retrievalRate,
            int currentModelIndex)
        {
            if (learnedCount < 0 ||
                retentionRate < 0f || retentionRate > 1f ||
                retrievalRate < 0f || retrievalRate > 1f ||
                currentModelIndex < 0 || currentModelIndex >= ModelRequirements.Length)
            {
                return ServiceResult<GateProgressionEvaluation>.Fail(ErrorCode.InvalidInput);
            }

            var effectiveVocab = learnedCount * retentionRate * retrievalRate;
            var currentRequired = ModelRequirements[currentModelIndex];

            var unlockedIndex = 0;
            for (var i = 0; i < ModelRequirements.Length; i++)
            {
                if (effectiveVocab >= ModelRequirements[i])
                {
                    unlockedIndex = i;
                }
                else
                {
                    break;
                }
            }

            return ServiceResult<GateProgressionEvaluation>.Ok(new GateProgressionEvaluation
            {
                CurrentModelIndex = currentModelIndex,
                CurrentRequiredVocab = currentRequired,
                EffectiveVocab = effectiveVocab,
                CanPassCurrentGate = effectiveVocab >= currentRequired,
                HighestUnlockedModelIndex = unlockedIndex,
                HighestUnlockedRequiredVocab = ModelRequirements[unlockedIndex]
            });
        }

        public ServiceResult<RecoveryGateEvaluation> EvaluateRecoveryGate(
            float coreCoverageRate,
            float requiredCoverageRate,
            int consecutiveRecoveryCycleFailures,
            int daysSinceLastDemotion)
        {
            if (coreCoverageRate < 0f || coreCoverageRate > 1f ||
                requiredCoverageRate <= 0f || requiredCoverageRate > 1f ||
                consecutiveRecoveryCycleFailures < 0 ||
                daysSinceLastDemotion < 0)
            {
                return ServiceResult<RecoveryGateEvaluation>.Fail(ErrorCode.InvalidInput);
            }

            if (coreCoverageRate >= requiredCoverageRate)
            {
                return ServiceResult<RecoveryGateEvaluation>.Ok(new RecoveryGateEvaluation
                {
                    NeedsRecoveryGate = false,
                    ShouldDemote = false,
                    DemotionBlockedByProtection = false,
                    ProtectionDaysRemaining = 0
                });
            }

            if (consecutiveRecoveryCycleFailures < DemotionFailureThreshold)
            {
                return ServiceResult<RecoveryGateEvaluation>.Ok(new RecoveryGateEvaluation
                {
                    NeedsRecoveryGate = true,
                    ShouldDemote = false,
                    DemotionBlockedByProtection = false,
                    ProtectionDaysRemaining = 0
                });
            }

            if (daysSinceLastDemotion >= DemotionProtectionDays)
            {
                return ServiceResult<RecoveryGateEvaluation>.Ok(new RecoveryGateEvaluation
                {
                    NeedsRecoveryGate = true,
                    ShouldDemote = true,
                    DemotionBlockedByProtection = false,
                    ProtectionDaysRemaining = 0
                });
            }

            return ServiceResult<RecoveryGateEvaluation>.Ok(new RecoveryGateEvaluation
            {
                NeedsRecoveryGate = true,
                ShouldDemote = false,
                DemotionBlockedByProtection = true,
                ProtectionDaysRemaining = DemotionProtectionDays - daysSinceLastDemotion
            });
        }

        public ServiceResult<BossRecallGateEvaluation> EvaluateBossRecallGate(
            float activeRecallQuestionRatio,
            float activeRecallAccuracy,
            float requiredRecallRatio,
            float requiredRecallAccuracy)
        {
            if (activeRecallQuestionRatio < 0f || activeRecallQuestionRatio > 1f ||
                activeRecallAccuracy < 0f || activeRecallAccuracy > 1f ||
                requiredRecallRatio <= 0f || requiredRecallRatio > 1f ||
                requiredRecallAccuracy <= 0f || requiredRecallAccuracy > 1f)
            {
                return ServiceResult<BossRecallGateEvaluation>.Fail(ErrorCode.InvalidInput);
            }

            var meetsRatio = activeRecallQuestionRatio >= requiredRecallRatio;
            var meetsAccuracy = activeRecallAccuracy >= requiredRecallAccuracy;

            return ServiceResult<BossRecallGateEvaluation>.Ok(new BossRecallGateEvaluation
            {
                MeetsRecallRatio = meetsRatio,
                MeetsRecallAccuracy = meetsAccuracy,
                CanPassBossGate = meetsRatio && meetsAccuracy
            });
        }

        public ServiceResult<FinalMasteryGateEvaluation> EvaluateFinalMasteryGate(
            float masteryCoverageRate,
            int stableDaysAtHundredPercent)
        {
            if (masteryCoverageRate < 0f || masteryCoverageRate > 1f || stableDaysAtHundredPercent < 0)
            {
                return ServiceResult<FinalMasteryGateEvaluation>.Fail(ErrorCode.InvalidInput);
            }

            var mainClear = masteryCoverageRate >= MainClearCoverageRate;
            var trueClear = masteryCoverageRate >= TrueClearCoverageRate &&
                            stableDaysAtHundredPercent >= TrueClearStableDays;

            return ServiceResult<FinalMasteryGateEvaluation>.Ok(new FinalMasteryGateEvaluation
            {
                IsMainClearEligible = mainClear,
                IsTrueClearEligible = trueClear,
                RequiredMainClearCoverageRate = MainClearCoverageRate,
                RequiredTrueClearCoverageRate = TrueClearCoverageRate,
                RequiredStableDaysAtHundredPercent = TrueClearStableDays
            });
        }
    }
}
