using System;
using System.Collections.Generic;
using System.Linq;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using UnityEngine;

namespace MnemosyneArcana.Prototype
{
    public sealed class PrototypeSandboxController : MonoBehaviour
    {
        private enum HandPreset
        {
            Word,
            Pair,
            Triple,
            Flush,
            GrammarChain
        }

        private RunManagerV2 _runManager = new RunManagerV2();
        private readonly ScoringManagerV2 _scoringManager = new ScoringManagerV2();
        private readonly ShopManagerV2 _shopManager = new ShopManagerV2();
        private readonly LearningManagerV2 _learningManager = new LearningManagerV2();
        private readonly MetaManagerV2 _metaManager = new MetaManagerV2();

        private readonly List<ShopOffer> _shopOffers = new List<ShopOffer>();
        private readonly List<string> _logLines = new List<string>();

        private RunDifficultyProfile _difficulty = RunDifficultyProfile.Standard;
        private HandPreset _handPreset = HandPreset.Pair;
        private int _seed = 20260216;
        private int _metaLp = 80;
        private int _metaXp = 0;
        private int _simCardBaseChips = 8;
        private int _simUpgradeLevel = 0;
        private float _simAdditiveMult = 0f;
        private float _simFactor = 1.0f;
        private int _simWrongCount = 0;
        private int _lastHandScore;
        private string _unlockNodeId = "FLU_01";
        private Vector2 _scroll;
        private bool _retryUsed;
        private Contract _selectedContract;

        // Legacy sandbox: keep code for reference, but no auto-bootstrap.

        private void Start()
        {
            StartNewRun();
            Log("原型沙盒已啟動，可用按鈕推進流程。");
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(12, 12, 620, Screen.height - 24), GUI.skin.box);
            _scroll = GUILayout.BeginScrollView(_scroll);

            DrawRunPanel();
            DrawScoringPanel();
            DrawShopPanel();
            DrawLearningPanel();
            DrawMetaPanel();
            DrawLogPanel();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawRunPanel()
        {
            GUILayout.Label("Run 控制", GUI.skin.label);

            GUILayout.BeginHorizontal();
            GUILayout.Label("難度", GUILayout.Width(70));
            if (GUILayout.Button(PrototypeUiText.DifficultyZh(_difficulty), GUILayout.Width(130)))
            {
                _difficulty = (RunDifficultyProfile)(((int)_difficulty + 1) % 3);
            }
            GUILayout.Label("種子", GUILayout.Width(40));
            var seedText = GUILayout.TextField(_seed.ToString(), GUILayout.Width(100));
            if (int.TryParse(seedText, out var parsedSeed))
            {
                _seed = parsedSeed;
            }
            if (GUILayout.Button("重開新局", GUILayout.Width(120)))
            {
                StartNewRun();
            }
            GUILayout.EndHorizontal();

            var s = _runManager.CurrentState;
            GUILayout.Label($"階段={PrototypeUiText.PhaseZh(s.Phase)} | 關卡=第{s.Ante}關 {PrototypeUiText.BlindZh(s.BlindType)} | 目標={s.TargetScore} | 目前分={s.CurrentScore} | 出牌={s.PlaysLeft} | 金錢={s.Money}");
            GUILayout.Space(4);
        }

        private void DrawScoringPanel()
        {
            GUILayout.Label("計分沙盒", GUI.skin.label);
            GUILayout.BeginHorizontal();
            GUILayout.Label("預設牌組", GUILayout.Width(70));
            if (GUILayout.Button(_handPreset.ToString(), GUILayout.Width(120)))
            {
                _handPreset = (HandPreset)(((int)_handPreset + 1) % Enum.GetValues(typeof(HandPreset)).Length);
            }
            GUILayout.Label("基礎籌碼", GUILayout.Width(75));
            _simCardBaseChips = Mathf.Clamp(ParseIntField(_simCardBaseChips, 45), 1, 99);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("升級", GUILayout.Width(70));
            _simUpgradeLevel = Mathf.Clamp(ParseIntField(_simUpgradeLevel, 45), 0, 9);
            GUILayout.Label("答錯", GUILayout.Width(50));
            _simWrongCount = Mathf.Clamp(ParseIntField(_simWrongCount, 45), 0, 5);
            GUILayout.Label("加法倍率", GUILayout.Width(60));
            _simAdditiveMult = ParseFloatField(_simAdditiveMult, 55);
            GUILayout.Label("乘區", GUILayout.Width(50));
            _simFactor = Mathf.Clamp(ParseFloatField(_simFactor, 55), 1f, 5f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("模擬出牌並提交", GUILayout.Width(180)))
            {
                SimulateAndSubmitHand();
            }
            if (GUILayout.Button("結算盲注", GUILayout.Width(120)))
            {
                ResolveBlind();
            }
            if (GUILayout.Button("商店後前進", GUILayout.Width(140)))
            {
                AdvanceAfterShop();
            }
            GUILayout.EndHorizontal();

            GUILayout.Label($"上次出牌分數 = {_lastHandScore}");
            GUILayout.Space(4);
        }

