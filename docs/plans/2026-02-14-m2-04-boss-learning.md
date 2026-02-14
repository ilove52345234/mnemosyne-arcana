# M2-04 Boss 學習規則 Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Implement Boss blind learning mechanics: question type +1 level, consecutive-3 streak bonus, and all-correct reward.

**Architecture:** Extend `LearningManagerV2.GetEffectiveLevel` for Boss +1 rule. Add two new static methods for streak bonus and all-correct reward. New DTOs `BossStreakBonus` and `BossRewardResult` in DomainModels. New methods added to `ILearningService`.

**Tech Stack:** C# / Unity EditMode tests (NUnit) / Existing service pattern (`ServiceResult<T>`)

**Design doc:** `docs/plans/2026-02-14-m2-04-boss-learning-design.md`

---

### Task 1: Add BossStreakBonus and BossRewardResult DTOs

**Files:**
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs`

**Step 1: Add DTOs after DecayResult class (before ServiceResult)**

Insert after `DecayResult` class (after line 187), before `ServiceResult<T>`:

```csharp
public sealed class BossStreakBonus
{
    public int ConsecutiveCorrect { get; set; }
    public float ChipMultiplier { get; set; } = 1.0f;
}

public sealed class WordLevelUp
{
    public string WordId { get; set; } = string.Empty;
    public LearningLevel FromLevel { get; set; }
    public LearningLevel ToLevel { get; set; }
}

public sealed class BossRewardResult
{
    public bool AllCorrect { get; set; }
    public IReadOnlyList<WordLevelUp> UpgradedWords { get; set; } = Array.Empty<WordLevelUp>();
    public int SkippedAtMax { get; set; }
}
```

**Step 2: Commit**

```bash
git add Assets/MnemosyneArcana/Scripts/Core/Contracts/DomainModels.cs
git commit -m "feat(m2): add BossStreakBonus, WordLevelUp, BossRewardResult DTOs"
```

---

### Task 2: Add Boss methods to ILearningService

**Files:**
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs`

**Step 1: Add two new methods to ILearningService**

After `ResolveWrongAnswerChoice` method:

```csharp
BossStreakBonus GetBossStreakBonus(int consecutiveCorrect);
ServiceResult<BossRewardResult> ApplyBossAllCorrectReward(IReadOnlyList<WordProgress> playedWords);
```

**Step 2: Commit**

```bash
git add Assets/MnemosyneArcana/Scripts/Core/Contracts/ServiceInterfaces.cs
git commit -m "feat(m2): add Boss streak and reward methods to ILearningService"
```

---

### Task 3: Write failing tests for all Boss learning rules

**Files:**
- Create: `Assets/MnemosyneArcana/Tests/EditMode/BossLearningTests.cs`

**Step 1: Write complete test file**

