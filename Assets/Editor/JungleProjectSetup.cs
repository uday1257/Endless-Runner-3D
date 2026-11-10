#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// On first open: creates Assets/Scenes/JungleLevel.unity (with GameManager), adds it to Build Settings
// and registers the "Obstacle" and "Coin" tags from the lab PDF. Menu: Tools > Jungle Runner.
[InitializeOnLoad]
public static class JungleProjectSetup
{
    const string ScenePath = "Assets/Scenes/JungleLevel.unity";

    static JungleProjectSetup()
    {
        EditorApplication.delayCall += AutoSetup;
    }

    static void AutoSetup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling) return;
        if (File.Exists(ScenePath)) return;
        CreateScene();
    }

    [MenuItem("Tools/Jungle Runner/Create or Rebuild JungleLevel Scene")]
    public static void CreateScene()
    {
        AddTag("Obstacle");
        AddTag("Coin");
        Directory.CreateDirectory("Assets/Scenes");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        var go = new GameObject("GameManager");
        go.AddComponent<GameManager>();
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("JungleLevel scene created. Press Play to run the game.");
    }

    static void AddTag(string tag)
    {
        try
        {
            var tm = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var tags = tm.FindProperty("tags");
            for (int i = 0; i < tags.arraySize; i++)
                if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
            tags.InsertArrayElementAtIndex(tags.arraySize);
            tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
            tm.ApplyModifiedProperties();
        }
        catch (System.Exception) { /* tags are optional; the game uses components */ }
    }
}
#endif
