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
    }
}
