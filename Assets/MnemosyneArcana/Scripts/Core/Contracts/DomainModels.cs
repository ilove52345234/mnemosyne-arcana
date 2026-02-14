using System;
using System.Collections.Generic;

namespace MnemosyneArcana.Core.Contracts
{
    public enum Element { Life, Force, Mind, Matter, Abstract }
    public enum PartOfSpeech { N, V, A, D }
    public enum LearningLevel { Lv0, Lv1, Lv2, Lv3, Lv4 }
    public enum BlindType { Small, Big, Boss }
    public enum HandType
    {
        Word,
        PoSPair,
        ElemPair,
        PoSTriple,
        GrammarChain,
        ElemTriple,
        FullHouse,
        ElemFlush,
        PoSFlush,
        GrammarFlush
    }

    public enum AnswerResult { Correct, Wrong, RetryAccepted, GambleSuccess, GambleFailed }
    public enum ErrorCode { None, InvalidInput, ConfigMissing, StateConflict, PersistenceFailed, MigrationFailed, NotImplemented }
    public enum ShopOfferCategory { Sense, Material, Affix, Course }
    public enum WrongAnswerChoice { AcceptLoss, RetryWithCost, Gamble }

    public sealed class PlayedCard
    {
        public string WordId { get; set; } = string.Empty;
        public Element Element { get; set; }
        public PartOfSpeech PartOfSpeech { get; set; }
        public int BaseChips { get; set; }
        public LearningLevel LearningLevel { get; set; }
        public float ChipMultiplier { get; set; } = 1f;
        public bool IsAnswerWrong { get; set; }
        public IReadOnlyList<string> VersionTags { get; set; } = Array.Empty<string>();
    }

    public sealed class RunModifiers
    {
        public int HandUpgradeLevel { get; set; }
        public float AdditiveMultTotal { get; set; }
        public int HandMultDelta { get; set; }
        public IReadOnlyList<float> MultiplicativeFactors { get; set; } = Array.Empty<float>();
    }

    public sealed class RunContext
    {
        public int Ante { get; set; }
        public BlindType BlindType { get; set; }
        public int PlaysLeft { get; set; }
        public int DiscardsLeft { get; set; }
        public LearningLevel CurrentLevel { get; set; } = LearningLevel.Lv0;
        public int ConsecutiveWrongCount { get; set; }
    }

    public sealed class ScoreBreakdown
    {
        public HandType HandType { get; set; } = HandType.Word;
        public int BaseHandChips { get; set; }
        public int UpgradedHandChips { get; set; }
        public int CardChipsTotal { get; set; }
        public int BaseHandMult { get; set; }
        public int UpgradedHandMult { get; set; }
        public float AdditiveMultTotal { get; set; }
        public int WrongAnswers { get; set; }
        public int EffectiveHandMult { get; set; }
        public IReadOnlyList<float> MultiplicativeFactors { get; set; } = Array.Empty<float>();
        public int FinalScore { get; set; }
    }

    public sealed class LearningResult
    {
        public bool IsCorrect { get; set; }
        public string QuestionMode { get; set; } = string.Empty;
        public float TimeLimitSeconds { get; set; }
        public float ChipMultiplier { get; set; } = 1f;
        public int HandMultDelta { get; set; }
        public LearningLevel NextLevel { get; set; } = LearningLevel.Lv0;
        public LearningLevel EffectiveLevel { get; set; } = LearningLevel.Lv0;
        public bool IsAutoResolved { get; set; }
        public bool DecayUpdated { get; set; }
    }

    public sealed class WrongAnswerChoiceResult
    {
        public WrongAnswerChoice Choice { get; set; }
        public bool Accepted { get; set; }
        public bool RetryConsumed { get; set; }
        public int MoneySpent { get; set; }
        public int RemainingMoney { get; set; }
        public AnswerResult FinalAnswerResult { get; set; } = AnswerResult.Wrong;
        public float OverrideChipMultiplier { get; set; } = 0.5f;
    }

    public sealed class Contract
    {
        public string ContractId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ContractType { get; set; } = string.Empty;
        public int Tier { get; set; } = 1;
        public int LpReward { get; set; }
    }

