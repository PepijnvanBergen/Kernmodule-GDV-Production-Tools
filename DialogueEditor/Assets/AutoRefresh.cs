using UnityEditor;

[InitializeOnLoad]
public static class AutoRefreshFix
{
    static AutoRefreshFix()
    {
        EditorApplication.playModeStateChanged += PlayRefresh;
    }

    private static void PlayRefresh(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            AssetDatabase.Refresh();
        }
    }
}