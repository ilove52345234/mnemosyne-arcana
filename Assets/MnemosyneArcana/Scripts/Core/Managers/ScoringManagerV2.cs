using System;
using System.Collections.Generic;
using System.Linq;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class ScoringManagerV2 : IScoringService
    {
        private static readonly IReadOnlyDictionary<HandType, (int Chips, int Mult, int ChipsGrowth, int MultGrowth)> BaseStats =
            new Dictionary<HandType, (int Chips, int Mult, int ChipsGrowth, int MultGrowth)>
            {
                { HandType.Word, (5, 1, 10, 1) },
                { HandType.PoSPair, (10, 2, 15, 1) },
                { HandType.ElemPair, (15, 2, 15, 1) },
                { HandType.PoSTriple, (30, 3, 20, 2) },
                { HandType.GrammarChain, (30, 4, 30, 3) },
                { HandType.ElemTriple, (35, 3, 20, 2) },
                { HandType.FullHouse, (40, 4, 25, 2) },
                { HandType.ElemFlush, (50, 5, 30, 3) },
                { HandType.PoSFlush, (60, 6, 35, 3) },
                { HandType.GrammarFlush, (100, 8, 40, 4) }
            };

        public ServiceResult<ScoreBreakdown> EvaluateHand(IReadOnlyList<PlayedCard> cards, RunModifiers modifiers)
        {
            if (cards == null || cards.Count < 1 || cards.Count > 5)
            {
                return ServiceResult<ScoreBreakdown>.Fail(ErrorCode.InvalidInput);
            }

            var handType = DetermineHandType(cards);
            var (baseChips, baseMult, chipsGrowth, multGrowth) = BaseStats[handType];
            var upgradeLevel = Math.Max(0, modifiers?.HandUpgradeLevel ?? 0);
            var upgradedChips = baseChips + chipsGrowth * upgradeLevel;
            var upgradedMult = baseMult + multGrowth * upgradeLevel;
            var wrongAnswers = cards.Count(c => c.IsAnswerWrong);
            var cardChipsTotal = cards.Sum(ComputeCardChips);

            var additiveMult = modifiers?.AdditiveMultTotal ?? 0f;
            var externalHandMultDelta = modifiers?.HandMultDelta ?? 0;
            var factors = (modifiers?.MultiplicativeFactors ?? Array.Empty<float>()).ToArray();
            var effectiveHandMult = Math.Max(1, upgradedMult + externalHandMultDelta - wrongAnswers);
            var computedMult = Math.Max(1f, effectiveHandMult + additiveMult);

            var rawScore = (upgradedChips + cardChipsTotal) * computedMult;
            foreach (var factor in factors)
            {
                rawScore *= Math.Max(1f, factor);
            }

            var breakdown = new ScoreBreakdown
            {
                HandType = handType,
                BaseHandChips = baseChips,
                UpgradedHandChips = upgradedChips,
                CardChipsTotal = cardChipsTotal,
                BaseHandMult = baseMult,
                UpgradedHandMult = upgradedMult,
                AdditiveMultTotal = additiveMult,
                WrongAnswers = wrongAnswers,
                EffectiveHandMult = effectiveHandMult,
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

        private static int ComputeCardChips(PlayedCard card)
        {
            var chipMultiplier = Math.Max(0f, card.ChipMultiplier);
            if (card.IsAnswerWrong)
            {
                chipMultiplier = Math.Min(chipMultiplier, 0.5f);
            }

            return (int)Math.Floor(Math.Max(0, card.BaseChips) * chipMultiplier);
        }
    }
}
