using System;
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
            return GenerateOffers(ante, seed, isBossShop, null);
        }

        public ServiceResult<IReadOnlyList<ShopOffer>> GenerateOffers(int ante, int seed, bool isBossShop, CurriculumEffectSnapshot effects)
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

            var offerSlots = System.Math.Max(1, OfferSlots + (effects?.NurtureCandidateExtraCount ?? 0));

            var weightedPool = Pool
                .Select(item =>
                {
                    var anteWeight = ResolveAnteWeight(item.Category, item.BaseWeight, ante);
                    if (effects != null)
                    {
                        anteWeight = ApplyEffectWeight(item.Id, item.Category, anteWeight, effects);
                    }
                    return (item, weight: anteWeight);
                })
                .Where(x => x.weight > 0)
                .ToList();

            var result = new List<ShopOffer>(offerSlots);
            var pickedIds = new HashSet<string>();
            while (result.Count < offerSlots && weightedPool.Count > 0)
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

        public ServiceResult<IReadOnlyList<ShopOfferCategory>> PreviewNextRefreshCategories(int ante, int seed, CurriculumEffectSnapshot effects)
        {
            var previewCount = effects?.NextRefreshPreviewCategoryCount ?? 0;
            if (previewCount <= 0)
            {
                return ServiceResult<IReadOnlyList<ShopOfferCategory>>.Ok(Array.Empty<ShopOfferCategory>());
            }

            var generated = GenerateOffers(ante, seed + 1, false, effects);
            if (!generated.IsSuccess)
            {
                return ServiceResult<IReadOnlyList<ShopOfferCategory>>.Fail(generated.Error);
            }

            var categories = generated.Value
                .Take(previewCount)
                .Select(x => x.Category)
                .ToArray();
            return ServiceResult<IReadOnlyList<ShopOfferCategory>>.Ok(categories);
        }

        public int GetTrainingCost(LearningLevel fromLevel, LearningLevel toLevel, int baseCost, CurriculumEffectSnapshot effects)
        {
            var discount = 0;
            if (effects != null)
            {
                if (fromLevel == LearningLevel.Lv1 && toLevel == LearningLevel.Lv2)
                {
                    discount = effects.Lv1To2TrainingDiscount;
                }
                else if (fromLevel == LearningLevel.Lv2 && toLevel == LearningLevel.Lv3)
                {
                    discount = effects.Lv2To3TrainingDiscount;
                }
            }

            return System.Math.Max(1, baseCost - discount);
        }

        public int GetNurtureCarryLockSlots(CurriculumEffectSnapshot effects)
        {
            return System.Math.Max(0, effects?.NurtureLockCarrySlots ?? 0);
        }

        public int GetFirstAnteShopCouponAmount(bool isFirstShopInAnte, CurriculumEffectSnapshot effects)
        {
            if (!isFirstShopInAnte || effects == null)
            {
                return 0;
            }

            return System.Math.Max(0, effects.FirstAnteShopCoupon);
        }

        public PackGuaranteeMode GetFirstPackGuaranteeMode(CurriculumEffectSnapshot effects)
        {
            return effects?.FirstPackGuaranteeMode ?? PackGuaranteeMode.None;
        }

        public int GetEffectiveOfferWeight(string offerId, int ante, CurriculumEffectSnapshot effects)
        {
            var item = Pool.FirstOrDefault(x => x.Id == offerId);
            if (string.IsNullOrWhiteSpace(item.Id))
            {
                return 0;
            }

            var weight = ResolveAnteWeight(item.Category, item.BaseWeight, ante);
            if (effects != null)
            {
                weight = ApplyEffectWeight(item.Id, item.Category, weight, effects);
            }

            return System.Math.Max(0, weight);
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

        private static int ApplyEffectWeight(string offerId, ShopOfferCategory category, int currentWeight, CurriculumEffectSnapshot effects)
        {
            var weight = currentWeight;
            if (category == ShopOfferCategory.Sense && effects.SenseOfferWeightBonusRate > 0f)
            {
                weight = (int)System.Math.Max(1, System.Math.Floor(weight * (1f + effects.SenseOfferWeightBonusRate)));
            }

            if (category == ShopOfferCategory.Affix &&
                offerId == "AFFIX_GOLD_PROCESS" &&
                effects.AffixToolWeightBonusRate > 0f)
            {
                weight = (int)System.Math.Max(1, System.Math.Floor(weight * (1f + effects.AffixToolWeightBonusRate)));
            }

            return weight;
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
            return PurchaseOffer(offer, currentMoney, null);
        }

        public ServiceResult<PurchaseResult> PurchaseOffer(ShopOffer offer, int currentMoney, CurriculumEffectSnapshot effects)
        {
            if (offer == null || string.IsNullOrWhiteSpace(offer.OfferId))
            {
                return ServiceResult<PurchaseResult>.Fail(ErrorCode.InvalidInput);
            }

            if (currentMoney < 0)
            {
                return ServiceResult<PurchaseResult>.Fail(ErrorCode.InvalidInput);
            }

            var finalPrice = offer.Price;
            if (effects != null && offer.Category == ShopOfferCategory.Material)
            {
                finalPrice = (int)System.Math.Max(1, System.Math.Floor(offer.Price * (1f - effects.MaterialPriceDiscountRate)));
            }

            if (currentMoney < finalPrice)
            {
                return ServiceResult<PurchaseResult>.Ok(new PurchaseResult
                {
                    Success = false,
                    Cost = finalPrice,
                    RemainingMoney = currentMoney,
                    LpRebate = 0,
                    OfferId = offer.OfferId,
                    Error = ErrorCode.StateConflict
                });
            }

            var lpRebate = 0;
            if (effects != null && offer.Category == ShopOfferCategory.Course)
            {
                lpRebate = System.Math.Max(0, effects.CourseLpRebate);
            }

            return ServiceResult<PurchaseResult>.Ok(new PurchaseResult
            {
                Success = true,
                Cost = finalPrice,
                RemainingMoney = currentMoney - finalPrice,
                LpRebate = lpRebate,
                OfferId = offer.OfferId,
                Error = ErrorCode.None
            });
        }

        public ServiceResult<int> GetRerollCost(int rerollCount)
        {
            return GetRerollCost(rerollCount, null, false, false);
        }

        public ServiceResult<int> GetRerollCost(int rerollCount, CurriculumEffectSnapshot effects, bool isFirstRerollInShop, bool contractJustCompleted)
        {
            if (rerollCount < 0)
            {
                return ServiceResult<int>.Fail(ErrorCode.InvalidInput);
            }

            if (effects != null && contractJustCompleted && effects.ResetNextRerollCostToFiveAfterContract)
            {
                return ServiceResult<int>.Ok(5);
            }

            var cost = RerollBaseCost + rerollCount * RerollCostStep;
            if (effects != null && isFirstRerollInShop)
            {
                cost = System.Math.Max(1, cost - effects.FirstShopRerollDiscount);
            }

            return ServiceResult<int>.Ok(cost);
        }
    }
}
