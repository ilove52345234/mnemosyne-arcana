using MnemosyneArcana.Core.Contracts;
using MnemosyneArcana.Core.Runtime;

namespace MnemosyneArcana.Prototype
{
    public static class PrototypeUiText
    {
        public static string DifficultyZh(RunDifficultyProfile profile)
        {
            return profile switch
            {
                RunDifficultyProfile.Relaxed => "輕鬆",
                RunDifficultyProfile.Standard => "標準",
                RunDifficultyProfile.Challenging => "挑戰",
                _ => profile.ToString()
            };
        }

        public static string BlindZh(BlindType blind)
        {
            return blind switch
            {
                BlindType.Small => "小盲注",
                BlindType.Big => "大盲注",
                BlindType.Boss => "魔王盲注",
                _ => blind.ToString()
            };
        }

        public static string PhaseZh(RunPhase phase)
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
                RunPhase.BossResolve => "魔王結算",
                RunPhase.RunComplete => "通關",
                RunPhase.RunFail => "失敗",
                _ => phase.ToString()
            };
        }

        public static string OfferZh(ShopOfferCategory category)
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
