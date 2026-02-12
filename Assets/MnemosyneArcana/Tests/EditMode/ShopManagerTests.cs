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
