# 04 - Data Contracts

## WordEntryV2

Required fields:
- id
- english
- chinese
- phonetic
- partOfSpeech
- element
- difficulty
- confusables[]
- baseChips
- lexiconTier

## DeckCardV2

Required fields:
- wordId
- versionTag
- chipModifier
- additiveMultiplier
- multiplicativeMultiplier
- flags[]

## MetaProgressV2

Required fields:
- playerLevel
- xp
- lp
- highestStake
- unlockedLexiconTiers[]
- curriculumNodes[]
- deckProfiles[]
- achievements[]
- contractHistory

## Serialization Rules

1. Include `saveVersion`
2. Serialize enums as strings
3. Ignore unknown fields safely
