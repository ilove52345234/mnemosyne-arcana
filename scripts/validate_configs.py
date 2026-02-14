#!/usr/bin/env python3
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
CONFIG_DIR = ROOT / "configs"

REQUIRED_WORD_FIELDS = {
    "id", "english", "chinese", "partOfSpeech", "element", "difficulty", "baseChips", "lexiconTier"
}
VALID_PARTS_OF_SPEECH = {"N", "V", "A", "D"}
VALID_ELEMENTS = {"Life", "Force", "Mind", "Matter", "Abstract"}
VALID_TIERS = {"T1", "T2", "T3", "T4", "T5"}

REQUIRED_META_FIELDS = {
    "saveVersion", "playerLevel", "xp", "lp", "highestStake", "unlockedLexiconTiers",
    "curriculumNodes", "deckProfiles", "achievements", "contractHistory"
}


def fail(msg: str) -> None:
    print(f"[FAIL] {msg}")
    sys.exit(1)


def load_json(path: pathlib.Path):
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except Exception as exc:
        fail(f"Invalid JSON at {path}: {exc}")


def validate_word_entries(path: pathlib.Path) -> None:
    data = load_json(path)
    entries = data.get("words", [])
    if not isinstance(entries, list):
        fail("word_entries.v2.json: 'words' must be a list")
    if len(entries) < 100:
        fail("word_entries.v2.json: requires at least 100 entries for T1/T2 playable baseline")

    ids = set()
    tier_counts = {"T1": 0, "T2": 0}
    pos_counts = {k: 0 for k in VALID_PARTS_OF_SPEECH}
    elem_counts = {k: 0 for k in VALID_ELEMENTS}

    for idx, entry in enumerate(entries):
        missing = REQUIRED_WORD_FIELDS - set(entry.keys())
        if missing:
            fail(f"word_entries.v2.json: words[{idx}] missing fields: {sorted(missing)}")

        wid = entry["id"]
        if wid in ids:
            fail(f"word_entries.v2.json: duplicate id '{wid}'")
        ids.add(wid)

        pos = entry["partOfSpeech"]
        elem = entry["element"]
        tier = entry["lexiconTier"]
        difficulty = entry["difficulty"]
        base_chips = entry["baseChips"]

        if pos not in VALID_PARTS_OF_SPEECH:
            fail(f"word_entries.v2.json: words[{idx}] invalid partOfSpeech '{pos}'")
        if elem not in VALID_ELEMENTS:
            fail(f"word_entries.v2.json: words[{idx}] invalid element '{elem}'")
        if tier not in VALID_TIERS:
            fail(f"word_entries.v2.json: words[{idx}] invalid lexiconTier '{tier}'")
        if not isinstance(difficulty, int) or difficulty < 1 or difficulty > 10:
            fail(f"word_entries.v2.json: words[{idx}] difficulty must be int in [1,10]")
        if not isinstance(base_chips, int) or base_chips < 3 or base_chips > 12:
            fail(f"word_entries.v2.json: words[{idx}] baseChips must be int in [3,12]")

        if tier in tier_counts:
            tier_counts[tier] += 1
        pos_counts[pos] += 1
        elem_counts[elem] += 1

    if tier_counts["T1"] < 40 or tier_counts["T2"] < 40:
        fail(f"word_entries.v2.json: requires T1>=40 and T2>=40, got {tier_counts}")

    for pos, count in pos_counts.items():
        if count < 20:
            fail(f"word_entries.v2.json: partOfSpeech '{pos}' has too few entries ({count}), expected >=20")

    for elem, count in elem_counts.items():
        if count < 20:
            fail(f"word_entries.v2.json: element '{elem}' has too few entries ({count}), expected >=20")

    print(f"[INFO] word entries: total={len(entries)} T1={tier_counts['T1']} T2={tier_counts['T2']}")


def validate_meta(path: pathlib.Path) -> None:
    data = load_json(path)
    missing = REQUIRED_META_FIELDS - set(data.keys())
    if missing:
        fail(f"meta_progress.v2.json missing fields: {sorted(missing)}")
    if data.get("lp", 0) < 0 or data.get("xp", 0) < 0:
        fail("meta_progress.v2.json: xp/lp must be non-negative")


def main() -> None:
    if not CONFIG_DIR.exists():
        print("[WARN] configs/ not found. Nothing to validate yet.")
        return

    word_file = CONFIG_DIR / "word_entries.v2.json"
    meta_file = CONFIG_DIR / "meta_progress.v2.json"

    if word_file.exists():
        validate_word_entries(word_file)
    else:
        print("[WARN] Missing configs/word_entries.v2.json")

    if meta_file.exists():
        validate_meta(meta_file)
    else:
        print("[WARN] Missing configs/meta_progress.v2.json")

    print("[OK] Config validation completed.")


if __name__ == "__main__":
    main()
