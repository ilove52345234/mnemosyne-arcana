using System;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class S4LongCycleDistributionTests
    {
        private GateProgressionManagerV2 _gate;
        private DecayManagerV2 _decay;

        [SetUp]
        public void SetUp()
        {
            _gate = new GateProgressionManagerV2();
            _decay = new DecayManagerV2();
        }

        [Test]
        public void S4_RecoveryGate_ThirtySeedDistribution_ShowsExpectedOrdering()
        {
            var lowNeedsRecovery = 0;
            var midNeedsRecovery = 0;
            var highNeedsRecovery = 0;

            var lowDemotions = 0;
            var midDemotions = 0;
            var highDemotions = 0;

            for (var seed = 0; seed < 30; seed++)
            {
                var rng = new Random(4400 + seed);

                var lowCoverage = 0.70f + (float)rng.NextDouble() * 0.08f;
                var midCoverage = 0.80f + (float)rng.NextDouble() * 0.08f;
                var highCoverage = 0.88f + (float)rng.NextDouble() * 0.08f;

                var low = _gate.EvaluateRecoveryGate(lowCoverage, 0.85f, consecutiveRecoveryCycleFailures: 2, daysSinceLastDemotion: seed % 10);
                var mid = _gate.EvaluateRecoveryGate(midCoverage, 0.85f, consecutiveRecoveryCycleFailures: 1, daysSinceLastDemotion: 10);
                var high = _gate.EvaluateRecoveryGate(highCoverage, 0.85f, consecutiveRecoveryCycleFailures: 0, daysSinceLastDemotion: 10);

                Assert.IsTrue(low.IsSuccess);
                Assert.IsTrue(mid.IsSuccess);
                Assert.IsTrue(high.IsSuccess);

                if (low.Value.NeedsRecoveryGate) lowNeedsRecovery++;
                if (mid.Value.NeedsRecoveryGate) midNeedsRecovery++;
                if (high.Value.NeedsRecoveryGate) highNeedsRecovery++;

                if (low.Value.ShouldDemote) lowDemotions++;
                if (mid.Value.ShouldDemote) midDemotions++;
                if (high.Value.ShouldDemote) highDemotions++;
            }

            Assert.Greater(lowNeedsRecovery, midNeedsRecovery);
            Assert.Greater(midNeedsRecovery, highNeedsRecovery);
            Assert.Greater(lowDemotions, midDemotions);
            Assert.AreEqual(0, highDemotions);
        }

        [Test]
        public void S4_Decay_SevenFourteenThirtyDayDistribution_DegradesMonotonically()
        {
            var now = new DateTime(2026, 2, 18, 0, 0, 0, DateTimeKind.Utc);

            var avgLevelDay7 = 0f;
            var avgLevelDay14 = 0f;
            var avgLevelDay30 = 0f;

            for (var seed = 0; seed < 30; seed++)
            {
                var word = new WordProgress
                {
                    WordId = "s4-dist-" + seed,
                    Level = LearningLevel.Lv4,
                    Pool = WordPool.Mastered,
                    LastPracticed = now
                };

                var day7 = ProjectDecay(word, now.AddDays(7));
                var day14 = ProjectDecay(word, now.AddDays(14));
                var day30 = ProjectDecay(word, now.AddDays(30));

                avgLevelDay7 += (int)day7.Level;
                avgLevelDay14 += (int)day14.Level;
                avgLevelDay30 += (int)day30.Level;
            }

            avgLevelDay7 /= 30f;
            avgLevelDay14 /= 30f;
            avgLevelDay30 /= 30f;

            Assert.Greater(avgLevelDay7, avgLevelDay14);
            Assert.Greater(avgLevelDay14, avgLevelDay30);
            Assert.LessOrEqual(avgLevelDay30, 1.0f);
        }

        private WordProgress ProjectDecay(WordProgress source, DateTime checkpoint)
        {
            var clone = new WordProgress
            {
                WordId = source.WordId,
                Level = source.Level,
                Pool = source.Pool,
                LastPracticed = source.LastPracticed
            };

            var simulatedLastPracticed = clone.LastPracticed;
            for (var i = 0; i < 8; i++)
            {
                var result = _decay.EvaluateDecay(clone, checkpoint);
                if (!result.Decayed)
                {
                    break;
                }

                clone.Level = result.NewLevel;
                clone.Pool = result.NewPool;

                var consumedDays = GetDecayDays(result.PreviousLevel);
                if (consumedDays <= 0)
                {
                    break;
                }

                simulatedLastPracticed = simulatedLastPracticed.AddDays(consumedDays);
                if (simulatedLastPracticed > checkpoint)
                {
                    simulatedLastPracticed = checkpoint;
                }

                clone.LastPracticed = simulatedLastPracticed;
            }

            return clone;
        }

        private static int GetDecayDays(LearningLevel level)
        {
            return level switch
            {
                LearningLevel.Lv1 => 1,
                LearningLevel.Lv2 => 3,
                LearningLevel.Lv3 => 7,
                LearningLevel.Lv4 => 7,
                _ => -1
            };
        }
    }
}
