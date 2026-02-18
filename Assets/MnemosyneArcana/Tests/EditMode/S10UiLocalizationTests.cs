using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Runtime;
using MnemosyneArcana.Prototype;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace MnemosyneArcana.Tests.EditMode
{
    [TestFixture]
    public class S10UiLocalizationTests
    {
        [TearDown]
        public void TearDown()
        {
            var ui = Object.FindObjectOfType<PrototypeCardGameUiController>();
            if (ui != null)
            {
                Object.DestroyImmediate(ui.gameObject);
            }
        }

        [Test]
        public void S10_M1_CoreMenu_UsesTraditionalChineseLabels()
        {
            var ui = CreateAndInitializeUi();

            var texts = ui.GetComponentsInChildren<Text>(true).Select(x => x.text).ToArray();

            Assert.IsTrue(texts.Contains("抽新手牌"));
            Assert.IsTrue(texts.Contains("開始答題並出牌"));
            Assert.IsTrue(texts.Contains("結算盲注"));
            Assert.IsTrue(texts.Contains("前往下一關"));
            Assert.IsTrue(texts.Contains("重開本局"));
            Assert.IsTrue(texts.Contains("生成商店商品"));
            Assert.IsTrue(texts.Contains("購買第一項"));

            var forbiddenLegacyLabels = new[]
            {
                "Start New Run",
                "Generate Shop Offers",
                "Auto Buy First",
                "Run Control",
                "Event Log",
                "Clear Log"
            };

            foreach (var label in forbiddenLegacyLabels)
            {
                Assert.IsFalse(texts.Contains(label), $"Unexpected English menu label: {label}");
            }
        }

        [Test]
        public void S10_M2_StatusAndTuning_UseTraditionalChineseTerms()
        {
            var ui = CreateAndInitializeUi();

            var statusText = GetPrivateText(ui, "_statusText");
            var tuningText = GetPrivateText(ui, "_tuningText");
            var metaText = GetPrivateText(ui, "_metaText");

            Assert.IsTrue(statusText.Contains("關卡：第"));
            Assert.IsFalse(statusText.Contains("Ante"));
            Assert.IsTrue(tuningText.Contains("種子："));
            Assert.IsFalse(tuningText.Contains("Seed："));
            Assert.IsTrue(tuningText.Contains("魔王通過："));
            Assert.IsFalse(tuningText.Contains("Boss通過："));
            Assert.IsTrue(tuningText.Contains("主線/真結局通關："));
            Assert.IsFalse(tuningText.Contains("Main/True Clear："));
            Assert.IsTrue(metaText.Contains("經驗="));
            Assert.IsTrue(metaText.Contains("學習點="));
        }

        [Test]
        public void S10_M3_LearningArea_AllowsEnglishWordStem()
        {
            var ui = CreateAndInitializeUi();

            var handContainer = GetPrivateRectTransform(ui, "_handContainer");
            Assert.IsNotNull(handContainer);

            var cardTexts = handContainer.GetComponentsInChildren<Text>(true).Select(x => x.text).ToArray();
            var hasEnglishStem = cardTexts.Any(x => Regex.IsMatch(x, "[A-Za-z]{3,}"));
            Assert.IsTrue(hasEnglishStem, "Learning area should keep English stem for vocabulary cards.");
        }

        [Test]
        public void S10_M4_SharedUiTerms_AreTraditionalChinese()
        {
            Assert.AreEqual("標準", PrototypeUiText.DifficultyZh(RunDifficultyProfile.Standard));
            Assert.AreEqual("魔王盲注", PrototypeUiText.BlindZh(BlindType.Boss));
            Assert.AreEqual("魔王結算", PrototypeUiText.PhaseZh(RunPhase.BossResolve));
            Assert.AreEqual("課程", PrototypeUiText.OfferZh(ShopOfferCategory.Course));

            Assert.IsFalse(PrototypeUiText.BlindZh(BlindType.Boss).Contains("Boss"));
            Assert.IsFalse(PrototypeUiText.PhaseZh(RunPhase.BossResolve).Contains("Boss"));
        }

        [Test]
        public void S10_M5_NewUiDisablesLegacyPrototypeControllers()
        {
            var legacyGameObject = new GameObject("LegacyPrototypeUi");
            var legacy = legacyGameObject.AddComponent<PrototypeGameScreenController>();
            Assert.IsTrue(legacy.enabled);

            var ui = CreateAndInitializeUi();
            Assert.IsNotNull(ui);
            Assert.IsFalse(legacy.enabled);

            Object.DestroyImmediate(legacyGameObject);
        }

        [Test]
        public void S10_M6_PlayerMode_HidesTuningAndDevButtons()
        {
            var ui = CreateAndInitializeUi();
            var texts = ui.GetComponentsInChildren<Text>(true).Select(x => x.text).ToArray();

            var forbiddenPlayerModeLabels = new[]
            {
                "展開調參",
                "收合調參",
                "難度切換",
                "籌碼 +1",
                "籌碼 -1",
                "一鍵跑到通關",
                "連跑3局",
                "失敗後重開演示",
                "驗證全部用例",
                "全流程最終驗收",
                "10模型驗證",
                "10模型30輪"
            };

            foreach (var label in forbiddenPlayerModeLabels)
            {
                Assert.IsFalse(texts.Contains(label), $"Player mode should hide dev/tuning label: {label}");
            }
        }

        private static string GetPrivateText(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field: {fieldName}");
            var text = field.GetValue(instance) as Text;
            Assert.IsNotNull(text, $"Field is not Text: {fieldName}");
            return text.text;
        }

        private static RectTransform GetPrivateRectTransform(object instance, string fieldName)
        {
            var field = instance.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, $"Missing field: {fieldName}");
            return field.GetValue(instance) as RectTransform;
        }

        private static PrototypeCardGameUiController CreateAndInitializeUi()
        {
            PrototypePlayModeBootstrap.EnsurePrototypeUiForCurrentScene();
            var ui = Object.FindObjectOfType<PrototypeCardGameUiController>();
            Assert.IsNotNull(ui);

            var awake = typeof(PrototypeCardGameUiController).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(awake, "PrototypeCardGameUiController.Awake not found");
            awake.Invoke(ui, null);
            return ui;
        }
    }
}
