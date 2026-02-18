using System;
using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;

namespace MnemosyneArcana.Core.Managers
{
    public sealed class DecayManagerV2 : IDecayService
    {
        private static int GetDecayDays(LearningLevel level)
        {
            return level switch
            {
                LearningLevel.Lv1 => 1,
                LearningLevel.Lv2 => 3,
                LearningLevel.Lv3 => 7,
                LearningLevel.Lv4 => 7,
                _ => -1 // Lv0: never decays
            };
        }

        private static LearningLevel GetDecayedLevel(LearningLevel level)
        {
            return level switch
            {
                LearningLevel.Lv1 => LearningLevel.Lv0,
                LearningLevel.Lv2 => LearningLevel.Lv1,
                LearningLevel.Lv3 => LearningLevel.Lv2,
                LearningLevel.Lv4 => LearningLevel.Lv3,
                _ => LearningLevel.Lv0
            };
        }

        private static WordPool GetDecayedPool(LearningLevel level)
        {
            // Lv4 退化到 Lv3 留在 Learning，不進 Decayed
            if (level == LearningLevel.Lv4)
            {
                return WordPool.Learning;
            }
            return WordPool.Decayed;
        }

        public DecayResult EvaluateDecay(WordProgress word, DateTime now)
        {
            return EvaluateDecay(word, now, null);
        }

        public DecayResult EvaluateDecay(WordProgress word, DateTime now, CurriculumEffectSnapshot effects)
        {
            if (word == null) throw new ArgumentNullException(nameof(word));

            if (effects != null && word.Level == LearningLevel.Lv4 && effects.Lv4DecayProtectionLayers > 0)
            {
                return new DecayResult
                {
                    WordId = word.WordId,
                    Decayed = false,
                    PreviousLevel = word.Level,
                    NewLevel = word.Level,
                    PreviousPool = word.Pool,
                    NewPool = word.Pool
                };
            }

            var decayDays = GetDecayDays(word.Level);

            if (decayDays < 0)
            {
                return new DecayResult
                {
                    WordId = word.WordId,
                    Decayed = false,
                    PreviousLevel = word.Level,
                    NewLevel = word.Level,
                    PreviousPool = word.Pool,
                    NewPool = word.Pool
                };
            }

            var elapsed = now - word.LastPracticed;
            var shouldDecay = elapsed.TotalDays >= decayDays;

            if (!shouldDecay)
            {
                return new DecayResult
                {
                    WordId = word.WordId,
                    Decayed = false,
                    PreviousLevel = word.Level,
                    NewLevel = word.Level,
                    PreviousPool = word.Pool,
                    NewPool = word.Pool
                };
            }

            return new DecayResult
            {
                WordId = word.WordId,
                Decayed = true,
                PreviousLevel = word.Level,
                NewLevel = GetDecayedLevel(word.Level),
                PreviousPool = word.Pool,
                NewPool = GetDecayedPool(word.Level)
            };
        }

        public IReadOnlyList<DecayResult> EvaluateBatch(IReadOnlyList<WordProgress> words, DateTime now)
        {
            return EvaluateBatch(words, now, null);
        }

        public IReadOnlyList<DecayResult> EvaluateBatch(IReadOnlyList<WordProgress> words, DateTime now, CurriculumEffectSnapshot effects)
        {
            if (words == null) throw new ArgumentNullException(nameof(words));

            var results = new List<DecayResult>(words.Count);
            for (var i = 0; i < words.Count; i++)
            {
                results.Add(EvaluateDecay(words[i], now, effects));
            }
            return results;
        }

        public void ResetDecayTimer(WordProgress word, DateTime now)
        {
            if (word == null) throw new ArgumentNullException(nameof(word));
            word.LastPracticed = now;
        }
    }
}
