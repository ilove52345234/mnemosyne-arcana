#if UNITY_EDITOR
using UnityEditor;

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
            PrototypePlayModeBootstrap.EnsurePrototypeUiForCurrentScene();
        }
    }
}
#endif