    public sealed class ShopOffer
    {
        public string OfferId { get; set; } = string.Empty;
        public ShopOfferCategory Category { get; set; }
        public int Price { get; set; }
        public int Weight { get; set; }
    }

    public sealed class PurchaseResult
    {
        public bool Success { get; set; }
        public int Cost { get; set; }
        public int RemainingMoney { get; set; }
        public string OfferId { get; set; } = string.Empty;
        public ErrorCode Error { get; set; } = ErrorCode.None;
    }

    public sealed class RunTelemetry
    {
        public int TotalHandsPlayed { get; set; }
        public int TotalWrongAnswers { get; set; }
        public bool ContractCompleted { get; set; }
    }

    public sealed class ContractSettlement
    {
        public string ContractId { get; set; } = string.Empty;
        public bool Completed { get; set; }
        public int LpBonusRaw { get; set; }
        public int LpBonusCapped { get; set; }
        public bool CapApplied { get; set; }
    }

    public sealed class RunResult
    {
        public bool IsClear { get; set; }
        public int HighestAnte { get; set; }
        public int ScoreTotal { get; set; }
    }

    public sealed class MetaProgress
    {
        public int SaveVersion { get; set; } = 2;
        public int PlayerLevel { get; set; }
        public int Xp { get; set; }
        public int Lp { get; set; }
        public int HighestStake { get; set; }
        public IReadOnlyList<string> UnlockedLexiconTiers { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> CurriculumNodes { get; set; } = Array.Empty<string>();
    }

    public sealed class MetaSettlement
    {
        public int XpGained { get; set; }
        public int LpGainedBase { get; set; }
        public int LpGainedContract { get; set; }
        public int LpGainedTotal { get; set; }
        public IReadOnlyList<string> UnlockedNodes { get; set; } = Array.Empty<string>();
        public IReadOnlyList<string> UnlockedLexiconTiers { get; set; } = Array.Empty<string>();
    }

    public sealed class UnlockResult
    {
        public bool Success { get; set; }
        public string NodeId { get; set; } = string.Empty;
        public int SpentLp { get; set; }
        public int RemainingLp { get; set; }
        public ErrorCode Error { get; set; } = ErrorCode.None;
        public IReadOnlyList<string> UnlockedNodes { get; set; } = Array.Empty<string>();
    }

    public enum WordPool { Locked, Discoverable, Learning, Mastered, Decayed }

    public sealed class WordProgress
    {
        public string WordId { get; set; } = string.Empty;
        public LearningLevel Level { get; set; } = LearningLevel.Lv0;
        public WordPool Pool { get; set; } = WordPool.Discoverable;
        public DateTime LastPracticed { get; set; } = DateTime.MinValue;
    }

    public sealed class DecayResult
    {
        public string WordId { get; set; } = string.Empty;
        public bool Decayed { get; set; }
        public LearningLevel PreviousLevel { get; set; }
        public LearningLevel NewLevel { get; set; }
        public WordPool PreviousPool { get; set; }
        public WordPool NewPool { get; set; }
    }

    public sealed class BossStreakBonus
    {
        public int ConsecutiveCorrect { get; set; }
        public float ChipMultiplier { get; set; } = 1.0f;
    }

    public sealed class WordLevelUp
    {
        public string WordId { get; set; } = string.Empty;
        public LearningLevel FromLevel { get; set; }
        public LearningLevel ToLevel { get; set; }
    }

    public sealed class BossRewardResult
    {
        public bool AllCorrect { get; set; }
        public IReadOnlyList<WordLevelUp> UpgradedWords { get; set; } = Array.Empty<WordLevelUp>();
        public int SkippedAtMax { get; set; }
    }

    public sealed class ServiceResult<T>
    {
        private ServiceResult(bool isSuccess, T value, ErrorCode error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public bool IsSuccess { get; }
        public T Value { get; }
        public ErrorCode Error { get; }

        public static ServiceResult<T> Ok(T value) => new ServiceResult<T>(true, value, ErrorCode.None);
        public static ServiceResult<T> Fail(ErrorCode error) => new ServiceResult<T>(false, default, error);
    }
}
