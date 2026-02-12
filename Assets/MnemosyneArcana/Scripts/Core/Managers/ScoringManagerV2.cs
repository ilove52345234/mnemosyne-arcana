using System;
using System.Collections.Generic;
using System.Linq;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class ScoringManagerV2 : IScoringService
    {
        private static readonly IReadOnlyDictionary<HandType, (int Chips, int Mult)> BaseStats =
            new Dictionary<HandType, (int Chips, int Mult)>
            {
                { HandType.Word, (5, 1) },
                { HandType.PoSPair, (10, 2) },
                { HandType.ElemPair, (15, 2) },
                { HandType.PoSTriple, (30, 3) },
                { HandType.GrammarChain, (30, 4) },
                { HandType.ElemTriple, (35, 3) },
                { HandType.FullHouse, (40, 4) },
                { HandType.ElemFlush, (50, 5) },
                { HandType.PoSFlush, (60, 6) },
                { HandType.GrammarFlush, (100, 8) }
            };

        public ServiceResult<ScoreBreakdown> EvaluateHand(IReadOnlyList<PlayedCard> cards, RunModifiers modifiers)
        {
            if (cards == null || cards.Count < 1 || cards.Count > 5)
            {
                return ServiceResult<ScoreBreakdown>.Fail(ErrorCode.InvalidInput);
            }

            var handType = DetermineHandType(cards);
            var (baseChips, baseMult) = BaseStats[handType];
            var cardChipsTotal = cards.Sum(c => Math.Max(0, c.BaseChips));

            var additiveMult = modifiers?.AdditiveMultTotal ?? 0f;
            var factors = (modifiers?.MultiplicativeFactors ?? Array.Empty<float>()).ToArray();
            var computedMult = Math.Max(1f, baseMult + additiveMult);

            var rawScore = (baseChips + cardChipsTotal) * computedMult;
            foreach (var factor in factors)
            {
                rawScore *= Math.Max(1f, factor);
            }

            var breakdown = new ScoreBreakdown
            {
                HandType = handType,
                BaseHandChips = baseChips,
                CardChipsTotal = cardChipsTotal,
                BaseHandMult = baseMult,
                AdditiveMultTotal = additiveMult,
                MultiplicativeFactors = factors,
                FinalScore = (int)Math.Floor(rawScore)
            };

            return ServiceResult<ScoreBreakdown>.Ok(breakdown);
        }

        private static HandType DetermineHandType(IReadOnlyList<PlayedCard> cards)
        {
            var count = cards.Count;
            var isGrammarChain = IsGrammarChain(cards);
            var isSameElement = cards.All(c => c.Element == cards[0].Element);
            var isSamePos = cards.All(c => c.PartOfSpeech == cards[0].PartOfSpeech);
            var posGroups = cards.GroupBy(c => c.PartOfSpeech).Select(g => g.Count()).OrderByDescending(x => x).ToArray();
            var hasElemTriple = cards.GroupBy(c => c.Element).Any(g => g.Count() >= 3);
            var hasPosTriple = posGroups.Any(x => x >= 3);
            var hasElemPair = cards.GroupBy(c => c.Element).Any(g => g.Count() >= 2);
            var hasPosPair = posGroups.Any(x => x >= 2);
            var isFullHouse = count == 5 && posGroups.Length == 2 && posGroups[0] == 3 && posGroups[1] == 2;

            if (isGrammarChain && isSameElement) return HandType.GrammarFlush;
            if (count == 5 && isSamePos) return HandType.PoSFlush;
            if (count == 5 && isSameElement) return HandType.ElemFlush;
            if (isFullHouse) return HandType.FullHouse;
            if (count >= 3 && isGrammarChain) return HandType.GrammarChain;
            if (count >= 3 && hasElemTriple) return HandType.ElemTriple;
            if (count >= 3 && hasPosTriple) return HandType.PoSTriple;
            if (count >= 2 && hasPosPair) return HandType.PoSPair;
            if (count >= 2 && hasElemPair) return HandType.ElemPair;
            return HandType.Word;
        }

        private static bool IsGrammarChain(IReadOnlyList<PlayedCard> cards)
        {
            if (cards.Count < 3) return false;

            var prev = GetPosOrder(cards[0].PartOfSpeech);
            for (var i = 1; i < cards.Count; i++)
            {
                var current = GetPosOrder(cards[i].PartOfSpeech);
                if (current < prev)
                {
                    return false;
                }

                prev = current;
            }

            return true;
        }

        private static int GetPosOrder(PartOfSpeech partOfSpeech)
        {
            return partOfSpeech switch
            {
                PartOfSpeech.A => 0,
                PartOfSpeech.N => 1,
                PartOfSpeech.V => 2,
                PartOfSpeech.D => 3,
                _ => 99
            };
        }
    }
}
