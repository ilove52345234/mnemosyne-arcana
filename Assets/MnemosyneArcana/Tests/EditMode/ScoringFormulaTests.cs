using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    public class ScoringFormulaTests
    {
        [Test]
        public void EvaluateHand_AppliesHandUpgradeGrowth()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.N, Element.Life, 5),
                    Card(PartOfSpeech.N, Element.Mind, 5)
                },
                new RunModifiers { HandUpgradeLevel = 2 });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(HandType.PoSPair, result.Value.HandType);
            Assert.AreEqual(10, result.Value.BaseHandChips);
            Assert.AreEqual(40, result.Value.UpgradedHandChips);
            Assert.AreEqual(2, result.Value.BaseHandMult);
            Assert.AreEqual(4, result.Value.UpgradedHandMult);
            Assert.AreEqual(200, result.Value.FinalScore);
        }

        [Test]
        public void EvaluateHand_AppliesWrongAnswerPenalty()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.N, Element.Life, 10),
                    Card(PartOfSpeech.N, Element.Force, 10, isWrong: true)
                },
                new RunModifiers());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Value.WrongAnswers);
            Assert.AreEqual(15, result.Value.CardChipsTotal);
            Assert.AreEqual(1, result.Value.EffectiveHandMult);
            Assert.AreEqual(25, result.Value.FinalScore);
        }

        [Test]
        public void EvaluateHand_AppliesFullFormulaWithModifiers()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.N, Element.Mind, 6),
                    Card(PartOfSpeech.N, Element.Force, 7)
                },
                new RunModifiers
                {
                    AdditiveMultTotal = 1f,
                    MultiplicativeFactors = new[] { 1.5f, 1.1f }
                });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(113, result.Value.FinalScore);
        }

        [Test]
        public void EvaluateHand_WithMasteryEffects_BoostsLv4CardsAndMultipliers()
        {
            var manager = new ScoringManagerV2();
            var effects = new CurriculumEffectSnapshot
            {
                Lv4CardFlatChipBonus = 2,
                FirstTwoLv4CardsAdditiveMultBonus = 1,
                Lv4ConcentratedBuildMultiplierBonusRate = 0.08f
            };

            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.N, Element.Life, 10, level: LearningLevel.Lv4),
                    Card(PartOfSpeech.V, Element.Life, 10, level: LearningLevel.Lv4),
                    Card(PartOfSpeech.A, Element.Life, 10, level: LearningLevel.Lv4),
                    Card(PartOfSpeech.D, Element.Life, 10, level: LearningLevel.Lv4)
                },
                new RunModifiers(), effects);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(48, result.Value.CardChipsTotal);
            Assert.AreEqual(448, result.Value.FinalScore);
        }

        private static PlayedCard Card(PartOfSpeech pos, Element element, int baseChips, bool isWrong = false, LearningLevel level = LearningLevel.Lv1)
        {
            return new PlayedCard
            {
                WordId = "word_test",
                PartOfSpeech = pos,
                Element = element,
                BaseChips = baseChips,
                IsAnswerWrong = isWrong,
                LearningLevel = level
            };
        }
    }
}
