using System;
using System.Collections.Generic;
using NUnit.Framework;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class DecayManagerTests
    {
        private DecayManagerV2 _decay;

        [SetUp]
        public void SetUp()
        {
            _decay = new DecayManagerV2();
        }

        // TC-DECAY-001: Lv1 超過 1 天未練 → Lv0, Decayed
        [Test]
        public void Lv1_Over1Day_DecaysToLv0Decayed()
        {
            var word = new WordProgress
            {
                WordId = "apple",
                Level = LearningLevel.Lv1,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2026, 2, 10, 12, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 11, 12, 0, 1, DateTimeKind.Utc);

            var result = _decay.EvaluateDecay(word, now);

            Assert.IsTrue(result.Decayed);
            Assert.AreEqual(LearningLevel.Lv1, result.PreviousLevel);
            Assert.AreEqual(LearningLevel.Lv0, result.NewLevel);
            Assert.AreEqual(WordPool.Learning, result.PreviousPool);
            Assert.AreEqual(WordPool.Decayed, result.NewPool);
        }

        // TC-DECAY-002: Lv2 剛好 3 天 → Lv1, Decayed
        [Test]
        public void Lv2_Exactly3Days_DecaysToLv1Decayed()
        {
            var word = new WordProgress
            {
                WordId = "banana",
                Level = LearningLevel.Lv2,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc);

            var result = _decay.EvaluateDecay(word, now);

            Assert.IsTrue(result.Decayed);
            Assert.AreEqual(LearningLevel.Lv1, result.NewLevel);
            Assert.AreEqual(WordPool.Decayed, result.NewPool);
        }

        // TC-DECAY-003: Lv3 + 6 天未練 → 不退化
        [Test]
        public void Lv3_6Days_NoDecay()
        {
            var word = new WordProgress
            {
                WordId = "cherry",
                Level = LearningLevel.Lv3,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2026, 2, 8, 0, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc);

            var result = _decay.EvaluateDecay(word, now);

            Assert.IsFalse(result.Decayed);
            Assert.AreEqual(LearningLevel.Lv3, result.NewLevel);
            Assert.AreEqual(WordPool.Learning, result.NewPool);
        }

        // TC-DECAY-004: Lv4 超過 7 天 → Lv3, Learning (not Decayed)
        [Test]
        public void Lv4_Over7Days_DecaysToLv3Learning()
        {
            var word = new WordProgress
            {
                WordId = "dragon",
                Level = LearningLevel.Lv4,
                Pool = WordPool.Mastered,
                LastPracticed = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 8, 0, 0, 1, DateTimeKind.Utc);

            var result = _decay.EvaluateDecay(word, now);

            Assert.IsTrue(result.Decayed);
            Assert.AreEqual(LearningLevel.Lv4, result.PreviousLevel);
            Assert.AreEqual(LearningLevel.Lv3, result.NewLevel);
            Assert.AreEqual(WordPool.Mastered, result.PreviousPool);
            Assert.AreEqual(WordPool.Learning, result.NewPool);
        }

        // TC-DECAY-005: Lv0 永不退化
        [Test]
        public void Lv0_NeverDecays()
        {
            var word = new WordProgress
            {
                WordId = "egg",
                Level = LearningLevel.Lv0,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc);

            var result = _decay.EvaluateDecay(word, now);

            Assert.IsFalse(result.Decayed);
            Assert.AreEqual(LearningLevel.Lv0, result.NewLevel);
        }

        // TC-DECAY-006: 答對重設計時
        [Test]
        public void ResetDecayTimer_UpdatesLastPracticed()
        {
            var word = new WordProgress
            {
                WordId = "fish",
                Level = LearningLevel.Lv2,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2026, 2, 10, 0, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 14, 12, 0, 0, DateTimeKind.Utc);

            _decay.ResetDecayTimer(word, now);

            Assert.AreEqual(now, word.LastPracticed);
        }

        // TC-DECAY-007: 批次退化多詞，各自獨立判定
        [Test]
        public void EvaluateBatch_IndependentPerWord()
        {
            var words = new List<WordProgress>
            {
                new WordProgress
                {
                    WordId = "grape",
                    Level = LearningLevel.Lv1,
                    Pool = WordPool.Learning,
                    LastPracticed = new DateTime(2026, 2, 12, 0, 0, 0, DateTimeKind.Utc)
                },
                new WordProgress
                {
                    WordId = "honey",
                    Level = LearningLevel.Lv3,
                    Pool = WordPool.Learning,
                    LastPracticed = new DateTime(2026, 2, 13, 0, 0, 0, DateTimeKind.Utc)
                }
            };
            var now = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc);

            var results = _decay.EvaluateBatch(words, now);

            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results[0].Decayed);
            Assert.AreEqual(LearningLevel.Lv0, results[0].NewLevel);
            Assert.IsFalse(results[1].Decayed);
            Assert.AreEqual(LearningLevel.Lv3, results[1].NewLevel);
        }

        // Edge: Lv1 剛好未滿 1 天 → 不退化
        [Test]
        public void Lv1_JustUnder1Day_NoDecay()
        {
            var word = new WordProgress
            {
                WordId = "ice",
                Level = LearningLevel.Lv1,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2026, 2, 13, 0, 0, 1, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc);

            var result = _decay.EvaluateDecay(word, now);

            Assert.IsFalse(result.Decayed);
        }

        // Edge: Lv3 剛好 7 天 → 退化
        [Test]
        public void Lv3_Exactly7Days_Decays()
        {
            var word = new WordProgress
            {
                WordId = "jam",
                Level = LearningLevel.Lv3,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2026, 2, 7, 0, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 14, 0, 0, 0, DateTimeKind.Utc);

            var result = _decay.EvaluateDecay(word, now);

            Assert.IsTrue(result.Decayed);
            Assert.AreEqual(LearningLevel.Lv2, result.NewLevel);
            Assert.AreEqual(WordPool.Decayed, result.NewPool);
        }

        [Test]
        public void Lv4_WithMasteryDecayProtection_DoesNotDecay()
        {
            var word = new WordProgress
            {
                WordId = "keeper",
                Level = LearningLevel.Lv4,
                Pool = WordPool.Mastered,
                LastPracticed = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc)
            };
            var now = new DateTime(2026, 2, 20, 0, 0, 0, DateTimeKind.Utc);
            var effects = new CurriculumEffectSnapshot { Lv4DecayProtectionLayers = 1 };

            var result = _decay.EvaluateDecay(word, now, effects);

            Assert.IsFalse(result.Decayed);
            Assert.AreEqual(LearningLevel.Lv4, result.NewLevel);
            Assert.AreEqual(WordPool.Mastered, result.NewPool);
        }
    }
}
