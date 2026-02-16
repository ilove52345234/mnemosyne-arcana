#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace MnemosyneArcana.Prototype.Editor
{
    [InitializeOnLoad]
    public static class PrototypeEditorPlayBootstrap
    {
        static PrototypeEditorPlayBootstrap()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
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
#endif
