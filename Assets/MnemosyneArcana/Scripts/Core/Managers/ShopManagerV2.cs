using System.Collections.Generic;
using System.Linq;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class ShopManagerV2
    {
        private const int OfferSlots = 5;

        private static readonly IReadOnlyList<(string Id, ShopOfferCategory Category, int MinPrice, int MaxPrice, int BaseWeight)> Pool =
            new List<(string Id, ShopOfferCategory Category, int MinPrice, int MaxPrice, int BaseWeight)>
            {
                ("SENSE_GRAMMAR_INTUITION", ShopOfferCategory.Sense, 4, 8, 10),
                ("SENSE_POS_RADAR", ShopOfferCategory.Sense, 4, 8, 10),
                ("SENSE_ELEMENT_RESONANCE", ShopOfferCategory.Sense, 4, 8, 10),
                ("MAT_ENGLISH_GRAMMAR", ShopOfferCategory.Material, 3, 6, 14),
                ("MAT_TOPIC_READING", ShopOfferCategory.Material, 3, 6, 14),
                ("MAT_ADV_GRAMMAR", ShopOfferCategory.Material, 3, 6, 12),
                ("AFFIX_SUFFIX_LY", ShopOfferCategory.Affix, 2, 4, 18),
                ("AFFIX_PREFIX_RE", ShopOfferCategory.Affix, 2, 4, 18),
                ("AFFIX_GOLD_PROCESS", ShopOfferCategory.Affix, 2, 4, 12),
                ("COURSE_FAST_TRACK", ShopOfferCategory.Course, 10, 10, 4),
                ("COURSE_DEEP_STUDY", ShopOfferCategory.Course, 10, 10, 4)
            };

        public ServiceResult<IReadOnlyList<ShopOffer>> GenerateOffers(int ante, int seed)
        {
            if (ante < 1)
            {
                return ServiceResult<IReadOnlyList<ShopOffer>>.Fail(ErrorCode.InvalidInput);
            }

            var random = new System.Random(seed + ante * 9973);
            var weightedPool = Pool
                .Select(item =>
                {
                    var anteWeight = item.Category == ShopOfferCategory.Course ? (ante >= 2 ? item.BaseWeight : 0) : item.BaseWeight;
                    return (item, weight: anteWeight);
                })
                .Where(x => x.weight > 0)
                .ToList();

            var result = new List<ShopOffer>(OfferSlots);
            var pickedIds = new HashSet<string>();
            while (result.Count < OfferSlots && weightedPool.Count > 0)
            {
                var totalWeight = weightedPool.Sum(x => x.weight);
                var roll = random.Next(0, totalWeight);
                var accum = 0;
                (string Id, ShopOfferCategory Category, int MinPrice, int MaxPrice, int BaseWeight) picked = default;

                foreach (var candidate in weightedPool)
                {
                    accum += candidate.weight;
                    if (roll < accum)
                    {
                        picked = candidate.item;
                        break;
                    }
                }

                if (pickedIds.Contains(picked.Id))
                {
                    weightedPool.RemoveAll(x => x.item.Id == picked.Id);
                    continue;
                }

                var price = random.Next(picked.MinPrice, picked.MaxPrice + 1);
                result.Add(new ShopOffer
                {
                    OfferId = picked.Id,
                    Category = picked.Category,
                    Price = price,
                    Weight = weightedPool.First(x => x.item.Id == picked.Id).weight
                });
                pickedIds.Add(picked.Id);
                weightedPool.RemoveAll(x => x.item.Id == picked.Id);
            }

            return ServiceResult<IReadOnlyList<ShopOffer>>.Ok(result);
        }

        public ServiceResult<PurchaseResult> PurchaseOffer(ShopOffer offer, int currentMoney)
        {
            if (offer == null || string.IsNullOrWhiteSpace(offer.OfferId))
            {
                return ServiceResult<PurchaseResult>.Fail(ErrorCode.InvalidInput);
            }

            if (currentMoney < 0)
            {
                return ServiceResult<PurchaseResult>.Fail(ErrorCode.InvalidInput);
            }

            if (currentMoney < offer.Price)
            {
                return ServiceResult<PurchaseResult>.Ok(new PurchaseResult
                {
                    Success = false,
                    Cost = offer.Price,
                    RemainingMoney = currentMoney,
                    OfferId = offer.OfferId,
                    Error = ErrorCode.StateConflict
                });
            }

            return ServiceResult<PurchaseResult>.Ok(new PurchaseResult
            {
                Success = true,
                Cost = offer.Price,
                RemainingMoney = currentMoney - offer.Price,
                OfferId = offer.OfferId,
                Error = ErrorCode.None
            });
        }
    }
}
