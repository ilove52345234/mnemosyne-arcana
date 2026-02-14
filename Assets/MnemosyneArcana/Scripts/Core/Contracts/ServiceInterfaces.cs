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
}
