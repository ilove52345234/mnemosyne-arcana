using System;
using System.Collections.Generic;
using System.Linq;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class MetaManagerV2 : IMetaProgressService, IContractService
    {
        private const int XpPerAnte = 20;
        private const int XpClearBonus = 50;
        private const int LpPerAnte = 2;
        private const int LpClearBonus = 5;
        private const int ContractCount = 3;

        private static readonly Contract[] ContractPool = new[]
        {
            new Contract { ContractId = "CT_NAT_001", Name = "語序達人", ContractType = "Natural", Tier = 1, LpReward = 6 },
            new Contract { ContractId = "CT_NAT_002", Name = "元素收集", ContractType = "Natural", Tier = 1, LpReward = 5 },
            new Contract { ContractId = "CT_NAT_003", Name = "高分挑戰", ContractType = "Natural", Tier = 2, LpReward = 8 },
            new Contract { ContractId = "CT_NAT_004", Name = "不重擲通關", ContractType = "Natural", Tier = 2, LpReward = 8 },
            new Contract { ContractId = "CT_LRN_001", Name = "詞彙復習", ContractType = "Learning", Tier = 1, LpReward = 5 },
            new Contract { ContractId = "CT_LRN_002", Name = "退化池回補", ContractType = "Learning", Tier = 2, LpReward = 9 },
            new Contract { ContractId = "CT_LRN_003", Name = "升級達人", ContractType = "Learning", Tier = 2, LpReward = 7 },
            new Contract { ContractId = "CT_LRN_004", Name = "全對挑戰", ContractType = "Learning", Tier = 3, LpReward = 12 },
            new Contract { ContractId = "CT_STY_001", Name = "冒險家", ContractType = "Style", Tier = 2, LpReward = 9 },
            new Contract { ContractId = "CT_STY_002", Name = "賭徒", ContractType = "Style", Tier = 3, LpReward = 14 },
            new Contract { ContractId = "CT_STY_003", Name = "極簡主義", ContractType = "Style", Tier = 3, LpReward = 15 },
        };

        private sealed class CurriculumNodeDef
        {
            public int Cost { get; set; }
            public IReadOnlyList<IReadOnlyList<string>> RequiredAnyOfGroups { get; set; } = Array.Empty<IReadOnlyList<string>>();
            public IReadOnlyList<string> MutexWith { get; set; } = Array.Empty<string>();
        }

        // 完整課程樹：4 分支 x 12 層（含 3/6/10 層 A/B 互斥）
        private static readonly IReadOnlyDictionary<string, CurriculumNodeDef> CurriculumNodeDefs = BuildCurriculumNodeDefs();

        private static IReadOnlyDictionary<string, CurriculumNodeDef> BuildCurriculumNodeDefs()
        {
            var defs = new Dictionary<string, CurriculumNodeDef>(StringComparer.Ordinal);
            AddBranch(defs, "FLU");
            AddBranch(defs, "LEX");
            AddBranch(defs, "BLD");
            AddBranch(defs, "MAS");
            return defs;
        }

        private static void AddBranch(IDictionary<string, CurriculumNodeDef> defs, string branch)
        {
            var n01 = $"{branch}_01";
            var n02 = $"{branch}_02";
            var n03a = $"{branch}_03A";
            var n03b = $"{branch}_03B";
            var n04 = $"{branch}_04";
            var n05 = $"{branch}_05";
            var n06a = $"{branch}_06A";
            var n06b = $"{branch}_06B";
            var n07 = $"{branch}_07";
            var n08 = $"{branch}_08";
            var n09 = $"{branch}_09";
            var n10a = $"{branch}_10A";
            var n10b = $"{branch}_10B";
            var n11 = $"{branch}_11";
            var n12 = $"{branch}_12";

            defs[n01] = new CurriculumNodeDef { Cost = 20 };
            defs[n02] = new CurriculumNodeDef { Cost = 25, RequiredAnyOfGroups = AnyOf(n01) };
            defs[n03a] = new CurriculumNodeDef { Cost = 30, RequiredAnyOfGroups = AnyOf(n02), MutexWith = new[] { n03b } };
            defs[n03b] = new CurriculumNodeDef { Cost = 30, RequiredAnyOfGroups = AnyOf(n02), MutexWith = new[] { n03a } };
            defs[n04] = new CurriculumNodeDef { Cost = 35, RequiredAnyOfGroups = AnyOf(n03a, n03b) };
            defs[n05] = new CurriculumNodeDef { Cost = 35, RequiredAnyOfGroups = AnyOf(n04) };
            defs[n06a] = new CurriculumNodeDef { Cost = 40, RequiredAnyOfGroups = AnyOf(n05), MutexWith = new[] { n06b } };
            defs[n06b] = new CurriculumNodeDef { Cost = 40, RequiredAnyOfGroups = AnyOf(n05), MutexWith = new[] { n06a } };
            defs[n07] = new CurriculumNodeDef { Cost = 45, RequiredAnyOfGroups = AnyOf(n06a, n06b) };
            defs[n08] = new CurriculumNodeDef { Cost = 50, RequiredAnyOfGroups = AnyOf(n07) };
            defs[n09] = new CurriculumNodeDef { Cost = 55, RequiredAnyOfGroups = AnyOf(n08) };
            defs[n10a] = new CurriculumNodeDef { Cost = 60, RequiredAnyOfGroups = AnyOf(n09), MutexWith = new[] { n10b } };
            defs[n10b] = new CurriculumNodeDef { Cost = 60, RequiredAnyOfGroups = AnyOf(n09), MutexWith = new[] { n10a } };
            defs[n11] = new CurriculumNodeDef { Cost = 65, RequiredAnyOfGroups = AnyOf(n10a, n10b) };
            defs[n12] = new CurriculumNodeDef { Cost = 70, RequiredAnyOfGroups = AnyOf(n11) };
        }

        private static IReadOnlyList<IReadOnlyList<string>> AnyOf(params string[] nodes)
        {
            return new IReadOnlyList<string>[] { nodes };
        }

        public ServiceResult<MetaSettlement> SettleRun(RunResult runResult, MetaProgress current)
        {
            if (runResult == null || current == null)
            {
                return ServiceResult<MetaSettlement>.Fail(ErrorCode.InvalidInput);
            }

            var xp = runResult.HighestAnte * XpPerAnte + (runResult.IsClear ? XpClearBonus : 0);
            var lpBase = runResult.HighestAnte * LpPerAnte + (runResult.IsClear ? LpClearBonus : 0);

            return ServiceResult<MetaSettlement>.Ok(new MetaSettlement
            {
                XpGained = xp,
                LpGainedBase = lpBase,
                LpGainedContract = 0,
                LpGainedTotal = lpBase
            });
        }

        public ServiceResult<IReadOnlyList<Contract>> GenerateContracts(MetaProgress meta, int seed)
        {
            return GenerateContracts(meta, seed, null);
        }

        public ServiceResult<IReadOnlyList<Contract>> GenerateContracts(MetaProgress meta, int seed, CurriculumEffectSnapshot effects)
        {
            if (meta == null)
            {
                return ServiceResult<IReadOnlyList<Contract>>.Fail(ErrorCode.InvalidInput);
            }

            var rng = new Random(seed);
            var pool = new List<Contract>(ContractPool);
            var selected = new List<Contract>(ContractCount);

            for (int i = 0; i < ContractCount && pool.Count > 0; i++)
            {
                var index = RollWeightedContractIndex(rng, pool, effects);
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return ServiceResult<IReadOnlyList<Contract>>.Ok(selected);
        }

        public ServiceResult<ContractSettlement> SettleContract(Contract contract, RunTelemetry telemetry)
        {
            if (contract == null || telemetry == null)
            {
                return ServiceResult<ContractSettlement>.Fail(ErrorCode.InvalidInput);
            }

            var completed = telemetry.ContractCompleted;
            var rawLp = completed ? contract.LpReward : 0;

            return ServiceResult<ContractSettlement>.Ok(new ContractSettlement
            {
                ContractId = contract.ContractId,
                Completed = completed,
                LpBonusRaw = rawLp,
                LpBonusCapped = rawLp,
                CapApplied = false
            });
        }

        public ServiceResult<ContractSettlement> SettleContractWithCap(Contract contract, RunTelemetry telemetry, int lpBase)
        {
            return SettleContractWithCap(contract, telemetry, lpBase, null);
        }

        public ServiceResult<ContractSettlement> SettleContractWithCap(Contract contract, RunTelemetry telemetry, int lpBase, CurriculumEffectSnapshot effects)
        {
            if (contract == null || telemetry == null)
            {
                return ServiceResult<ContractSettlement>.Fail(ErrorCode.InvalidInput);
            }

            var completed = telemetry.ContractCompleted;
            var rawLp = completed ? contract.LpReward : 0;
            if (completed && effects != null)
            {
                if (string.Equals(contract.ContractType, "Learning", StringComparison.OrdinalIgnoreCase))
                {
                    rawLp = (int)Math.Floor(rawLp * (1f + effects.LearningContractLpBonusRate));
                }

                if (string.Equals(contract.ContractType, "Mastery", StringComparison.OrdinalIgnoreCase))
                {
                    rawLp = (int)Math.Floor(rawLp * (1f + effects.MasteryContractLpBonusRate));
                }
            }

            var capLimit = lpBase > 0 ? (int)(lpBase * 45L / 55) : 0;
            var cappedLp = Math.Min(rawLp, capLimit);
            var capApplied = cappedLp < rawLp;

            return ServiceResult<ContractSettlement>.Ok(new ContractSettlement
            {
                ContractId = contract.ContractId,
                Completed = completed,
                LpBonusRaw = rawLp,
                LpBonusCapped = cappedLp,
                CapApplied = capApplied
            });
        }

        public ServiceResult<UnlockResult> TryUnlockNode(string nodeId, MetaProgress current)
        {
            if (string.IsNullOrWhiteSpace(nodeId) || current == null)
            {
                return ServiceResult<UnlockResult>.Fail(ErrorCode.InvalidInput);
            }

            if (!CurriculumNodeDefs.TryGetValue(nodeId, out var nodeDef))
            {
                return ServiceResult<UnlockResult>.Fail(ErrorCode.InvalidInput);
            }

            var unlocked = new HashSet<string>(current.CurriculumNodes ?? Array.Empty<string>());
            if (unlocked.Contains(nodeId))
            {
                return ServiceResult<UnlockResult>.Fail(ErrorCode.StateConflict);
            }

            foreach (var mutexNode in nodeDef.MutexWith)
            {
                if (unlocked.Contains(mutexNode))
                {
                    return ServiceResult<UnlockResult>.Fail(ErrorCode.StateConflict);
                }
            }

            foreach (var group in nodeDef.RequiredAnyOfGroups)
            {
                if (!group.Any(unlocked.Contains))
                {
                    return ServiceResult<UnlockResult>.Fail(ErrorCode.StateConflict);
                }
            }

            if (current.Lp < nodeDef.Cost)
            {
                return ServiceResult<UnlockResult>.Fail(ErrorCode.StateConflict);
            }

            unlocked.Add(nodeId);
            return ServiceResult<UnlockResult>.Ok(new UnlockResult
            {
                Success = true,
                NodeId = nodeId,
                SpentLp = nodeDef.Cost,
                RemainingLp = current.Lp - nodeDef.Cost,
                Error = ErrorCode.None,
                UnlockedNodes = unlocked.OrderBy(x => x).ToArray()
            });
        }

        public ServiceResult<CurriculumEffectSnapshot> GetCurriculumEffects(MetaProgress current)
        {
            if (current == null)
            {
                return ServiceResult<CurriculumEffectSnapshot>.Fail(ErrorCode.InvalidInput);
            }

            var snapshot = new CurriculumEffectSnapshot();
            var unlocked = current.CurriculumNodes ?? Array.Empty<string>();

            foreach (var node in unlocked)
            {
                if (!CurriculumNodeDefs.ContainsKey(node))
                {
                    continue;
                }

                snapshot.UnlockedNodeCount++;
                switch (node)
                {
                    case "FLU_01": snapshot.Lv1Lv2TimeBonusSec += 0.2f; break;
                    case "FLU_02": snapshot.WrongPenaltyReductionRate += 0.10f; break;
                    case "FLU_06A": snapshot.ListeningTimeBonusSec += 0.2f; break;
                    case "FLU_08": snapshot.RetryCostDiscount += 1; break;
                    case "FLU_10B": snapshot.WrongPenaltyReductionRate += 0.15f; break;
                    case "FLU_10A": snapshot.BossTimeBonusRate += 0.10f; break;
                    case "FLU_11": snapshot.LearningContractLpBonusRate += 0.10f; break;

                    case "LEX_01": snapshot.DecayedPoolWeightBonusRate += 0.10f; break;
                    case "LEX_02": snapshot.StaleWordWeightBonusRate += 0.20f; break;
                    case "LEX_09": snapshot.LexiconUnlockLpCostDiscountRate += 0.08f; break;
                    case "LEX_10A": snapshot.LexiconUnlockRunRequirementDiscountRate += 0.10f; break;
                    case "LEX_10B": snapshot.LexiconUnlockCoverageDiscountRate += 0.05f; break;
                    case "LEX_11": snapshot.LearningContractQualityBonusRate += 0.20f; break;
                    case "LEX_03A": snapshot.ShortWordDropBiasRate += 0.08f; break;
                    case "LEX_03B": snapshot.LongWordDropBiasRate += 0.12f; break;

                    case "BLD_04": snapshot.FirstShopRerollDiscount += 2; break;
                    case "BLD_05": snapshot.MaterialPriceDiscountRate += 0.10f; break;
                    case "BLD_08": snapshot.FirstAnteShopCoupon += 2; break;
                    case "BLD_11": snapshot.ResetNextRerollCostToFiveAfterContract = true; break;
                    case "BLD_12": snapshot.FirstLv4UpgradeMoneyRefund += 2; break;
                    case "BLD_09": snapshot.CourseLpRebate += 1; break;

                    case "MAS_01": snapshot.Lv4CardFlatChipBonus += 2; break;
                    case "MAS_02": snapshot.FirstTwoLv4CardsAdditiveMultBonus += 1; break;
                    case "MAS_03A": snapshot.Lv4DecayProtectionLayers += 1; break;
                    case "MAS_06A": snapshot.Lv4ConcentratedBuildMultiplierBonusRate += 0.08f; break;
                    case "MAS_06B": snapshot.Lv4BalancedBuildMultiplierBonusRate += 0.08f; break;
                    case "MAS_10A": snapshot.MasteryContractLpBonusRate += 0.15f; break;
                    case "MAS_10B": snapshot.MasteryContractRequirementReduction += 1; break;
                    case "MAS_08": snapshot.IgnoreFirstLv4WrongHandMultPenalty = true; break;
                }
            }

            return ServiceResult<CurriculumEffectSnapshot>.Ok(snapshot);
        }

        public ServiceResult<LexiconUnlockRequirement> GetLexiconUnlockRequirement(
            int baseLpCost,
            int baseRequiredRuns,
            float baseRequiredCoverageRate,
            CurriculumEffectSnapshot effects)
        {
            if (baseLpCost < 0 || baseRequiredRuns < 0 || baseRequiredCoverageRate < 0f)
            {
                return ServiceResult<LexiconUnlockRequirement>.Fail(ErrorCode.InvalidInput);
            }

            var lpCost = baseLpCost;
            var requiredRuns = baseRequiredRuns;
            var requiredCoverageRate = baseRequiredCoverageRate;

            if (effects != null)
            {
                var lpPercent = Math.Max(0, 100 - (int)Math.Round(effects.LexiconUnlockLpCostDiscountRate * 100f, MidpointRounding.AwayFromZero));
                var runPercent = Math.Max(0, 100 - (int)Math.Round(effects.LexiconUnlockRunRequirementDiscountRate * 100f, MidpointRounding.AwayFromZero));

                lpCost = (int)Math.Max(0, Math.Round(baseLpCost * lpPercent / 100.0, MidpointRounding.AwayFromZero));
                requiredRuns = (int)Math.Max(0, Math.Round(baseRequiredRuns * runPercent / 100.0, MidpointRounding.AwayFromZero));
                requiredCoverageRate = Math.Max(0f, baseRequiredCoverageRate * (1f - effects.LexiconUnlockCoverageDiscountRate));
            }

            return ServiceResult<LexiconUnlockRequirement>.Ok(new LexiconUnlockRequirement
            {
                LpCost = lpCost,
                RequiredRuns = requiredRuns,
                RequiredCoverageRate = requiredCoverageRate
            });
        }

        public ServiceResult<IReadOnlyList<WeightedWordCandidate>> BuildLexiconDropWeights(
            IReadOnlyList<WordProgress> words,
            IReadOnlyDictionary<string, int> staleRunCounts,
            CurriculumEffectSnapshot effects)
        {
            return BuildLexiconDropWeights(words, staleRunCounts, null, effects);
        }

        public ServiceResult<IReadOnlyList<WeightedWordCandidate>> BuildLexiconDropWeights(
            IReadOnlyList<WordProgress> words,
            IReadOnlyDictionary<string, int> staleRunCounts,
            IReadOnlyDictionary<string, int> wordLengths,
            CurriculumEffectSnapshot effects)
        {
            if (words == null)
            {
                return ServiceResult<IReadOnlyList<WeightedWordCandidate>>.Fail(ErrorCode.InvalidInput);
            }

            var result = new List<WeightedWordCandidate>(words.Count);
            for (var i = 0; i < words.Count; i++)
            {
                var word = words[i];
                var weight = 100f;
                if (effects != null)
                {
                    if (word.Pool == WordPool.Decayed)
                    {
                        weight *= (1f + effects.DecayedPoolWeightBonusRate);
                    }

                    if (staleRunCounts != null &&
                        staleRunCounts.TryGetValue(word.WordId, out var staleRuns) &&
                        staleRuns >= 3)
                    {
                        weight *= (1f + effects.StaleWordWeightBonusRate);
                    }

                    if (wordLengths != null && wordLengths.TryGetValue(word.WordId, out var wordLength))
                    {
                        if (wordLength >= 3 && wordLength <= 5)
                        {
                            weight *= (1f + effects.ShortWordDropBiasRate);
                        }
                        else if (wordLength >= 6)
                        {
                            weight *= (1f + effects.LongWordDropBiasRate);
                        }
                    }
                }

                result.Add(new WeightedWordCandidate
                {
                    WordId = word.WordId,
                    Weight = (int)Math.Max(1, Math.Floor(weight))
                });
            }

            return ServiceResult<IReadOnlyList<WeightedWordCandidate>>.Ok(result);
        }

        public ServiceResult<int> GetContractRequirementAfterCurriculum(
            int baseRequirement,
            Contract contract,
            CurriculumEffectSnapshot effects)
        {
            if (baseRequirement < 0 || contract == null)
            {
                return ServiceResult<int>.Fail(ErrorCode.InvalidInput);
            }

            var required = baseRequirement;
            if (effects != null &&
                string.Equals(contract.ContractType, "Mastery", StringComparison.OrdinalIgnoreCase))
            {
                required = Math.Max(1, baseRequirement - effects.MasteryContractRequirementReduction);
            }

            return ServiceResult<int>.Ok(required);
        }

        private static int RollWeightedContractIndex(Random rng, IReadOnlyList<Contract> pool, CurriculumEffectSnapshot effects)
        {
            if (effects == null || effects.LearningContractQualityBonusRate <= 0f)
            {
                return rng.Next(pool.Count);
            }

            var weights = new int[pool.Count];
            var total = 0;
            for (var i = 0; i < pool.Count; i++)
            {
                var weight = 100;
                var contract = pool[i];
                if (string.Equals(contract.ContractType, "Learning", StringComparison.OrdinalIgnoreCase))
                {
                    var tierWeight = 1f + (contract.Tier - 1) * effects.LearningContractQualityBonusRate;
                    weight = (int)Math.Max(1, Math.Floor(weight * tierWeight));
                }

                weights[i] = weight;
                total += weight;
            }

            var roll = rng.Next(total);
            var acc = 0;
            for (var i = 0; i < pool.Count; i++)
            {
                acc += weights[i];
                if (roll < acc)
                {
                    return i;
                }
            }

            return pool.Count - 1;
        }
    }
}
