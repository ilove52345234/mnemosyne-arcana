using System;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;
using UnityEngine;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class S7FinalGateValidationTests
    {
        private GateProgressionManagerV2 _gate;

        [SetUp]
        public void SetUp()
        {
            _gate = new GateProgressionManagerV2();
        }

        [Test]
        public void S7_M1_LowProfile_CannotPassMainClear()
        {
            var result = _gate.EvaluateFinalMasteryGate(masteryCoverageRate: 0.92f, stableDaysAtHundredPercent: 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.IsMainClearEligible);
            Assert.IsFalse(result.Value.IsTrueClearEligible);
        }

        [Test]
        public void S7_M2_MidProfile_PassesMainClearOnly()
        {
            var result = _gate.EvaluateFinalMasteryGate(masteryCoverageRate: 0.96f, stableDaysAtHundredPercent: 0);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsMainClearEligible);
            Assert.IsFalse(result.Value.IsTrueClearEligible);
        }

        [Test]
        public void S7_M3_HighProfile_PassesTrueClearAfterSevenStableDays()
        {
            var result = _gate.EvaluateFinalMasteryGate(masteryCoverageRate: 1.0f, stableDaysAtHundredPercent: 7);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.IsMainClearEligible);
            Assert.IsTrue(result.Value.IsTrueClearEligible);
        }

        [Test]
        public void S7_M4_EdgeProfile_EndlessLongRun_IsStableAcrossThirtySeeds()
        {
            var invalidTransitions = 0;
            var mainClearSeeds = 0;
            var trueClearSeeds = 0;

            for (var seed = 0; seed < 30; seed++)
            {
                var rng = new System.Random(8400 + seed);
                var stableDaysAtHundred = 0;
                var hasMainClear = false;
                var hasTrueClear = false;

                for (var day = 0; day < 180; day++)
                {
                    var mastery = SimulateEdgeMastery(rng, day);
                    stableDaysAtHundred = mastery >= 1f ? stableDaysAtHundred + 1 : 0;

                    var gate = _gate.EvaluateFinalMasteryGate(mastery, stableDaysAtHundred);
                    Assert.IsTrue(gate.IsSuccess);

                    if (gate.Value.IsTrueClearEligible && !gate.Value.IsMainClearEligible)
                    {
                        invalidTransitions++;
                    }

                    hasMainClear |= gate.Value.IsMainClearEligible;
                    hasTrueClear |= gate.Value.IsTrueClearEligible;
                }

                if (hasMainClear)
                {
                    mainClearSeeds++;
                }

                if (hasTrueClear)
                {
                    trueClearSeeds++;
                }
            }

            Assert.AreEqual(0, invalidTransitions);
            Assert.AreEqual(30, mainClearSeeds);
            Assert.GreaterOrEqual(trueClearSeeds, 10);
        }

        [Test]
        public void S7_M9_ThirtyRuns_ClearRateMonitoring()
        {
            const int runs = 30;
            const int modelIndex = 9;
            const int baseSeed = 20260216;

            var clearCount = 0;

            for (var seedOffset = 0; seedOffset < runs; seedOffset++)
            {
                var rng = new System.Random(baseSeed + 5000 + seedOffset * 97);
                var run = new RunManagerV2();
                run.StartRun(baseSeed);

                var guard = 0;
                while (run.CurrentState.Phase != Core.Runtime.RunPhase.RunComplete &&
                       run.CurrentState.Phase != Core.Runtime.RunPhase.RunFail &&
                       guard < 512)
                {
                    guard++;
                    var phase = run.CurrentState.Phase;
                    if (phase == Core.Runtime.RunPhase.HandSelect)
                    {
                        var correct = 0;
                        for (var i = 0; i < 5; i++)
                        {
                            var answerChance = 0.35f + modelIndex * 0.055f;
                            answerChance -= 0.14f; // M9 penalty from prototype batch logic
                            answerChance = Mathf.Clamp01(answerChance);
                            if (rng.NextDouble() < answerChance)
                            {
                                correct++;
                            }
                        }

                        var handScore = BuildModelHandScore(
                            run.CurrentState.TargetScore,
                            run.CurrentState.PlaysLeft,
                            correct,
                            modelIndex,
                            rng);
                        run.SubmitHandScore(handScore);
                    }
                    else if (phase == Core.Runtime.RunPhase.BlindResult)
                    {
                        run.ResolveBlindResult();
                    }
                    else if (phase == Core.Runtime.RunPhase.Shop)
                    {
                        run.AdvanceAfterShop();
                    }
                }

                if (run.CurrentState.Phase == Core.Runtime.RunPhase.RunComplete)
                {
                    clearCount++;
                }
            }

            var clearRate = clearCount / (float)runs;
            TestContext.WriteLine($"S7 M9 clear rate: {clearCount}/{runs} ({clearRate:P1})");

            // Monitoring sanity guard: should neither be impossible nor guaranteed.
            Assert.Greater(clearCount, 0);
            Assert.Less(clearCount, runs);
        }

        private static float SimulateEdgeMastery(System.Random rng, int day)
        {
            // Build repeating "streak windows" to emulate long-run endless sessions near thresholds.
            if (day % 18 is >= 0 and <= 7)
            {
                return 1f;
            }

            var jitter = ((float)rng.NextDouble() - 0.5f) * 0.08f;
            var value = 0.965f + jitter;
            return Math.Clamp(value, 0.90f, 1f);
        }

        private static int BuildModelHandScore(int targetScore, int playsLeft, int correctCount, int modelIndex, System.Random rng)
        {
            var basePerPlay = targetScore / Mathf.Max(1, playsLeft);
            var modelFactor = 0.40f + 0.05f * Mathf.Clamp(modelIndex, 0, 9);
            if (modelIndex == 8)
            {
                modelFactor += 0.02f;
            }
            else if (modelIndex >= 9)
            {
                modelFactor = 0.46f;
            }

            var accuracyFactor = modelFactor + 0.08f * Mathf.Clamp(correctCount, 0, 5);
            var volatility = modelIndex >= 9
                ? 0.55f + (float)rng.NextDouble() * 0.55f
                : 0.90f + (float)rng.NextDouble() * 0.20f;

            var score = Mathf.RoundToInt(basePerPlay * accuracyFactor * volatility);
            return Mathf.Max(0, score);
        }
    }
}