        private void DrawShopPanel()
        {
            GUILayout.Label("商店沙盒", GUI.skin.label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成商店商品", GUILayout.Width(170)))
            {
                GenerateOffers();
            }
            if (GUILayout.Button("自動買第一項", GUILayout.Width(120)))
            {
                TryBuyFirstOffer();
            }
            GUILayout.EndHorizontal();

            if (_shopOffers.Count == 0)
            {
                GUILayout.Label("- 目前沒有商品 -");
            }
            else
            {
                foreach (var offer in _shopOffers)
                {
                    GUILayout.Label($"{offer.OfferId} | {PrototypeUiText.OfferZh(offer.Category)} | ${offer.Price}");
                }
            }

            GUILayout.Space(4);
        }

        private void DrawLearningPanel()
        {
            GUILayout.Label("學習沙盒", GUI.skin.label);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("答對", GUILayout.Width(120)))
            {
                ApplyLearningAnswer(AnswerResult.Correct);
            }
            if (GUILayout.Button("答錯", GUILayout.Width(120)))
            {
                ApplyLearningAnswer(AnswerResult.Wrong);
            }
            if (GUILayout.Button("答錯 -> 重答", GUILayout.Width(120)))
            {
                ResolveWrongChoice(WrongAnswerChoice.RetryWithCost);
            }
            if (GUILayout.Button("答錯 -> 賭一把", GUILayout.Width(130)))
            {
                ResolveWrongChoice(WrongAnswerChoice.Gamble);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }

        private void DrawMetaPanel()
        {
            GUILayout.Label("局外沙盒", GUI.skin.label);

            GUILayout.BeginHorizontal();
            GUILayout.Label("LP", GUILayout.Width(22));
            _metaLp = Mathf.Max(0, ParseIntField(_metaLp, 60));
            GUILayout.Label("XP", GUILayout.Width(22));
            _metaXp = Mathf.Max(0, ParseIntField(_metaXp, 60));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("節點", GUILayout.Width(38));
            _unlockNodeId = GUILayout.TextField(_unlockNodeId, GUILayout.Width(90));
            if (GUILayout.Button("嘗試解鎖", GUILayout.Width(100)))
            {
                TryUnlockNode();
            }
            if (GUILayout.Button("生成契約", GUILayout.Width(130)))
            {
                GenerateContract();
            }
            if (GUILayout.Button("結算契約", GUILayout.Width(120)))
            {
                SettleSelectedContract();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
        }

        private void DrawLogPanel()
        {
            GUILayout.Label("事件紀錄", GUI.skin.label);
            if (GUILayout.Button("清空紀錄", GUILayout.Width(90)))
            {
                _logLines.Clear();
            }

            foreach (var line in _logLines.TakeLast(20))
            {
                GUILayout.Label(line);
            }
        }

        private void StartNewRun()
        {
            _runManager = new RunManagerV2(_difficulty);
            _runManager.StartRun(_seed);
            _shopOffers.Clear();
            _retryUsed = false;
            Log($"新局開始：難度={PrototypeUiText.DifficultyZh(_difficulty)}，種子={_seed}");
        }

        private void SimulateAndSubmitHand()
        {
            var cards = BuildCardsByPreset(_handPreset);
            var factors = Mathf.Approximately(_simFactor, 1f) ? Array.Empty<float>() : new[] { _simFactor };
            var breakdownResult = _scoringManager.EvaluateHand(cards, new RunModifiers
            {
                HandUpgradeLevel = _simUpgradeLevel,
                AdditiveMultTotal = _simAdditiveMult,
                MultiplicativeFactors = factors
            });

            if (!breakdownResult.IsSuccess)
            {
                Log($"手牌計分失敗：{breakdownResult.Error}");
                return;
            }

            _lastHandScore = breakdownResult.Value.FinalScore;
            var submit = _runManager.SubmitHandScore(_lastHandScore);
            if (!submit.IsSuccess)
            {
                Log($"提交分數失敗：{submit.Error}");
                return;
            }

            Log($"出牌 {_handPreset}: +{_lastHandScore}，階段={PrototypeUiText.PhaseZh(_runManager.CurrentState.Phase)}，分數={_runManager.CurrentState.CurrentScore}/{_runManager.CurrentState.TargetScore}");
        }

        private void ResolveBlind()
        {
            var result = _runManager.ResolveBlindResult();
            if (!result.IsSuccess)
            {
                Log($"盲注結算失敗：{result.Error}");
                return;
            }

            Log($"盲注結果：通過={result.Value.Passed}，下一階段={PrototypeUiText.PhaseZh(result.Value.NextPhase)}");
            if (result.Value.NextPhase == RunPhase.Shop)
            {
                GenerateOffers();
            }
        }

        private void AdvanceAfterShop()
        {
            var result = _runManager.AdvanceAfterShop();
            if (!result.IsSuccess)
            {
                Log($"商店後前進失敗：{result.Error}");
                return;
            }

            _shopOffers.Clear();
            Log($"推進到：第{result.Value.Ante}關 {PrototypeUiText.BlindZh(result.Value.BlindType)}，目標={result.Value.TargetScore}");
        }

        private void GenerateOffers()
        {
            var state = _runManager.CurrentState;
            var isBossShop = state.BlindType == BlindType.Boss;
            var offers = _shopManager.GenerateOffers(state.Ante, _seed + state.Ante * 31, isBossShop);
            if (!offers.IsSuccess)
            {
                Log($"生成商品失敗：{offers.Error}");
                return;
            }

            _shopOffers.Clear();
            _shopOffers.AddRange(offers.Value);
            Log($"商店已生成 {_shopOffers.Count} 項商品（魔王商店={isBossShop}）");
        }

        private void TryBuyFirstOffer()
        {
            if (_shopOffers.Count == 0)
            {
                Log("目前沒有可購買商品。");
                return;
            }

            var state = _runManager.CurrentState;
            var offer = _shopOffers[0];
            var buy = _shopManager.PurchaseOffer(offer, state.Money);
            if (!buy.IsSuccess)
            {
                Log($"購買失敗：{buy.Error}");
                return;
            }

            if (!buy.Value.Success)
            {
                Log($"金錢不足：{offer.OfferId} 需要 ${offer.Price}");
                return;
            }

            state.Money = buy.Value.RemainingMoney;
            _shopOffers.RemoveAt(0);
            Log($"已購買 {offer.OfferId}，剩餘金錢 {state.Money}");
        }

        private void ApplyLearningAnswer(AnswerResult answer)
        {
            var state = _runManager.CurrentState;
            var context = new RunContext
            {
                Ante = state.Ante,
                BlindType = state.BlindType,
                PlaysLeft = state.PlaysLeft,
                DiscardsLeft = state.DiscardsLeft,
                CurrentLevel = LearningLevel.Lv2
            };

            var result = _learningManager.ApplyAnswer("demo_word", answer, context);
            if (!result.IsSuccess)
            {
                Log($"學習結算失敗：{result.Error}");
                return;
            }

            var r = result.Value;
            Log($"學習：{answer}，題型={r.QuestionMode}，倍率x{r.ChipMultiplier:0.##}，下一級={r.NextLevel}，自動={r.IsAutoResolved}");
        }

        private void ResolveWrongChoice(WrongAnswerChoice choice)
        {
            var state = _runManager.CurrentState;
            var result = _learningManager.ResolveWrongAnswerChoice(choice, state.Money, _retryUsed, _seed + state.Ante);
            if (!result.IsSuccess)
            {
                Log($"答錯選擇處理失敗：{result.Error}");
                return;
            }

            var r = result.Value;
            _retryUsed = _retryUsed || r.RetryConsumed;
            state.Money = r.RemainingMoney;
            Log($"答錯選擇={choice}，最終={r.FinalAnswerResult}，金錢={r.RemainingMoney}，已用重答={_retryUsed}");
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
                Log($"節點解鎖失敗：{result.Error}（{_unlockNodeId}）");
                return;
            }

            _metaLp = result.Value.RemainingLp;
            Log($"節點解鎖成功：{_unlockNodeId}，剩餘學習點={_metaLp}");
        }

        private void GenerateContract()
        {
            var result = _metaManager.GenerateContracts(new MetaProgress { Lp = _metaLp, Xp = _metaXp }, _seed);
            if (!result.IsSuccess || result.Value.Count == 0)
            {
                Log($"生成契約失敗：{result.Error}");
                return;
            }

            _selectedContract = result.Value[0];
            Log($"已選契約：{_selectedContract.ContractId}（{_selectedContract.Name}） +{_selectedContract.LpReward}學習點");
        }

        private void SettleSelectedContract()
        {
            if (_selectedContract == null)
            {
                Log("尚未選擇契約。");
                return;
            }

            var settlement = _metaManager.SettleContractWithCap(_selectedContract, new RunTelemetry { ContractCompleted = true }, lpBase: 20);
            if (!settlement.IsSuccess)
            {
                Log($"契約結算失敗：{settlement.Error}");
                return;
            }

            _metaLp += settlement.Value.LpBonusCapped;
            Log($"契約已結算：+{settlement.Value.LpBonusCapped} 學習點（原始={settlement.Value.LpBonusRaw}）");
        }

        private IReadOnlyList<PlayedCard> BuildCardsByPreset(HandPreset preset)
        {
            var cards = new List<PlayedCard>();
            switch (preset)
            {
                case HandPreset.Word:
                    cards.Add(CreateCard(Element.Life, PartOfSpeech.N, wrong: _simWrongCount > 0));
                    break;
                case HandPreset.Pair:
                    cards.Add(CreateCard(Element.Mind, PartOfSpeech.N, wrong: _simWrongCount > 0));
                    cards.Add(CreateCard(Element.Mind, PartOfSpeech.V, wrong: _simWrongCount > 1));
                    break;
                case HandPreset.Triple:
                    cards.Add(CreateCard(Element.Abstract, PartOfSpeech.N, wrong: _simWrongCount > 0));
                    cards.Add(CreateCard(Element.Abstract, PartOfSpeech.V, wrong: _simWrongCount > 1));
                    cards.Add(CreateCard(Element.Abstract, PartOfSpeech.A, wrong: _simWrongCount > 2));
                    break;
                case HandPreset.Flush:
                    cards.Add(CreateCard(Element.Force, PartOfSpeech.N, wrong: _simWrongCount > 0));
                    cards.Add(CreateCard(Element.Force, PartOfSpeech.V, wrong: _simWrongCount > 1));
                    cards.Add(CreateCard(Element.Force, PartOfSpeech.A, wrong: _simWrongCount > 2));
                    cards.Add(CreateCard(Element.Force, PartOfSpeech.D, wrong: _simWrongCount > 3));
                    cards.Add(CreateCard(Element.Force, PartOfSpeech.N, wrong: _simWrongCount > 4));
                    break;
                case HandPreset.GrammarChain:
                    cards.Add(CreateCard(Element.Matter, PartOfSpeech.A, wrong: _simWrongCount > 0));
                    cards.Add(CreateCard(Element.Life, PartOfSpeech.N, wrong: _simWrongCount > 1));
                    cards.Add(CreateCard(Element.Mind, PartOfSpeech.V, wrong: _simWrongCount > 2));
                    cards.Add(CreateCard(Element.Abstract, PartOfSpeech.D, wrong: _simWrongCount > 3));
                    break;
            }

            return cards;
        }

        private PlayedCard CreateCard(Element element, PartOfSpeech pos, bool wrong)
        {
            return new PlayedCard
            {
                WordId = Guid.NewGuid().ToString("N"),
                Element = element,
                PartOfSpeech = pos,
                BaseChips = _simCardBaseChips,
                LearningLevel = LearningLevel.Lv2,
                ChipMultiplier = wrong ? 0.5f : 1f,
                IsAnswerWrong = wrong
            };
        }

        private void Log(string message)
        {
            _logLines.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        private int ParseIntField(int current, int width)
        {
            var text = GUILayout.TextField(current.ToString(), GUILayout.Width(width));
            return int.TryParse(text, out var parsed) ? parsed : current;
        }

        private float ParseFloatField(float current, int width)
        {
            var text = GUILayout.TextField(current.ToString("0.##"), GUILayout.Width(width));
            return float.TryParse(text, out var parsed) ? parsed : current;
        }
    }
}
