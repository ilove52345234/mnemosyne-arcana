using System;
using System.Collections.Generic;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using UnityEngine;

namespace MnemosyneArcana.Prototype
{
    public sealed class PrototypeGameScreenController : MonoBehaviour
    {
        private sealed class DemoWord
        {
            public string Text;
            public Element Element;
            public PartOfSpeech Pos;
            public LearningLevel Level;
        }

        private RunManagerV2 _runManager = new RunManagerV2(RunDifficultyProfile.Standard);
        private readonly ScoringManagerV2 _scoringManager = new ScoringManagerV2();
        private readonly ShopManagerV2 _shopManager = new ShopManagerV2();
        private readonly LearningManagerV2 _learningManager = new LearningManagerV2();
        private readonly MetaManagerV2 _metaManager = new MetaManagerV2();

        private readonly List<DemoWord> _deck = new List<DemoWord>();
        private readonly List<DemoWord> _hand = new List<DemoWord>();
        private readonly List<ShopOffer> _offers = new List<ShopOffer>();
        private readonly List<string> _logs = new List<string>();

        private Vector2 _logScroll;
        private RunDifficultyProfile _difficulty = RunDifficultyProfile.Standard;
        private int _seed = 20260216;
        private int _baseChips = 8;
        private int _upgradeLevel;
        private int _wrongWordCount;
        private float _addMult;
        private float _factor = 1.0f;
        private int _metaLp = 80;
        private int _metaXp;
        private string _unlockNodeId = "FLU_01";
        private Contract _selectedContract;
        private ScoreBreakdown _lastBreakdown;

        // Disabled bootstrap: replaced by PrototypeCardGameUiController.

        private void Start()
        {
            BuildDeck();
            StartRun();
            DrawHand();
            AddLog("已啟動遊戲畫面原型。可以直接操作並調整右側參數。");
        }

        private void OnGUI()
        {
            var sw = Screen.width;
            var sh = Screen.height;

            DrawTopBar(new Rect(10, 10, sw - 20, 72));
            DrawHandArea(new Rect(10, 92, sw * 0.52f, sh * 0.48f));
            DrawBattleArea(new Rect(10, 92 + sh * 0.48f + 8, sw * 0.52f, sh - (92 + sh * 0.48f + 18)));
            DrawControlArea(new Rect(sw * 0.54f, 92, sw * 0.44f - 10, sh * 0.66f));
            DrawLogArea(new Rect(sw * 0.54f, 92 + sh * 0.66f + 8, sw * 0.44f - 10, sh - (100 + sh * 0.66f)));
        }

        private void DrawTopBar(Rect rect)
        {
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("Mnemosyne Arcana - 遊戲畫面原型（開發試玩）");
            var s = _runManager.CurrentState;
            GUILayout.Label(
                $"階段：{PhaseZh(s.Phase)}  |  關卡：Ante {s.Ante} {BlindZh(s.BlindType)}  |  目標分：{s.TargetScore}  |  目前分：{s.CurrentScore}  |  出牌：{s.PlaysLeft}  |  金錢：${s.Money}");
            GUILayout.EndArea();
        }

        private void DrawHandArea(Rect rect)
        {
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("手牌區（單字維持英文）");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("抽新手牌", GUILayout.Width(110)))
            {
                DrawHand();
                AddLog("已重抽手牌。");
            }

            if (GUILayout.Button("答對並出牌", GUILayout.Width(110)))
            {
                PlayCurrentHand(false);
            }

            if (GUILayout.Button("答錯並出牌", GUILayout.Width(110)))
            {
                PlayCurrentHand(true);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(6);

            GUILayout.BeginHorizontal();
            for (var i = 0; i < _hand.Count; i++)
            {
                var word = _hand[i];
                GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150), GUILayout.Height(120));
                GUILayout.Label(word.Text);
                GUILayout.Label($"元素：{ElementZh(word.Element)}");
                GUILayout.Label($"詞性：{PosZh(word.Pos)}");
                GUILayout.Label($"等級：{word.Level}");
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawBattleArea(Rect rect)
        {
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("戰鬥區");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("結算盲注", GUILayout.Width(100)))
            {
                ResolveBlind();
            }

