using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    public class LearningManagerTests
    {
        [Test]
        public void ApplyAnswer_Lv0Correct_UsesLv0BehaviorAndLevelsUp()
        {
            var manager = new LearningManagerV2();
            var result = manager.ApplyAnswer("word_001", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Small,
                CurrentLevel = LearningLevel.Lv0
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsCorrect);
            Assert.AreEqual("4_choice_reading", result.Value.QuestionMode);
            Assert.AreEqual(3.0f, result.Value.TimeLimitSeconds);
            Assert.AreEqual(0.8f, result.Value.ChipMultiplier);
            Assert.AreEqual(LearningLevel.Lv1, result.Value.NextLevel);
        }

        [Test]
        public void ApplyAnswer_Lv3Wrong_AppliesPenaltyAndNoLevelUp()
        {
            var manager = new LearningManagerV2();
            var result = manager.ApplyAnswer("word_002", AnswerResult.Wrong, new RunContext
            {
                BlindType = BlindType.Big,
                CurrentLevel = LearningLevel.Lv3
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.IsCorrect);
            Assert.AreEqual(0.5f, result.Value.ChipMultiplier);
            Assert.AreEqual(-1, result.Value.HandMultDelta);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.NextLevel);
        }

        [Test]
        public void ApplyAnswer_Lv4Boss_UsesLv3Behavior()
        {
            var manager = new LearningManagerV2();
            var result = manager.ApplyAnswer("word_003", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv4
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.EffectiveLevel);
            Assert.AreEqual("spelling", result.Value.QuestionMode);
            Assert.IsFalse(result.Value.IsAutoResolved);
        }

        [Test]
        public void ApplyAnswer_GambleSuccess_TreatedAsCorrect()
        {
            var manager = new LearningManagerV2();
            var result = manager.ApplyAnswer("word_004", AnswerResult.GambleSuccess, new RunContext
            {
                BlindType = BlindType.Small,
                CurrentLevel = LearningLevel.Lv2
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsCorrect);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.NextLevel);
        }

        [Test]
        public void ApplyAnswer_EmptyWordId_ReturnsInvalidInput()
        {
            var manager = new LearningManagerV2();
            var result = manager.ApplyAnswer("", AnswerResult.Correct, new RunContext());

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }

        [Test]
        public void ResolveWrongAnswerChoice_AcceptLoss_KeepsMoneyAndPenalty()
        {
            var manager = new LearningManagerV2();
            var result = manager.ResolveWrongAnswerChoice(WrongAnswerChoice.AcceptLoss, 9, retryUsed: false, seed: 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(9, result.Value.RemainingMoney);
            Assert.AreEqual(AnswerResult.Wrong, result.Value.FinalAnswerResult);
            Assert.AreEqual(0.5f, result.Value.OverrideChipMultiplier);
        }

        [Test]
        public void ResolveWrongAnswerChoice_RetryWithCost_SpendsTwo()
        {
            var manager = new LearningManagerV2();
            var result = manager.ResolveWrongAnswerChoice(WrongAnswerChoice.RetryWithCost, 10, retryUsed: false, seed: 1);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(8, result.Value.RemainingMoney);
            Assert.IsTrue(result.Value.RetryConsumed);
            Assert.AreEqual(AnswerResult.RetryAccepted, result.Value.FinalAnswerResult);
        }

        [Test]
        public void ResolveWrongAnswerChoice_RetryUsed_ReturnsStateConflict()
        {
            var manager = new LearningManagerV2();
            var result = manager.ResolveWrongAnswerChoice(WrongAnswerChoice.RetryWithCost, 10, retryUsed: true, seed: 1);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void ResolveWrongAnswerChoice_Gamble_IsDeterministicBySeed()
        {
            var manager = new LearningManagerV2();
            var first = manager.ResolveWrongAnswerChoice(WrongAnswerChoice.Gamble, 5, retryUsed: false, seed: 1234);
            var second = manager.ResolveWrongAnswerChoice(WrongAnswerChoice.Gamble, 5, retryUsed: false, seed: 1234);

            Assert.IsTrue(first.IsSuccess);
            Assert.IsTrue(second.IsSuccess);
            Assert.AreEqual(first.Value.FinalAnswerResult, second.Value.FinalAnswerResult);
            Assert.AreEqual(first.Value.OverrideChipMultiplier, second.Value.OverrideChipMultiplier);
        }

        [Test]
        public void ApplyAnswer_WithFluEffects_AdjustsTimeAndWrongPenalty()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot
            {
                Lv1Lv2TimeBonusSec = 0.2f,
                ListeningTimeBonusSec = 0.2f,
                BossTimeBonusRate = 0.1f,
                WrongPenaltyReductionRate = 0.1f
            };

            var result = manager.ApplyAnswer("word_005", AnswerResult.Wrong, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv1
            }, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3.19f, result.Value.TimeLimitSeconds, 0.001f);
            Assert.AreEqual(0.45f, result.Value.ChipMultiplier, 0.0001f);
        }

        [Test]
        public void ResolveWrongAnswerChoice_RetryCostDiscountedByCurriculum()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { RetryCostDiscount = 1 };
            var result = manager.ResolveWrongAnswerChoice(WrongAnswerChoice.RetryWithCost, 10, retryUsed: false, seed: 1, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(9, result.Value.RemainingMoney);
            Assert.AreEqual(1, result.Value.MoneySpent);
        }

        [Test]
        public void ApplyAnswer_Lv4FirstWrong_WithMas08_IgnoresHandMultPenalty()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { IgnoreFirstLv4WrongHandMultPenalty = true };

            var result = manager.ApplyAnswer(
                "word_lv4",
                AnswerResult.Wrong,
                new RunContext { BlindType = BlindType.Boss, CurrentLevel = LearningLevel.Lv4 },
                effects,
                isFirstLv4WrongThisRun: true);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Value.HandMultDelta);
        }

        [Test]
        public void ResolveWrongAnswerChoice_WithFlu04_FirstWrongRetryIsFree()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { FreeRetryOnFirstWrongOption = true };

            var result = manager.ResolveWrongAnswerChoice(
                WrongAnswerChoice.RetryWithCost,
                currentMoney: 2,
                retryUsed: false,
                seed: 1,
                effects,
                isFirstWrongInRun: true);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Value.MoneySpent);
            Assert.AreEqual(2, result.Value.RemainingMoney);
        }

        [Test]
        public void GetConsecutiveWrongReliefThreshold_WithFlu05_ReducesThreshold()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { ConsecutiveWrongReliefThresholdDelta = 1 };

            var threshold = manager.GetConsecutiveWrongReliefThreshold(3, effects);

            Assert.AreEqual(2, threshold);
        }

        [Test]
        public void GetStreakBonusDurationTurns_WithFlu09_AddsOneTurn()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { StreakBonusDurationExtraTurns = 1 };

            var duration = manager.GetStreakBonusDurationTurns(1, effects);

            Assert.AreEqual(2, duration);
        }

        [Test]
        public void GetBossAllCorrectExtraLv4UpgradeCount_WithMas07_ReturnsOne()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { BossAllCorrectExtraLv4UpgradeCount = 1 };

            var count = manager.GetBossAllCorrectExtraLv4UpgradeCount(effects);

            Assert.AreEqual(1, count);
        }

        [Test]
        public void ApplyAnswer_Lv3Correct_WithFlu03B_IncreasesChipMultiplier()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { Lv3CorrectRewardBonusRate = 0.12f };

            var result = manager.ApplyAnswer("word_lv3", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Small,
                CurrentLevel = LearningLevel.Lv3
            }, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1.68f, result.Value.ChipMultiplier, 0.0001f);
        }

        [Test]
        public void ApplyAnswer_Lv4Wrong_WithMas03B_ReducesWrongPenalty()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { Lv4NegativeAffixResistanceRate = 0.10f };

            var result = manager.ApplyAnswer("word_lv4", AnswerResult.Wrong, new RunContext
            {
                BlindType = BlindType.Small,
                CurrentLevel = LearningLevel.Lv4
            }, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0.55f, result.Value.ChipMultiplier, 0.0001f);
        }

        [Test]
        public void FluAndBldRuntimeHelpers_ExposeExpectedValues()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot
            {
                Lv1Lv2EasyQuestionRateBonus = 0.08f,
                SpellingToleranceExtraLetters = 1,
                SpellingTolerancePerRunLimit = 3,
                IgnoreFirstLv4DemotionPerRun = true,
                FirstLv4UpgradeMoneyRefund = 2
            };

            Assert.AreEqual(0.08f, manager.GetEasyQuestionRateBonusForLv1Lv2(effects), 0.0001f);
            Assert.AreEqual(1, manager.GetSpellingToleranceExtraLetters(effects));
            Assert.AreEqual(3, manager.GetSpellingTolerancePerRunLimit(effects));
            Assert.IsTrue(manager.ShouldIgnoreFirstLv4Demotion(true, effects));
            Assert.AreEqual(2, manager.GetFirstLv4UpgradeMoneyRefund(true, effects));
            Assert.AreEqual(0, manager.GetFirstLv4UpgradeMoneyRefund(false, effects));
        }

        [Test]
        public void GetLv4NegativeAffixResistanceRate_WithMas03B_ReturnsConfiguredRate()
        {
            var manager = new LearningManagerV2();
            var effects = new CurriculumEffectSnapshot { Lv4NegativeAffixResistanceRate = 0.10f };

            Assert.AreEqual(0.10f, manager.GetLv4NegativeAffixResistanceRate(effects), 0.0001f);
        }
    }
}
