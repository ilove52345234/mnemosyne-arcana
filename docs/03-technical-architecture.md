# 03 - Technical Architecture

## Stack Baseline

- Unity + C#
- Data-driven JSON configs
- Testable pure C# domain logic

## Target Runtime Modules

1. RunManagerV2
2. ScoringManagerV2
3. LearningManagerV2
4. ShopManagerV2
5. MetaManagerV2

## Data Flow (Hand)

1. UI submits selected cards
2. Scoring evaluates hand type
3. Learning applies answer outcome modifiers
4. Run checks blind target progress
5. Settlement updates run/meta states

## Save Files

- `save/meta_progress.json`
- `save/word_progress.json`
- `save/run_snapshot.json` (optional)

## Engineering Rules

- Keep domain logic deterministic and unit-testable
- Keep MonoBehaviours as orchestration shells
- Version all save schemas
