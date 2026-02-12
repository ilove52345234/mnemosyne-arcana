using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using NUnit.Framework;

namespace MnemosyneArcana.Tests.EditMode
{
    public class ScoringHandTypeTests
    {
        [Test]
        public void EvaluateHand_A_N_V_IsGrammarChain()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.A, Element.Mind, 5),
                    Card(PartOfSpeech.N, Element.Force, 5),
                    Card(PartOfSpeech.V, Element.Life, 5)
                },
                new RunModifiers());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(HandType.GrammarChain, result.Value.HandType);
        }

        [Test]
        public void EvaluateHand_A_A_V_IsGrammarChain()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.A, Element.Mind, 5),
                    Card(PartOfSpeech.A, Element.Matter, 5),
                    Card(PartOfSpeech.V, Element.Life, 5)
                },
                new RunModifiers());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(HandType.GrammarChain, result.Value.HandType);
        }

        [Test]
        public void EvaluateHand_N_A_V_IsNotGrammarChain()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.N, Element.Life, 5),
                    Card(PartOfSpeech.A, Element.Mind, 5),
                    Card(PartOfSpeech.V, Element.Force, 5)
                },
                new RunModifiers());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(HandType.Word, result.Value.HandType);
        }

        [Test]
        public void EvaluateHand_ThreePlusTwoPos_IsFullHouse()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.N, Element.Mind, 5),
                    Card(PartOfSpeech.N, Element.Life, 5),
                    Card(PartOfSpeech.N, Element.Force, 5),
                    Card(PartOfSpeech.V, Element.Matter, 5),
                    Card(PartOfSpeech.V, Element.Abstract, 5)
                },
                new RunModifiers());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(HandType.FullHouse, result.Value.HandType);
        }

        [Test]
        public void EvaluateHand_GrammarChainAndSameElement_IsGrammarFlush()
        {
            var manager = new ScoringManagerV2();
            var result = manager.EvaluateHand(
                new[]
                {
                    Card(PartOfSpeech.A, Element.Mind, 5),
                    Card(PartOfSpeech.N, Element.Mind, 5),
                    Card(PartOfSpeech.V, Element.Mind, 5),
                    Card(PartOfSpeech.D, Element.Mind, 5)
                },
                new RunModifiers());

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(HandType.GrammarFlush, result.Value.HandType);
        }

        [Test]
        public void EvaluateHand_SameInput_IsDeterministic()
        {
            var manager = new ScoringManagerV2();
            var cards = new[]
            {
                Card(PartOfSpeech.N, Element.Mind, 6),
                Card(PartOfSpeech.N, Element.Force, 7)
            };
            var modifiers = new RunModifiers
            {
                AdditiveMultTotal = 1f,
                MultiplicativeFactors = new[] { 1.5f, 1.1f }
            };

            var first = manager.EvaluateHand(cards, modifiers);
            var second = manager.EvaluateHand(cards, modifiers);

            Assert.IsTrue(first.IsSuccess);
            Assert.IsTrue(second.IsSuccess);
            Assert.AreEqual(first.Value.HandType, second.Value.HandType);
            Assert.AreEqual(first.Value.FinalScore, second.Value.FinalScore);
        }

        private static PlayedCard Card(PartOfSpeech pos, Element element, int baseChips)
        {
            return new PlayedCard
            {
                WordId = "word_test",
                PartOfSpeech = pos,
                Element = element,
                BaseChips = baseChips,
                LearningLevel = LearningLevel.Lv1
            };
        }
    }
}
