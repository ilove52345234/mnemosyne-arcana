using System;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class PlayableLoopUseCaseTests
    {
        [Test]
        public void UseCase_FirstBlindToShopPurchaseAndAdvance_Works()
        {
            var run = new RunManagerV2(RunDifficultyProfile.Standard);
            var shop = new ShopManagerV2();
            var scoring = new ScoringManagerV2();
            var learning = new LearningManagerV2();

            run.StartRun(20260217);
            Assert.AreEqual(RunPhase.HandSelect, run.CurrentState.Phase);
            Assert.AreEqual(BlindType.Small, run.CurrentState.BlindType);

            var learn = learning.ApplyAnswer("resonance", AnswerResult.Correct, new RunContext
            {
                Ante = run.CurrentState.Ante,
                BlindType = run.CurrentState.BlindType,
                CurrentLevel = LearningLevel.Lv2,
                PlaysLeft = run.CurrentState.PlaysLeft,
                DiscardsLeft = run.CurrentState.DiscardsLeft
            });
            Assert.IsTrue(learn.IsSuccess);
            Assert.IsTrue(learn.Value.IsCorrect);

            var score = scoring.EvaluateHand(new[]
            {
                new PlayedCard
                {
                    WordId = "resonance",
                    Element = Element.Abstract,
                    PartOfSpeech = PartOfSpeech.N,
                    BaseChips = 8,
                    LearningLevel = LearningLevel.Lv2,
                    ChipMultiplier = learn.Value.ChipMultiplier
                },
                new PlayedCard
                {
                    WordId = "cascade",
                    Element = Element.Force,
                    PartOfSpeech = PartOfSpeech.V,
                    BaseChips = 8,
                    LearningLevel = LearningLevel.Lv2,
                    ChipMultiplier = 1.0f
                }
            }, new RunModifiers());
            Assert.IsTrue(score.IsSuccess);

            var handScore = Math.Max(score.Value.FinalScore, run.CurrentState.TargetScore);
            var submit = run.SubmitHandScore(handScore);
            Assert.IsTrue(submit.IsSuccess);
            Assert.AreEqual(RunPhase.BlindResult, run.CurrentState.Phase);

            var resolve = run.ResolveBlindResult();
            Assert.IsTrue(resolve.IsSuccess);
            Assert.IsTrue(resolve.Value.Passed);
            Assert.AreEqual(RunPhase.Shop, run.CurrentState.Phase);

            var offers = shop.GenerateOffers(run.CurrentState.Ante, seed: 1234, isBossShop: false);
            Assert.IsTrue(offers.IsSuccess);
            Assert.IsNotEmpty(offers.Value);

            var first = offers.Value[0];
            var purchase = shop.PurchaseOffer(first, run.CurrentState.Money);
            Assert.IsTrue(purchase.IsSuccess);
            Assert.IsTrue(purchase.Value.Success);
            run.CurrentState.Money = purchase.Value.RemainingMoney;

            var advance = run.AdvanceAfterShop();
            Assert.IsTrue(advance.IsSuccess);
            Assert.AreEqual(RunPhase.HandSelect, run.CurrentState.Phase);
            Assert.AreEqual(BlindType.Big, run.CurrentState.BlindType);
            Assert.AreEqual(1, run.CurrentState.Ante);
        }

        [Test]
        public void UseCase_CompleteRunAndSettleMeta_ContractRatioWithin45Percent()
        {
            var run = new RunManagerV2(RunDifficultyProfile.Standard);
            var shop = new ShopManagerV2();
            var meta = new MetaManagerV2();

            run.StartRun(777);
            var safety = 0;

            while (run.CurrentState.Phase != RunPhase.RunComplete && safety < 64)
            {
                safety++;

                var submit = run.SubmitHandScore(run.CurrentState.TargetScore);
                Assert.IsTrue(submit.IsSuccess);

                var resolve = run.ResolveBlindResult();
                Assert.IsTrue(resolve.IsSuccess);
                Assert.IsTrue(resolve.Value.Passed);

                if (run.CurrentState.Phase == RunPhase.RunComplete)
                {
                    break;
                }

                Assert.AreEqual(RunPhase.Shop, run.CurrentState.Phase);
                var offers = shop.GenerateOffers(run.CurrentState.Ante, 9000 + safety, run.CurrentState.BlindType == BlindType.Boss);
                Assert.IsTrue(offers.IsSuccess);
                if (offers.Value.Count > 0)
                {
                    var buy = shop.PurchaseOffer(offers.Value[0], run.CurrentState.Money);
                    Assert.IsTrue(buy.IsSuccess);
                    if (buy.Value.Success)
                    {
                        run.CurrentState.Money = buy.Value.RemainingMoney;
                    }
                }

                var advance = run.AdvanceAfterShop();
                Assert.IsTrue(advance.IsSuccess);
            }

            Assert.AreEqual(RunPhase.RunComplete, run.CurrentState.Phase);
            Assert.LessOrEqual(safety, 24);

            var settlement = meta.SettleRun(new RunResult
            {
                IsClear = true,
                HighestAnte = 8,
                ScoreTotal = 100000
            }, new MetaProgress());
            Assert.IsTrue(settlement.IsSuccess);
            Assert.AreEqual(210, settlement.Value.XpGained);
            Assert.AreEqual(21, settlement.Value.LpGainedBase);

            var contracts = meta.GenerateContracts(new MetaProgress(), 2026);
            Assert.IsTrue(contracts.IsSuccess);
            Assert.AreEqual(3, contracts.Value.Count);

            var contractResult = meta.SettleContractWithCap(
                contracts.Value[0],
                new RunTelemetry { ContractCompleted = true },
                settlement.Value.LpGainedBase);
            Assert.IsTrue(contractResult.IsSuccess);

            var totalLp = settlement.Value.LpGainedBase + contractResult.Value.LpBonusCapped;
            var ratio = totalLp == 0 ? 0f : contractResult.Value.LpBonusCapped / (float)totalLp;
            Assert.LessOrEqual(ratio, 0.45f + 0.0001f);
        }
    }
}