```csharp
using System.Collections.Generic;
using NUnit.Framework;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class BossLearningTests
    {
        private LearningManagerV2 _learning;

        [SetUp]
        public void SetUp()
        {
            _learning = new LearningManagerV2();
        }

        // === Boss 題型 +1 階 ===

        // TC-BOSS-001: Boss + Lv0 → effective Lv1
        [Test]
        public void Boss_Lv0_EffectiveLv1()
        {
            var result = _learning.ApplyAnswer("w1", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv0
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv1, result.Value.EffectiveLevel);
            Assert.AreEqual("2_choice_reading", result.Value.QuestionMode);
        }

        // TC-BOSS-002: Boss + Lv2 → effective Lv3
        [Test]
        public void Boss_Lv2_EffectiveLv3()
        {
            var result = _learning.ApplyAnswer("w2", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv2
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.EffectiveLevel);
            Assert.AreEqual("spelling", result.Value.QuestionMode);
        }

        // TC-BOSS-003: Boss + Lv3 → stays Lv3 (capped)
        [Test]
        public void Boss_Lv3_StaysLv3()
        {
            var result = _learning.ApplyAnswer("w3", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv3
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.EffectiveLevel);
            Assert.AreEqual("spelling", result.Value.QuestionMode);
        }

        // TC-BOSS-004: Boss + Lv4 → effective Lv3 (already implemented, regression)
        [Test]
        public void Boss_Lv4_EffectiveLv3()
        {
            var result = _learning.ApplyAnswer("w4", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Boss,
                CurrentLevel = LearningLevel.Lv4
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.EffectiveLevel);
            Assert.AreEqual("spelling", result.Value.QuestionMode);
            Assert.IsFalse(result.Value.IsAutoResolved);
        }

        // TC-BOSS-009: Non-Boss blind → no level shift for Lv0
        [Test]
        public void NonBoss_Lv0_StaysLv0()
        {
            var result = _learning.ApplyAnswer("w5", AnswerResult.Correct, new RunContext
            {
                BlindType = BlindType.Small,
                CurrentLevel = LearningLevel.Lv0
            });

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(LearningLevel.Lv0, result.Value.EffectiveLevel);
            Assert.AreEqual("4_choice_reading", result.Value.QuestionMode);
        }

        // === 連對 3 題獎勵 ===

        // TC-BOSS-005: 3 consecutive correct → chipMultiplier x2
        [Test]
        public void BossStreak_3Correct_ChipX2()
        {
            var bonus = _learning.GetBossStreakBonus(3);

            Assert.AreEqual(3, bonus.ConsecutiveCorrect);
            Assert.AreEqual(2.0f, bonus.ChipMultiplier);
        }

        // TC-BOSS-006: 2 correct (not yet 3) → no bonus
        [Test]
        public void BossStreak_2Correct_NoBonus()
        {
            var bonus = _learning.GetBossStreakBonus(2);

            Assert.AreEqual(2, bonus.ConsecutiveCorrect);
            Assert.AreEqual(1.0f, bonus.ChipMultiplier);
        }

        // streak=6 → bonus again (every 3)
        [Test]
        public void BossStreak_6Correct_ChipX2Again()
        {
            var bonus = _learning.GetBossStreakBonus(6);

            Assert.AreEqual(6, bonus.ConsecutiveCorrect);
            Assert.AreEqual(2.0f, bonus.ChipMultiplier);
        }

        // streak=4 → no bonus (not multiple of 3)
        [Test]
        public void BossStreak_4Correct_NoBonus()
        {
            var bonus = _learning.GetBossStreakBonus(4);

            Assert.AreEqual(1.0f, bonus.ChipMultiplier);
        }

        // streak=0 → no bonus
        [Test]
        public void BossStreak_0_NoBonus()
        {
            var bonus = _learning.GetBossStreakBonus(0);

            Assert.AreEqual(1.0f, bonus.ChipMultiplier);
        }

        // === Boss 全對獎勵 ===

        // TC-BOSS-007: All correct → each played word +1 level
        [Test]
        public void BossAllCorrect_UpgradesPlayedWords()
        {
            var words = new List<WordProgress>
            {
                new WordProgress { WordId = "a", Level = LearningLevel.Lv1, Pool = WordPool.Learning },
                new WordProgress { WordId = "b", Level = LearningLevel.Lv2, Pool = WordPool.Learning }
            };

            var result = _learning.ApplyBossAllCorrectReward(words);

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.AllCorrect);
            Assert.AreEqual(2, result.Value.UpgradedWords.Count);
            Assert.AreEqual(LearningLevel.Lv2, result.Value.UpgradedWords[0].ToLevel);
            Assert.AreEqual(LearningLevel.Lv3, result.Value.UpgradedWords[1].ToLevel);
            Assert.AreEqual(0, result.Value.SkippedAtMax);
        }

        // TC-BOSS-008: Lv4 word → skipped (already max)
        [Test]
        public void BossAllCorrect_Lv4Skipped()
        {
            var words = new List<WordProgress>
            {
                new WordProgress { WordId = "a", Level = LearningLevel.Lv3, Pool = WordPool.Learning },
                new WordProgress { WordId = "b", Level = LearningLevel.Lv4, Pool = WordPool.Mastered }
            };

            var result = _learning.ApplyBossAllCorrectReward(words);

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(1, result.Value.UpgradedWords.Count);
            Assert.AreEqual("a", result.Value.UpgradedWords[0].WordId);
            Assert.AreEqual(LearningLevel.Lv4, result.Value.UpgradedWords[0].ToLevel);
            Assert.AreEqual(1, result.Value.SkippedAtMax);
        }

        // Empty list → valid but no upgrades
        [Test]
        public void BossAllCorrect_EmptyList_NoUpgrades()
        {
            var result = _learning.ApplyBossAllCorrectReward(new List<WordProgress>());

            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.Value.AllCorrect);
            Assert.AreEqual(0, result.Value.UpgradedWords.Count);
        }

        // Null input → error
        [Test]
        public void BossAllCorrect_NullInput_ReturnsError()
        {
            var result = _learning.ApplyBossAllCorrectReward(null);

            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(ErrorCode.InvalidInput, result.Error);
        }
    }
}
```

