using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class LearningManagerV2 : ILearningService
    {
        private const int RetryCost = 2;

        public ServiceResult<LearningResult> ApplyAnswer(string wordId, AnswerResult answer, RunContext runContext)
        {
            return ApplyAnswer(wordId, answer, runContext, null, false);
        }

        public ServiceResult<LearningResult> ApplyAnswer(string wordId, AnswerResult answer, RunContext runContext, CurriculumEffectSnapshot effects)
        {
            return ApplyAnswer(wordId, answer, runContext, effects, false);
        }

        public ServiceResult<LearningResult> ApplyAnswer(
            string wordId,
            AnswerResult answer,
            RunContext runContext,
            CurriculumEffectSnapshot effects,
            bool isFirstLv4WrongThisRun)
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
            var (questionMode, baseTimeLimitSec, baseChipMultiplier, autoResolved) = GetBehaviorByLevel(effectiveLevel);
            var timeLimitSec = baseTimeLimitSec;
            if (effects != null)
            {
                if (effectiveLevel == LearningLevel.Lv1 || effectiveLevel == LearningLevel.Lv2)
                {
                    timeLimitSec += effects.Lv1Lv2TimeBonusSec;
                }

                if (effectiveLevel == LearningLevel.Lv2)
                {
                    timeLimitSec += effects.ListeningTimeBonusSec;
                }

                if (runContext.BlindType == BlindType.Boss)
                {
                    timeLimitSec *= (1f + effects.BossTimeBonusRate);
                }
            }

            var wrongChipMultiplier = 0.5f;
            if (effects != null)
            {
                wrongChipMultiplier = System.Math.Max(0f, wrongChipMultiplier * (1f - effects.WrongPenaltyReductionRate));
                if (runContext.CurrentLevel == LearningLevel.Lv4 && effects.Lv4NegativeAffixResistanceRate > 0f)
                {
                    wrongChipMultiplier = System.Math.Min(1f, wrongChipMultiplier * (1f + effects.Lv4NegativeAffixResistanceRate));
                }
            }

            var chipMultiplier = isWrong ? wrongChipMultiplier : baseChipMultiplier;
            if (isCorrect &&
                effects != null &&
                effectiveLevel == LearningLevel.Lv3 &&
                effects.Lv3CorrectRewardBonusRate > 0f)
            {
                chipMultiplier *= (1f + effects.Lv3CorrectRewardBonusRate);
            }

            var nextLevel = isCorrect ? LevelUp(runContext.CurrentLevel) : runContext.CurrentLevel;
            var handMultDelta = isWrong ? -1 : 0;
            if (isWrong &&
                effects != null &&
                effects.IgnoreFirstLv4WrongHandMultPenalty &&
                runContext.CurrentLevel == LearningLevel.Lv4 &&
                isFirstLv4WrongThisRun)
            {
                handMultDelta = 0;
            }

            var result = new LearningResult
            {
                IsCorrect = isCorrect,
                QuestionMode = questionMode,
                TimeLimitSeconds = timeLimitSec,
                ChipMultiplier = chipMultiplier,
                HandMultDelta = handMultDelta,
                NextLevel = nextLevel,
                EffectiveLevel = effectiveLevel,
                IsAutoResolved = autoResolved,
                DecayUpdated = isCorrect
            };

            return ServiceResult<LearningResult>.Ok(result);
        }

        public ServiceResult<WrongAnswerChoiceResult> ResolveWrongAnswerChoice(WrongAnswerChoice choice, int currentMoney, bool retryUsed, int seed)
        {
            return ResolveWrongAnswerChoice(choice, currentMoney, retryUsed, seed, null, false);
        }

        public ServiceResult<WrongAnswerChoiceResult> ResolveWrongAnswerChoice(WrongAnswerChoice choice, int currentMoney, bool retryUsed, int seed, CurriculumEffectSnapshot effects)
        {
            return ResolveWrongAnswerChoice(choice, currentMoney, retryUsed, seed, effects, false);
        }

        public ServiceResult<WrongAnswerChoiceResult> ResolveWrongAnswerChoice(
            WrongAnswerChoice choice,
            int currentMoney,
            bool retryUsed,
            int seed,
            CurriculumEffectSnapshot effects,
            bool isFirstWrongInRun)
        {
            if (currentMoney < 0)
            {
                return ServiceResult<WrongAnswerChoiceResult>.Fail(ErrorCode.InvalidInput);
            }

            var retryCost = System.Math.Max(1, RetryCost - (effects?.RetryCostDiscount ?? 0));

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
                    var useFreeRetry = effects != null &&
                                       effects.FreeRetryOnFirstWrongOption &&
                                       isFirstWrongInRun &&
                                       !retryUsed;
                    var effectiveRetryCost = useFreeRetry ? 0 : retryCost;

                    if (retryUsed || currentMoney < effectiveRetryCost)
                    {
                        return ServiceResult<WrongAnswerChoiceResult>.Fail(ErrorCode.StateConflict);
                    }

                    return ServiceResult<WrongAnswerChoiceResult>.Ok(new WrongAnswerChoiceResult
                    {
                        Choice = choice,
                        Accepted = true,
                        RetryConsumed = true,
                        MoneySpent = effectiveRetryCost,
                        RemainingMoney = currentMoney - effectiveRetryCost,
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

        public int GetConsecutiveWrongReliefThreshold(int baseThreshold, CurriculumEffectSnapshot effects)
        {
            var threshold = baseThreshold;
            if (effects != null)
            {
                threshold -= effects.ConsecutiveWrongReliefThresholdDelta;
            }

            return System.Math.Max(1, threshold);
        }

        public int GetStreakBonusDurationTurns(int baseDuration, CurriculumEffectSnapshot effects)
        {
            if (effects == null)
            {
                return System.Math.Max(0, baseDuration);
            }

            return System.Math.Max(0, baseDuration + effects.StreakBonusDurationExtraTurns);
        }

        public int GetBossAllCorrectExtraLv4UpgradeCount(CurriculumEffectSnapshot effects)
        {
            return System.Math.Max(0, effects?.BossAllCorrectExtraLv4UpgradeCount ?? 0);
        }

        public float GetEasyQuestionRateBonusForLv1Lv2(CurriculumEffectSnapshot effects)
        {
            return System.Math.Max(0f, effects?.Lv1Lv2EasyQuestionRateBonus ?? 0f);
        }

        public int GetSpellingToleranceExtraLetters(CurriculumEffectSnapshot effects)
        {
            return System.Math.Max(0, effects?.SpellingToleranceExtraLetters ?? 0);
        }

        public int GetSpellingTolerancePerRunLimit(CurriculumEffectSnapshot effects)
        {
            return System.Math.Max(0, effects?.SpellingTolerancePerRunLimit ?? 0);
        }

        public bool ShouldIgnoreFirstLv4Demotion(bool isFirstLv4DemotionInRun, CurriculumEffectSnapshot effects)
        {
            return isFirstLv4DemotionInRun && (effects?.IgnoreFirstLv4DemotionPerRun ?? false);
        }

        public int GetFirstLv4UpgradeMoneyRefund(bool isFirstLv4UpgradeThisRun, CurriculumEffectSnapshot effects)
        {
            if (!isFirstLv4UpgradeThisRun || effects == null)
            {
                return 0;
            }

            return System.Math.Max(0, effects.FirstLv4UpgradeMoneyRefund);
        }

        public float GetLv4NegativeAffixResistanceRate(CurriculumEffectSnapshot effects)
        {
            return System.Math.Max(0f, effects?.Lv4NegativeAffixResistanceRate ?? 0f);
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
