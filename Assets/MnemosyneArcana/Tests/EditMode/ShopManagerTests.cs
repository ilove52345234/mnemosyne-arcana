using System.Linq;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    public class ShopManagerTests
    {
        [Test]
        public void GenerateOffers_SameSeed_IsDeterministic()
        {
            var manager = new ShopManagerV2();
            var first = manager.GenerateOffers(2, 12345);
            var second = manager.GenerateOffers(2, 12345);

            Assert.IsTrue(first.IsSuccess);
            Assert.IsTrue(second.IsSuccess);
            Assert.AreEqual(5, first.Value.Count);
            Assert.AreEqual(5, second.Value.Count);
            CollectionAssert.AreEqual(first.Value.Select(x => x.OfferId), second.Value.Select(x => x.OfferId));
            CollectionAssert.AreEqual(first.Value.Select(x => x.Price), second.Value.Select(x => x.Price));
        }

        [Test]
        public void GenerateOffers_Ante1_HasNoCourseOffer()
        {
            var manager = new ShopManagerV2();
            var result = manager.GenerateOffers(1, 7);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.Any(x => x.Category == ShopOfferCategory.Course));
        }

        [Test]
        public void GenerateOffers_BossShop_ReturnsTwoCourseChoices()
        {
            var manager = new ShopManagerV2();
            var result = manager.GenerateOffers(4, 2026, isBossShop: true);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(2, result.Value.Count);
            Assert.IsTrue(result.Value.All(x => x.Category == ShopOfferCategory.Course));
            Assert.IsTrue(result.Value.All(x => x.Price == 10));
            Assert.AreNotEqual(result.Value[0].OfferId, result.Value[1].OfferId);
        }

        [Test]
        public void GenerateOffers_Prices_StayWithinConfiguredBands()
        {
            var manager = new ShopManagerV2();
            var result = manager.GenerateOffers(6, 33);

            Assert.IsTrue(result.IsSuccess);
            foreach (var offer in result.Value)
            {
                switch (offer.Category)
                {
                    case ShopOfferCategory.Sense:
                        Assert.That(offer.Price, Is.InRange(4, 8));
                        break;
                    case ShopOfferCategory.Material:
                        Assert.That(offer.Price, Is.InRange(3, 6));
                        break;
                    case ShopOfferCategory.Affix:
                        Assert.That(offer.Price, Is.InRange(2, 4));
                        break;
                    case ShopOfferCategory.Course:
                        Assert.AreEqual(10, offer.Price);
                        break;
                }
            }
        }

        [Test]
        public void PurchaseOffer_EnoughMoney_Succeeds()
        {
            var manager = new ShopManagerV2();
            var offer = new ShopOffer
            {
                OfferId = "MAT_ENGLISH_GRAMMAR",
                Category = ShopOfferCategory.Material,
                Price = 5
            };

            var result = manager.PurchaseOffer(offer, 12);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Success);
            Assert.AreEqual(7, result.Value.RemainingMoney);
        }

        [Test]
        public void PurchaseOffer_NotEnoughMoney_FailsGracefully()
        {
            var manager = new ShopManagerV2();
            var offer = new ShopOffer
            {
                OfferId = "SENSE_POS_RADAR",
                Category = ShopOfferCategory.Sense,
                Price = 8
            };

            var result = manager.PurchaseOffer(offer, 3);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.Value.Success);
            Assert.AreEqual(ErrorCode.StateConflict, result.Value.Error);
            Assert.AreEqual(3, result.Value.RemainingMoney);
        }

        [Test]
        public void GetRerollCost_TwentyRolls_IsStrictlyIncreasing()
        {
            var manager = new ShopManagerV2();
            var previous = 0;

            for (var i = 0; i < 20; i++)
            {
                var cost = manager.GetRerollCost(i);
                Assert.IsTrue(cost.IsSuccess);
                Assert.Greater(cost.Value, previous);
                previous = cost.Value;
            }
        }

        [Test]
        public void RerollEconomy_Budget80_CannotSustainTwentyRollsAndLosesBuyWindows()
        {
            var manager = new ShopManagerV2();
            var money = 80;
            var performedRerolls = 0;
            var canBuyAnyOfferRounds = 0;

            for (var i = 0; i < 20; i++)
            {
                var rerollCost = manager.GetRerollCost(i);
                Assert.IsTrue(rerollCost.IsSuccess);

                if (money < rerollCost.Value)
                {
                    break;
                }

                money -= rerollCost.Value;
                performedRerolls++;

                var offers = manager.GenerateOffers(ante: 6, seed: 9000 + i, isBossShop: false);
                Assert.IsTrue(offers.IsSuccess);

                if (offers.Value.Any(x => x.Price <= money))
                {
                    canBuyAnyOfferRounds++;
                }
            }

            Assert.Less(performedRerolls, 20);
            Assert.GreaterOrEqual(performedRerolls, 8);
            Assert.Less(canBuyAnyOfferRounds, performedRerolls);
        }

        [Test]
        public void GetRerollCost_WithBuildEffects_AppliesFirstDiscountAndContractReset()
        {
            var manager = new ShopManagerV2();
            var effects = new CurriculumEffectSnapshot
            {
                FirstShopRerollDiscount = 2,
                ResetNextRerollCostToFiveAfterContract = true
            };

            var first = manager.GetRerollCost(0, effects, isFirstRerollInShop: true, contractJustCompleted: false);
            var reset = manager.GetRerollCost(10, effects, isFirstRerollInShop: false, contractJustCompleted: true);

            Assert.IsTrue(first.IsSuccess);
            Assert.AreEqual(1, first.Value);
            Assert.IsTrue(reset.IsSuccess);
            Assert.AreEqual(5, reset.Value);
        }

        [Test]
        public void PurchaseOffer_MaterialDiscount_FromBuildNode()
        {
            var manager = new ShopManagerV2();
            var offer = new ShopOffer
            {
                OfferId = "MAT_TOPIC_READING",
                Category = ShopOfferCategory.Material,
                Price = 6
            };

            var effects = new CurriculumEffectSnapshot { MaterialPriceDiscountRate = 0.10f };
            var result = manager.PurchaseOffer(offer, currentMoney: 6, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Success);
            Assert.AreEqual(5, result.Value.Cost);
            Assert.AreEqual(1, result.Value.RemainingMoney);
        }

        [Test]
        public void PurchaseOffer_CourseAppliesLpRebate_FromBuildNode()
        {
            var manager = new ShopManagerV2();
            var offer = new ShopOffer
            {
                OfferId = "COURSE_FAST_TRACK",
                Category = ShopOfferCategory.Course,
                Price = 10
            };

            var effects = new CurriculumEffectSnapshot { CourseLpRebate = 1 };
            var result = manager.PurchaseOffer(offer, currentMoney: 10, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.Success);
            Assert.AreEqual(1, result.Value.LpRebate);
        }

        [Test]
        public void GenerateOffers_WithBld02_IncreasesOfferSlotsByOne()
        {
            var manager = new ShopManagerV2();
            var effects = new CurriculumEffectSnapshot { NurtureCandidateExtraCount = 1 };
            var result = manager.GenerateOffers(ante: 4, seed: 2026, isBossShop: false, effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(6, result.Value.Count);
        }

        [Test]
        public void PreviewNextRefreshCategories_WithBld01_ReturnsOneCategory()
        {
            var manager = new ShopManagerV2();
            var effects = new CurriculumEffectSnapshot { NextRefreshPreviewCategoryCount = 1 };
            var preview = manager.PreviewNextRefreshCategories(ante: 3, seed: 100, effects);

            Assert.IsTrue(preview.IsSuccess);
            Assert.AreEqual(1, preview.Value.Count);
        }

        [Test]
        public void GetTrainingCost_WithBld03_DiscountsMatchingTransitions()
        {
            var manager = new ShopManagerV2();
            var effects = new CurriculumEffectSnapshot
            {
                Lv1To2TrainingDiscount = 1,
                Lv2To3TrainingDiscount = 1
            };

            var lv1To2 = manager.GetTrainingCost(LearningLevel.Lv1, LearningLevel.Lv2, baseCost: 4, effects);
            var lv2To3 = manager.GetTrainingCost(LearningLevel.Lv2, LearningLevel.Lv3, baseCost: 5, effects);
            var lv0To1 = manager.GetTrainingCost(LearningLevel.Lv0, LearningLevel.Lv1, baseCost: 3, effects);

            Assert.AreEqual(3, lv1To2);
            Assert.AreEqual(4, lv2To3);
            Assert.AreEqual(3, lv0To1);
        }

        [Test]
        public void GetEffectiveOfferWeight_WithBld06AAnd06B_AppliesCategoryBonuses()
        {
            var manager = new ShopManagerV2();
            var effects = new CurriculumEffectSnapshot
            {
                SenseOfferWeightBonusRate = 0.08f,
                AffixToolWeightBonusRate = 0.12f
            };

            var senseWeight = manager.GetEffectiveOfferWeight("SENSE_POS_RADAR", ante: 6, effects);
            var affixWeight = manager.GetEffectiveOfferWeight("AFFIX_GOLD_PROCESS", ante: 6, effects);
            var otherAffixWeight = manager.GetEffectiveOfferWeight("AFFIX_PREFIX_RE", ante: 6, effects);

            Assert.AreEqual(12, senseWeight);
            Assert.AreEqual(13, affixWeight);
            Assert.AreEqual(18, otherAffixWeight);
        }

        [Test]
        public void GetNurtureCarryLockSlots_WithBld07_ReturnsAdditionalCarrySlot()
        {
            var manager = new ShopManagerV2();
            var effects = new CurriculumEffectSnapshot { NurtureLockCarrySlots = 1 };

            var slots = manager.GetNurtureCarryLockSlots(effects);

            Assert.AreEqual(1, slots);
        }

        [Test]
        public void GetFirstAnteShopCouponAmount_WithBld08_OnlyAppliesOnFirstShop()
        {
            var manager = new ShopManagerV2();
            var effects = new CurriculumEffectSnapshot { FirstAnteShopCoupon = 2 };

            Assert.AreEqual(2, manager.GetFirstAnteShopCouponAmount(true, effects));
            Assert.AreEqual(0, manager.GetFirstAnteShopCouponAmount(false, effects));
        }

        [Test]
        public void GetFirstPackGuaranteeMode_WithBld10_ReturnsConfiguredGuarantee()
        {
            var manager = new ShopManagerV2();
            var learning = new CurriculumEffectSnapshot { FirstPackGuaranteeMode = PackGuaranteeMode.LearningTool };
            var build = new CurriculumEffectSnapshot { FirstPackGuaranteeMode = PackGuaranteeMode.BuildTool };

            Assert.AreEqual(PackGuaranteeMode.LearningTool, manager.GetFirstPackGuaranteeMode(learning));
            Assert.AreEqual(PackGuaranteeMode.BuildTool, manager.GetFirstPackGuaranteeMode(build));
            Assert.AreEqual(PackGuaranteeMode.None, manager.GetFirstPackGuaranteeMode(null));
        }
    }
}
