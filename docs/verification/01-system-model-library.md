# 01 - System Model Library

## 1. 使用方式
- 每個系統至少執行 `M-Low`、`M-Mid`、`M-High` 三模型。
- 核心平衡系統（S1/S3/S4/S7）加跑 `M-Edge`。
- 每模型至少 3 seeds；S4/S7 建議 30 輪批次。

## 2. 全域玩家模型
- M-Low：低掌握、低資源、低穩定（新手/高遺忘）
- M-Mid：中掌握、中資源（一般玩家）
- M-High：高掌握、高資源（熟練玩家）
- M-Edge：極端條件（高壓詞條、連敗、長週期遺忘）

## 3. 系統模型定義

### S1 Run/Blind
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S1-M1 | M-Low | Ante1~2 失敗分支可恢復 |
| S1-M2 | M-Mid | Ante 推進與商店節奏正常 |
| S1-M3 | M-High | 可推進至高 Ante 且不崩潰 |
| S1-M4 | M-Edge | 連續失敗/重開流程無死鎖 |

### S2 Scoring/HandType
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S2-M1 | M-Low | 低牌型時分數下限與懲罰 |
| S2-M2 | M-Mid | 常見牌型分數分佈 |
| S2-M3 | M-High | 高牌型/高倍率不溢位 |

### S3 Learning/Boss
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S3-M1 | M-Low | 答錯三選一成本與保底 |
| S3-M2 | M-Mid | Lv0~Lv4 行為與成長 |
| S3-M3 | M-High | Boss 全對獎勵與效率提升 |
| S3-M4 | M-Edge | 連錯、連對切換下狀態穩定 |

### S4 Gate/Recovery/Demotion
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S4-M1 | M-Low | 有效詞彙不足時卡關合理 |
| S4-M2 | M-Mid | 回補關可恢復，不無限打轉 |
| S4-M3 | M-High | 高詞彙量可推進高關 |
| S4-M4 | M-Edge | 7/14/30 天遺忘與退回保護 |

### S5 Shop/Economy
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S5-M1 | M-Low | 低資金仍有可買選項 |
| S5-M2 | M-Mid | 權重與價格帶符合 SoT |
| S5-M3 | M-High | 高資金購買路徑不失衡 |

### S6 Meta/Contract/Curriculum
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S6-M1 | M-Low | XP/LP 基礎結算正確 |
| S6-M2 | M-Mid | 契約生成/結算與 cap 正確 |
| S6-M3 | M-High | 課程樹前置/互斥/解鎖正確 |

### S7 Final Gate/Endless
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S7-M1 | M-Low | 不能靠運氣越過 95% |
| S7-M2 | M-Mid | 95% 可主線通關 |
| S7-M3 | M-High | 100%+7 天才真通關 |
| S7-M4 | M-Edge | 無盡模式長局穩定性 |

### S8 Telemetry/Observability
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S8-M1 | M-Low | GATE_TOO_HARD 觸發正確 |
| S8-M2 | M-Mid | 告警與流程狀態一致 |
| S8-M3 | M-High | GATE_TOO_EASY 觸發正確 |

### S9 NFR
| 模型 | 描述 | 主要驗證 |
|---|---|---|
| S9-M1 | M-Low Device | 低規效能與記憶體 |
| S9-M2 | M-Mid Device | 一般裝置穩定運行 |
| S9-M3 | M-High Load | 高負載長時間穩定 |

## 4. 目前已校準成果（快照）
- 10 模型 30 輪：M0~M8 卡點穩定命中。
- M9：`12/30 (40%)`，已落在目標區間 `30%~60%`。
