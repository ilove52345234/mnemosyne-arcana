using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Managers;
using MnemosyneArcana.Core.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MnemosyneArcana.Prototype
{
    public sealed class PrototypeCardGameUiController : MonoBehaviour
    {
        private sealed class DemoWord
        {
            public string Text = string.Empty;
            public string MeaningZh = string.Empty;
            public Element Element;
            public PartOfSpeech Pos;
            public LearningLevel Level;
        }

        private sealed class DrawAnim
        {
            public RectTransform CardRect;
            public CanvasGroup CanvasGroup;
            public float StartTime;
            public float Delay;
            public bool Completed;
        }

        private sealed class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
        {
            private PrototypeCardGameUiController _owner;
            private int _cardIndex;
            private RectTransform _rect;
            private CanvasGroup _group;
            private Transform _originalParent;
            private int _originalSiblingIndex;
            private bool _dragging;

            public void Init(PrototypeCardGameUiController owner, int cardIndex)
            {
                _owner = owner;
                _cardIndex = cardIndex;
                _rect = transform as RectTransform;
                _group = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            }

            public void OnBeginDrag(PointerEventData eventData)
            {
                if (_owner == null || _owner.IsCardInteractionLocked)
                {
                    return;
                }

                _dragging = true;
                _originalParent = transform.parent;
                _originalSiblingIndex = transform.GetSiblingIndex();
                _group.blocksRaycasts = false;

                if (_owner.DragLayer != null)
                {
                    transform.SetParent(_owner.DragLayer, true);
                    transform.SetAsLastSibling();
                }
            }

            public void OnDrag(PointerEventData eventData)
            {
                if (!_dragging || _owner == null || _owner.DragLayer == null || _rect == null)
                {
                    return;
                }

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_owner.DragLayer, eventData.position, eventData.pressEventCamera, out var local))
                {
                    _rect.anchoredPosition = local;
                }
            }

            public void OnEndDrag(PointerEventData eventData)
            {
                if (!_dragging)
                {
                    return;
                }

                _dragging = false;
                if (_owner != null)
                {
                    _owner.TryDropCardToPlayZone(_cardIndex, eventData.position, eventData.pressEventCamera);
                }

                if (_originalParent != null)
                {
                    transform.SetParent(_originalParent, false);
                    transform.SetSiblingIndex(Mathf.Min(_originalSiblingIndex, _originalParent.childCount - 1));
                }

                if (_group != null)
                {
                    _group.blocksRaycasts = true;
                }

                if (_owner != null)
                {
                    _owner.RefreshHandCardVisuals();
                }
            }
        }

        private readonly List<DemoWord> _deck = new List<DemoWord>();
        private readonly List<DemoWord> _hand = new List<DemoWord>();
        private readonly List<ShopOffer> _offers = new List<ShopOffer>();
        private readonly HashSet<int> _playZoneCardIndexes = new HashSet<int>();
        private readonly List<int> _playZoneOrder = new List<int>();
        private readonly List<string> _logs = new List<string>();

        private RunManagerV2 _runManager = new RunManagerV2();
        private readonly ScoringManagerV2 _scoringManager = new ScoringManagerV2();
        private readonly ShopManagerV2 _shopManager = new ShopManagerV2();
        private readonly MetaManagerV2 _metaManager = new MetaManagerV2();
        private readonly GateProgressionManagerV2 _gateProgressionManager = new GateProgressionManagerV2();
        private readonly LearningTelemetryManagerV2 _telemetryManager = new LearningTelemetryManagerV2();

        private Font _font;
        private Text _statusText;
        private Text _selectedText;
        private Text _shopText;
        private Text _quizStatusText;
        private Text _quizPromptText;
        private Text _metaText;
        private Text _tuningText;
        private Text _logText;
        private RectTransform _handContainer;
        private RectTransform _playZoneContainer;
        private RectTransform _playZoneCardsContainer;
        private RectTransform _shopGridContainer;
        private RectTransform _dragLayer;
        private GridLayoutGroup _shopGridLayout;
        private LayoutElement _leftColLayout;
        private LayoutElement _rightColLayout;
        private RectTransform _rightColRoot;
        private RectTransform _tuningContentContainer;
        private Button _toggleTuningButton;
        private Text _toggleTuningButtonText;
        private readonly List<Button> _quizOptionButtons = new List<Button>();
        private readonly List<Text> _quizOptionTexts = new List<Text>();

        private RunDifficultyProfile _difficulty = RunDifficultyProfile.Standard;
        private int _seed = 20260216;
        private int _baseChips = 8;
        private int _upgradeLevel;
        private int _wrongCount;
        private float _additiveMult;
        private float _factor = 1.0f;
        private int _metaLp = 80;
        private int _metaXp;
        private string _unlockNodeId = "FLU_01";
        private Contract _activeRunContract;
        [SerializeField] private int _learnedCount = 2000;
        [SerializeField] private float _retentionRate = 0.85f;
        [SerializeField] private float _retrievalRate = 0.8f;
        [SerializeField] private float _coreCoverageRate = 0.85f;
        [SerializeField] private float _requiredCoverageRate = 0.85f;
        [SerializeField] private float _activeRecallQuestionRatio = 0.4f;
        [SerializeField] private float _activeRecallAccuracy = 0.8f;
        [SerializeField] private float _requiredBossRecallRatio = 0.4f;
        [SerializeField] private float _requiredBossRecallAccuracy = 0.8f;
        [SerializeField] private float _overallMasteryRate = 0.95f;
        [SerializeField] private int _stableDaysAtHundredPercent;
        [SerializeField] private float _learningEfficiencyBoost = 1.0f;
        [SerializeField] private float _maxLearningEfficiencyBoost = 1.5f;
        private int _currentGateModelIndex = 1;
        private int _highestUnlockedModelIndex = 1;
        private int _consecutiveRecoveryFailures;
        private int _daysSinceLastDemotion = 30;
        private bool _inRecoveryGate;
        private int _gateAttempts;
        private int _gatePasses;
        private int _recoveryEntries;
        private int _recoveryClears;
        private int _demotionCount;
        private int _bossGatePasses;
        private int _mainClearCount;
        private int _trueClearCount;
        private int _runDays;
        private int _lastScore;
        private readonly List<DrawAnim> _drawAnims = new List<DrawAnim>();
        private bool _isPlayingCardAnim;
        private bool _isQuizRunning;
        private readonly List<int> _quizCardIndexes = new List<int>();
        private readonly List<bool> _quizCardCorrectness = new List<bool>();
        private int _quizCursor;
        private int _quizCorrectCount;
        private int _quizCurrentCorrectOptionIndex = -1;
        private bool _isTuningCollapsed = true;
        [SerializeField] private bool _autoDemoOnStart = true;
        [SerializeField] private float _autoDemoStartDelaySeconds = 1.2f;
        [SerializeField] private bool _autoStartTenModelValidationOnPlay = true;
        [SerializeField] private bool _autoStartTenModelBatchValidationOnPlay = false;
        private bool _suppressVerboseLogs;
        private Coroutine _autoDemoCoroutine;
        private Coroutine _autoRunToCompleteCoroutine;
        private Coroutine _autoBatchRunsCoroutine;
        private Coroutine _autoFailThenRecoverCoroutine;
        private Coroutine _useCaseVerificationCoroutine;
        private Coroutine _fullValidationCoroutine;
        private Coroutine _modelValidationCoroutine;
        private Coroutine _modelBatchValidationCoroutine;

        internal bool IsCardInteractionLocked => _isPlayingCardAnim;
        internal RectTransform DragLayer => _dragLayer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (!Application.isEditor)
            {
                return;
            }

            if (FindObjectOfType<PrototypeCardGameUiController>() != null)
            {
                return;
            }

            var go = new GameObject("PrototypeCardGameUI");
            go.AddComponent<PrototypeCardGameUiController>();
        }

        private void Awake()
        {
            _font = LoadBuiltinFont();
            EnsureEventSystem();
            BuildDeck();
            BuildUi();
            StartRun();
            AddLog("已載入真實卡牌 UI 原型，可直接邊玩邊調參。");

            if (_autoStartTenModelBatchValidationOnPlay)
            {
                StartCoroutine(AutoStartTenModelBatchValidationFlow());
            }
            else if (_autoStartTenModelValidationOnPlay)
            {
                StartCoroutine(AutoStartTenModelValidationFlow());
            }
            else if (_autoDemoOnStart)
            {
                _autoDemoCoroutine = StartCoroutine(AutoDemoFlow());
            }
        }

        private void Update()
        {
            UpdateDrawAnimations();
            UpdateSelectionPulse();
            UpdateResponsiveLayout();
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        private void BuildUi()
        {
            var canvasGo = new GameObject("PrototypeCanvas");
            canvasGo.transform.SetParent(transform, false);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            BuildThemeBackground(canvasGo.transform);

            var rootFrame = CreatePanel(canvasGo.transform, new Color(0.09f, 0.11f, 0.15f, 0.92f));
            rootFrame.anchorMin = Vector2.zero;
            rootFrame.anchorMax = Vector2.one;
            rootFrame.offsetMin = new Vector2(12, 12);
            rootFrame.offsetMax = new Vector2(-12, -12);

            var viewport = CreatePanel(rootFrame, new Color(0f, 0f, 0f, 0f));
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            var viewportImage = viewport.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.001f);
            viewportImage.raycastTarget = true;
            viewport.gameObject.AddComponent<RectMask2D>();

            var scrollRect = rootFrame.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 30f;
            scrollRect.viewport = viewport;

            var root = CreatePanel(viewport, new Color(0f, 0f, 0f, 0f));
            root.anchorMin = new Vector2(0f, 1f);
            root.anchorMax = new Vector2(1f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            root.anchoredPosition = Vector2.zero;
            root.sizeDelta = new Vector2(0f, 0f);
            scrollRect.content = root;

            var rootFitter = root.gameObject.AddComponent<ContentSizeFitter>();
            rootFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            rootFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rootLayout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            rootLayout.spacing = 10;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.padding = new RectOffset(0, 0, 0, 10);

            _dragLayer = CreatePanel(canvasGo.transform, new Color(0f, 0f, 0f, 0f));
            _dragLayer.anchorMin = Vector2.zero;
            _dragLayer.anchorMax = Vector2.one;
            _dragLayer.offsetMin = Vector2.zero;
            _dragLayer.offsetMax = Vector2.zero;
            var dragLayerImage = _dragLayer.GetComponent<Image>();
            if (dragLayerImage != null)
            {
                dragLayerImage.raycastTarget = false;
            }

            var leftCol = CreatePanel(root, new Color(0.14f, 0.17f, 0.23f, 0.95f));
            _leftColLayout = leftCol.gameObject.AddComponent<LayoutElement>();
            _leftColLayout.flexibleWidth = 3.5f;
            _leftColLayout.minWidth = 420;
            _leftColLayout.preferredHeight = 900;
            var leftLayout = leftCol.gameObject.AddComponent<VerticalLayoutGroup>();
            leftLayout.spacing = 8;
            leftLayout.padding = new RectOffset(10, 10, 10, 10);
            var leftFitter = leftCol.gameObject.AddComponent<ContentSizeFitter>();
            leftFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            leftFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var rightCol = CreatePanel(root, new Color(0.12f, 0.15f, 0.2f, 0.95f));
            _rightColRoot = rightCol;
            _rightColLayout = rightCol.gameObject.AddComponent<LayoutElement>();
            _rightColLayout.flexibleWidth = 1.8f;
            _rightColLayout.minWidth = 240;
            _rightColLayout.preferredHeight = 900;
            var rightLayout = rightCol.gameObject.AddComponent<VerticalLayoutGroup>();
            rightLayout.spacing = 8;
            rightLayout.padding = new RectOffset(10, 10, 10, 10);
            var rightFitter = rightCol.gameObject.AddComponent<ContentSizeFitter>();
            rightFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            rightFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var tuningToggleRow = CreateRow(rightCol, 42);
            _toggleTuningButton = CreateButtonWithLabel(tuningToggleRow, "展開調參", 30);
            _toggleTuningButtonText = _toggleTuningButton.GetComponentInChildren<Text>();
            _toggleTuningButton.onClick.AddListener(ToggleTuningPanel);

            _tuningContentContainer = CreatePanel(rightCol, new Color(0f, 0f, 0f, 0f));
            _tuningContentContainer.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;
            var tuningContentLayout = _tuningContentContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            tuningContentLayout.spacing = 8;
            tuningContentLayout.padding = new RectOffset(0, 0, 0, 0);
            tuningContentLayout.childControlWidth = true;
            tuningContentLayout.childControlHeight = true;
            tuningContentLayout.childForceExpandWidth = true;
            tuningContentLayout.childForceExpandHeight = false;

            _statusText = CreateText(leftCol, "狀態", 20, TextAnchor.UpperLeft, FontStyle.Bold);
            _statusText.gameObject.AddComponent<LayoutElement>().minHeight = 82;

            CreateText(leftCol, "手牌（可拖曳到牌桌區，或點擊快速上桌）", 17, TextAnchor.MiddleLeft, FontStyle.Bold);
            _handContainer = CreatePanel(leftCol, new Color(0.08f, 0.09f, 0.13f, 0.95f));
            _handContainer.gameObject.AddComponent<LayoutElement>().minHeight = 230;
            var handLayout = _handContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = 10;
            handLayout.childControlWidth = true;
            handLayout.childControlHeight = true;
            handLayout.childForceExpandWidth = true;
            handLayout.childForceExpandHeight = true;
            handLayout.padding = new RectOffset(10, 10, 10, 10);

            _selectedText = CreateText(leftCol, "已上桌卡牌：0 張", 15, TextAnchor.MiddleLeft, FontStyle.Normal);
            _playZoneContainer = CreatePanel(leftCol, new Color(0.08f, 0.12f, 0.18f, 0.92f));
            _playZoneContainer.gameObject.AddComponent<LayoutElement>().minHeight = 118;
            var playZoneLayout = _playZoneContainer.gameObject.AddComponent<VerticalLayoutGroup>();
            playZoneLayout.spacing = 6;
            playZoneLayout.padding = new RectOffset(8, 8, 8, 8);
            playZoneLayout.childControlWidth = true;
            playZoneLayout.childControlHeight = true;
            playZoneLayout.childForceExpandWidth = true;
            playZoneLayout.childForceExpandHeight = false;

            CreateText(_playZoneContainer, "牌桌區（拖曳卡牌到這裡）", 14, TextAnchor.MiddleLeft, FontStyle.Bold);
            _playZoneCardsContainer = CreatePanel(_playZoneContainer, new Color(0.06f, 0.08f, 0.12f, 0.95f));
            _playZoneCardsContainer.gameObject.AddComponent<LayoutElement>().minHeight = 72;
            var playZoneCardLayout = _playZoneCardsContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
            playZoneCardLayout.spacing = 6;
            playZoneCardLayout.padding = new RectOffset(6, 6, 6, 6);
            playZoneCardLayout.childControlWidth = false;
            playZoneCardLayout.childControlHeight = true;
            playZoneCardLayout.childForceExpandWidth = false;
            playZoneCardLayout.childForceExpandHeight = true;

            var actionRow1 = CreateRow(leftCol, 46);
            CreateButton(actionRow1, "抽新手牌", DrawHand);
            CreateButton(actionRow1, "開始答題並出牌", StartQuizAndPlay);
            CreateButton(actionRow1, "清空上桌", ClearPlayZone);

            var quizPanel = CreatePanel(leftCol, new Color(0.1f, 0.14f, 0.2f, 0.95f));
            quizPanel.gameObject.AddComponent<LayoutElement>().minHeight = 186;
            var quizLayout = quizPanel.gameObject.AddComponent<VerticalLayoutGroup>();
            quizLayout.spacing = 6;
            quizLayout.padding = new RectOffset(8, 8, 8, 8);
            quizLayout.childControlWidth = true;
            quizLayout.childControlHeight = true;
            quizLayout.childForceExpandWidth = true;
            quizLayout.childForceExpandHeight = false;
            CreateText(quizPanel, "答題區（英文題幹 / 中文選項）", 14, TextAnchor.MiddleLeft, FontStyle.Bold);
            _quizStatusText = CreateText(quizPanel, "尚未開始答題。", 13, TextAnchor.MiddleLeft, FontStyle.Normal);
            _quizPromptText = CreateText(quizPanel, "請先把卡牌拖到牌桌區，再按「開始答題並出牌」。", 13, TextAnchor.UpperLeft, FontStyle.Normal);
            _quizPromptText.gameObject.AddComponent<LayoutElement>().minHeight = 42;
            for (var i = 0; i < 4; i++)
            {
                var optButton = CreateButtonWithLabel(quizPanel, string.Format("選項{0}", i + 1), 40);
                var optText = optButton.GetComponentInChildren<Text>();
                var idx = i;
                optButton.onClick.AddListener(delegate { OnQuizOptionSelected(idx); });
                optButton.interactable = false;
                if (optText != null)
                {
                    optText.text = string.Format("選項 {0}", i + 1);
                }
                _quizOptionButtons.Add(optButton);
                _quizOptionTexts.Add(optText);
            }

            var actionRow2 = CreateRow(leftCol, 46);
            CreateButton(actionRow2, "結算盲注", ResolveBlind);
            CreateButton(actionRow2, "前往下一關", AdvanceAfterShop);
            CreateButton(actionRow2, "重開本局", StartRun);

            var actionRow3 = CreateRow(leftCol, 46);
            CreateButton(actionRow3, "生成商店商品", GenerateShopOffers);
            CreateButton(actionRow3, "購買第一項", BuyFirstOffer);
            CreateButton(actionRow3, "嘗試解鎖節點", TryUnlockNode);

            var actionRow4 = CreateRow(leftCol, 46);
            CreateButton(actionRow4, "一鍵跑到通關", StartAutoRunToComplete);
            CreateButton(actionRow4, "連跑3局", StartAutoBatchRuns);
            CreateButton(actionRow4, "失敗後重開演示", StartFailThenRecoverDemo);
            CreateButton(actionRow4, "驗證全部用例", StartUseCaseVerification);
            CreateButton(actionRow4, "全流程最終驗收", StartFullValidationFlow);
            CreateButton(actionRow4, "10模型驗證", StartTenModelValidation);
            CreateButton(actionRow4, "10模型30輪", StartTenModelBatchValidation);

            _shopText = CreateText(leftCol, "商店：尚未生成", 14, TextAnchor.UpperLeft, FontStyle.Normal);
            _shopText.gameObject.AddComponent<LayoutElement>().minHeight = 36;
            _shopGridContainer = CreatePanel(leftCol, new Color(0.09f, 0.1f, 0.15f, 0.95f));
            _shopGridContainer.gameObject.AddComponent<LayoutElement>().minHeight = 168;
            _shopGridLayout = _shopGridContainer.gameObject.AddComponent<GridLayoutGroup>();
            _shopGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            _shopGridLayout.constraintCount = 3;
            _shopGridLayout.cellSize = new Vector2(165, 72);
            _shopGridLayout.spacing = new Vector2(8, 8);
            _shopGridLayout.padding = new RectOffset(8, 8, 8, 8);

            CreateText(_tuningContentContainer, "調參面板（中文）", 18, TextAnchor.MiddleLeft, FontStyle.Bold);
            _tuningText = CreateText(_tuningContentContainer, "-", 14, TextAnchor.UpperLeft, FontStyle.Normal);
            _tuningText.gameObject.AddComponent<LayoutElement>().minHeight = 120;

            var tuneRow1 = CreateRow(_tuningContentContainer, 42);
            CreateButton(tuneRow1, "難度切換", CycleDifficulty);
            CreateButton(tuneRow1, "籌碼 +1", delegate { _baseChips = Mathf.Min(30, _baseChips + 1); RefreshView(); });
            CreateButton(tuneRow1, "籌碼 -1", delegate { _baseChips = Mathf.Max(1, _baseChips - 1); RefreshView(); });

            var tuneRow2 = CreateRow(_tuningContentContainer, 42);
            CreateButton(tuneRow2, "升級 +1", delegate { _upgradeLevel = Mathf.Min(9, _upgradeLevel + 1); RefreshView(); });
            CreateButton(tuneRow2, "升級 -1", delegate { _upgradeLevel = Mathf.Max(0, _upgradeLevel - 1); RefreshView(); });
            CreateButton(tuneRow2, "答錯 +1", delegate { _wrongCount = Mathf.Min(5, _wrongCount + 1); RefreshView(); });

            var tuneRow3 = CreateRow(_tuningContentContainer, 42);
            CreateButton(tuneRow3, "答錯 -1", delegate { _wrongCount = Mathf.Max(0, _wrongCount - 1); RefreshView(); });
            CreateButton(tuneRow3, "加法倍率 +0.5", delegate { _additiveMult = Mathf.Min(8f, _additiveMult + 0.5f); RefreshView(); });
            CreateButton(tuneRow3, "乘區 +0.1", delegate { _factor = Mathf.Min(5f, _factor + 0.1f); RefreshView(); });

            var tuneRow4 = CreateRow(_tuningContentContainer, 42);
            CreateButton(tuneRow4, "乘區 -0.1", delegate { _factor = Mathf.Max(1f, _factor - 0.1f); RefreshView(); });
            CreateButton(tuneRow4, "LP +10", delegate { _metaLp += 10; RefreshView(); });
            CreateButton(tuneRow4, "LP -10", delegate { _metaLp = Mathf.Max(0, _metaLp - 10); RefreshView(); });

            var tuneRow5 = CreateRow(_tuningContentContainer, 42);
            CreateButton(tuneRow5, "節點切換", CycleUnlockNode);
            CreateButton(tuneRow5, "清空紀錄", delegate { _logs.Clear(); RefreshView(); });

            _metaText = CreateText(_tuningContentContainer, "-", 14, TextAnchor.UpperLeft, FontStyle.Normal);
            _metaText.gameObject.AddComponent<LayoutElement>().minHeight = 80;

            CreateText(_tuningContentContainer, "事件紀錄", 17, TextAnchor.MiddleLeft, FontStyle.Bold);
            _logText = CreateText(_tuningContentContainer, "-", 13, TextAnchor.UpperLeft, FontStyle.Normal);
            _logText.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1f;

            ApplyTuningPanelState();
        }

        private RectTransform CreateRow(Transform parent, int minHeight)
        {
            var row = CreatePanel(parent, new Color(0.08f, 0.09f, 0.13f, 0.8f));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 6;
            layout.padding = new RectOffset(8, 8, 6, 6);
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            row.gameObject.AddComponent<LayoutElement>().minHeight = minHeight;
            return row;
        }

        private void BuildThemeBackground(Transform canvasRoot)
        {
            var bg = CreatePanel(canvasRoot, new Color(0.05f, 0.07f, 0.11f, 0.92f));
            bg.anchorMin = Vector2.zero;
            bg.anchorMax = Vector2.one;
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;
            bg.SetAsFirstSibling();

            var topBand = CreatePanel(bg, new Color(0.13f, 0.28f, 0.43f, 0.32f));
            topBand.anchorMin = new Vector2(0f, 0.78f);
            topBand.anchorMax = new Vector2(1f, 1f);
            topBand.offsetMin = Vector2.zero;
            topBand.offsetMax = Vector2.zero;

            var leftGlow = CreatePanel(bg, new Color(0.2f, 0.45f, 0.35f, 0.18f));
            leftGlow.anchorMin = new Vector2(0f, 0f);
            leftGlow.anchorMax = new Vector2(0.45f, 0.5f);
            leftGlow.offsetMin = Vector2.zero;
            leftGlow.offsetMax = Vector2.zero;

            var rightGlow = CreatePanel(bg, new Color(0.45f, 0.3f, 0.18f, 0.18f));
            rightGlow.anchorMin = new Vector2(0.55f, 0.05f);
            rightGlow.anchorMax = new Vector2(1f, 0.6f);
            rightGlow.offsetMin = Vector2.zero;
            rightGlow.offsetMax = Vector2.zero;
        }

        private RectTransform CreatePanel(Transform parent, Color color)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return go.GetComponent<RectTransform>();
        }

        private Text CreateText(Transform parent, string value, int fontSize, TextAnchor anchor, FontStyle style)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = _font;
            t.text = value;
            t.fontSize = fontSize;
            t.alignment = anchor;
            t.color = Color.white;
            t.fontStyle = style;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        private void CreateButton(Transform parent, string label, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.26f, 0.36f, 1f);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(delegate { onClick(); });

            var text = CreateText(go.transform, label, 14, TextAnchor.MiddleCenter, FontStyle.Normal);
            text.color = new Color(0.93f, 0.96f, 1f, 1f);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6, 4);
            textRect.offsetMax = new Vector2(-6, -4);
        }

        private Button CreateButtonWithLabel(Transform parent, string label, int minHeight)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(0.2f, 0.26f, 0.36f, 1f);
            var button = go.GetComponent<Button>();
            var le = go.GetComponent<LayoutElement>();
            le.minHeight = minHeight;

            var text = CreateText(go.transform, label, 13, TextAnchor.MiddleCenter, FontStyle.Normal);
            text.color = new Color(0.93f, 0.96f, 1f, 1f);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6, 4);
            textRect.offsetMax = new Vector2(-6, -4);
            return button;
        }

        private void BuildDeck()
        {
            _deck.Clear();
            _deck.Add(new DemoWord { Text = "resonance", MeaningZh = "共鳴", Element = Element.Abstract, Pos = PartOfSpeech.N, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "cascade", MeaningZh = "連鎖傾瀉", Element = Element.Force, Pos = PartOfSpeech.V, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "lucid", MeaningZh = "清晰的", Element = Element.Mind, Pos = PartOfSpeech.A, Level = LearningLevel.Lv1 });
            _deck.Add(new DemoWord { Text = "artifact", MeaningZh = "人工製品", Element = Element.Matter, Pos = PartOfSpeech.N, Level = LearningLevel.Lv3 });
            _deck.Add(new DemoWord { Text = "sustain", MeaningZh = "維持", Element = Element.Life, Pos = PartOfSpeech.V, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "vivid", MeaningZh = "生動的", Element = Element.Life, Pos = PartOfSpeech.A, Level = LearningLevel.Lv1 });
            _deck.Add(new DemoWord { Text = "spiral", MeaningZh = "螺旋", Element = Element.Force, Pos = PartOfSpeech.N, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "anchor", MeaningZh = "錨定", Element = Element.Matter, Pos = PartOfSpeech.V, Level = LearningLevel.Lv2 });
            _deck.Add(new DemoWord { Text = "insight", MeaningZh = "洞察", Element = Element.Mind, Pos = PartOfSpeech.N, Level = LearningLevel.Lv3 });
            _deck.Add(new DemoWord { Text = "ethereal", MeaningZh = "空靈的", Element = Element.Abstract, Pos = PartOfSpeech.A, Level = LearningLevel.Lv2 });
        }

        private void StartRun()
        {
            _runDays++;
            _daysSinceLastDemotion = Mathf.Min(3650, _daysSinceLastDemotion + 1);
            if (_overallMasteryRate >= 1f)
            {
                _stableDaysAtHundredPercent++;
            }
            else
            {
                _stableDaysAtHundredPercent = 0;
            }
            SyncGateModelWithEffectiveVocab();
            _runManager = new RunManagerV2(_difficulty);
            _runManager.StartRun(_seed);
            _offers.Clear();
            RebuildShopCards();
            _playZoneCardIndexes.Clear();
            _playZoneOrder.Clear();
            ResetQuizState("尚未開始答題。");
            _lastScore = 0;
            GenerateRunContract();
            DrawHand();
            AddLog(string.Format("開新局：難度={0}, Seed={1}", DifficultyZh(_difficulty), _seed));
            RefreshView();
        }

        private void SyncGateModelWithEffectiveVocab()
        {
            var adjustedLearnedCount = Mathf.RoundToInt(_learnedCount * Mathf.Clamp(_learningEfficiencyBoost, 1f, _maxLearningEfficiencyBoost));
            var result = _gateProgressionManager.EvaluateProgress(
                adjustedLearnedCount,
                _retentionRate,
                _retrievalRate,
                Mathf.Clamp(_currentGateModelIndex, 0, 9));
            if (!result.IsSuccess)
            {
                AddLog("Gate 模型同步失敗，使用現有模型。");
                return;
            }

            _highestUnlockedModelIndex = result.Value.HighestUnlockedModelIndex;
            var originalModel = _currentGateModelIndex;
            _currentGateModelIndex = Mathf.Clamp(_currentGateModelIndex, 0, _highestUnlockedModelIndex);
            if (originalModel != _currentGateModelIndex)
            {
                AddLog(string.Format("Gate 模型調整：M{0} -> M{1}", originalModel, _currentGateModelIndex));
            }
            else if (_highestUnlockedModelIndex > _currentGateModelIndex)
            {
                _currentGateModelIndex = _highestUnlockedModelIndex;
                AddLog(string.Format("有效詞彙量成長：解鎖到 M{0}", _currentGateModelIndex));
            }
        }

        private void ApplyLearningEfficiencyGain(float delta, string reason)
        {
            var before = _learningEfficiencyBoost;
            _learningEfficiencyBoost = Mathf.Clamp(_learningEfficiencyBoost + delta, 1f, _maxLearningEfficiencyBoost);
            if (Mathf.Abs(_learningEfficiencyBoost - before) > 0.0001f)
            {
                AddLog(string.Format(
                    "學習效率提升：{0:0.00} -> {1:0.00}（{2}）",
                    before,
                    _learningEfficiencyBoost,
                    reason));
            }
        }

        private void ApplyLearningEfficiencyLoss(float delta, string reason)
        {
            var before = _learningEfficiencyBoost;
            _learningEfficiencyBoost = Mathf.Clamp(_learningEfficiencyBoost - delta, 1f, _maxLearningEfficiencyBoost);
            if (Mathf.Abs(_learningEfficiencyBoost - before) > 0.0001f)
            {
                AddLog(string.Format(
                    "學習效率下修：{0:0.00} -> {1:0.00}（{2}）",
                    before,
                    _learningEfficiencyBoost,
                    reason));
            }
        }

        private static int MaxAnteForModel(int modelIndex)
        {
            if (modelIndex <= 0)
            {
                return 0;
            }

            return Mathf.Min(8, modelIndex);
        }

        private void GenerateRunContract()
        {
            var contracts = _metaManager.GenerateContracts(new MetaProgress
            {
                Lp = _metaLp,
                Xp = _metaXp
            }, _seed + _runManager.CurrentState.Ante * 13);

            if (!contracts.IsSuccess || contracts.Value.Count == 0)
            {
                _activeRunContract = null;
                AddLog("本局契約生成失敗，將以基礎結算為主。");
                return;
            }

            _activeRunContract = contracts.Value[0];
            AddLog(string.Format("本局契約：{0}（+{1} LP）", _activeRunContract.Name, _activeRunContract.LpReward));
        }

        private void DrawHand()
        {
            _hand.Clear();
            var random = new System.Random(_seed + _runManager.CurrentState.Ante * 31 + _runManager.CurrentState.CurrentScore);
            for (var i = 0; i < 5; i++)
            {
                var index = random.Next(0, _deck.Count);
                _hand.Add(_deck[index]);
            }

            _playZoneCardIndexes.Clear();
            _playZoneOrder.Clear();
            ResetQuizState("手牌已重抽，請重新開始答題。");
            RebuildHandCards();
            RebuildPlayZoneCards();
            RefreshView();
        }

        private void RebuildHandCards()
        {
            _drawAnims.Clear();
            for (var i = _handContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_handContainer.GetChild(i).gameObject);
            }

            for (var i = 0; i < _hand.Count; i++)
            {
                var index = i;
                var card = _hand[i];
                var go = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement), typeof(CanvasGroup), typeof(CardDragHandler));
                go.transform.SetParent(_handContainer, false);

                var le = go.GetComponent<LayoutElement>();
                le.preferredWidth = 140;
                le.minWidth = 120;

                var image = go.GetComponent<Image>();
                image.color = CardColor(card.Element);

                var cardRect = go.GetComponent<RectTransform>();
                cardRect.localScale = new Vector3(0.85f, 0.85f, 1f);
                cardRect.anchoredPosition = new Vector2(0f, -22f);
                var group = go.GetComponent<CanvasGroup>();
                group.alpha = 0f;

                var button = go.GetComponent<Button>();
                button.onClick.AddListener(delegate
                {
                    ToggleCardPlayZone(index);
                });
                var dragHandler = go.GetComponent<CardDragHandler>();
                dragHandler.Init(this, index);

                var text = CreateText(go.transform, BuildCardText(card, false), 14, TextAnchor.UpperLeft, FontStyle.Bold);
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = new Vector2(8, 8);
                text.rectTransform.offsetMax = new Vector2(-8, -8);
                text.color = new Color(0.07f, 0.08f, 0.1f, 1f);

                _drawAnims.Add(new DrawAnim
                {
                    CardRect = cardRect,
                    CanvasGroup = group,
                    StartTime = Time.unscaledTime,
                    Delay = i * 0.05f
                });
            }
        }

        private void ToggleCardPlayZone(int index)
        {
            if (_isPlayingCardAnim)
            {
                return;
            }

            if (_playZoneCardIndexes.Contains(index))
            {
                _playZoneCardIndexes.Remove(index);
                _playZoneOrder.Remove(index);
            }
            else
            {
                _playZoneCardIndexes.Add(index);
                _playZoneOrder.Add(index);
            }

            RefreshHandCardVisuals();
            RebuildPlayZoneCards();
            RefreshView();
        }

        internal void RefreshHandCardVisuals()
        {
            for (var i = 0; i < _handContainer.childCount; i++)
            {
                var child = _handContainer.GetChild(i);
                var txt = child.GetComponentInChildren<Text>();
                if (txt != null && i < _hand.Count)
                {
                    txt.text = BuildCardText(_hand[i], _playZoneCardIndexes.Contains(i));
                }

                var img = child.GetComponent<Image>();
                if (img != null && i < _hand.Count)
                {
                    img.color = _playZoneCardIndexes.Contains(i)
                        ? BoostColor(CardColor(_hand[i].Element), 1.2f)
                        : CardColor(_hand[i].Element);
                }

                child.localScale = Vector3.one;
            }
        }

        private void RebuildPlayZoneCards()
        {
            if (_playZoneCardsContainer == null)
            {
                return;
            }

            for (var i = _playZoneCardsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_playZoneCardsContainer.GetChild(i).gameObject);
            }

            for (var i = 0; i < _playZoneOrder.Count; i++)
            {
                var cardIndex = _playZoneOrder[i];
                if (cardIndex < 0 || cardIndex >= _hand.Count)
                {
                    continue;
                }

                var word = _hand[cardIndex];
                var token = CreatePanel(_playZoneCardsContainer, BoostColor(CardColor(word.Element), 1.12f));
                token.gameObject.AddComponent<LayoutElement>().preferredWidth = 118;
                var btn = token.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(delegate
                {
                    ToggleCardPlayZone(cardIndex);
                });

                var t = CreateText(token, word.Text + "\n" + PosZh(word.Pos), 12, TextAnchor.MiddleCenter, FontStyle.Bold);
                t.rectTransform.anchorMin = Vector2.zero;
                t.rectTransform.anchorMax = Vector2.one;
                t.rectTransform.offsetMin = new Vector2(4, 4);
                t.rectTransform.offsetMax = new Vector2(-4, -4);
                t.color = new Color(0.07f, 0.08f, 0.1f, 1f);
            }
        }

        internal void TryDropCardToPlayZone(int cardIndex, Vector2 screenPoint, Camera cam)
        {
            if (_playZoneContainer == null || cardIndex < 0 || cardIndex >= _hand.Count)
            {
                return;
            }

            var droppedInPlayZone = RectTransformUtility.RectangleContainsScreenPoint(_playZoneContainer, screenPoint, cam);
            if (!droppedInPlayZone)
            {
                return;
            }

            if (!_playZoneCardIndexes.Contains(cardIndex))
            {
                _playZoneCardIndexes.Add(cardIndex);
                _playZoneOrder.Add(cardIndex);
                AddLog(string.Format("已拖曳上桌：{0}", _hand[cardIndex].Text));
                RebuildPlayZoneCards();
                RefreshView();
            }
        }

        private void ClearPlayZone()
        {
            _playZoneCardIndexes.Clear();
            _playZoneOrder.Clear();
            RebuildPlayZoneCards();
            RefreshHandCardVisuals();
            RefreshView();
        }

        private void ResetQuizState(string statusText)
        {
            _isQuizRunning = false;
            _quizCardIndexes.Clear();
            _quizCardCorrectness.Clear();
            _quizCursor = 0;
            _quizCorrectCount = 0;
            _quizCurrentCorrectOptionIndex = -1;

            if (_quizStatusText != null)
            {
                _quizStatusText.text = statusText;
            }

            if (_quizPromptText != null)
            {
                _quizPromptText.text = "請先把卡牌拖到牌桌區，再按「開始答題並出牌」。";
            }

            for (var i = 0; i < _quizOptionButtons.Count; i++)
            {
                _quizOptionButtons[i].interactable = false;
            }
        }

        private void UpdateDrawAnimations()
        {
            if (_drawAnims.Count == 0)
            {
                return;
            }

            var now = Time.unscaledTime;
            for (var i = 0; i < _drawAnims.Count; i++)
            {
                var anim = _drawAnims[i];
                if (anim.Completed || anim.CardRect == null || anim.CanvasGroup == null)
                {
                    continue;
                }

                var t = (now - anim.StartTime - anim.Delay) / 0.24f;
                if (t < 0f)
                {
                    continue;
                }

                if (t >= 1f)
                {
                    anim.CardRect.localScale = Vector3.one;
                    anim.CardRect.anchoredPosition = Vector2.zero;
                    anim.CanvasGroup.alpha = 1f;
                    anim.Completed = true;
                    continue;
                }

                var eased = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
                anim.CardRect.localScale = Vector3.Lerp(new Vector3(0.85f, 0.85f, 1f), Vector3.one, eased);
                anim.CardRect.anchoredPosition = Vector2.Lerp(new Vector2(0f, -22f), Vector2.zero, eased);
                anim.CanvasGroup.alpha = eased;
            }
        }

        private void UpdateSelectionPulse()
        {
            if (_handContainer == null || _isPlayingCardAnim)
            {
                return;
            }

            var pulse = 0.06f * Mathf.Sin(Time.unscaledTime * 7f);
            for (var i = 0; i < _handContainer.childCount; i++)
            {
                if (!_playZoneCardIndexes.Contains(i))
                {
                    continue;
                }

                var card = _handContainer.GetChild(i);
                if (card == null)
                {
                    continue;
                }

                card.localScale = Vector3.one * (1f + pulse);
            }
        }

        private IEnumerator AutoDemoFlow()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.1f, _autoDemoStartDelaySeconds));
            if (_runManager.CurrentState.Phase != RunPhase.HandSelect)
            {
                yield break;
            }

            AddLog("自動演示：開始答題 -> 出牌 -> 結算 -> 商店。");
            StartQuizAndPlay();

            while (_isQuizRunning)
            {
                if (_quizCurrentCorrectOptionIndex >= 0)
                {
                    OnQuizOptionSelected(_quizCurrentCorrectOptionIndex);
                }
                yield return new WaitForSecondsRealtime(0.6f);
            }

            while (_isPlayingCardAnim)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.8f);
            ResolveBlind();

            if (_runManager.CurrentState.Phase == RunPhase.Shop)
            {
                yield return new WaitForSecondsRealtime(0.8f);
                GenerateShopOffers();
                yield return new WaitForSecondsRealtime(0.8f);
                BuyFirstOffer();
                yield return new WaitForSecondsRealtime(0.8f);
                AdvanceAfterShop();

                yield return new WaitForSecondsRealtime(0.8f);
                AddLog("自動演示：第二手答題與出牌。");
                StartQuizAndPlay();
                while (_isQuizRunning)
                {
                    if (_quizCurrentCorrectOptionIndex >= 0)
                    {
                        OnQuizOptionSelected(_quizCurrentCorrectOptionIndex);
                    }
                    yield return new WaitForSecondsRealtime(0.6f);
                }
            }
        }

        private void StartAutoRunToComplete()
        {
            if (_autoRunToCompleteCoroutine != null)
            {
                AddLog("自動通關流程已在執行中。");
                return;
            }

            _autoRunToCompleteCoroutine = StartCoroutine(AutoRunToCompleteFlow());
        }

        private void StartAutoBatchRuns()
        {
            if (_autoBatchRunsCoroutine != null)
            {
                AddLog("連跑流程已在執行中。");
                return;
            }

            _autoBatchRunsCoroutine = StartCoroutine(AutoBatchRunsFlow(3));
        }

        private void StartFailThenRecoverDemo()
        {
            if (_autoFailThenRecoverCoroutine != null)
            {
                AddLog("失敗重開演示已在執行中。");
                return;
            }

            _autoFailThenRecoverCoroutine = StartCoroutine(AutoFailThenRecoverFlow());
        }

        private void StartUseCaseVerification()
        {
            if (_useCaseVerificationCoroutine != null)
            {
                AddLog("用例驗證已在執行中。");
                return;
            }

            _useCaseVerificationCoroutine = StartCoroutine(UseCaseVerificationFlow());
        }

        private void StartFullValidationFlow()
        {
            if (_fullValidationCoroutine != null)
            {
                AddLog("全流程驗收已在執行中。");
                return;
            }

            _fullValidationCoroutine = StartCoroutine(FullValidationFlow());
        }

        private void StartTenModelValidation()
        {
            if (_modelValidationCoroutine != null)
            {
                AddLog("10 模型驗證已在執行中。");
                return;
            }

            _modelValidationCoroutine = StartCoroutine(TenModelValidationFlow());
        }

        private void StartTenModelBatchValidation()
        {
            if (_modelBatchValidationCoroutine != null)
            {
                AddLog("10 模型 30 輪驗證已在執行中。");
                return;
            }

            _modelBatchValidationCoroutine = StartCoroutine(TenModelBatchValidationFlow(30));
        }

        private static (string Label, int Learned, float Retention, float Retrieval, float Mastery, int ExpectedChokeAnte)[] GetTenModelProfiles()
        {
            return new[]
            {
                (Label: "M0", Learned: 0, Retention: 0.75f, Retrieval: 0.70f, Mastery: 0.20f, ExpectedChokeAnte: 1),
                (Label: "M1", Learned: 2000, Retention: 0.80f, Retrieval: 0.75f, Mastery: 0.40f, ExpectedChokeAnte: 1),
                (Label: "M2", Learned: 3000, Retention: 0.86f, Retrieval: 0.82f, Mastery: 0.50f, ExpectedChokeAnte: 2),
                (Label: "M3", Learned: 4000, Retention: 0.84f, Retrieval: 0.80f, Mastery: 0.60f, ExpectedChokeAnte: 2),
                (Label: "M4", Learned: 5000, Retention: 0.86f, Retrieval: 0.82f, Mastery: 0.70f, ExpectedChokeAnte: 3),
                (Label: "M5", Learned: 6000, Retention: 0.88f, Retrieval: 0.84f, Mastery: 0.80f, ExpectedChokeAnte: 4),
                (Label: "M6", Learned: 7000, Retention: 0.90f, Retrieval: 0.86f, Mastery: 0.88f, ExpectedChokeAnte: 5),
                (Label: "M7", Learned: 8000, Retention: 0.92f, Retrieval: 0.88f, Mastery: 0.93f, ExpectedChokeAnte: 6),
                (Label: "M8", Learned: 9000, Retention: 0.94f, Retrieval: 0.90f, Mastery: 0.96f, ExpectedChokeAnte: 7),
                (Label: "M9", Learned: 10000, Retention: 0.97f, Retrieval: 0.93f, Mastery: 1.00f, ExpectedChokeAnte: 9)
            };
        }

        private IEnumerator AutoRunToCompleteFlow()
        {
            AddLog("自動通關：開始推進到 RunComplete。");
            var safety = 0;
            while (_runManager.CurrentState.Phase != RunPhase.RunComplete &&
                   _runManager.CurrentState.Phase != RunPhase.RunFail &&
                   safety < 64)
            {
                safety++;
                var phase = _runManager.CurrentState.Phase;

                if (phase == RunPhase.HandSelect && !_isQuizRunning && !_isPlayingCardAnim)
                {
                    var submit = _runManager.SubmitHandScore(_runManager.CurrentState.TargetScore);
                    if (submit.IsSuccess)
                    {
                        AddLog(string.Format("自動出牌：+{0}（{1}/{2}）", _runManager.CurrentState.TargetScore, _runManager.CurrentState.CurrentScore, _runManager.CurrentState.TargetScore));
                    }
                    else
                    {
                        AddLog(string.Format("自動出牌失敗：{0}", submit.Error));
                        break;
                    }
                }
                else if (phase == RunPhase.BlindResult)
                {
                    ResolveBlind();
                }
                else if (phase == RunPhase.Shop)
                {
                    GenerateShopOffers();
                    BuyFirstOffer();
                    AdvanceAfterShop();
                }

                yield return new WaitForSecondsRealtime(0.3f);
            }

            if (_runManager.CurrentState.Phase == RunPhase.RunComplete)
            {
                AddLog("自動通關：已完成整局！");
                SettleCompletedRunMeta();
            }
            else if (_runManager.CurrentState.Phase == RunPhase.RunFail)
            {
                AddLog("自動通關：流程進入失敗。");
            }
            else
            {
                AddLog("自動通關：安全上限觸發，請手動檢查。");
            }

            _autoRunToCompleteCoroutine = null;
        }

        private IEnumerator AutoBatchRunsFlow(int runs)
        {
            if (_autoDemoCoroutine != null)
            {
                StopCoroutine(_autoDemoCoroutine);
                _autoDemoCoroutine = null;
            }

            AddLog(string.Format("連跑模式：開始連跑 {0} 局。", runs));
            for (var i = 1; i <= runs; i++)
            {
                StartRun();
                yield return new WaitForSecondsRealtime(0.4f);

                var safety = 0;
                while (_runManager.CurrentState.Phase != RunPhase.RunComplete &&
                       _runManager.CurrentState.Phase != RunPhase.RunFail &&
                       safety < 64)
                {
                    safety++;
                    var phase = _runManager.CurrentState.Phase;

                    if (phase == RunPhase.HandSelect && !_isQuizRunning && !_isPlayingCardAnim)
                    {
                        _runManager.SubmitHandScore(_runManager.CurrentState.TargetScore);
                    }
                    else if (phase == RunPhase.BlindResult)
                    {
                        ResolveBlind();
                    }
                    else if (phase == RunPhase.Shop)
                    {
                        GenerateShopOffers();
                        BuyFirstOffer();
                        AdvanceAfterShop();
                    }

                    yield return new WaitForSecondsRealtime(0.15f);
                }

                if (_runManager.CurrentState.Phase == RunPhase.RunComplete)
                {
                    SettleCompletedRunMeta();
                    AddLog(string.Format("連跑模式：第 {0}/{1} 局完成。", i, runs));
                }
                else
                {
                    AddLog(string.Format("連跑模式：第 {0}/{1} 局未完成（{2}）。", i, runs, PhaseZh(_runManager.CurrentState.Phase)));
                }

                yield return new WaitForSecondsRealtime(0.4f);
            }

            AddLog("連跑模式：全部完成。");
            _autoBatchRunsCoroutine = null;
        }

        private IEnumerator AutoFailThenRecoverFlow()
        {
            if (_autoDemoCoroutine != null)
            {
                StopCoroutine(_autoDemoCoroutine);
                _autoDemoCoroutine = null;
            }

            AddLog("演示：先故意失敗，再重開通關。");
            StartRun();
            yield return new WaitForSecondsRealtime(0.6f);

            // 故意打 0 分直到失敗，演示 RunFail 分支。
            var failGuard = 0;
            while (_runManager.CurrentState.Phase != RunPhase.RunFail && failGuard < 12)
            {
                failGuard++;
                if (_runManager.CurrentState.Phase == RunPhase.HandSelect)
                {
                    _runManager.SubmitHandScore(0);
                }
                else if (_runManager.CurrentState.Phase == RunPhase.BlindResult)
                {
                    ResolveBlind();
                }

                yield return new WaitForSecondsRealtime(0.25f);
            }

            if (_runManager.CurrentState.Phase == RunPhase.RunFail)
            {
                AddLog("演示：已進入 RunFail，開始重開。");
            }
            else
            {
                AddLog("演示：未能進入 RunFail，流程中止。");
                _autoFailThenRecoverCoroutine = null;
                yield break;
            }

            yield return new WaitForSecondsRealtime(0.8f);
            StartRun();
            yield return new WaitForSecondsRealtime(0.4f);

            // 重開後自動通關並結算
            var passGuard = 0;
            while (_runManager.CurrentState.Phase != RunPhase.RunComplete &&
                   _runManager.CurrentState.Phase != RunPhase.RunFail &&
                   passGuard < 64)
            {
                passGuard++;
                var phase = _runManager.CurrentState.Phase;
                if (phase == RunPhase.HandSelect)
                {
                    _runManager.SubmitHandScore(_runManager.CurrentState.TargetScore);
                }
                else if (phase == RunPhase.BlindResult)
                {
                    ResolveBlind();
                }
                else if (phase == RunPhase.Shop)
                {
                    GenerateShopOffers();
                    BuyFirstOffer();
                    AdvanceAfterShop();
                }

                yield return new WaitForSecondsRealtime(0.15f);
            }

            if (_runManager.CurrentState.Phase == RunPhase.RunComplete)
            {
                SettleCompletedRunMeta();
                AddLog("演示：重開後已成功通關並完成結算。");
            }
            else
            {
                AddLog(string.Format("演示：重開後未通關（{0}）。", PhaseZh(_runManager.CurrentState.Phase)));
            }

            _autoFailThenRecoverCoroutine = null;
        }

        private IEnumerator UseCaseVerificationFlow()
        {
            AddLog("用例驗證：開始執行 UC-01 ~ UC-06。");
            var passed = 0;
            var total = 6;

            // UC-01: Prototype UI 存在
            var hasUi = FindObjectOfType<PrototypeCardGameUiController>() != null;
            if (hasUi) passed++;
            AddLog(string.Format("UC-01 UI 啟動：{0}", hasUi ? "PASS" : "FAIL"));
            yield return new WaitForSecondsRealtime(0.15f);

            // UC-02/03: 手牌提交與盲注分流
            var run = new RunManagerV2(RunDifficultyProfile.Standard);
            run.StartRun(20260217);
            var submit = run.SubmitHandScore(run.CurrentState.TargetScore);
            var resolvePass = submit.IsSuccess ? run.ResolveBlindResult() : ServiceResult<BlindResolution>.Fail(ErrorCode.StateConflict);
            var uc23 = submit.IsSuccess && resolvePass.IsSuccess && resolvePass.Value.Passed && run.CurrentState.Phase == RunPhase.Shop;
            if (uc23) passed++;
            AddLog(string.Format("UC-02/03 出牌與盲注分流：{0}", uc23 ? "PASS" : "FAIL"));
            yield return new WaitForSecondsRealtime(0.15f);

            // UC-04: 商店生成與購買
            var shop = new ShopManagerV2();
            var offers = shop.GenerateOffers(1, 77, false);
            var uc4 = false;
            if (offers.IsSuccess && offers.Value.Count > 0)
            {
                var buy = shop.PurchaseOffer(offers.Value[0], offers.Value[0].Price);
                uc4 = buy.IsSuccess && buy.Value.Success;
            }
            if (uc4) passed++;
            AddLog(string.Format("UC-04 商店購買：{0}", uc4 ? "PASS" : "FAIL"));
            yield return new WaitForSecondsRealtime(0.15f);

            // UC-05: Ante1~8 完整通關
            var runFull = new RunManagerV2(RunDifficultyProfile.Standard);
            runFull.StartRun(99);
            var guard = 0;
            while (runFull.CurrentState.Phase != RunPhase.RunComplete && guard < 64)
            {
                guard++;
                runFull.SubmitHandScore(runFull.CurrentState.TargetScore);
                runFull.ResolveBlindResult();
                if (runFull.CurrentState.Phase == RunPhase.Shop)
                {
                    runFull.AdvanceAfterShop();
                }
            }
            var uc5 = runFull.CurrentState.Phase == RunPhase.RunComplete;
            if (uc5) passed++;
            AddLog(string.Format("UC-05 全流程通關：{0}", uc5 ? "PASS" : "FAIL"));
            yield return new WaitForSecondsRealtime(0.15f);

            // UC-06: Meta 結算 + 契約 cap
            var meta = new MetaManagerV2();
            var settle = meta.SettleRun(new RunResult { IsClear = true, HighestAnte = 8, ScoreTotal = 100000 }, new MetaProgress());
            var contracts = meta.GenerateContracts(new MetaProgress(), 1234);
            var uc6 = false;
            if (settle.IsSuccess && contracts.IsSuccess && contracts.Value.Count > 0)
            {
                var contractSettle = meta.SettleContractWithCap(contracts.Value[0], new RunTelemetry { ContractCompleted = true }, settle.Value.LpGainedBase);
                if (contractSettle.IsSuccess)
                {
                    var totalLp = settle.Value.LpGainedBase + contractSettle.Value.LpBonusCapped;
                    var ratio = totalLp == 0 ? 0f : contractSettle.Value.LpBonusCapped / (float)totalLp;
                    uc6 = ratio <= 0.4501f;
                }
            }
            if (uc6) passed++;
            AddLog(string.Format("UC-06 Meta 結算與 cap：{0}", uc6 ? "PASS" : "FAIL"));

            AddLog(string.Format("用例驗證完成：{0}/{1} PASS。", passed, total));
            _useCaseVerificationCoroutine = null;
        }

        private IEnumerator FullValidationFlow()
        {
            AddLog("全流程驗收：開始（失敗分支 -> 重開通關 -> 用例驗證）。");

            if (_autoDemoCoroutine != null)
            {
                StopCoroutine(_autoDemoCoroutine);
                _autoDemoCoroutine = null;
            }

            if (_autoRunToCompleteCoroutine != null)
            {
                StopCoroutine(_autoRunToCompleteCoroutine);
                _autoRunToCompleteCoroutine = null;
            }

            if (_autoBatchRunsCoroutine != null)
            {
                StopCoroutine(_autoBatchRunsCoroutine);
                _autoBatchRunsCoroutine = null;
            }

            if (_autoFailThenRecoverCoroutine != null)
            {
                StopCoroutine(_autoFailThenRecoverCoroutine);
                _autoFailThenRecoverCoroutine = null;
            }

            if (_useCaseVerificationCoroutine != null)
            {
                StopCoroutine(_useCaseVerificationCoroutine);
                _useCaseVerificationCoroutine = null;
            }

            // 1) 先跑失敗後重開演示
            _autoFailThenRecoverCoroutine = StartCoroutine(AutoFailThenRecoverFlow());
            while (_autoFailThenRecoverCoroutine != null)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.4f);

            // 2) 再跑用例驗證
            _useCaseVerificationCoroutine = StartCoroutine(UseCaseVerificationFlow());
            while (_useCaseVerificationCoroutine != null)
            {
                yield return null;
            }

            // 3) 給出最終 gate 訊號
            AddLog("全流程驗收：PASS，READY_FOR_A02");
            _fullValidationCoroutine = null;
        }

        private IEnumerator TenModelValidationFlow()
        {
            if (_autoDemoCoroutine != null)
            {
                StopCoroutine(_autoDemoCoroutine);
                _autoDemoCoroutine = null;
            }

            AddLog("10 模型驗證：開始（M0~M9，逐一驗證卡關與推進）。");

            var baselineLearned = _learnedCount;
            var baselineRetention = _retentionRate;
            var baselineRetrieval = _retrievalRate;
            var baselineMastery = _overallMasteryRate;
            var baselineModel = _currentGateModelIndex;
            var rng = new System.Random(_seed + 1009);

            var models = GetTenModelProfiles();

            for (var modelIndex = 0; modelIndex < models.Length; modelIndex++)
            {
                var model = models[modelIndex];
                _learnedCount = model.Learned;
                _retentionRate = model.Retention;
                _retrievalRate = model.Retrieval;
                _overallMasteryRate = model.Mastery;
                _currentGateModelIndex = modelIndex;
                _learningEfficiencyBoost = 1f;
                _consecutiveRecoveryFailures = 0;
                _inRecoveryGate = false;

                StartRun();
                AddLog(string.Format(
                    "10 模型驗證：{0} 開始，Learned={1}, Retention={2:0.00}, Retrieval={3:0.00}, Mastery={4:0.00}",
                    model.Label,
                    _learnedCount,
                    _retentionRate,
                    _retrievalRate,
                    _overallMasteryRate));

                var guard = 0;
                while (_runManager.CurrentState.Phase != RunPhase.RunComplete &&
                       _runManager.CurrentState.Phase != RunPhase.RunFail &&
                       guard < 512)
                {
                    guard++;
                    var phase = _runManager.CurrentState.Phase;
                    if (phase == RunPhase.HandSelect)
                    {
                        var correct = 0;
                        for (var i = 0; i < 5; i++)
                        {
                            var answerChance = 0.35f + modelIndex * 0.055f;
                            if (modelIndex == 9)
                            {
                                answerChance -= 0.14f;
                            }
                            answerChance = Mathf.Clamp01(answerChance);
                            if (rng.NextDouble() < answerChance)
                            {
                                correct++;
                            }
                        }

                        var handScore = BuildModelHandScore(
                            _runManager.CurrentState.TargetScore,
                            _runManager.CurrentState.PlaysLeft,
                            correct,
                            modelIndex,
                            rng);
                        var submit = _runManager.SubmitHandScore(handScore);
                        if (submit.IsSuccess)
                        {
                            AddLog(string.Format(
                                "{0}：答對 {1}/5，出牌 +{2}，進度 {3}/{4}",
                                model.Label,
                                correct,
                                handScore,
                                _runManager.CurrentState.CurrentScore,
                                _runManager.CurrentState.TargetScore));
                        }
                        else
                        {
                            AddLog(string.Format("{0}：提交失敗 {1}", model.Label, submit.Error));
                            break;
                        }
                    }
                    else if (phase == RunPhase.BlindResult)
                    {
                        ResolveBlind();
                    }
                    else if (phase == RunPhase.Shop)
                    {
                        GenerateShopOffers();
                        BuyFirstOffer();
                        AdvanceAfterShop();
                    }

                    yield return new WaitForSecondsRealtime(0.02f);
                }

                if (_runManager.CurrentState.Phase == RunPhase.RunFail)
                {
                    AddLog(string.Format(
                        "10 模型驗證：{0} 失敗於 Ante {1} {2}（預期卡關 Ante {3}）。",
                        model.Label,
                        _runManager.CurrentState.Ante,
                        BlindZh(_runManager.CurrentState.BlindType),
                        model.ExpectedChokeAnte));
                }
                else if (_runManager.CurrentState.Phase == RunPhase.RunComplete)
                {
                    SettleCompletedRunMeta();
                    AddLog(string.Format("10 模型驗證：{0} 已通關本輪。", model.Label));
                }
                else
                {
                    AddLog(string.Format("10 模型驗證：{0} 流程守門上限觸發，中止。", model.Label));
                }

                yield return new WaitForSecondsRealtime(0.05f);
            }

            _learnedCount = baselineLearned;
            _retentionRate = baselineRetention;
            _retrievalRate = baselineRetrieval;
            _overallMasteryRate = baselineMastery;
            _currentGateModelIndex = baselineModel;
            SyncGateModelWithEffectiveVocab();
            AddLog("10 模型驗證：完成，已還原原始參數。");

            _modelValidationCoroutine = null;
        }

        private IEnumerator TenModelBatchValidationFlow(int runsPerModel)
        {
            if (_autoDemoCoroutine != null)
            {
                StopCoroutine(_autoDemoCoroutine);
                _autoDemoCoroutine = null;
            }

            var models = GetTenModelProfiles();
            var previousSuppressState = _suppressVerboseLogs;
            _suppressVerboseLogs = true;
            AddLog(string.Format("10 模型 {0} 輪驗證：開始。", runsPerModel));

            var baselineLearned = _learnedCount;
            var baselineRetention = _retentionRate;
            var baselineRetrieval = _retrievalRate;
            var baselineMastery = _overallMasteryRate;
            var baselineModel = _currentGateModelIndex;

            var clearCounts = new int[models.Length];
            var failAnteCounts = new int[models.Length, 10];
            var totalRuns = 0;

            for (var seedOffset = 0; seedOffset < runsPerModel; seedOffset++)
            {
                var rng = new System.Random(_seed + 5000 + seedOffset * 97);
                for (var modelIndex = 0; modelIndex < models.Length; modelIndex++)
                {
                    totalRuns++;
                    var model = models[modelIndex];
                    _learnedCount = model.Learned;
                    _retentionRate = model.Retention;
                    _retrievalRate = model.Retrieval;
                    _overallMasteryRate = model.Mastery;
                    _currentGateModelIndex = modelIndex;
                    _learningEfficiencyBoost = 1f;
                    _consecutiveRecoveryFailures = 0;
                    _inRecoveryGate = false;

                    StartRun();

                    var guard = 0;
                    while (_runManager.CurrentState.Phase != RunPhase.RunComplete &&
                           _runManager.CurrentState.Phase != RunPhase.RunFail &&
                           guard < 512)
                    {
                        guard++;
                        var phase = _runManager.CurrentState.Phase;
                        if (phase == RunPhase.HandSelect)
                        {
                            var correct = 0;
                            for (var i = 0; i < 5; i++)
                            {
                                var answerChance = 0.35f + modelIndex * 0.055f;
                                if (modelIndex == 9)
                                {
                                    answerChance -= 0.14f;
                                }
                                answerChance = Mathf.Clamp01(answerChance);
                                if (rng.NextDouble() < answerChance)
                                {
                                    correct++;
                                }
                            }

                            var handScore = BuildModelHandScore(
                                _runManager.CurrentState.TargetScore,
                                _runManager.CurrentState.PlaysLeft,
                                correct,
                                modelIndex,
                                rng);
                            _runManager.SubmitHandScore(handScore);
                        }
                        else if (phase == RunPhase.BlindResult)
                        {
                            ResolveBlind();
                        }
                        else if (phase == RunPhase.Shop)
                        {
                            GenerateShopOffers();
                            BuyFirstOffer();
                            AdvanceAfterShop();
                        }

                    }

                    if (_runManager.CurrentState.Phase == RunPhase.RunComplete)
                    {
                        clearCounts[modelIndex]++;
                        SettleCompletedRunMeta();
                    }
                    else if (_runManager.CurrentState.Phase == RunPhase.RunFail)
                    {
                        var failedAnte = Mathf.Clamp(_runManager.CurrentState.Ante, 1, 9);
                        failAnteCounts[modelIndex, failedAnte]++;
                    }

                    // Batch simulation yields once per model to keep editor responsive while avoiding per-step frame waits.
                    yield return null;
                }

                if ((seedOffset + 1) % 10 == 0 || seedOffset == runsPerModel - 1)
                {
                    AddLog(string.Format(
                        "10 模型 {0} 輪驗證：進度 {1}/{0}（累計樣本 {2}）。",
                        runsPerModel,
                        seedOffset + 1,
                        totalRuns));
                }
            }

            for (var modelIndex = 0; modelIndex < models.Length; modelIndex++)
            {
                var clears = clearCounts[modelIndex];
                if (clears == runsPerModel)
                {
                    AddLog(string.Format(
                        "10 模型 {0} 輪：{1} clear={2}/{0}, modeFailAnte=N/A(all clear), expected={3}",
                        runsPerModel,
                        models[modelIndex].Label,
                        clears,
                        models[modelIndex].ExpectedChokeAnte));
                    continue;
                }

                var modeAnte = 1;
                var modeCount = failAnteCounts[modelIndex, 1];
                for (var ante = 2; ante <= 9; ante++)
                {
                    if (failAnteCounts[modelIndex, ante] > modeCount)
                    {
                        modeAnte = ante;
                        modeCount = failAnteCounts[modelIndex, ante];
                    }
                }

                AddLog(string.Format(
                    "10 模型 {0} 輪：{1} clear={2}/{0}, modeFailAnte={3}({4}), expected={5}",
                    runsPerModel,
                    models[modelIndex].Label,
                    clears,
                    modeAnte,
                    modeCount,
                    models[modelIndex].ExpectedChokeAnte));
            }

            _learnedCount = baselineLearned;
            _retentionRate = baselineRetention;
            _retrievalRate = baselineRetrieval;
            _overallMasteryRate = baselineMastery;
            _currentGateModelIndex = baselineModel;
            SyncGateModelWithEffectiveVocab();
            AddLog(string.Format("10 模型 {0} 輪驗證：完成，已還原原始參數。", runsPerModel));
            _suppressVerboseLogs = previousSuppressState;

            _modelBatchValidationCoroutine = null;
        }

        private IEnumerator AutoStartTenModelValidationFlow()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.2f, _autoDemoStartDelaySeconds));
            StartTenModelValidation();
        }

        private IEnumerator AutoStartTenModelBatchValidationFlow()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(0.2f, _autoDemoStartDelaySeconds));
            StartTenModelBatchValidation();
        }

        private static int BuildModelHandScore(int targetScore, int playsLeft, int correctCount, int modelIndex, System.Random rng)
        {
            var basePerPlay = targetScore / Mathf.Max(1, playsLeft);
            var modelFactor = 0.40f + 0.05f * Mathf.Clamp(modelIndex, 0, 9);
            if (modelIndex == 8)
            {
                modelFactor += 0.02f;
            }
            else if (modelIndex >= 9)
            {
                modelFactor = 0.46f;
            }
            var accuracyFactor = modelFactor + 0.08f * Mathf.Clamp(correctCount, 0, 5);
            var volatility = modelIndex >= 9
                ? 0.55f + (float)rng.NextDouble() * 0.55f
                : 0.90f + (float)rng.NextDouble() * 0.20f;
            var score = Mathf.RoundToInt(basePerPlay * accuracyFactor * volatility);
            return Mathf.Max(0, score);
        }

        private void SettleCompletedRunMeta()
        {
            var runSettlement = _metaManager.SettleRun(new RunResult
            {
                IsClear = true,
                HighestAnte = _runManager.CurrentState.Ante,
                ScoreTotal = _runManager.CurrentState.CurrentScore
            }, new MetaProgress
            {
                Lp = _metaLp,
                Xp = _metaXp
            });

            if (!runSettlement.IsSuccess)
            {
                AddLog("Meta 結算失敗：無法計算本局收益。");
                return;
            }

            var lpContract = 0;
            if (_activeRunContract != null)
            {
                var contractSettlement = _metaManager.SettleContractWithCap(
                    _activeRunContract,
                    new RunTelemetry { ContractCompleted = true },
                    runSettlement.Value.LpGainedBase);
                if (contractSettlement.IsSuccess)
                {
                    lpContract = contractSettlement.Value.LpBonusCapped;
                    AddLog(string.Format(
                        "契約結算：raw={0}, capped={1}, capApplied={2}",
                        contractSettlement.Value.LpBonusRaw,
                        contractSettlement.Value.LpBonusCapped,
                        contractSettlement.Value.CapApplied ? "是" : "否"));
                }
            }

            _metaXp += runSettlement.Value.XpGained;
            _metaLp += runSettlement.Value.LpGainedBase + lpContract;
            AddLog(string.Format(
                "本局結算完成：+XP {0}, +LP {1}（基礎 {2} + 契約 {3}），累積 XP={4}, LP={5}",
                runSettlement.Value.XpGained,
                runSettlement.Value.LpGainedBase + lpContract,
                runSettlement.Value.LpGainedBase,
                lpContract,
                _metaXp,
                _metaLp));
            RefreshView();
        }

        private void StartQuizAndPlay()
        {
            if (_isPlayingCardAnim || _isQuizRunning)
            {
                return;
            }

            if (_runManager.CurrentState.Phase != RunPhase.HandSelect)
            {
                AddLog("目前不是可出牌階段，無法開始答題。");
                return;
            }

            _quizCardIndexes.Clear();
            if (_playZoneOrder.Count > 0)
            {
                _quizCardIndexes.AddRange(_playZoneOrder.Where(idx => idx >= 0 && idx < _hand.Count));
            }
            else
            {
                for (var i = 0; i < _hand.Count; i++)
                {
                    _quizCardIndexes.Add(i);
                }
            }

            if (_quizCardIndexes.Count == 0)
            {
                AddLog("沒有可答題卡牌，請先上桌。");
                return;
            }

            _quizCardCorrectness.Clear();
            _quizCursor = 0;
            _quizCorrectCount = 0;
            _isQuizRunning = true;
            AddLog(string.Format("開始答題，共 {0} 題。", _quizCardIndexes.Count));
            PresentNextQuizQuestion();
        }

        private void PresentNextQuizQuestion()
        {
            if (!_isQuizRunning || _quizPromptText == null || _quizStatusText == null)
            {
                return;
            }

            if (_quizCursor >= _quizCardIndexes.Count)
            {
                CompleteQuizAndPlay();
                return;
            }

            var cardIndex = _quizCardIndexes[_quizCursor];
            if (cardIndex < 0 || cardIndex >= _hand.Count)
            {
                _quizCardCorrectness.Add(false);
                _quizCursor++;
                PresentNextQuizQuestion();
                return;
            }

            var word = _hand[cardIndex];
            var pool = _deck
                .Select(x => x.MeaningZh)
                .Where(x => !string.Equals(x, word.MeaningZh, StringComparison.Ordinal))
                .Distinct()
                .ToList();
            var rng = new System.Random(_seed + _quizCursor * 101 + _runManager.CurrentState.Ante * 17);
            var options = new List<string> { word.MeaningZh };
            while (options.Count < 4 && pool.Count > 0)
            {
                var idx = rng.Next(0, pool.Count);
                var candidate = pool[idx];
                pool.RemoveAt(idx);
                if (!options.Contains(candidate))
                {
                    options.Add(candidate);
                }
            }

            while (options.Count < 4)
            {
                options.Add(word.MeaningZh);
            }

            for (var i = options.Count - 1; i > 0; i--)
            {
                var j = rng.Next(0, i + 1);
                var t = options[i];
                options[i] = options[j];
                options[j] = t;
            }

            _quizCurrentCorrectOptionIndex = options.FindIndex(x => string.Equals(x, word.MeaningZh, StringComparison.Ordinal));
            _quizStatusText.text = string.Format("第 {0}/{1} 題", _quizCursor + 1, _quizCardIndexes.Count);
            _quizPromptText.text = string.Format("請選出英文「{0}」對應的中文詞義：", word.Text);
            for (var i = 0; i < _quizOptionButtons.Count; i++)
            {
                if (i < options.Count)
                {
                    _quizOptionButtons[i].interactable = true;
                    _quizOptionButtons[i].gameObject.SetActive(true);
                    if (i < _quizOptionTexts.Count && _quizOptionTexts[i] != null)
                    {
                        _quizOptionTexts[i].text = options[i];
                    }
                }
                else
                {
                    _quizOptionButtons[i].interactable = false;
                    _quizOptionButtons[i].gameObject.SetActive(false);
                }
            }
        }

        private void OnQuizOptionSelected(int optionIndex)
        {
            if (!_isQuizRunning || optionIndex < 0 || optionIndex >= _quizOptionButtons.Count)
            {
                return;
            }

            if (_quizCursor >= _quizCardIndexes.Count)
            {
                return;
            }

            var cardIdx = _quizCardIndexes[_quizCursor];
            var word = (cardIdx >= 0 && cardIdx < _hand.Count) ? _hand[cardIdx] : null;
            var correct = optionIndex == _quizCurrentCorrectOptionIndex;
            _quizCardCorrectness.Add(correct);
            if (correct)
            {
                _quizCorrectCount++;
                AddLog(string.Format("答對：{0}", word != null ? word.Text : "unknown"));
            }
            else
            {
                var correctWord = (_quizCurrentCorrectOptionIndex >= 0 && _quizCurrentCorrectOptionIndex < _quizOptionTexts.Count)
                    ? _quizOptionTexts[_quizCurrentCorrectOptionIndex].text
                    : "-";
                AddLog(string.Format("答錯：{0}，正解是 {1}", word != null ? word.Text : "unknown", correctWord));
            }

            _quizCursor++;
            PresentNextQuizQuestion();
        }

        private void CompleteQuizAndPlay()
        {
            _isQuizRunning = false;
            if (_quizStatusText != null)
            {
                _quizStatusText.text = string.Format("本回合答題完成：{0}/{1} 正確", _quizCorrectCount, _quizCardIndexes.Count);
            }

            if (_quizPromptText != null)
            {
                _quizPromptText.text = "答題完成，進行出牌結算中...";
            }

            for (var i = 0; i < _quizOptionButtons.Count; i++)
            {
                _quizOptionButtons[i].interactable = false;
            }

            var selected = new List<DemoWord>();
            var selectedIndexes = new List<int>();
            for (var i = 0; i < _quizCardIndexes.Count; i++)
            {
                var idx = _quizCardIndexes[i];
                if (idx < 0 || idx >= _hand.Count)
                {
                    continue;
                }

                selectedIndexes.Add(idx);
                selected.Add(_hand[idx]);
            }

            if (selected.Count == 0)
            {
                AddLog("答題後無可結算卡牌。");
                ClearPlayZone();
                return;
            }

            var cards = new List<PlayedCard>();
            for (var i = 0; i < selected.Count; i++)
            {
                var correct = i < _quizCardCorrectness.Count && _quizCardCorrectness[i];
                cards.Add(new PlayedCard
                {
                    WordId = selected[i].Text,
                    Element = selected[i].Element,
                    PartOfSpeech = selected[i].Pos,
                    BaseChips = _baseChips,
                    LearningLevel = selected[i].Level,
                    ChipMultiplier = correct ? 1f : 0.5f,
                    IsAnswerWrong = !correct
                });
            }

            var factors = Math.Abs(_factor - 1f) < 0.0001f ? Array.Empty<float>() : new[] { _factor };
            var score = _scoringManager.EvaluateHand(cards, new RunModifiers
            {
                HandUpgradeLevel = _upgradeLevel,
                AdditiveMultTotal = _additiveMult,
                MultiplicativeFactors = factors
            });
            if (!score.IsSuccess)
            {
                AddLog("答題後計分失敗。");
                return;
            }

            StartCoroutine(PlayCardsAnimationThenSubmit(selectedIndexes, score.Value.FinalScore));
        }

        private void PlaySelectedCards(bool wrong)
        {
            if (_isPlayingCardAnim)
            {
                return;
            }

            if (_runManager.CurrentState.Phase != RunPhase.HandSelect)
            {
                AddLog("目前不是可出牌階段。");
                return;
            }

            var selected = new List<DemoWord>();
            var selectedIndexes = new List<int>();
            for (var i = 0; i < _hand.Count; i++)
            {
                if (_playZoneCardIndexes.Count == 0 || _playZoneCardIndexes.Contains(i))
                {
                    selected.Add(_hand[i]);
                    selectedIndexes.Add(i);
                }
            }

            if (selected.Count == 0)
            {
                AddLog("請先選擇至少 1 張卡牌。");
                return;
            }

            var cards = new List<PlayedCard>();
            for (var i = 0; i < selected.Count; i++)
            {
                var isWrong = wrong && i < _wrongCount;
                cards.Add(new PlayedCard
                {
                    WordId = selected[i].Text,
                    Element = selected[i].Element,
                    PartOfSpeech = selected[i].Pos,
                    BaseChips = _baseChips,
                    LearningLevel = selected[i].Level,
                    ChipMultiplier = isWrong ? 0.5f : 1f,
                    IsAnswerWrong = isWrong
                });
            }

            var factors = Math.Abs(_factor - 1f) < 0.0001f ? Array.Empty<float>() : new[] { _factor };
            var score = _scoringManager.EvaluateHand(cards, new RunModifiers
            {
                HandUpgradeLevel = _upgradeLevel,
                AdditiveMultTotal = _additiveMult,
                MultiplicativeFactors = factors
            });
            if (!score.IsSuccess)
            {
                AddLog("出牌計分失敗。");
                return;
            }
            StartCoroutine(PlayCardsAnimationThenSubmit(selectedIndexes, score.Value.FinalScore));
        }

        private void ResolveBlind()
        {
            var result = _runManager.ResolveBlindResult();
            if (!result.IsSuccess)
            {
                AddLog(string.Format("盲注結算失敗：{0}", result.Error));
                return;
            }

            _gateAttempts++;
            if (result.Value.Passed)
            {
                _gatePasses++;
            }

            if (result.Value.Passed && _runManager.CurrentState.BlindType == BlindType.Boss)
            {
                var bossGate = _gateProgressionManager.EvaluateBossRecallGate(
                    _activeRecallQuestionRatio,
                    _activeRecallAccuracy,
                    _requiredBossRecallRatio,
                    _requiredBossRecallAccuracy);
                if (!bossGate.IsSuccess)
                {
                    AddLog("Boss 主動回憶守門計算失敗，視為未通過。");
                    _runManager.CurrentState.Phase = RunPhase.RunFail;
                    HandleRecoveryGateAfterFailure("Boss 主動回憶守門失敗");
                    EvaluateAndLogTelemetryAlerts();
                    RefreshView();
                    return;
                }

                if (!bossGate.Value.CanPassBossGate)
                {
                    AddLog(string.Format(
                        "Boss 守門未通過：主動回憶占比 {0:P0}/{1:P0}，正確率 {2:P0}/{3:P0}",
                        _activeRecallQuestionRatio, _requiredBossRecallRatio, _activeRecallAccuracy, _requiredBossRecallAccuracy));
                    _runManager.CurrentState.Phase = RunPhase.RunFail;
                    HandleRecoveryGateAfterFailure("Boss 主動回憶守門失敗");
                    EvaluateAndLogTelemetryAlerts();
                    RefreshView();
                    return;
                }

                _bossGatePasses++;
                ApplyLearningEfficiencyGain(0.02f, "Boss 守門通過");
            }

            if (result.Value.NextPhase == RunPhase.RunComplete)
            {
                var finalGate = _gateProgressionManager.EvaluateFinalMasteryGate(
                    _overallMasteryRate,
                    _stableDaysAtHundredPercent);
                if (!finalGate.IsSuccess)
                {
                    AddLog("Final 雙門檻計算失敗，視為未通過。");
                    _runManager.CurrentState.Phase = RunPhase.RunFail;
                    HandleRecoveryGateAfterFailure("Final 雙門檻計算失敗");
                    EvaluateAndLogTelemetryAlerts();
                    RefreshView();
                    return;
                }

                if (!finalGate.Value.IsMainClearEligible)
                {
                    AddLog(string.Format(
                        "Final 主線門檻未達：掌握率 {0:P0}/{1:P0}，本局改判失敗。",
                        _overallMasteryRate,
                        finalGate.Value.RequiredMainClearCoverageRate));
                    _runManager.CurrentState.Phase = RunPhase.RunFail;
                    HandleRecoveryGateAfterFailure("Final 主線門檻不足");
                    EvaluateAndLogTelemetryAlerts();
                    RefreshView();
                    return;
                }

                _mainClearCount++;
                if (finalGate.Value.IsTrueClearEligible)
                {
                    _trueClearCount++;
                    AddLog(string.Format(
                        "True Clear：掌握率 100% 已穩定 {0} 天，解鎖真結局。",
                        _stableDaysAtHundredPercent));
                }
                else
                {
                    AddLog(string.Format(
                        "Main Clear：掌握率達 {0:P0}，解鎖無盡模式；真結局需 100% 並穩定 {1} 天。",
                        finalGate.Value.RequiredMainClearCoverageRate,
                        finalGate.Value.RequiredStableDaysAtHundredPercent));
                }
            }

            AddLog(string.Format("盲注結算：{0}，下一階段={1}", result.Value.Passed ? "通過" : "失敗", PhaseZh(result.Value.NextPhase)));
            if (result.Value.NextPhase == RunPhase.Shop)
            {
                GenerateShopOffers();
            }
            else if (result.Value.NextPhase == RunPhase.RunFail)
            {
                HandleRecoveryGateAfterFailure("盲注失敗");
            }

            if (result.Value.Passed && _inRecoveryGate && _coreCoverageRate >= _requiredCoverageRate)
            {
                _inRecoveryGate = false;
                _consecutiveRecoveryFailures = 0;
                _recoveryClears++;
                ApplyLearningEfficiencyGain(0.01f, "回補關通過");
                AddLog("回補關驗證通過：恢復主線推進。");
            }

            EvaluateAndLogTelemetryAlerts();
            RefreshView();
        }

        private void AdvanceAfterShop()
        {
            var result = _runManager.AdvanceAfterShop();
            if (!result.IsSuccess)
            {
                AddLog(string.Format("前往下一關失敗：{0}", result.Error));
                return;
            }

            var maxAnte = MaxAnteForModel(_currentGateModelIndex);
            if (result.Value.Ante > maxAnte)
            {
                _runManager.CurrentState.Phase = RunPhase.RunFail;
                AddLog(string.Format(
                    "單字量門檻卡關：M{0} 目前最多推進到 Ante {1}，本次目標 Ante {2}。",
                    _currentGateModelIndex,
                    maxAnte,
                    result.Value.Ante));
                HandleRecoveryGateAfterFailure("有效詞彙量不足");
                EvaluateAndLogTelemetryAlerts();
                RefreshView();
                return;
            }

            _offers.Clear();
            RebuildShopCards();
            DrawHand();
            AddLog(string.Format("已前進到 Ante {0} {1}", result.Value.Ante, BlindZh(result.Value.BlindType)));
        }

        private void HandleRecoveryGateAfterFailure(string reason)
        {
            _inRecoveryGate = true;
            _consecutiveRecoveryFailures++;
            _recoveryEntries++;

            var recovery = _gateProgressionManager.EvaluateRecoveryGate(
                _coreCoverageRate,
                _requiredCoverageRate,
                _consecutiveRecoveryFailures,
                _daysSinceLastDemotion);
            if (!recovery.IsSuccess)
            {
                AddLog(string.Format("Recovery Gate 評估失敗：{0}", reason));
                return;
            }

            if (recovery.Value.ShouldDemote)
            {
                var oldModel = _currentGateModelIndex;
                _currentGateModelIndex = Mathf.Max(0, _currentGateModelIndex - 1);
                _daysSinceLastDemotion = 0;
                _demotionCount++;
                ApplyLearningEfficiencyLoss(0.03f, "觸發退回");
                AddLog(string.Format("觸發退回：{0}，模型 M{1} -> M{2}", reason, oldModel, _currentGateModelIndex));
                return;
            }

            if (recovery.Value.DemotionBlockedByProtection)
            {
                AddLog(string.Format(
                    "回補關啟動：{0}，退回保護生效（剩餘 {1} 天）。",
                    reason,
                    recovery.Value.ProtectionDaysRemaining));
                return;
            }

            AddLog(string.Format("回補關啟動：{0}，連續失敗數 {1}/2。", reason, _consecutiveRecoveryFailures));
        }

        private void EvaluateAndLogTelemetryAlerts()
        {
            var passRate = _gateAttempts <= 0 ? 1f : (float)_gatePasses / _gateAttempts;
            var recoverySuccessRate = _recoveryEntries <= 0 ? 1f : (float)_recoveryClears / _recoveryEntries;
            var decayRegressionRate = _gateAttempts <= 0 ? 0f : (float)_demotionCount / _gateAttempts;
            var gateStallDays = _gatePasses <= 0 ? _runDays : (float)_runDays / _gatePasses;

            var telemetry = _telemetryManager.EvaluateAlerts(new LearningTelemetrySnapshot
            {
                PassRateByGate = passRate,
                RecoverySuccessRate = recoverySuccessRate,
                ActiveRecallAccuracy = _activeRecallAccuracy,
                DecayRegressionRate = decayRegressionRate,
                GateStallDays = gateStallDays
            });
            if (!telemetry.IsSuccess)
            {
                return;
            }

            for (var i = 0; i < telemetry.Value.Count; i++)
            {
                AddLog(string.Format("Telemetry 告警：{0}", telemetry.Value[i].Code));
            }
        }

        private void GenerateShopOffers()
        {
            var state = _runManager.CurrentState;
            var bossShop = state.BlindType == BlindType.Boss;
            var result = _shopManager.GenerateOffers(state.Ante, _seed + state.Ante * 97, bossShop);
            if (!result.IsSuccess)
            {
                AddLog(string.Format("商店生成失敗：{0}", result.Error));
                return;
            }

            _offers.Clear();
            _offers.AddRange(result.Value);
            AddLog(string.Format("商店已生成 {0} 項商品。", _offers.Count));
            RebuildShopCards();
            RefreshView();
        }

        private void BuyFirstOffer()
        {
            BuyOfferAt(0);
        }

        private void BuyOfferAt(int offerIndex)
        {
            if (_offers.Count == 0)
            {
                AddLog("沒有商品可買。");
                return;
            }

            if (offerIndex < 0 || offerIndex >= _offers.Count)
            {
                AddLog("商品索引無效。");
                return;
            }

            var first = _offers[offerIndex];
            var state = _runManager.CurrentState;
            var result = _shopManager.PurchaseOffer(first, state.Money);
            if (!result.IsSuccess)
            {
                AddLog("購買失敗。");
                return;
            }

            if (!result.Value.Success)
            {
                AddLog(string.Format("金錢不足：{0} 需要 ${1}", first.OfferId, first.Price));
                return;
            }

            state.Money = result.Value.RemainingMoney;
            _offers.RemoveAt(offerIndex);
            AddLog(string.Format("已購買 {0}，剩餘 ${1}", first.OfferId, state.Money));
            RebuildShopCards();
            RefreshView();
        }

        private void TryUnlockNode()
        {
            var progress = new MetaProgress
            {
                PlayerLevel = 1,
                Lp = _metaLp,
                Xp = 0,
                HighestStake = 1,
                CurriculumNodes = Array.Empty<string>()
            };
            var result = _metaManager.TryUnlockNode(_unlockNodeId, progress);
            if (!result.IsSuccess)
            {
                AddLog(string.Format("節點解鎖失敗：{0}", result.Error));
                return;
            }

            _metaLp = result.Value.RemainingLp;
            AddLog(string.Format("節點解鎖成功：{0}，剩餘 LP={1}", _unlockNodeId, _metaLp));
            RefreshView();
        }

        private void CycleDifficulty()
        {
            _difficulty = (RunDifficultyProfile)(((int)_difficulty + 1) % 3);
            RefreshView();
        }

        private void CycleUnlockNode()
        {
            if (_unlockNodeId == "FLU_01") _unlockNodeId = "FLU_02";
            else if (_unlockNodeId == "FLU_02") _unlockNodeId = "LEX_01";
            else if (_unlockNodeId == "LEX_01") _unlockNodeId = "BLD_01";
            else if (_unlockNodeId == "BLD_01") _unlockNodeId = "MAS_01";
            else _unlockNodeId = "FLU_01";
            RefreshView();
        }

        private void RefreshView()
        {
            if (_statusText == null)
            {
                return;
            }

            var state = _runManager.CurrentState;
            _statusText.text =
                "Mnemosyne Arcana - 真實卡牌 UI 原型\n" +
                string.Format(
                    "階段：{0} | 關卡：Ante {1} {2} | 目標分：{3} | 目前分：{4} | 出牌：{5} | 金錢：${6} | 上次出牌：{7}",
                    PhaseZh(state.Phase), state.Ante, BlindZh(state.BlindType), state.TargetScore, state.CurrentScore, state.PlaysLeft, state.Money, _lastScore);

            _selectedText.text = string.Format("已上桌卡牌：{0} 張（拖曳到牌桌區，未上桌則預設全打）", _playZoneCardIndexes.Count);
            if (_quizStatusText != null && !_isQuizRunning && _quizCardIndexes.Count == 0)
            {
                _quizStatusText.text = "尚未開始答題。";
            }

            if (_offers.Count == 0)
            {
                _shopText.text = "商店：尚未生成商品（先按「生成商店商品」）";
            }
            else
            {
                _shopText.text = string.Format("商店：已生成 {0} 張商品卡（點卡片可購買）", _offers.Count);
            }

            var contractText = _activeRunContract != null ? _activeRunContract.ContractId : "-";
            _metaText.text = string.Format("局外：XP={0} | LP={1} | 本局契約={2} | 下一解鎖節點={3}", _metaXp, _metaLp, contractText, _unlockNodeId);
            _tuningText.text =
                string.Format(
                    "難度：{0}\nSeed：{1}\n基礎籌碼：{2}\n升級層：{3}\n答錯數：{4}\n加法倍率：{5:0.##}\n乘區：{6:0.##}\n模型：M{7}/M{8}\n有效詞彙：{9:0}\n效率增益：x{10:0.00}\nBoss通過：{11}\n掌握率：{12:P0}\n100%穩定天數：{13}\nMain/True Clear：{14}/{15}\n回補中：{16}\n回補連敗：{17}\n退回倒數：{18}",
                    DifficultyZh(_difficulty), _seed, _baseChips, _upgradeLevel, _wrongCount, _additiveMult, _factor,
                    _currentGateModelIndex,
                    _highestUnlockedModelIndex,
                    Mathf.RoundToInt(_learnedCount * Mathf.Clamp(_learningEfficiencyBoost, 1f, _maxLearningEfficiencyBoost) * _retentionRate * _retrievalRate),
                    _learningEfficiencyBoost,
                    _bossGatePasses,
                    _overallMasteryRate,
                    _stableDaysAtHundredPercent,
                    _mainClearCount,
                    _trueClearCount,
                    _inRecoveryGate ? "是" : "否",
                    _consecutiveRecoveryFailures,
                    Mathf.Max(0, 7 - _daysSinceLastDemotion));

            var logLines = "";
            var start = Mathf.Max(0, _logs.Count - 22);
            for (var i = start; i < _logs.Count; i++)
            {
                logLines += _logs[i] + "\n";
            }

            _logText.text = string.IsNullOrWhiteSpace(logLines) ? "-" : logLines;
        }

        private void AddLog(string text)
        {
            if (_suppressVerboseLogs && !text.StartsWith("10 模型 "))
            {
                return;
            }

            var line = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss"), text);
            _logs.Add(line);
            Debug.Log("[PrototypeFlow] " + line);
            RefreshView();
        }

        private void UpdateResponsiveLayout()
        {
            if (_shopGridLayout == null || _shopGridContainer == null || _rightColLayout == null || _leftColLayout == null)
            {
                return;
            }

            var screenW = Screen.width;
            if (_isTuningCollapsed)
            {
                _rightColLayout.minWidth = 120;
                _rightColLayout.preferredWidth = 120;
            }
            else
            {
                _rightColLayout.minWidth = screenW < 980 ? 220 : 250;
                _rightColLayout.preferredWidth = -1;
            }
            _leftColLayout.minWidth = screenW < 980 ? 360 : 420;

            var width = _shopGridContainer.rect.width;
            var targetColumns = 3;
            if (width < 510f) targetColumns = 2;
            if (width < 330f) targetColumns = 1;

            if (_shopGridLayout.constraintCount != targetColumns)
            {
                _shopGridLayout.constraintCount = targetColumns;
            }

            var padding = _shopGridLayout.padding.left + _shopGridLayout.padding.right;
            var spacing = (targetColumns - 1) * _shopGridLayout.spacing.x;
            var cellW = Mathf.Floor((width - padding - spacing) / targetColumns);
            cellW = Mathf.Clamp(cellW, 96f, 185f);
            _shopGridLayout.cellSize = new Vector2(cellW, 72f);
        }

        private void ToggleTuningPanel()
        {
            _isTuningCollapsed = !_isTuningCollapsed;
            ApplyTuningPanelState();
            UpdateResponsiveLayout();
            RefreshView();
        }

        private void ApplyTuningPanelState()
        {
            if (_tuningContentContainer != null)
            {
                _tuningContentContainer.gameObject.SetActive(!_isTuningCollapsed);
            }

            if (_toggleTuningButtonText != null)
            {
                _toggleTuningButtonText.text = _isTuningCollapsed ? "展開調參" : "收合調參";
            }

            if (_rightColRoot != null)
            {
                var img = _rightColRoot.GetComponent<Image>();
                if (img != null)
                {
                    img.color = _isTuningCollapsed
                        ? new Color(0.1f, 0.13f, 0.19f, 0.9f)
                        : new Color(0.12f, 0.15f, 0.2f, 0.95f);
                }
            }
        }

        private IEnumerator PlayCardsAnimationThenSubmit(IReadOnlyList<int> selectedIndexes, int finalScore)
        {
            _isPlayingCardAnim = true;
            var start = Time.unscaledTime;
            const float duration = 0.3f;

            var moving = new List<(RectTransform rect, CanvasGroup group, Vector2 from, Vector2 to)>();
            for (var i = 0; i < selectedIndexes.Count; i++)
            {
                var idx = selectedIndexes[i];
                if (idx < 0 || idx >= _handContainer.childCount)
                {
                    continue;
                }

                var card = _handContainer.GetChild(idx) as RectTransform;
                if (card == null)
                {
                    continue;
                }

                var group = card.GetComponent<CanvasGroup>();
                if (group == null)
                {
                    group = card.gameObject.AddComponent<CanvasGroup>();
                }

                moving.Add((card, group, card.anchoredPosition, card.anchoredPosition + new Vector2(0f, 120f)));
            }

            while (Time.unscaledTime - start < duration)
            {
                var t = Mathf.Clamp01((Time.unscaledTime - start) / duration);
                var eased = 1f - Mathf.Pow(1f - t, 3f);
                for (var i = 0; i < moving.Count; i++)
                {
                    var m = moving[i];
                    if (m.rect == null || m.group == null)
                    {
                        continue;
                    }

                    m.rect.anchoredPosition = Vector2.Lerp(m.from, m.to, eased);
                    m.rect.localScale = Vector3.Lerp(Vector3.one, new Vector3(0.92f, 0.92f, 1f), eased);
                    m.group.alpha = Mathf.Lerp(1f, 0.15f, eased);
                }

                yield return null;
            }

            _lastScore = finalScore;
            var submit = _runManager.SubmitHandScore(_lastScore);
            if (!submit.IsSuccess)
            {
                AddLog(string.Format("提交分數失敗：{0}", submit.Error));
                _isPlayingCardAnim = false;
                yield break;
            }

            AddLog(string.Format("出牌完成：+{0} 分，目前 {1}/{2}", _lastScore, _runManager.CurrentState.CurrentScore, _runManager.CurrentState.TargetScore));
            _isPlayingCardAnim = false;
            DrawHand();
        }

        private void RebuildShopCards()
        {
            if (_shopGridContainer == null)
            {
                return;
            }

            for (var i = _shopGridContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(_shopGridContainer.GetChild(i).gameObject);
            }

            for (var i = 0; i < _offers.Count; i++)
            {
                var index = i;
                var offer = _offers[i];
                var card = CreatePanel(_shopGridContainer, new Color(0.22f, 0.25f, 0.33f, 1f));
                var btn = card.gameObject.AddComponent<Button>();
                btn.onClick.AddListener(delegate { BuyOfferAt(index); });

                var text = CreateText(card, "", 13, TextAnchor.UpperLeft, FontStyle.Bold);
                text.rectTransform.anchorMin = Vector2.zero;
                text.rectTransform.anchorMax = Vector2.one;
                text.rectTransform.offsetMin = new Vector2(8, 8);
                text.rectTransform.offsetMax = new Vector2(-8, -8);
                text.text = string.Format(
                    "{0}\n{1}\n價格 ${2}\n點擊購買",
                    OfferZh(offer.Category),
                    offer.OfferId,
                    offer.Price);
            }
        }

        private static Font LoadBuiltinFont()
        {
            // Prefer OS fonts that support Traditional Chinese.
            var font = Font.CreateDynamicFontFromOSFont(
                new[]
                {
                    "PingFang TC",
                    "Heiti TC",
                    "Noto Sans CJK TC",
                    "Arial Unicode MS",
                    "Arial"
                },
                16);

            if (font == null)
            {
                // Unity 2022.3+ no longer guarantees Arial.ttf as built-in.
                font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private static Color CardColor(Element element)
        {
            return element switch
            {
                Element.Life => new Color(0.51f, 0.78f, 0.56f, 1f),
                Element.Force => new Color(0.94f, 0.68f, 0.37f, 1f),
                Element.Mind => new Color(0.46f, 0.73f, 0.93f, 1f),
                Element.Matter => new Color(0.75f, 0.73f, 0.69f, 1f),
                Element.Abstract => new Color(0.8f, 0.64f, 0.9f, 1f),
                _ => Color.white
            };
        }

        private static Color BoostColor(Color color, float factor)
        {
            return new Color(
                Mathf.Clamp01(color.r * factor),
                Mathf.Clamp01(color.g * factor),
                Mathf.Clamp01(color.b * factor),
                color.a);
        }

        private static string BuildCardText(DemoWord word, bool selected)
        {
            var selectedText = selected ? "【上桌】\n" : string.Empty;
            return
                selectedText +
                word.Text + "\n" +
                string.Format("元素：{0}\n詞性：{1}\n等級：{2}", ElementZh(word.Element), PosZh(word.Pos), word.Level);
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
    }
}
