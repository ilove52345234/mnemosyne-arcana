using System;
using System.Collections.Generic;
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
            return ServiceResult<UnlockResult>.Fail(ErrorCode.NotImplemented);
        }
    }
}
