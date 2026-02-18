using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class MetaManagerTests
    {
        private MetaManagerV2 _meta;

        [SetUp]
        public void SetUp()
        {
            _meta = new MetaManagerV2();
        }

        [Test]
        public void SettleRun_Ante5Clear_XP150_LP15()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = true, HighestAnte = 5, ScoreTotal = 50000 },
                new MetaProgress { Xp = 0, Lp = 0 });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(150, result.Value.XpGained);
            Assert.AreEqual(15, result.Value.LpGainedBase);
        }

        [Test]
        public void SettleRun_Ante3Fail_XP60_LP6()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = false, HighestAnte = 3, ScoreTotal = 1500 },
                new MetaProgress { Xp = 100, Lp = 20 });
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(60, result.Value.XpGained);
            Assert.AreEqual(6, result.Value.LpGainedBase);
        }

        [Test]
        public void SettleRun_Ante1Fail_XP20_LP2()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = false, HighestAnte = 1, ScoreTotal = 50 },
                new MetaProgress());
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(20, result.Value.XpGained);
            Assert.AreEqual(2, result.Value.LpGainedBase);
        }

        [Test]
        public void SettleRun_NullRunResult_Error()
        {
            var result = _meta.SettleRun(null, new MetaProgress());
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }

        [Test]
        public void GenerateContracts_SameSeed_SameResult()
        {
            var meta = new MetaProgress { Lp = 50 };
            var first = _meta.GenerateContracts(meta, 12345);
            var second = _meta.GenerateContracts(meta, 12345);
            Assert.IsTrue(first.IsSuccess);
            Assert.IsTrue(second.IsSuccess);
            Assert.AreEqual(first.Value.Count, second.Value.Count);
            for (int i = 0; i < first.Value.Count; i++)
            {
                Assert.AreEqual(first.Value[i].ContractId, second.Value[i].ContractId);
                Assert.AreEqual(first.Value[i].LpReward, second.Value[i].LpReward);
            }
        }

        [Test]
        public void GenerateContracts_Returns3()
        {
            var result = _meta.GenerateContracts(new MetaProgress(), 999);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(3, result.Value.Count);
        }

        [Test]
        public void GenerateContracts_AllFieldsPopulated()
        {
            var result = _meta.GenerateContracts(new MetaProgress(), 42);
            Assert.IsTrue(result.IsSuccess);
            foreach (var c in result.Value)
            {
                Assert.IsFalse(string.IsNullOrEmpty(c.ContractId));
                Assert.IsFalse(string.IsNullOrEmpty(c.ContractType));
                Assert.That(c.Tier, Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(3));
                Assert.That(c.LpReward, Is.GreaterThan(0));
            }
        }

        [Test]
        public void SettleContract_Completed_ReturnsLpReward()
        {
            var contract = new Contract { ContractId = "CT_NAT_001", LpReward = 8, Tier = 2 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContract(contract, telemetry);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Completed);
            Assert.AreEqual(8, result.Value.LpBonusRaw);
        }

        [Test]
        public void SettleContract_NotCompleted_ZeroLp()
        {
            var contract = new Contract { ContractId = "CT_NAT_001", LpReward = 8, Tier = 2 };
            var telemetry = new RunTelemetry { ContractCompleted = false };
            var result = _meta.SettleContract(contract, telemetry);
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.Completed);
            Assert.AreEqual(0, result.Value.LpBonusRaw);
            Assert.AreEqual(0, result.Value.LpBonusCapped);
        }

        [Test]
        public void SettleContractWithCap_UnderCap_NoAdjustment()
        {
            var contract = new Contract { ContractId = "CT_01", LpReward = 6, Tier = 1 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContractWithCap(contract, telemetry, lpBase: 15);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(6, result.Value.LpBonusRaw);
            Assert.AreEqual(6, result.Value.LpBonusCapped);
            Assert.IsFalse(result.Value.CapApplied);
        }

        [Test]
        public void SettleContractWithCap_OverCap_Capped()
        {
            var contract = new Contract { ContractId = "CT_02", LpReward = 15, Tier = 3 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContractWithCap(contract, telemetry, lpBase: 6);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(15, result.Value.LpBonusRaw);
            Assert.AreEqual(4, result.Value.LpBonusCapped);  // floor(6*45/55) = 4
            Assert.IsTrue(result.Value.CapApplied);
        }

        [Test]
        public void SettleContractWithCap_LearningBonusFromCurriculum_AppliesBeforeCap()
        {
            var contract = new Contract { ContractId = "CT_LRN_001", ContractType = "Learning", LpReward = 10, Tier = 2 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var effects = new CurriculumEffectSnapshot { LearningContractLpBonusRate = 0.10f };

            var result = _meta.SettleContractWithCap(contract, telemetry, lpBase: 30, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(11, result.Value.LpBonusRaw);
            Assert.AreEqual(11, result.Value.LpBonusCapped);
        }

        [Test]
        public void SettleContractWithCap_ZeroBase_CapIsZero()
        {
            var contract = new Contract { ContractId = "CT_03", LpReward = 10, Tier = 2 };
            var telemetry = new RunTelemetry { ContractCompleted = true };
            var result = _meta.SettleContractWithCap(contract, telemetry, lpBase: 0);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(0, result.Value.LpBonusCapped);
            Assert.IsTrue(result.Value.CapApplied);
        }

        [Test]
        public void SettleRun_WithContractLp_TotalCorrect()
        {
            var result = _meta.SettleRun(
                new RunResult { IsClear = true, HighestAnte = 4, ScoreTotal = 5000 },
                new MetaProgress());
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(130, result.Value.XpGained);
            Assert.AreEqual(13, result.Value.LpGainedBase);
            Assert.AreEqual(13, result.Value.LpGainedTotal);
        }

        [Test]
        public void TryUnlockNode_EnoughLpAndNoPrereq_Succeeds()
        {
            var result = _meta.TryUnlockNode("FLU_01", new MetaProgress
            {
                Lp = 30,
                CurriculumNodes = new string[0]
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Success);
            Assert.AreEqual("FLU_01", result.Value.NodeId);
            Assert.AreEqual(20, result.Value.SpentLp);
            Assert.AreEqual(10, result.Value.RemainingLp);
            CollectionAssert.Contains(result.Value.UnlockedNodes, "FLU_01");
        }

        [Test]
        public void TryUnlockNode_MissingPrereq_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("FLU_02", new MetaProgress
            {
                Lp = 50,
                CurriculumNodes = new string[0]
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void TryUnlockNode_MutexConflict_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("FLU_03B", new MetaProgress
            {
                Lp = 100,
                CurriculumNodes = new[] { "FLU_01", "FLU_02", "FLU_03A" }
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void TryUnlockNode_InsufficientLp_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("MAS_01", new MetaProgress
            {
                Lp = 10,
                CurriculumNodes = new string[0]
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void TryUnlockNode_AlreadyUnlocked_ReturnsStateConflict()
        {
            var result = _meta.TryUnlockNode("LEX_01", new MetaProgress
            {
                Lp = 100,
                CurriculumNodes = new[] { "LEX_01" }
            });

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.StateConflict, result.Error);
        }

        [Test]
        public void Curriculum_AllDefinedNodes_CanUnlockWithSatisfiedConditions()
        {
            foreach (var node in GetCurriculumNodeDefs())
            {
                var prereqs = node.RequiredAnyOfGroups
                    .Where(group => group.Length > 0)
                    .Select(group => group[0])
                    .Distinct()
                    .ToArray();

                var result = _meta.TryUnlockNode(node.NodeId, new MetaProgress
                {
                    Lp = node.Cost + 10,
                    CurriculumNodes = prereqs
                });

                Assert.IsTrue(result.IsSuccess, $"Node {node.NodeId} should unlock when prerequisites are met.");
                Assert.AreEqual(node.Cost, result.Value.SpentLp);
                CollectionAssert.Contains(result.Value.UnlockedNodes, node.NodeId);
            }
        }

        [Test]
        public void Curriculum_AllDefinedNodes_EnforcePrereqAndMutexConstraints()
        {
            foreach (var node in GetCurriculumNodeDefs())
            {
                if (node.RequiredAnyOfGroups.Length > 0)
                {
                    var missingPrereq = _meta.TryUnlockNode(node.NodeId, new MetaProgress
                    {
                        Lp = node.Cost + 10,
                        CurriculumNodes = Array.Empty<string>()
                    });
                    Assert.IsFalse(missingPrereq.IsSuccess, $"Node {node.NodeId} should fail without prerequisites.");
                    Assert.AreEqual(ErrorCode.StateConflict, missingPrereq.Error);
                }

                if (node.MutexWith.Length > 0)
                {
                    var prereqs = node.RequiredAnyOfGroups
                        .Where(group => group.Length > 0)
                        .Select(group => group[0])
                        .Distinct()
                        .ToList();
                    prereqs.Add(node.MutexWith[0]);

                    var mutexConflict = _meta.TryUnlockNode(node.NodeId, new MetaProgress
                    {
                        Lp = node.Cost + 10,
                        CurriculumNodes = prereqs.ToArray()
                    });
                    Assert.IsFalse(mutexConflict.IsSuccess, $"Node {node.NodeId} should fail when mutex node is unlocked.");
                    Assert.AreEqual(ErrorCode.StateConflict, mutexConflict.Error);
                }
            }
        }

        [Test]
        public void GetCurriculumEffects_MapsRepresentativeNodesToRuntimeModifiers()
        {
            var result = _meta.GetCurriculumEffects(new MetaProgress
            {
                CurriculumNodes = new[]
                {
                    "FLU_01", "FLU_03A", "FLU_03B", "FLU_04", "FLU_05", "FLU_06B", "FLU_07", "FLU_08", "FLU_09", "FLU_12",
                    "BLD_01", "BLD_02", "BLD_03A", "BLD_04", "BLD_06A", "BLD_06B", "BLD_07", "BLD_09", "BLD_10A",
                    "MAS_01", "MAS_03A", "MAS_03B", "MAS_04", "MAS_05", "MAS_07", "MAS_08", "MAS_09", "MAS_11", "MAS_12",
                    "LEX_03A", "LEX_04", "LEX_05", "LEX_06A", "LEX_06B", "LEX_07", "LEX_08", "LEX_09", "LEX_12"
                }
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(38, result.Value.UnlockedNodeCount);
            Assert.AreEqual(0.2f, result.Value.Lv1Lv2TimeBonusSec, 0.0001f);
            Assert.AreEqual(1, result.Value.RetryCostDiscount);
            Assert.AreEqual(2, result.Value.FirstShopRerollDiscount);
            Assert.AreEqual(2, result.Value.Lv4CardFlatChipBonus);
            Assert.AreEqual(1, result.Value.Lv4DecayProtectionLayers);
            Assert.AreEqual(0.08f, result.Value.LexiconUnlockLpCostDiscountRate, 0.0001f);
            Assert.AreEqual(0.08f, result.Value.ShortWordDropBiasRate, 0.0001f);
            Assert.AreEqual(1, result.Value.CourseLpRebate);
            Assert.IsTrue(result.Value.IgnoreFirstLv4WrongHandMultPenalty);
            Assert.IsTrue(result.Value.FreeRetryOnFirstWrongOption);
            Assert.AreEqual(1, result.Value.ConsecutiveWrongReliefThresholdDelta);
            Assert.AreEqual(1, result.Value.PerfectRunLpBonus);
            Assert.AreEqual(1, result.Value.StreakBonusDurationExtraTurns);
            Assert.AreEqual(1, result.Value.NextRefreshPreviewCategoryCount);
            Assert.AreEqual(1, result.Value.NurtureCandidateExtraCount);
            Assert.AreEqual(1, result.Value.Lv1To2TrainingDiscount);
            Assert.AreEqual(0.08f, result.Value.SenseOfferWeightBonusRate, 0.0001f);
            Assert.AreEqual(0.12f, result.Value.AffixToolWeightBonusRate, 0.0001f);
            Assert.AreEqual(1, result.Value.NurtureLockCarrySlots);
            Assert.AreEqual(PackGuaranteeMode.LearningTool, result.Value.FirstPackGuaranteeMode);
            Assert.AreEqual(1, result.Value.BossAllCorrectExtraLv4UpgradeCount);
            Assert.AreEqual(1, result.Value.FirstLv4PlayContractProgressBonus);
            Assert.AreEqual(2, result.Value.MasteryRunLpBonusOnEightLv4);
            Assert.AreEqual(1, result.Value.MasterySettlementLpPerThreeLv4);
            Assert.AreEqual(4, result.Value.MasterySettlementLpBonusCap);
            Assert.AreEqual(0.08f, result.Value.Lv1Lv2EasyQuestionRateBonus, 0.0001f);
            Assert.AreEqual(0.12f, result.Value.Lv3CorrectRewardBonusRate, 0.0001f);
            Assert.AreEqual(1, result.Value.SpellingToleranceExtraLetters);
            Assert.AreEqual(3, result.Value.SpellingTolerancePerRunLimit);
            Assert.IsTrue(result.Value.IgnoreFirstLv4DemotionPerRun);
            Assert.AreEqual(0.15f, result.Value.WeakWordWeightBonusRate, 0.0001f);
            Assert.AreEqual(1, result.Value.DecayTimerExtendDaysOnDecayedHit);
            Assert.AreEqual(0.20f, result.Value.ElementGapWeightBonusRate, 0.0001f);
            Assert.AreEqual(0.20f, result.Value.PosGapWeightBonusRate, 0.0001f);
            Assert.AreEqual(1, result.Value.GuaranteedLv4LexiconCountPerRun);
            Assert.IsTrue(result.Value.PreferRecentDecayedWhenOverflow);
            Assert.AreEqual(1, result.Value.FirstDecayedPlayLpBonus);
            Assert.AreEqual(2, result.Value.FirstDecayedPlayLpBonusCap);
            Assert.AreEqual(0.10f, result.Value.Lv4NegativeAffixResistanceRate, 0.0001f);
            Assert.AreEqual(2, result.Value.MasteryRunLpBonusOnFiveLv4);
            Assert.AreEqual(2, result.Value.Lv3To4RequirementReduction);
            Assert.AreEqual(12, result.Value.Lv3To4RequirementMinimum);
        }

        [Test]
        public void Curriculum_AllDefinedNodes_MapToAtLeastOneRuntimeEffect()
        {
            var defaultSnapshot = new CurriculumEffectSnapshot();
            var effectProps = typeof(CurriculumEffectSnapshot)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.Name != nameof(CurriculumEffectSnapshot.UnlockedNodeCount))
                .ToArray();

            foreach (var node in GetCurriculumNodeDefs())
            {
                var result = _meta.GetCurriculumEffects(new MetaProgress { CurriculumNodes = new[] { node.NodeId } });
                Assert.IsTrue(result.IsSuccess, $"GetCurriculumEffects should succeed for {node.NodeId}");
                Assert.AreEqual(1, result.Value.UnlockedNodeCount, $"{node.NodeId} should count as one unlocked effect source.");

                var hasDelta = effectProps.Any(prop =>
                {
                    var current = prop.GetValue(result.Value);
                    var baseline = prop.GetValue(defaultSnapshot);
                    return !Equals(current, baseline);
                });

                Assert.IsTrue(hasDelta, $"{node.NodeId} should map to at least one runtime effect field.");
            }
        }

        [Test]
        public void GetLexiconUnlockRequirement_AppliesLexiconDiscounts()
        {
            var effects = new CurriculumEffectSnapshot
            {
                LexiconUnlockLpCostDiscountRate = 0.08f,
                LexiconUnlockRunRequirementDiscountRate = 0.10f,
                LexiconUnlockCoverageDiscountRate = 0.05f
            };

            var result = _meta.GetLexiconUnlockRequirement(baseLpCost: 100, baseRequiredRuns: 20, baseRequiredCoverageRate: 0.80f, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(92, result.Value.LpCost);
            Assert.AreEqual(18, result.Value.RequiredRuns);
            Assert.AreEqual(0.76f, result.Value.RequiredCoverageRate, 0.0001f);
        }

        [Test]
        public void BuildLexiconDropWeights_DecayedAndStaleWords_GetHigherWeight()
        {
            var effects = new CurriculumEffectSnapshot
            {
                DecayedPoolWeightBonusRate = 0.10f,
                StaleWordWeightBonusRate = 0.20f
            };

            var words = new[]
            {
                new WordProgress { WordId = "w_decayed_stale", Pool = WordPool.Decayed, Level = LearningLevel.Lv2 },
                new WordProgress { WordId = "w_normal", Pool = WordPool.Learning, Level = LearningLevel.Lv2 }
            };
            var stale = new Dictionary<string, int>
            {
                { "w_decayed_stale", 3 },
                { "w_normal", 0 }
            };

            var result = _meta.BuildLexiconDropWeights(words, stale, effects);
            Assert.IsTrue(result.IsSuccess);

            var boosted = result.Value.First(x => x.WordId == "w_decayed_stale");
            var normal = result.Value.First(x => x.WordId == "w_normal");
            Assert.AreEqual(132, boosted.Weight);
            Assert.AreEqual(100, normal.Weight);
        }

        [Test]
        public void BuildLexiconDropWeights_LengthBias_AdjustsShortAndLongWords()
        {
            var effects = new CurriculumEffectSnapshot
            {
                ShortWordDropBiasRate = 0.08f,
                LongWordDropBiasRate = 0.12f
            };

            var words = new[]
            {
                new WordProgress { WordId = "short_word", Pool = WordPool.Learning, Level = LearningLevel.Lv1 },
                new WordProgress { WordId = "long_word", Pool = WordPool.Learning, Level = LearningLevel.Lv1 }
            };
            var lengths = new Dictionary<string, int>
            {
                { "short_word", 4 },
                { "long_word", 7 }
            };

            var result = _meta.BuildLexiconDropWeights(words, null, lengths, effects);
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(108, result.Value.First(x => x.WordId == "short_word").Weight);
            Assert.AreEqual(112, result.Value.First(x => x.WordId == "long_word").Weight);
        }

        [Test]
        public void BuildLexiconDropWeights_WithLex0406And08_AppliesWeakGapAndRecentDecayPriority()
        {
            var effects = new CurriculumEffectSnapshot
            {
                WeakWordWeightBonusRate = 0.15f,
                ElementGapWeightBonusRate = 0.20f,
                PosGapWeightBonusRate = 0.20f,
                PreferRecentDecayedWhenOverflow = true
            };

            var words = new[]
            {
                new WordProgress { WordId = "w_target", Pool = WordPool.Decayed, Level = LearningLevel.Lv2 },
                new WordProgress { WordId = "w_normal", Pool = WordPool.Learning, Level = LearningLevel.Lv2 }
            };

            var result = _meta.BuildLexiconDropWeights(
                words,
                staleRunCounts: null,
                wordLengths: null,
                weakWordIds: new HashSet<string> { "w_target" },
                elementGapWordIds: new HashSet<string> { "w_target" },
                posGapWordIds: new HashSet<string> { "w_target" },
                recentDecayedWordIds: new HashSet<string> { "w_target" },
                isDecayedPoolOverflowed: true,
                effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(198, result.Value.First(x => x.WordId == "w_target").Weight);
            Assert.AreEqual(100, result.Value.First(x => x.WordId == "w_normal").Weight);
        }

        [Test]
        public void GetContractRequirementAfterCurriculum_MasteryRequirementReducedButNotBelowOne()
        {
            var contract = new Contract { ContractId = "CT_MAS_001", ContractType = "Mastery", Tier = 2, LpReward = 10 };
            var effects = new CurriculumEffectSnapshot { MasteryContractRequirementReduction = 2 };

            var reduced = _meta.GetContractRequirementAfterCurriculum(3, contract, effects);
            var floorOne = _meta.GetContractRequirementAfterCurriculum(1, contract, effects);

            Assert.IsTrue(reduced.IsSuccess);
            Assert.AreEqual(1, reduced.Value);
            Assert.IsTrue(floorOne.IsSuccess);
            Assert.AreEqual(1, floorOne.Value);
        }

        [Test]
        public void GetPerfectRunLpBonus_WithFlu07_OnlyGrantsOnce()
        {
            var effects = new CurriculumEffectSnapshot { PerfectRunLpBonus = 1 };

            var granted = _meta.GetPerfectRunLpBonus(isAllCorrectInRun: true, alreadyGrantedThisRun: false, effects);
            var blocked = _meta.GetPerfectRunLpBonus(isAllCorrectInRun: true, alreadyGrantedThisRun: true, effects);

            Assert.AreEqual(1, granted);
            Assert.AreEqual(0, blocked);
        }

        [Test]
        public void GetContractProgressBonusOnFirstLv4Play_WithMas09_OnlyFirstPlayGetsBonus()
        {
            var effects = new CurriculumEffectSnapshot { FirstLv4PlayContractProgressBonus = 1 };

            var granted = _meta.GetContractProgressBonusOnFirstLv4Play(isFirstLv4PlayInRun: true, effects);
            var blocked = _meta.GetContractProgressBonusOnFirstLv4Play(isFirstLv4PlayInRun: false, effects);

            Assert.AreEqual(1, granted);
            Assert.AreEqual(0, blocked);
        }

        [Test]
        public void GetMasteryRunLpBonus_WithMas11_RequiresEightLv4AndSingleGrant()
        {
            var effects = new CurriculumEffectSnapshot { MasteryRunLpBonusOnEightLv4 = 2 };

            var underThreshold = _meta.GetMasteryRunLpBonus(lv4PlaysInRun: 7, alreadyGrantedThisRun: false, effects);
            var granted = _meta.GetMasteryRunLpBonus(lv4PlaysInRun: 8, alreadyGrantedThisRun: false, effects);
            var blocked = _meta.GetMasteryRunLpBonus(lv4PlaysInRun: 9, alreadyGrantedThisRun: true, effects);

            Assert.AreEqual(0, underThreshold);
            Assert.AreEqual(2, granted);
            Assert.AreEqual(0, blocked);
        }

        [Test]
        public void GetMasteryRunLpBonusOnFiveLv4_WithMas04_RequiresFiveLv4AndSingleGrant()
        {
            var effects = new CurriculumEffectSnapshot { MasteryRunLpBonusOnFiveLv4 = 2 };

            var under = _meta.GetMasteryRunLpBonusOnFiveLv4(lv4PlaysInRun: 4, alreadyGrantedThisRun: false, effects);
            var granted = _meta.GetMasteryRunLpBonusOnFiveLv4(lv4PlaysInRun: 5, alreadyGrantedThisRun: false, effects);
            var blocked = _meta.GetMasteryRunLpBonusOnFiveLv4(lv4PlaysInRun: 8, alreadyGrantedThisRun: true, effects);

            Assert.AreEqual(0, under);
            Assert.AreEqual(2, granted);
            Assert.AreEqual(0, blocked);
        }

        [Test]
        public void GetMasterySettlementLpBonus_WithMas12_AppliesPerThreeAndCap()
        {
            var effects = new CurriculumEffectSnapshot
            {
                MasterySettlementLpPerThreeLv4 = 1,
                MasterySettlementLpBonusCap = 4
            };

            var noBlock = _meta.GetMasterySettlementLpBonus(lv4PlayedCount: 2, effects);
            var twoBlocks = _meta.GetMasterySettlementLpBonus(lv4PlayedCount: 6, effects);
            var capped = _meta.GetMasterySettlementLpBonus(lv4PlayedCount: 15, effects);

            Assert.AreEqual(0, noBlock);
            Assert.AreEqual(2, twoBlocks);
            Assert.AreEqual(4, capped);
        }

        [Test]
        public void GetLv3To4RequirementAfterCurriculum_WithMas05_AppliesReductionAndFloor()
        {
            var effects = new CurriculumEffectSnapshot
            {
                Lv3To4RequirementReduction = 2,
                Lv3To4RequirementMinimum = 12
            };

            Assert.AreEqual(13, _meta.GetLv3To4RequirementAfterCurriculum(15, effects));
            Assert.AreEqual(12, _meta.GetLv3To4RequirementAfterCurriculum(12, effects));
            Assert.AreEqual(0, _meta.GetLv3To4RequirementAfterCurriculum(-1, effects));
        }

        [Test]
        public void LexiconRuntimeHelpers_WithLex0512And07And08_ExposeExpectedBehavior()
        {
            var effects = new CurriculumEffectSnapshot
            {
                DecayTimerExtendDaysOnDecayedHit = 1,
                FirstDecayedPlayLpBonus = 1,
                FirstDecayedPlayLpBonusCap = 2,
                GuaranteedLv4LexiconCountPerRun = 1,
                PreferRecentDecayedWhenOverflow = true
            };

            Assert.AreEqual(1, _meta.GetDecayTimerExtensionDaysOnDecayedHit(effects));
            Assert.AreEqual(1, _meta.GetGuaranteedLv4LexiconCountPerRun(effects));
            Assert.IsTrue(_meta.ShouldPrioritizeRecentDecayedWordsOnOverflow(effects));
            Assert.AreEqual(1, _meta.GetFirstDecayedPlayLpBonus(isFirstDecayedPlayInRun: true, alreadyGrantedInRun: 0, effects));
            Assert.AreEqual(0, _meta.GetFirstDecayedPlayLpBonus(isFirstDecayedPlayInRun: false, alreadyGrantedInRun: 0, effects));
            Assert.AreEqual(0, _meta.GetFirstDecayedPlayLpBonus(isFirstDecayedPlayInRun: true, alreadyGrantedInRun: 2, effects));
        }

        private static IReadOnlyList<CurriculumNodeInfo> GetCurriculumNodeDefs()
        {
            var defsField = typeof(MetaManagerV2).GetField("CurriculumNodeDefs", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(defsField);

            var defsObject = defsField.GetValue(null);
            Assert.NotNull(defsObject);

            var result = new List<CurriculumNodeInfo>();
            var enumerable = (System.Collections.IEnumerable)defsObject;
            foreach (var entry in enumerable)
            {
                var entryType = entry.GetType();
                var keyProp = entryType.GetProperty("Key");
                var valueProp = entryType.GetProperty("Value");
                Assert.NotNull(keyProp);
                Assert.NotNull(valueProp);

                var nodeId = (string)keyProp.GetValue(entry);
                var value = valueProp.GetValue(entry);
                var valueType = value.GetType();

                var cost = (int)valueType.GetProperty("Cost").GetValue(value);
                var groupsRaw = (System.Collections.IEnumerable)valueType.GetProperty("RequiredAnyOfGroups").GetValue(value);
                var mutexRaw = (System.Collections.IEnumerable)valueType.GetProperty("MutexWith").GetValue(value);

                var groups = new List<string[]>();
                foreach (var groupObj in groupsRaw)
                {
                    var groupItems = new List<string>();
                    foreach (var groupItem in (System.Collections.IEnumerable)groupObj)
                    {
                        groupItems.Add(groupItem.ToString());
                    }

                    groups.Add(groupItems.ToArray());
                }

                var mutex = new List<string>();
                foreach (var m in mutexRaw)
                {
                    mutex.Add(m.ToString());
                }

                result.Add(new CurriculumNodeInfo
                {
                    NodeId = nodeId,
                    Cost = cost,
                    RequiredAnyOfGroups = groups.ToArray(),
                    MutexWith = mutex.ToArray()
                });
            }

            Assert.AreEqual(60, result.Count, "Curriculum should define 4 branches x 12 levels (with A/B splits).");
            CollectionAssert.Contains(result.Select(x => x.NodeId), "FLU_12");
            CollectionAssert.Contains(result.Select(x => x.NodeId), "LEX_12");
            CollectionAssert.Contains(result.Select(x => x.NodeId), "BLD_12");
            CollectionAssert.Contains(result.Select(x => x.NodeId), "MAS_12");
            return result;
        }

        private sealed class CurriculumNodeInfo
        {
            public string NodeId { get; set; }
            public int Cost { get; set; }
            public string[][] RequiredAnyOfGroups { get; set; }
            public string[] MutexWith { get; set; }
        }
    }
}
