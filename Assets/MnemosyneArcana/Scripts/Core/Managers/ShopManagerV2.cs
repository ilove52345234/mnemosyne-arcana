using System.Collections.Generic;
using System.Linq;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class ShopManagerV2
    {
        private const int OfferSlots = 5;
        private const int BossCourseSlots = 2;
        private const int RerollBaseCost = 1;
        private const int RerollCostStep = 1;

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

        public ServiceResult<IReadOnlyList<ShopOffer>> GenerateOffers(int ante, int seed, bool isBossShop = false)
        {
            if (ante < 1)
            {
                return ServiceResult<IReadOnlyList<ShopOffer>>.Fail(ErrorCode.InvalidInput);
            }

            var random = new System.Random(seed + ante * 9973);
            if (isBossShop)
            {
                return ServiceResult<IReadOnlyList<ShopOffer>>.Ok(GenerateBossCourseOffers(random));
            }

            var weightedPool = Pool
                .Select(item =>
                {
                    var anteWeight = ResolveAnteWeight(item.Category, item.BaseWeight, ante);
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

        private static int ResolveAnteWeight(ShopOfferCategory category, int baseWeight, int ante)
        {
            // Ante 1-2: 只出現機制與基礎養成，不出課程卡
            if (ante <= 2)
            {
                return category switch
                {
                    ShopOfferCategory.Material => baseWeight + 4,
                    ShopOfferCategory.Affix => baseWeight + 4,
                    ShopOfferCategory.Sense => baseWeight - 2,
                    ShopOfferCategory.Course => 0,
                    _ => baseWeight
                };
            }

            // Ante 3-5: 漸進提高語感比例，課程卡仍保留給 Boss 商店
            if (ante <= 5)
            {
                return category switch
                {
                    ShopOfferCategory.Material => baseWeight + 2,
                    ShopOfferCategory.Sense => baseWeight + 2,
                    ShopOfferCategory.Affix => baseWeight,
                    ShopOfferCategory.Course => 0,
                    _ => baseWeight
                };
            }

            // Ante 6-8: 完整詞條池，課程卡可低機率出現
            return category switch
            {
                ShopOfferCategory.Course => baseWeight,
                ShopOfferCategory.Sense => baseWeight + 2,
                ShopOfferCategory.Material => baseWeight + 1,
                _ => baseWeight
            };
        }

        private static IReadOnlyList<ShopOffer> GenerateBossCourseOffers(System.Random random)
        {
            var courses = Pool.Where(x => x.Category == ShopOfferCategory.Course).ToList();
            var result = new List<ShopOffer>(BossCourseSlots);
            var picked = new HashSet<string>();

            while (result.Count < BossCourseSlots && courses.Count > 0)
            {
                var idx = random.Next(courses.Count);
                var selected = courses[idx];
                courses.RemoveAt(idx);

                if (!picked.Add(selected.Id))
                {
                    continue;
                }

                result.Add(new ShopOffer
                {
                    OfferId = selected.Id,
                    Category = selected.Category,
                    Price = 10,
                    Weight = selected.BaseWeight
                });
            }

            return result;
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

        public ServiceResult<int> GetRerollCost(int rerollCount)
        {
            if (rerollCount < 0)
            {
                return ServiceResult<int>.Fail(ErrorCode.InvalidInput);
            }

            return ServiceResult<int>.Ok(RerollBaseCost + rerollCount * RerollCostStep);
        }
    }
}
