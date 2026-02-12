#!/usr/bin/env python3
import json
import pathlib
import sys

ROOT = pathlib.Path(__file__).resolve().parents[1]
CONFIG_DIR = ROOT / "configs"

REQUIRED_WORD_FIELDS = {
    "id", "english", "chinese", "partOfSpeech", "element", "difficulty", "baseChips", "lexiconTier"
}

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
    ids = set()
    for idx, entry in enumerate(entries):
        missing = REQUIRED_WORD_FIELDS - set(entry.keys())
        if missing:
            fail(f"word_entries.v2.json: words[{idx}] missing fields: {sorted(missing)}")
        wid = entry["id"]
        if wid in ids:
            fail(f"word_entries.v2.json: duplicate id '{wid}'")
        ids.add(wid)


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
