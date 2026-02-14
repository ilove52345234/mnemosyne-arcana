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
    }
}
