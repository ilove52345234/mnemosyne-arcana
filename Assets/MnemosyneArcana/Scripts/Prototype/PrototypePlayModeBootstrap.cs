using UnityEngine;

namespace MnemosyneArcana.Prototype
{
    public static class PrototypePlayModeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsurePrototypeUiInPlayMode()
        {
            if (!Application.isEditor)
            {
                return;
            }

            if (Object.FindObjectOfType<PrototypeCardGameUiController>() != null)
            {
                return;
            }

            var go = new GameObject("PrototypeCardGameUI");
            go.AddComponent<PrototypeCardGameUiController>();
        }
    }
}
