using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    public class RunFlowTests
    {
        [Test]
        public void StartRun_InitializesAnte1SmallBlind()
        {
            var manager = new RunManagerV2();
            manager.StartRun(2026);

            Assert.AreEqual(2026, manager.CurrentState.Seed);
            Assert.AreEqual(1, manager.CurrentState.Ante);
            Assert.AreEqual(BlindType.Small, manager.CurrentState.BlindType);
            Assert.AreEqual(100, manager.CurrentState.TargetScore);
            Assert.AreEqual(4, manager.CurrentState.PlaysLeft);
            Assert.AreEqual(RunPhase.HandSelect, manager.CurrentState.Phase);
        }

        [Test]
        public void ResolveBlindResult_WhenPass_EntersShop()
        {
            var manager = new RunManagerV2();
            manager.StartRun(1);
            manager.SubmitHandScore(100);

            var result = manager.ResolveBlindResult();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Passed);
            Assert.AreEqual(RunPhase.Shop, manager.CurrentState.Phase);
        }

        [Test]
        public void AdvanceAfterShop_SmallBlindToBigBlind()
        {
            var manager = new RunManagerV2();
            manager.StartRun(1);
            manager.SubmitHandScore(100);
            manager.ResolveBlindResult();

            var advance = manager.AdvanceAfterShop();

            Assert.IsTrue(advance.IsSuccess);
            Assert.AreEqual(BlindType.Big, manager.CurrentState.BlindType);
            Assert.AreEqual(150, manager.CurrentState.TargetScore);
            Assert.AreEqual(0, manager.CurrentState.CurrentScore);
            Assert.AreEqual(4, manager.CurrentState.PlaysLeft);
            Assert.AreEqual(RunPhase.HandSelect, manager.CurrentState.Phase);
        }

        [Test]
        public void ResolveBlindResult_WhenFail_EndsRun()
        {
            var manager = new RunManagerV2();
            manager.StartRun(2);
            manager.SubmitHandScore(10);
            manager.SubmitHandScore(10);
            manager.SubmitHandScore(10);
            manager.SubmitHandScore(10);

            var result = manager.ResolveBlindResult();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.Passed);
            Assert.AreEqual(RunPhase.RunFail, manager.CurrentState.Phase);
        }

        [Test]
        public void ResolveBlindResult_BossAnte8Pass_CompletesRun()
        {
            var manager = new RunManagerV2();
            manager.StartRun(3);
            manager.CurrentState.Ante = 8;
            manager.CurrentState.BlindType = BlindType.Boss;
            manager.CurrentState.TargetScore = 100000;
            manager.CurrentState.CurrentScore = 100000;
            manager.CurrentState.Phase = RunPhase.BlindResult;

            var result = manager.ResolveBlindResult();

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Passed);
            Assert.AreEqual(RunPhase.RunComplete, manager.CurrentState.Phase);
        }

        [Test]
        public void SubmitHandScore_NegativeScore_ReturnsInvalidInput()
        {
            var manager = new RunManagerV2();
            manager.StartRun(7);

            var result = manager.SubmitHandScore(-1);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }

        [Test]
        public void StartRun_StandardProfile_UsesSotBaseline()
        {
            var manager = new RunManagerV2(RunDifficultyProfile.Standard);
            manager.StartRun(99);
            Assert.AreEqual(100, manager.CurrentState.TargetScore);
        }

        [Test]
        public void StartRun_RelaxedProfile_HasLowerTargetThanStandard()
        {
            var standard = new RunManagerV2(RunDifficultyProfile.Standard);
            standard.StartRun(99);

            var relaxed = new RunManagerV2(RunDifficultyProfile.Relaxed);
            relaxed.StartRun(99);

            Assert.Less(relaxed.CurrentState.TargetScore, standard.CurrentState.TargetScore);
        }

        [Test]
        public void StartRun_ChallengingProfile_HasHigherTargetThanStandard()
        {
            var standard = new RunManagerV2(RunDifficultyProfile.Standard);
            standard.StartRun(99);

            var challenging = new RunManagerV2(RunDifficultyProfile.Challenging);
            challenging.StartRun(99);

            Assert.Greater(challenging.CurrentState.TargetScore, standard.CurrentState.TargetScore);
        }
    }
}
