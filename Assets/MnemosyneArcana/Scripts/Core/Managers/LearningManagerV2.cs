using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class LearningManagerV2 : ILearningService
    {
        private const int RetryCost = 2;

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

        public ServiceResult<WrongAnswerChoiceResult> ResolveWrongAnswerChoice(WrongAnswerChoice choice, int currentMoney, bool retryUsed, int seed)
        {
            if (currentMoney < 0)
            {
                return ServiceResult<WrongAnswerChoiceResult>.Fail(ErrorCode.InvalidInput);
            }

            switch (choice)
            {
                case WrongAnswerChoice.AcceptLoss:
                    return ServiceResult<WrongAnswerChoiceResult>.Ok(new WrongAnswerChoiceResult
                    {
                        Choice = choice,
                        Accepted = true,
                        RetryConsumed = retryUsed,
                        MoneySpent = 0,
                        RemainingMoney = currentMoney,
                        FinalAnswerResult = AnswerResult.Wrong,
                        OverrideChipMultiplier = 0.5f
                    });

                case WrongAnswerChoice.RetryWithCost:
                    if (retryUsed || currentMoney < RetryCost)
                    {
                        return ServiceResult<WrongAnswerChoiceResult>.Fail(ErrorCode.StateConflict);
                    }

                    return ServiceResult<WrongAnswerChoiceResult>.Ok(new WrongAnswerChoiceResult
                    {
                        Choice = choice,
                        Accepted = true,
                        RetryConsumed = true,
                        MoneySpent = RetryCost,
                        RemainingMoney = currentMoney - RetryCost,
                        FinalAnswerResult = AnswerResult.RetryAccepted,
                        OverrideChipMultiplier = 1.0f
                    });

                case WrongAnswerChoice.Gamble:
                    var random = new System.Random(seed);
                    var success = random.NextDouble() < 0.5;
                    return ServiceResult<WrongAnswerChoiceResult>.Ok(new WrongAnswerChoiceResult
                    {
                        Choice = choice,
                        Accepted = true,
                        RetryConsumed = retryUsed,
                        MoneySpent = 0,
                        RemainingMoney = currentMoney,
                        FinalAnswerResult = success ? AnswerResult.GambleSuccess : AnswerResult.GambleFailed,
                        OverrideChipMultiplier = success ? 1.0f : 0.0f
                    });

                default:
                    return ServiceResult<WrongAnswerChoiceResult>.Fail(ErrorCode.InvalidInput);
            }
        }

        public BossStreakBonus GetBossStreakBonus(int consecutiveCorrect)
        {
            var isStreakBonus = consecutiveCorrect > 0 && consecutiveCorrect % 3 == 0;
            return new BossStreakBonus
            {
                ConsecutiveCorrect = consecutiveCorrect,
                ChipMultiplier = isStreakBonus ? 2.0f : 1.0f
            };
        }

        public ServiceResult<BossRewardResult> ApplyBossAllCorrectReward(System.Collections.Generic.IReadOnlyList<WordProgress> playedWords)
        {
            if (playedWords == null)
            {
                return ServiceResult<BossRewardResult>.Fail(ErrorCode.InvalidInput);
            }

            var upgraded = new System.Collections.Generic.List<WordLevelUp>();
            var skippedAtMax = 0;

            for (var i = 0; i < playedWords.Count; i++)
            {
                var word = playedWords[i];
                if (word.Level == LearningLevel.Lv4)
                {
                    skippedAtMax++;
                    continue;
                }

                upgraded.Add(new WordLevelUp
                {
                    WordId = word.WordId,
                    FromLevel = word.Level,
                    ToLevel = LevelUp(word.Level)
                });
            }

            return ServiceResult<BossRewardResult>.Ok(new BossRewardResult
            {
                AllCorrect = true,
                UpgradedWords = upgraded,
                SkippedAtMax = skippedAtMax
            });
        }

        private static LearningLevel GetEffectiveLevel(LearningLevel level, BlindType blindType)
        {
            if (blindType != BlindType.Boss)
            {
                return level;
            }

            return level switch
            {
                LearningLevel.Lv4 => LearningLevel.Lv3,
                LearningLevel.Lv3 => LearningLevel.Lv3,
                LearningLevel.Lv2 => LearningLevel.Lv3,
                LearningLevel.Lv1 => LearningLevel.Lv2,
                LearningLevel.Lv0 => LearningLevel.Lv1,
                _ => level
            };
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
