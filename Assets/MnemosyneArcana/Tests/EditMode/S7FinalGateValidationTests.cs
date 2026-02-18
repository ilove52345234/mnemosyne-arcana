using System;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

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
                var rng = new Random(8400 + seed);
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

        private static float SimulateEdgeMastery(Random rng, int day)
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
    }
}
