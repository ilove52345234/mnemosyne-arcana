using System.Collections.Generic;
using NUnit.Framework;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class BossLearningTests
    {
        private LearningManagerV2 _learning;

        [SetUp]
        public void SetUp()
        {
            _learning = new LearningManagerV2();
        }

        // TC-BOSS-001: Boss + Lv0 → effective Lv1
        [Test]
        public void Boss_Lv0_EffectiveLv1()
        {
            var result = _learning.ApplyAnswer("w1", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv0
            });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv1, result.Value.EffectiveLevel);
            Assert.AreEqual("2_choice_reading", result.Value.QuestionMode);
        }

        // TC-BOSS-002: Boss + Lv2 → effective Lv3
        [Test]
        public void Boss_Lv2_EffectiveLv3()
        {
            var result = _learning.ApplyAnswer("w2", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv2
            });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.EffectiveLevel);
            Assert.AreEqual("spelling", result.Value.QuestionMode);
        }

        // TC-BOSS-003: Boss + Lv3 → stays Lv3
        [Test]
        public void Boss_Lv3_StaysLv3()
        {
            var result = _learning.ApplyAnswer("w3", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv3
            });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.EffectiveLevel);
            Assert.AreEqual("spelling", result.Value.QuestionMode);
        }

        // TC-BOSS-004: Boss + Lv4 → effective Lv3 (regression)
        [Test]
        public void Boss_Lv4_EffectiveLv3()
        {
            var result = _learning.ApplyAnswer("w4", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv4
            });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.EffectiveLevel);
            Assert.AreEqual("spelling", result.Value.QuestionMode);
            Assert.IsFalse(result.Value.IsAutoResolved);
        }

        // TC-BOSS-009: Non-Boss → no level shift
        [Test]
        public void NonBoss_Lv0_StaysLv0()
        {
            var result = _learning.ApplyAnswer("w5", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Small,
                CurrentLevel = LearningLevel.Lv0
            });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv0, result.Value.EffectiveLevel);
            Assert.AreEqual("4_choice_reading", result.Value.QuestionMode);
        }

        // TC-BOSS-005: streak=3 → x2
        [Test]
        public void BossStreak_3Correct_ChipX2()
        {
            var bonus = _learning.GetBossStreakBonus(3);
            Assert.AreEqual(3, bonus.ConsecutiveCorrect);
            Assert.AreEqual(2.0f, bonus.ChipMultiplier);
        }

        // TC-BOSS-006: streak=2 → no bonus
        [Test]
        public void BossStreak_2Correct_NoBonus()
        {
            var bonus = _learning.GetBossStreakBonus(2);
            Assert.AreEqual(1.0f, bonus.ChipMultiplier);
        }

        // streak=6 → x2 again
        [Test]
        public void BossStreak_6Correct_ChipX2Again()
        {
            var bonus = _learning.GetBossStreakBonus(6);
            Assert.AreEqual(2.0f, bonus.ChipMultiplier);
        }

        // streak=4 → no bonus
        [Test]
        public void BossStreak_4Correct_NoBonus()
        {
            var bonus = _learning.GetBossStreakBonus(4);
            Assert.AreEqual(1.0f, bonus.ChipMultiplier);
        }

        // streak=0 → no bonus
        [Test]
        public void BossStreak_0_NoBonus()
        {
            var bonus = _learning.GetBossStreakBonus(0);
            Assert.AreEqual(1.0f, bonus.ChipMultiplier);
        }

        // TC-BOSS-007: Boss all correct → upgrade words
        [Test]
        public void BossAllCorrect_UpgradesPlayedWords()
        {
            var words = new List<WordProgress>
            {
                new WordProgress { WordId = "a", Level = LearningLevel.Lv1, Pool = WordPool.Learning },
                new WordProgress { WordId = "b", Level = LearningLevel.Lv2, Pool = WordPool.Learning }
            };
            var result = _learning.ApplyBossAllCorrectReward(words);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.AllCorrect);
            Assert.AreEqual(2, result.Value.UpgradedWords.Count);
            Assert.AreEqual(LearningLevel.Lv2, result.Value.UpgradedWords[0].ToLevel);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.UpgradedWords[1].ToLevel);
            Assert.AreEqual(0, result.Value.SkippedAtMax);
        }

        // TC-BOSS-008: Lv4 skipped
        [Test]
        public void BossAllCorrect_Lv4Skipped()
        {
            var words = new List<WordProgress>
            {
                new WordProgress { WordId = "a", Level = LearningLevel.Lv3, Pool = WordPool.Learning },
                new WordProgress { WordId = "b", Level = LearningLevel.Lv4, Pool = WordPool.Mastered }
            };
            var result = _learning.ApplyBossAllCorrectReward(words);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Value.UpgradedWords.Count);
            Assert.AreEqual("a", result.Value.UpgradedWords[0].WordId);
            Assert.AreEqual(LearningLevel.Lv4, result.Value.UpgradedWords[0].ToLevel);
            Assert.AreEqual(1, result.Value.SkippedAtMax);
        }

        // Empty list
        [Test]
        public void BossAllCorrect_EmptyList_NoUpgrades()
        {
            var result = _learning.ApplyBossAllCorrectReward(new List<WordProgress>());
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.AllCorrect);
            Assert.AreEqual(0, result.Value.UpgradedWords.Count);
        }

        // Null input
        [Test]
        public void BossAllCorrect_NullInput_ReturnsError()
        {
            var result = _learning.ApplyBossAllCorrectReward(null);
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }
    }
}
