using System;
using System.Collections.Generic;

namespace MnemosyneArcana.Core.Contracts
{
    public interface IScoringService
    {
        ServiceResult<ScoreBreakdown> EvaluateHand(IReadOnlyList<PlayedCard> cards, RunModifiers modifiers);
    }

    public interface ILearningService
    {
        ServiceResult<LearningResult> ApplyAnswer(string wordId, AnswerResult answer, RunContext runContext);
        ServiceResult<WrongAnswerChoiceResult> ResolveWrongAnswerChoice(WrongAnswerChoice choice, int currentMoney, bool retryUsed, int seed);
        BossStreakBonus GetBossStreakBonus(int consecutiveCorrect);
        ServiceResult<BossRewardResult> ApplyBossAllCorrectReward(IReadOnlyList<WordProgress> playedWords);
    }

    public interface IContractService
    {
        ServiceResult<IReadOnlyList<Contract>> GenerateContracts(MetaProgress meta, int seed);
        ServiceResult<ContractSettlement> SettleContract(Contract contract, RunTelemetry telemetry);
        ServiceResult<ContractSettlement> SettleContractWithCap(Contract contract, RunTelemetry telemetry, int lpBase);
    }

    public interface IMetaProgressService
    {
        ServiceResult<MetaSettlement> SettleRun(RunResult runResult, MetaProgress current);
        ServiceResult<UnlockResult> TryUnlockNode(string nodeId, MetaProgress current);
    }

    public interface IDecayService
    {
        DecayResult EvaluateDecay(WordProgress word, DateTime now);
        IReadOnlyList<DecayResult> EvaluateBatch(IReadOnlyList<WordProgress> words, DateTime now);
        void ResetDecayTimer(WordProgress word, DateTime now);
    }

    public interface IGateProgressionService
    {
        ServiceResult<GateProgressionEvaluation> EvaluateProgress(
            int learnedCount,
            float retentionRate,
            float retrievalRate,
            int currentModelIndex);

        ServiceResult<RecoveryGateEvaluation> EvaluateRecoveryGate(
            float coreCoverageRate,
            float requiredCoverageRate,
            int consecutiveRecoveryCycleFailures,
            int daysSinceLastDemotion);

        ServiceResult<BossRecallGateEvaluation> EvaluateBossRecallGate(
            float activeRecallQuestionRatio,
            float activeRecallAccuracy,
            float requiredRecallRatio,
            float requiredRecallAccuracy);

        ServiceResult<FinalMasteryGateEvaluation> EvaluateFinalMasteryGate(
            float masteryCoverageRate,
            int stableDaysAtHundredPercent);
    }

    public interface ILearningTelemetryService
    {
        ServiceResult<IReadOnlyList<TelemetryAlert>> EvaluateAlerts(LearningTelemetrySnapshot snapshot);
    }
}
