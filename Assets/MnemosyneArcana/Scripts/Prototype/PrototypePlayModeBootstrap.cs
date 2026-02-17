using UnityEngine;

namespace MnemosyneArcana.Prototype
{
    public static class PrototypePlayModeBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsurePrototypeUiInPlayMode()
        {
            EnsurePrototypeUiForCurrentScene();
        }

        public static bool EnsurePrototypeUiForCurrentScene()
        {
            if (!Application.isEditor)
            {
                return false;
            }

            if (Object.FindObjectOfType<PrototypeCardGameUiController>() != null)
            {
                return false;
            }

            var go = new GameObject("PrototypeCardGameUI");
            go.AddComponent<PrototypeCardGameUiController>();
            return true;
        }
    }
}
