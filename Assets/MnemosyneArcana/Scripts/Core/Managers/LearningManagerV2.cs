using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class LearningManagerV2 : ILearningService
    {
        public ServiceResult<LearningResult> ApplyAnswer(string wordId, AnswerResult answer, RunContext runContext)
        {
            if (string.IsNullOrWhiteSpace(wordId) || runContext == null)
            {
                return ServiceResult<LearningResult>.Fail(ErrorCode.InvalidInput);
            }

            var isCorrect = answer == AnswerResult.Correct ||
                            answer == AnswerResult.RetryAccepted ||
                            answer == AnswerResult.GambleSuccess;
            var isWrong = answer == AnswerResult.Wrong || answer == AnswerResult.GambleFailed;

            var effectiveLevel = GetEffectiveLevel(runContext.CurrentLevel, runContext.BlindType);
            var (questionMode, timeLimitSec, baseChipMultiplier, autoResolved) = GetBehaviorByLevel(effectiveLevel);
            var chipMultiplier = isWrong ? 0.5f : baseChipMultiplier;
            var nextLevel = isCorrect ? LevelUp(runContext.CurrentLevel) : runContext.CurrentLevel;

            var result = new LearningResult
            {
                IsCorrect = isCorrect,
                QuestionMode = questionMode,
                TimeLimitSeconds = timeLimitSec,
                ChipMultiplier = chipMultiplier,
                HandMultDelta = isWrong ? -1 : 0,
                NextLevel = nextLevel,
                EffectiveLevel = effectiveLevel,
                IsAutoResolved = autoResolved,
                DecayUpdated = isCorrect
            };

            return ServiceResult<LearningResult>.Ok(result);
        }

        private static LearningLevel GetEffectiveLevel(LearningLevel level, BlindType blindType)
        {
            if (blindType == BlindType.Boss && level == LearningLevel.Lv4)
            {
                return LearningLevel.Lv3;
            }

            return level;
        }

        private static (string QuestionMode, float TimeLimitSeconds, float ChipMultiplier, bool IsAutoResolved) GetBehaviorByLevel(LearningLevel level)
        {
            return level switch
            {
                LearningLevel.Lv0 => ("4_choice_reading", 3.0f, 0.8f, false),
                LearningLevel.Lv1 => ("2_choice_reading", 2.0f, 1.0f, false),
                LearningLevel.Lv2 => ("2_choice_listening", 2.5f, 1.2f, false),
                LearningLevel.Lv3 => ("spelling", 4.0f, 1.5f, false),
                LearningLevel.Lv4 => ("auto", 0.0f, 1.5f, true),
                _ => ("4_choice_reading", 3.0f, 0.8f, false)
            };
        }

        private static LearningLevel LevelUp(LearningLevel current)
        {
            return current switch
            {
                LearningLevel.Lv0 => LearningLevel.Lv1,
                LearningLevel.Lv1 => LearningLevel.Lv2,
                LearningLevel.Lv2 => LearningLevel.Lv3,
                LearningLevel.Lv3 => LearningLevel.Lv4,
                _ => LearningLevel.Lv4
            };
        }
    }
}
