# 04 - 資料契約與遷移規格

## 1. 通用規則

1. 所有存檔必含 `saveVersion`
2. Enum 一律字串序列化（不可用整數）
3. Loader 必須忽略未知欄位
4. 必填欄位缺失時要有明確 fallback 或拒載訊息

## 2. WordEntryV2

### 2.1 欄位定義

| 欄位 | 型別 | 必填 | 預設 | 說明 |
|---|---|---|---|---|
| id | string | 是 | - | 穩定唯一識別 |
| english | string | 是 | - | 英文單字 |
| chinese | string | 是 | - | 中文釋義 |
| phonetic | string | 否 | "" | 音標 |
| partOfSpeech | string | 是 | - | N/V/A/D |
| element | string | 是 | - | Life/Force/Mind/Matter/Abstract |
| difficulty | int | 是 | 1 | 1-10 |
| confusables | string[] | 否 | [] | 混淆詞 id |
| baseChips | int | 是 | 3 | 單字基礎籌碼 |
| lexiconTier | string | 是 | T1 | T1~T5 |

### 2.2 JSON 範例

```json
{
  "id": "apple",
  "english": "apple",
  "chinese": "蘋果",
  "phonetic": "/ˈæpəl/",
  "partOfSpeech": "N",
  "element": "Life",
  "difficulty": 1,
  "confusables": ["apply", "maple"],
  "baseChips": 4,
  "lexiconTier": "T1"
}
```

## 3. DeckCardV2

### 3.1 欄位定義

| 欄位 | 型別 | 必填 | 預設 | 說明 |
|---|---|---|---|---|
| wordId | string | 是 | - | 對應 WordEntryV2.id |
| versionTag | string | 否 | "normal" | normal/foil/gold/... |
| chipModifier | int | 否 | 0 | 額外籌碼 |
| additiveMultiplier | float | 否 | 0 | 加算倍率 |
| multiplicativeMultiplier | float | 否 | 1 | 乘算倍率 |
| flags | string[] | 否 | [] | 特殊標記 |

### 3.2 JSON 範例

```json
{
  "wordId": "apple",
  "versionTag": "gold",
  "chipModifier": 3,
  "additiveMultiplier": 0,
  "multiplicativeMultiplier": 1.0,
  "flags": ["economy_bonus"]
}
```

## 4. MetaProgressV2

### 4.1 欄位定義

| 欄位 | 型別 | 必填 | 預設 | 說明 |
|---|---|---|---|---|
| saveVersion | int | 是 | 2 | 存檔版本 |
| playerLevel | int | 是 | 1 | 帳號等級 |
| xp | int | 是 | 0 | 累積 XP |
| lp | int | 是 | 0 | 可用 LP |
| highestStake | int | 是 | 0 | 最高難度 |
| unlockedLexiconTiers | string[] | 是 | ["T1"] | 已解鎖詞庫層 |
| curriculumNodes | string[] | 是 | [] | 已解鎖課程節點 |
| deckProfiles | object[] | 是 | [] | 牌組模板 |
| achievements | string[] | 是 | [] | 成就 id |
| contractHistory | object | 是 | {} | 契約統計 |

### 4.2 JSON 範例

```json
{
  "saveVersion": 2,
  "playerLevel": 8,
  "xp": 1560,
  "lp": 220,
  "highestStake": 2,
  "unlockedLexiconTiers": ["T1", "T2"],
  "curriculumNodes": ["FLU_01", "LEX_01", "BLD_01"],
  "deckProfiles": [
    {
      "id": "balanced",
      "name": "Balanced",
      "elementWeights": {"Life": 1, "Force": 1, "Mind": 1, "Matter": 1, "Abstract": 1},
      "posWeights": {"N": 1, "V": 1, "A": 1, "D": 1},
      "targetHandTypes": ["GrammarChain"]
    }
  ],
  "achievements": ["ACH_FIRST_RUN"],
  "contractHistory": {
    "completed": 12,
    "failed": 5,
    "lastContractId": "CT_NAT_001"
  }
}
```

## 5. Enum 字典

### 5.1 Element
- `Life`
- `Force`
- `Mind`
- `Matter`
- `Abstract`

### 5.2 PartOfSpeech
- `N`
- `V`
- `A`
- `D`

### 5.3 LearningLevel
- `Lv0`
- `Lv1`
- `Lv2`
- `Lv3`
- `Lv4`

### 5.4 HandType
- `Word`
- `PoSPair`
- `ElemPair`
- `PoSTriple`
- `GrammarChain`
- `ElemTriple`
- `FullHouse`
- `ElemFlush`
- `PoSFlush`
- `GrammarFlush`

## 6. 驗證規則

1. `wordId` 必須存在於詞庫
2. `baseChips` 不可小於 0
3. `multiplicativeMultiplier` 不可小於 1
4. `lp/xp` 不可為負數
5. `curriculumNodes` 不可同時包含互斥節點

## 7. 舊存檔遷移（v1 -> v2）

### 7.1 PlayerData 映射

保留：
- `level`
- `experience`
- `learningPoints -> lp`
- `unlockedAchievements`
- `totalPlayDays`

捨棄：
- STR/INT/CON/AGI/LUCK 及衍生戰鬥屬性

### 7.2 WordProgress 映射（舊 0-7 -> 新 0-4）

- `Locked/New/Known` -> `Lv0/Lv1`
- `Familiar/Remembered` -> `Lv2`
- `Proficient/Mastered` -> `Lv3`
- `Internalized` -> `Lv4`

### 7.3 遷移流程

1. 載入舊存檔
2. 產生 `backup/player_v1_backup.json`
3. 依映射規則轉換
4. 輸出新檔（`saveVersion=2`）
5. 驗證必填欄位與 enum 合法性

### 7.4 失敗處理

- migration 失敗：回退備份、標記不可進入正式 Run、引導重試
