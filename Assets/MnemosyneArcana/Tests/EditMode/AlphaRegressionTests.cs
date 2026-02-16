using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    public class AlphaRegressionTests
    {
        [Test]
        public void Ante1To8_AllBlindsPass_ReachesRunComplete()
        {
            var manager = new RunManagerV2(RunDifficultyProfile.Standard);
            manager.StartRun(20260216);

            var safety = 0;
            while (manager.CurrentState.Phase != RunPhase.RunComplete && safety < 32)
            {
                safety++;
                Assert.AreEqual(RunPhase.HandSelect, manager.CurrentState.Phase);

                var submit = manager.SubmitHandScore(manager.CurrentState.TargetScore);
                Assert.IsTrue(submit.IsSuccess);

                var resolve = manager.ResolveBlindResult();
                Assert.IsTrue(resolve.IsSuccess);
                Assert.IsTrue(resolve.Value.Passed);

                if (manager.CurrentState.Phase == RunPhase.RunComplete)
                {
                    break;
                }

                Assert.AreEqual(RunPhase.Shop, manager.CurrentState.Phase);
                var advance = manager.AdvanceAfterShop();
                Assert.IsTrue(advance.IsSuccess);
            }

            Assert.AreEqual(RunPhase.RunComplete, manager.CurrentState.Phase);
            Assert.LessOrEqual(safety, 24);
            Assert.AreEqual(8, manager.CurrentState.Ante);
            Assert.AreEqual(BlindType.Boss, manager.CurrentState.BlindType);
        }

        [Test]
        public void Ante3BigBlind_FailPath_EntersRunFail()
        {
            var manager = new RunManagerV2(RunDifficultyProfile.Standard);
            manager.StartRun(888);

            // 快速推進到 Ante3 Big
            while (!(manager.CurrentState.Ante == 3 && manager.CurrentState.BlindType == BlindType.Big))
            {
                manager.SubmitHandScore(manager.CurrentState.TargetScore);
                manager.ResolveBlindResult();
                if (manager.CurrentState.Phase == RunPhase.RunComplete)
                {
                    Assert.Fail("Unexpected RunComplete before Ante3 Big.");
                }

                manager.AdvanceAfterShop();
            }

            Assert.AreEqual(RunPhase.HandSelect, manager.CurrentState.Phase);
            manager.SubmitHandScore(0);
            manager.SubmitHandScore(0);
            manager.SubmitHandScore(0);
            manager.SubmitHandScore(0);

            var resolveFail = manager.ResolveBlindResult();
            Assert.IsTrue(resolveFail.IsSuccess);
            Assert.IsFalse(resolveFail.Value.Passed);
            Assert.AreEqual(RunPhase.RunFail, manager.CurrentState.Phase);
        }
    }
}