            if (GUILayout.Button("前往下一關", GUILayout.Width(100)))
            {
                AdvanceAfterShop();
            }

            if (GUILayout.Button("生成商店商品", GUILayout.Width(120)))
            {
                GenerateShopOffers();
            }

            if (GUILayout.Button("購買第一項", GUILayout.Width(100)))
            {
                BuyFirstOffer();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("最近一次出牌結算");
            if (_lastBreakdown == null)
            {
                GUILayout.Label("- 尚未出牌 -");
            }
            else
            {
                GUILayout.Label($"牌型：{_lastBreakdown.HandType} / 最終分：{_lastBreakdown.FinalScore}");
                GUILayout.Label($"基礎籌碼：{_lastBreakdown.BaseHandChips} -> 升級後：{_lastBreakdown.UpgradedHandChips}");
                GUILayout.Label($"手牌籌碼：{_lastBreakdown.CardChipsTotal} / 有效倍率：{_lastBreakdown.EffectiveHandMult}");
            }

            GUILayout.Space(8);
            GUILayout.Label("商店商品");
            if (_offers.Count == 0)
            {
                GUILayout.Label("- 目前沒有商品 -");
            }
            else
            {
                for (var i = 0; i < _offers.Count; i++)
                {
                    var offer = _offers[i];
                    GUILayout.Label($"{i + 1}. {OfferZh(offer.Category)} / {offer.OfferId} / ${offer.Price}");
                }
            }

            GUILayout.EndArea();
        }

