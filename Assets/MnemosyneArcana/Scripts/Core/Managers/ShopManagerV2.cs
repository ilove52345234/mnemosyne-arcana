using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class ShopManagerV2
    {
        public ServiceResult<IReadOnlyList<string>> GenerateOffers(int ante, int seed)
        {
            return ServiceResult<IReadOnlyList<string>>.Fail(ErrorCode.NotImplemented);
        }

        public ServiceResult<bool> PurchaseOffer(string offerId, int currentMoney)
        {
            return ServiceResult<bool>.Fail(ErrorCode.NotImplemented);
        }
    }
}
