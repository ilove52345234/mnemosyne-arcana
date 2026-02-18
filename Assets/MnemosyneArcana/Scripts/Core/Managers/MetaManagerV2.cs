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
            if (meta == null)
            {
                return ServiceResult<IReadOnlyList<Contract>>.Fail(ErrorCode.InvalidInput);
            }

            var rng = new Random(seed);
            var pool = new List<Contract>(ContractPool);
            var selected = new List<Contract>(ContractCount);

            for (int i = 0; i < ContractCount && pool.Count > 0; i++)
            {
                var index = rng.Next(pool.Count);
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
            if (contract == null || telemetry == null)
            {
                return ServiceResult<ContractSettlement>.Fail(ErrorCode.InvalidInput);
            }

            var completed = telemetry.ContractCompleted;
            var rawLp = completed ? contract.LpReward : 0;

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
    }
}
