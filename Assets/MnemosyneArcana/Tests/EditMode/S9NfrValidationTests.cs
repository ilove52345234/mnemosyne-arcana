using System;
using System.Collections.Generic;
using System.Diagnostics;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class S9NfrValidationTests
    {
        [Test]
        public void S9_M1_LowDevice_CoreLoops_FinishWithinBudget()
        {
            var gate = new GateProgressionManagerV2();
            var telemetry = new LearningTelemetryManagerV2();
            var decay = new DecayManagerV2();

            var word = new WordProgress
            {
                WordId = "nfr-low",
                Level = LearningLevel.Lv3,
                Pool = WordPool.Learning,
                LastPracticed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            };

            var sw = Stopwatch.StartNew();
            for (var i = 0; i < 50_000; i++)
            {
                var g = gate.EvaluateProgress(5000 + (i % 4000), 0.8f, 0.75f, i % 10);
                Assert.IsTrue(g.IsSuccess);

                var t = telemetry.EvaluateAlerts(new LearningTelemetrySnapshot
                {
                    PassRateByGate = 0.55f,
                    RecoverySuccessRate = 0.65f,
                    ActiveRecallAccuracy = 0.8f,
                    DecayRegressionRate = 0.2f,
                    GateStallDays = 2f
                });
                Assert.IsTrue(t.IsSuccess);

                var d = decay.EvaluateDecay(word, new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc));
                Assert.IsNotNull(d);
            }

            sw.Stop();
            Assert.Less(sw.ElapsedMilliseconds, 5000, "Low-device baseline exceeded budget.");
        }

        [Test]
        public void S9_M2_MidDevice_RunShopFlow_NoErrorsAndMemoryStable()
        {
            var run = new RunManagerV2(RunDifficultyProfile.Standard);
            var shop = new ShopManagerV2();

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var beforeBytes = GC.GetTotalMemory(true);

            for (var i = 0; i < 2000; i++)
            {
                run.StartRun(1000 + i);
                var submit = run.SubmitHandScore(run.CurrentState.TargetScore);
                Assert.IsTrue(submit.IsSuccess);

                var resolve = run.ResolveBlindResult();
                Assert.IsTrue(resolve.IsSuccess);
                Assert.IsTrue(resolve.Value.Passed);
                Assert.AreEqual(RunPhase.Shop, resolve.Value.NextPhase);

                var offers = shop.GenerateOffers(run.CurrentState.Ante, seed: 2000 + i);
                Assert.IsTrue(offers.IsSuccess);
                Assert.GreaterOrEqual(offers.Value.Count, 1);

                var purchase = shop.PurchaseOffer(offers.Value[0], run.CurrentState.Money);
                Assert.IsTrue(purchase.IsSuccess);

                var advance = run.AdvanceAfterShop();
                Assert.IsTrue(advance.IsSuccess);
                Assert.AreEqual(RunPhase.HandSelect, advance.Value.Phase);
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            var afterBytes = GC.GetTotalMemory(true);
            var growthBytes = afterBytes - beforeBytes;

            Assert.Less(growthBytes, 64L * 1024L * 1024L, "Mid-device memory growth exceeded 64MB.");
        }

        [Test]
        public void S9_M3_HighLoad_CompositeSoak_NoServiceFailures()
        {
            var scoring = new ScoringManagerV2();
            var gate = new GateProgressionManagerV2();
            var decay = new DecayManagerV2();
            var shop = new ShopManagerV2();

            var cards = new List<PlayedCard>
            {
                new PlayedCard { WordId = "a", Element = Element.Life, PartOfSpeech = PartOfSpeech.N, BaseChips = 10, ChipMultiplier = 1f },
                new PlayedCard { WordId = "b", Element = Element.Force, PartOfSpeech = PartOfSpeech.V, BaseChips = 12, ChipMultiplier = 1f },
                new PlayedCard { WordId = "c", Element = Element.Mind, PartOfSpeech = PartOfSpeech.A, BaseChips = 9, ChipMultiplier = 1f },
                new PlayedCard { WordId = "d", Element = Element.Matter, PartOfSpeech = PartOfSpeech.D, BaseChips = 8, ChipMultiplier = 1f },
                new PlayedCard { WordId = "e", Element = Element.Abstract, PartOfSpeech = PartOfSpeech.N, BaseChips = 11, ChipMultiplier = 1f }
            };

            var failures = 0;
            for (var i = 0; i < 20_000; i++)
            {
                var score = scoring.EvaluateHand(cards, new RunModifiers
                {
                    HandUpgradeLevel = i % 4,
                    AdditiveMultTotal = 1f,
                    HandMultDelta = 0,
                    MultiplicativeFactors = new[] { 1.1f, 1.2f }
                });
                if (!score.IsSuccess) failures++;

                var progress = gate.EvaluateProgress(2000 + (i % 9000), 0.85f, 0.82f, i % 10);
                if (!progress.IsSuccess) failures++;

                var recovery = gate.EvaluateRecoveryGate(0.8f, 0.85f, i % 3, i % 12);
                if (!recovery.IsSuccess) failures++;

                var final = gate.EvaluateFinalMasteryGate(0.95f + (i % 5) * 0.01f, i % 10);
                if (!final.IsSuccess) failures++;

                var decayResult = decay.EvaluateDecay(new WordProgress
                {
                    WordId = "soak-" + i,
                    Level = LearningLevel.Lv4,
                    Pool = WordPool.Mastered,
                    LastPracticed = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }, new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc));
                if (decayResult == null) failures++;

                var offers = shop.GenerateOffers(ante: 6, seed: 9000 + i);
                if (!offers.IsSuccess || offers.Value.Count == 0) failures++;
            }

            Assert.AreEqual(0, failures);
        }
    }
}
