#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>One-click setup for the three currently configured gameplay scenes.</summary>
public static class KDPolishInstaller
{
    [MenuItem("Tools/KD2510/Install Pause Menu In Gameplay Scenes")]
    public static void Install()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        var setup = EditorBuildSettings.scenes;
        int count = 0;
        foreach (var scene in setup)
        {
            if (!scene.enabled || scene.path.Contains("Main Menu")) continue;
            var opened = EditorSceneManager.OpenScene(scene.path, OpenSceneMode.Single);
            if (Object.FindFirstObjectByType<KDPolishMenu>() == null)
            {
                var root = new GameObject("KD2510 Pause Menu");
                root.AddComponent<KDPolishMenu>();
                EditorSceneManager.MarkSceneDirty(opened);
                EditorSceneManager.SaveScene(opened);
                count++;
            }
        }
        Debug.Log($"KD2510 polish: installed pause menu in {count} gameplay scenes.");
        EditorUtility.DisplayDialog("KD2510 Polish", $"Installed in {count} gameplay scenes. Press ESC during play to pause.", "OK");
    }
}
#endif
