using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    public class ManagerStubTests
    {
        [Test]
        public void RunManager_StartRun_SetsSeed()
        {
            var manager = new RunManagerV2();
            manager.StartRun(1234);
            Assert.AreEqual(1234, manager.CurrentState.Seed);
        }

        [Test]
        public void ScoringManager_Stub_ReturnsNotImplemented()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(new PlayedCard[0], new RunModifiers());
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.NotImplemented, result.Error);
        }

        [Test]
        public void LearningManager_Stub_ReturnsNotImplemented()
        {
            var manager = new LearningManagerV2();
            var result = manager.ApplyAnswer("word_001", AnswerResult.Correct, new RunContext());
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.NotImplemented, result.Error);
        }
    }
}