        private void DrawControlArea(Rect rect)
        {
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("調參面板（中文）");

            GUILayout.BeginHorizontal();
            GUILayout.Label("難度", GUILayout.Width(60));
            if (GUILayout.Button(DifficultyZh(_difficulty), GUILayout.Width(130)))
            {
                _difficulty = (RunDifficultyProfile)(((int)_difficulty + 1) % 3);
            }
            GUILayout.Label("Seed", GUILayout.Width(40));
            _seed = ParseIntField(_seed, 95);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("基礎籌碼", GUILayout.Width(60));
            _baseChips = Mathf.Clamp(ParseIntField(_baseChips, 55), 1, 30);
            GUILayout.Label("升級層", GUILayout.Width(50));
            _upgradeLevel = Mathf.Clamp(ParseIntField(_upgradeLevel, 55), 0, 9);
            GUILayout.Label("答錯數", GUILayout.Width(50));
            _wrongWordCount = Mathf.Clamp(ParseIntField(_wrongWordCount, 55), 0, 5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("加法倍率", GUILayout.Width(60));
            _addMult = ParseFloatField(_addMult, 70);
            GUILayout.Label("乘區", GUILayout.Width(30));
            _factor = Mathf.Clamp(ParseFloatField(_factor, 60), 1f, 5f);
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.Label("局外 / 學習測試");
            GUILayout.BeginHorizontal();
            GUILayout.Label("LP", GUILayout.Width(22));
            _metaLp = Mathf.Max(0, ParseIntField(_metaLp, 60));
            GUILayout.Label("XP", GUILayout.Width(22));
            _metaXp = Mathf.Max(0, ParseIntField(_metaXp, 60));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("節點", GUILayout.Width(30));
            _unlockNodeId = GUILayout.TextField(_unlockNodeId, GUILayout.Width(90));
            if (GUILayout.Button("嘗試解鎖", GUILayout.Width(90)))
            {
                TryUnlockNode();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成契約", GUILayout.Width(90)))
            {
                GenerateContract();
            }

            if (GUILayout.Button("結算契約", GUILayout.Width(90)))
            {
                SettleContract();
            }

            if (GUILayout.Button("模擬答錯選項", GUILayout.Width(120)))
            {
                SimulateWrongChoice();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("重開本局", GUILayout.Width(90)))
            {
                StartRun();
                DrawHand();
            }

            if (GUILayout.Button("清空事件紀錄", GUILayout.Width(120)))
            {
                _logs.Clear();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawLogArea(Rect rect)
        {
            GUILayout.BeginArea(rect, GUI.skin.box);
            GUILayout.Label("事件紀錄");
            _logScroll = GUILayout.BeginScrollView(_logScroll);
            var start = Mathf.Max(0, _logs.Count - 25);
            for (var i = start; i < _logs.Count; i++)
            {
                GUILayout.Label(_logs[i]);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void StartRun()
        {
            _runManager = new RunManagerV2(_difficulty);
            _runManager.StartRun(_seed);
            _offers.Clear();
            _selectedContract = null;
            _lastBreakdown = null;
            AddLog($"開新局：難度={DifficultyZh(_difficulty)}，Seed={_seed}");
        }

        private void DrawHand()
        {
            _hand.Clear();
            if (_deck.Count == 0)
            {
                BuildDeck();
            }

            var rng = new System.Random(_seed + _runManager.CurrentState.Ante * 17 + _runManager.CurrentState.CurrentScore);
            for (var i = 0; i < 5; i++)
            {
                var idx = rng.Next(0, _deck.Count);
                _hand.Add(_deck[idx]);
            }
        }

        private void PlayCurrentHand(bool simulateWrong)
        {
            if (_hand.Count == 0)
            {
                AddLog("手牌為空，請先抽牌。");
                return;
            }

            var cards = new List<PlayedCard>();
            for (var i = 0; i < _hand.Count; i++)
            {
                var word = _hand[i];
                var wrong = simulateWrong && i < _wrongWordCount;
                cards.Add(new PlayedCard
                {
                    WordId = word.Text,
                    Element = word.Element,
                    PartOfSpeech = word.Pos,
                    BaseChips = _baseChips,
                    LearningLevel = word.Level,
                    ChipMultiplier = wrong ? 0.5f : 1f,
                    IsAnswerWrong = wrong
                });
            }

            var factors = Mathf.Approximately(_factor, 1f) ? Array.Empty<float>() : new[] { _factor };
            var scoreResult = _scoringManager.EvaluateHand(cards, new RunModifiers
            {
                HandUpgradeLevel = _upgradeLevel,
                AdditiveMultTotal = _addMult,
                MultiplicativeFactors = factors
            });

            if (!scoreResult.IsSuccess)
            {
                AddLog($"出牌計分失敗：{scoreResult.Error}");
                return;
            }

            _lastBreakdown = scoreResult.Value;
            var submit = _runManager.SubmitHandScore(_lastBreakdown.FinalScore);
            if (!submit.IsSuccess)
            {
                AddLog($"提交手牌失敗：{submit.Error}（通常是階段不對）");
                return;
            }

            AddLog($"出牌完成：+{_lastBreakdown.FinalScore} 分，目前 {_runManager.CurrentState.CurrentScore}/{_runManager.CurrentState.TargetScore}");
            DrawHand();
        }

        private void ResolveBlind()
        {
            var result = _runManager.ResolveBlindResult();
            if (!result.IsSuccess)
            {
                AddLog($"盲注結算失敗：{result.Error}");
                return;
            }

            AddLog($"盲注結算：{(result.Value.Passed ? "通過" : "失敗")}，下一階段={PhaseZh(result.Value.NextPhase)}");
            if (result.Value.NextPhase == RunPhase.Shop)
            {
                GenerateShopOffers();
            }
        }

        private void AdvanceAfterShop()
        {
            var result = _runManager.AdvanceAfterShop();
            if (!result.IsSuccess)
            {
                AddLog($"前往下一關失敗：{result.Error}");
                return;
            }

            _offers.Clear();
            DrawHand();
            AddLog($"已前進到 Ante {result.Value.Ante} {BlindZh(result.Value.BlindType)}");
        }

        private void GenerateShopOffers()
        {
            var s = _runManager.CurrentState;
            var isBossShop = s.BlindType == BlindType.Boss;
            var result = _shopManager.GenerateOffers(s.Ante, _seed + s.Ante * 97, isBossShop);
            if (!result.IsSuccess)
            {
                AddLog($"商店生成失敗：{result.Error}");
                return;
            }

            _offers.Clear();
            _offers.AddRange(result.Value);
            AddLog($"商店已刷新，共 {_offers.Count} 個商品。");
        }

        private void BuyFirstOffer()
        {
            if (_offers.Count == 0)
            {
                AddLog("沒有商品可購買。");
                return;
            }

            var first = _offers[0];
            var state = _runManager.CurrentState;
            var result = _shopManager.PurchaseOffer(first, state.Money);
            if (!result.IsSuccess)
            {
                AddLog($"購買失敗：{result.Error}");
                return;
            }

            if (!result.Value.Success)
            {
                AddLog($"金錢不足，無法購買 {first.OfferId}");
                return;
            }

            state.Money = result.Value.RemainingMoney;
            _offers.RemoveAt(0);
            AddLog($"已購買 {first.OfferId}，剩餘 ${state.Money}");
        }

        private void TryUnlockNode()
        {
            var progress = new MetaProgress
            {
                PlayerLevel = 1,
                Xp = _metaXp,
                Lp = _metaLp,
                HighestStake = 1,
                CurriculumNodes = Array.Empty<string>()
            };
            var result = _metaManager.TryUnlockNode(_unlockNodeId, progress);
            if (!result.IsSuccess)
            {
                AddLog($"解鎖節點失敗：{result.Error}（{_unlockNodeId}）");
                return;
            }

            _metaLp = result.Value.RemainingLp;
            AddLog($"解鎖成功：{_unlockNodeId}，剩餘 LP={_metaLp}");
        }

        private void GenerateContract()
        {
            var result = _metaManager.GenerateContracts(new MetaProgress { Lp = _metaLp, Xp = _metaXp }, _seed + 7);
            if (!result.IsSuccess || result.Value.Count == 0)
            {
                AddLog("契約生成失敗。");
                return;
            }

            _selectedContract = result.Value[0];
            AddLog($"已選擇契約：{_selectedContract.Name}（+{_selectedContract.LpReward} LP）");
        }

        private void SettleContract()
        {
            if (_selectedContract == null)
            {
                AddLog("尚未有契約可結算。");
                return;
            }

            var result = _metaManager.SettleContractWithCap(
                _selectedContract,
                new RunTelemetry { ContractCompleted = true },
                lpBase: 20);
            if (!result.IsSuccess)
            {
                AddLog("契約結算失敗。");
                return;
            }

            _metaLp += result.Value.LpBonusCapped;
            AddLog($"契約結算完成，+{result.Value.LpBonusCapped} LP（raw={result.Value.LpBonusRaw}）");
        }

        private void SimulateWrongChoice()
        {
            var state = _runManager.CurrentState;
            var result = _learningManager.ResolveWrongAnswerChoice(WrongAnswerChoice.RetryWithCost, state.Money, false, _seed);
            if (!result.IsSuccess)
            {
                AddLog($"答錯選項模擬失敗：{result.Error}");
                return;
            }

            state.Money = result.Value.RemainingMoney;
            AddLog($"答錯選項（重答）-> 金錢 {state.Money}，最終結果 {result.Value.FinalAnswerResult}");
        }

        private void BuildDeck()
        {
            _deck.Clear();
            _deck.Add(new DemoWord { Text = "resonance", Element = Element.Abstract, Pos = PartOfSpeech.N, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "cascade", Element = Element.Force, Pos = PartOfSpeech.V, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "lucid", Element = Element.Mind, Pos = PartOfSpeech.A, Level = LearningLevel.Lv1 });
            _deck.Add(new DemoWord { Text = "artifact", Element = Element.Matter, Pos = PartOfSpeech.N, Level = LearningLevel.Lv3 });
            _deck.Add(new DemoWord { Text = "sustain", Element = Element.Life, Pos = PartOfSpeech.V, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "vivid", Element = Element.Life, Pos = PartOfSpeech.A, Level = LearningLevel.Lv1 });
            _deck.Add(new DemoWord { Text = "spiral", Element = Element.Force, Pos = PartOfSpeech.N, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "anchor", Element = Element.Matter, Pos = PartOfSpeech.V, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "insight", Element = Element.Mind, Pos = PartOfSpeech.N, Level = LearningLevel.Lv3 });
            _deck.Add(new DemoWord { Text = "ethereal", Element = Element.Abstract, Pos = PartOfSpeech.A, Level = LearningLevel.Lv2 });
        }

        private void AddLog(string text)
        {
            _logs.Add($"[{DateTime.Now:HH:mm:ss}] {text}");
        }

        private static int ParseIntField(int current, int width)
        {
            var text = GUILayout.TextField(current.ToString(), GUILayout.Width(width));
            return int.TryParse(text, out var parsed) ? parsed : current;
        }

        private static float ParseFloatField(float current, int width)
        {
            var text = GUILayout.TextField(current.ToString("0.##"), GUILayout.Width(width));
            return float.TryParse(text, out var parsed) ? parsed : current;
        }

        private static string DifficultyZh(RunDifficultyProfile profile)
        {
            return profile switch
            {
                RunDifficultyProfile.Relaxed => "輕鬆",
                RunDifficultyProfile.Standard => "標準",
                RunDifficultyProfile.Challenging => "挑戰",
                _ => profile.ToString()
            };
        }

        private static string BlindZh(BlindType blind)
        {
            return blind switch
            {
                BlindType.Small => "小盲注",
                BlindType.Big => "大盲注",
                BlindType.Boss => "Boss 盲注",
                _ => blind.ToString()
            };
        }

        private static string PhaseZh(RunPhase phase)
        {
            return phase switch
            {
                RunPhase.Boot => "初始化",
                RunPhase.RunStart => "開局",
                RunPhase.BlindStart => "盲注開始",
                RunPhase.HandSelect => "選牌",
                RunPhase.HandResolve => "手牌結算",
                RunPhase.BlindResult => "盲注結果",
                RunPhase.Shop => "商店",
                RunPhase.AnteAdvance => "關卡前進",
                RunPhase.BossResolve => "Boss 結算",
                RunPhase.RunComplete => "通關",
                RunPhase.RunFail => "失敗",
                _ => phase.ToString()
            };
        }

        private static string ElementZh(Element element)
        {
            return element switch
            {
                Element.Life => "生命",
                Element.Force => "力量",
                Element.Mind => "心智",
                Element.Matter => "物質",
                Element.Abstract => "抽象",
                _ => element.ToString()
            };
        }

        private static string PosZh(PartOfSpeech pos)
        {
            return pos switch
            {
                PartOfSpeech.N => "名詞",
                PartOfSpeech.V => "動詞",
                PartOfSpeech.A => "形容詞",
                PartOfSpeech.D => "副詞",
                _ => pos.ToString()
            };
        }

        private static string OfferZh(ShopOfferCategory category)
        {
            return category switch
            {
                ShopOfferCategory.Sense => "語感",
                ShopOfferCategory.Material => "教材",
                ShopOfferCategory.Affix => "詞綴",
                ShopOfferCategory.Course => "課程",
                _ => category.ToString()
            };
        }
    }
}