**Step 2: Commit**

```bash
git add Assets/MnemosyneArcana/Tests/EditMode/BossLearningTests.cs
git commit -m "test(m2): add Boss learning rule test cases TC-BOSS-001~009"
```

---

### Task 4: Implement Boss learning rules in LearningManagerV2

**Files:**
- Modify: `Assets/MnemosyneArcana/Scripts/Core/Managers/LearningManagerV2.cs`

**Step 1: Update GetEffectiveLevel for Boss +1 rule**

Replace the existing `GetEffectiveLevel` method (lines 99-107):

```csharp
private static LearningLevel GetEffectiveLevel(LearningLevel level, BlindType blindType)
{
    if (blindType != BlindType.Boss)
    {
        return level;
    }

    // Boss: Lv4 → Lv3, Lv0-Lv2 → +1, Lv3 → Lv3 (capped)
    return level switch
    {
        LearningLevel.Lv4 => LearningLevel.Lv3,
        LearningLevel.Lv3 => LearningLevel.Lv3,
        LearningLevel.Lv2 => LearningLevel.Lv3,
        LearningLevel.Lv1 => LearningLevel.Lv2,
        LearningLevel.Lv0 => LearningLevel.Lv1,
        _ => level
    };
}
```

**Step 2: Add GetBossStreakBonus method**

Add after `ResolveWrongAnswerChoice`:

```csharp
public BossStreakBonus GetBossStreakBonus(int consecutiveCorrect)
{
    // Every 3 consecutive correct answers, next card gets x2 chips
    var isStreakBonus = consecutiveCorrect > 0 && consecutiveCorrect % 3 == 0;
    return new BossStreakBonus
    {
        ConsecutiveCorrect = consecutiveCorrect,
        ChipMultiplier = isStreakBonus ? 2.0f : 1.0f
    };
}
```

**Step 3: Add ApplyBossAllCorrectReward method**

Add after `GetBossStreakBonus`:

```csharp
public ServiceResult<BossRewardResult> ApplyBossAllCorrectReward(IReadOnlyList<WordProgress> playedWords)
{
    if (playedWords == null)
    {
        return ServiceResult<BossRewardResult>.Fail(ErrorCode.InvalidInput);
    }

    var upgraded = new System.Collections.Generic.List<WordLevelUp>();
    var skippedAtMax = 0;

    for (var i = 0; i < playedWords.Count; i++)
    {
        var word = playedWords[i];
        if (word.Level == LearningLevel.Lv4)
        {
            skippedAtMax++;
            continue;
        }

        upgraded.Add(new WordLevelUp
        {
            WordId = word.WordId,
            FromLevel = word.Level,
            ToLevel = LevelUp(word.Level)
        });
    }

    return ServiceResult<BossRewardResult>.Ok(new BossRewardResult
    {
        AllCorrect = true,
        UpgradedWords = upgraded,
        SkippedAtMax = skippedAtMax
    });
}
```

**Step 4: Verify config validation**

Run: `bash scripts/validate_configs.sh`
Expected: PASS

**Step 5: Commit**

```bash
git add Assets/MnemosyneArcana/Scripts/Core/Managers/LearningManagerV2.cs
git commit -m "feat(m2): implement Boss learning rules (+1 level, streak bonus, all-correct reward)"
```

---

### Task 5: Update docs and status tracking

**Files:**
- Modify: `docs/17-test-matrix.md` — add Boss learning test cases
- Modify: `docs/18-api-and-domain-types.md` — add BossStreakBonus, WordLevelUp, BossRewardResult DTOs
- Modify: `docs/IMPLEMENTATION_STATUS.md` — M2-04 → Done, M2 → 100%
- Modify: `docs/SESSION_NOTES.md` — add handoff record
- Modify: `docs/PROJECT_EXECUTION_PLAN.md` — update progress

**Step 1: Update all docs per CLAUDE.md §2 requirements**

**Step 2: Commit and push**

```bash
git add docs/
git commit -m "docs(m2): update specs and status for M2-04 Boss learning rules"
git push
```
