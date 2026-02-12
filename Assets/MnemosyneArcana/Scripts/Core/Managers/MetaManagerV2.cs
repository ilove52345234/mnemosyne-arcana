using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class MetaManagerV2 : IMetaProgressService, IContractService
    {
        public ServiceResult<IReadOnlyList<Contract>> GenerateContracts(MetaProgress meta, int seed)
        {
            return ServiceResult<IReadOnlyList<Contract>>.Fail(ErrorCode.NotImplemented);
        }

        public ServiceResult<ContractSettlement> SettleContract(Contract contract, RunTelemetry telemetry)
        {
            return ServiceResult<ContractSettlement>.Fail(ErrorCode.NotImplemented);
        }

        public ServiceResult<MetaSettlement> SettleRun(RunResult runResult, MetaProgress current)
        {
            return ServiceResult<MetaSettlement>.Fail(ErrorCode.NotImplemented);
        }

        public ServiceResult<UnlockResult> TryUnlockNode(string nodeId, MetaProgress current)
        {
            return ServiceResult<UnlockResult>.Fail(ErrorCode.NotImplemented);
        }
    }
}
