using System;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using MnemosyneArcana.Prototype;
using NUnit.Framework;
using UnityEngine;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class UserStoryAcceptanceTests
    {
        [TearDown]
        public void TearDown()
        {
            var ui = UnityEngine.Object.FindObjectOfType<PrototypeCardGameUiController>();
            if (ui != null)
            {
                UnityEngine.Object.DestroyImmediate(ui.gameObject);
            }
        }

        [Test]
        public void US01_PlayStartsWithPrototypeUi()
        {
            var created = PrototypePlayModeBootstrap.EnsurePrototypeUiForCurrentScene();
            Assert.IsTrue(created);

            var ui = UnityEngine.Object.FindObjectOfType<PrototypeCardGameUiController>();
            Assert.IsNotNull(ui);

            var createdAgain = PrototypePlayModeBootstrap.EnsurePrototypeUiForCurrentScene();
            Assert.IsFalse(createdAgain);
        }

        [Test]
        public void US02_SingleHandCanScoreAndMoveToBlindResult()
        {
            var run = new RunManagerV2();
            run.StartRun(42);

            var submit = run.SubmitHandScore(run.CurrentState.TargetScore);
            Assert.IsTrue(submit.IsSuccess);
            Assert.AreEqual(RunPhase.BlindResult, run.CurrentState.Phase);
        }

        [Test]
        public void US03_BlindCanPassOrFail()
        {
            var passRun = new RunManagerV2();
            passRun.StartRun(100);
            passRun.SubmitHandScore(passRun.CurrentState.TargetScore);
            var pass = passRun.ResolveBlindResult();
            Assert.IsTrue(pass.IsSuccess);
            Assert.IsTrue(pass.Value.Passed);
            Assert.AreEqual(RunPhase.Shop, passRun.CurrentState.Phase);

            var failRun = new RunManagerV2();
            failRun.StartRun(101);
            failRun.SubmitHandScore(0);
            failRun.SubmitHandScore(0);
            failRun.SubmitHandScore(0);
            failRun.SubmitHandScore(0);
            var fail = failRun.ResolveBlindResult();
            Assert.IsTrue(fail.IsSuccess);
            Assert.IsFalse(fail.Value.Passed);
            Assert.AreEqual(RunPhase.RunFail, failRun.CurrentState.Phase);
        }

        [Test]
        public void US04_ShopCanGenerateAndPurchaseWithBalanceGuard()
        {
            var shop = new ShopManagerV2();
            var offers = shop.GenerateOffers(ante: 1, seed: 2026, isBossShop: false);
            Assert.IsTrue(offers.IsSuccess);
            Assert.IsNotEmpty(offers.Value);

            var first = offers.Value[0];
            var success = shop.PurchaseOffer(first, first.Price);
            Assert.IsTrue(success.IsSuccess);
            Assert.IsTrue(success.Value.Success);
            Assert.AreEqual(0, success.Value.RemainingMoney);

            var failed = shop.PurchaseOffer(first, Math.Max(0, first.Price - 1));
            Assert.IsTrue(failed.IsSuccess);
            Assert.IsFalse(failed.Value.Success);
            Assert.AreEqual(ErrorCode.StateConflict, failed.Value.Error);
        }

        [Test]
        public void US05_CanCompleteFullAnte1To8Run()
        {
            var run = new RunManagerV2(RunDifficultyProfile.Standard);
            run.StartRun(8888);

            var guard = 0;
            while (run.CurrentState.Phase != RunPhase.RunComplete && guard < 64)
            {
                guard++;
                var submit = run.SubmitHandScore(run.CurrentState.TargetScore);
                Assert.IsTrue(submit.IsSuccess);
                var resolve = run.ResolveBlindResult();
                Assert.IsTrue(resolve.IsSuccess);
                Assert.IsTrue(resolve.Value.Passed);

                if (run.CurrentState.Phase != RunPhase.RunComplete)
                {
                    var advance = run.AdvanceAfterShop();
                    Assert.IsTrue(advance.IsSuccess);
                }
            }

            Assert.AreEqual(RunPhase.RunComplete, run.CurrentState.Phase);
        }

        [Test]
        public void US06_MetaSettlementAndContractCapAreValid()
        {
            var meta = new MetaManagerV2();
            var settled = meta.SettleRun(new RunResult
            {
                IsClear = true,
                HighestAnte = 8,
                ScoreTotal = 120000
            }, new MetaProgress());
            Assert.IsTrue(settled.IsSuccess);
            Assert.Greater(settled.Value.XpGained, 0);
            Assert.Greater(settled.Value.LpGainedBase, 0);

            var contracts = meta.GenerateContracts(new MetaProgress(), seed: 77);
            Assert.IsTrue(contracts.IsSuccess);
            Assert.AreEqual(3, contracts.Value.Count);

            var contractSettlement = meta.SettleContractWithCap(
                contracts.Value[0],
                new RunTelemetry { ContractCompleted = true },
                settled.Value.LpGainedBase);
            Assert.IsTrue(contractSettlement.IsSuccess);

            var total = settled.Value.LpGainedBase + contractSettlement.Value.LpBonusCapped;
            var ratio = total == 0 ? 0f : contractSettlement.Value.LpBonusCapped / (float)total;
            Assert.LessOrEqual(ratio, 0.45f + 0.0001f);
        }

        [Test]
        public void US07_CanRestartRunAfterFailure()
        {
            var run = new RunManagerV2(RunDifficultyProfile.Standard);
            run.StartRun(501);

            run.SubmitHandScore(0);
            run.SubmitHandScore(0);
            run.SubmitHandScore(0);
            run.SubmitHandScore(0);
            var failed = run.ResolveBlindResult();
            Assert.IsTrue(failed.IsSuccess);
            Assert.AreEqual(RunPhase.RunFail, run.CurrentState.Phase);

            run.StartRun(502);
            Assert.AreEqual(RunPhase.HandSelect, run.CurrentState.Phase);
            Assert.AreEqual(1, run.CurrentState.Ante);
            Assert.AreEqual(BlindType.Small, run.CurrentState.BlindType);
            Assert.AreEqual(0, run.CurrentState.CurrentScore);
            Assert.AreEqual(4, run.CurrentState.PlaysLeft);
        }

        [Test]
        public void US08_BossShopAlwaysOffersTwoCoursesAtPrice10()
        {
            var shop = new ShopManagerV2();
            var offers = shop.GenerateOffers(ante: 4, seed: 9999, isBossShop: true);
            Assert.IsTrue(offers.IsSuccess);
            Assert.AreEqual(2, offers.Value.Count);
            Assert.AreEqual(ShopOfferCategory.Course, offers.Value[0].Category);
            Assert.AreEqual(ShopOfferCategory.Course, offers.Value[1].Category);
            Assert.AreEqual(10, offers.Value[0].Price);
            Assert.AreEqual(10, offers.Value[1].Price);
        }

        [Test]
        public void US09_WrongAnswerThreeChoicesWorkAsDesigned()
        {
            var learning = new LearningManagerV2();

            var accept = learning.ResolveWrongAnswerChoice(
                WrongAnswerChoice.AcceptLoss,
                currentMoney: 5,
                retryUsed: false,
                seed: 1);
            Assert.IsTrue(accept.IsSuccess);
            Assert.AreEqual(AnswerResult.Wrong, accept.Value.FinalAnswerResult);
            Assert.AreEqual(5, accept.Value.RemainingMoney);
            Assert.AreEqual(0.5f, accept.Value.OverrideChipMultiplier, 0.0001f);

            var retry = learning.ResolveWrongAnswerChoice(
                WrongAnswerChoice.RetryWithCost,
                currentMoney: 5,
                retryUsed: false,
                seed: 2);
            Assert.IsTrue(retry.IsSuccess);
            Assert.AreEqual(AnswerResult.RetryAccepted, retry.Value.FinalAnswerResult);
            Assert.AreEqual(3, retry.Value.RemainingMoney);
            Assert.IsTrue(retry.Value.RetryConsumed);

            var retryAgain = learning.ResolveWrongAnswerChoice(
                WrongAnswerChoice.RetryWithCost,
                currentMoney: 3,
                retryUsed: true,
                seed: 3);
            Assert.IsFalse(retryAgain.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, retryAgain.Error);
        }
    }
}
