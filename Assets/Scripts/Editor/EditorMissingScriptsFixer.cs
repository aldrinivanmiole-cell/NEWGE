using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

/// <summary>
/// Editor-only script to fix missing script references in the project
/// Use this in the Unity Editor to clean up missing scripts
/// </summary>
public class EditorMissingScriptsFixer : EditorWindow
{
    [MenuItem("Tools/Fix Missing Scripts")]
    public static void ShowWindow()
    {
        GetWindow<EditorMissingScriptsFixer>("Missing Scripts Fixer");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Missing Scripts Fixer", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Remove Missing Scripts from Current Scene"))
        {
            RemoveMissingScriptsFromCurrentScene();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Remove Missing Scripts from All Scenes"))
        {
            RemoveMissingScriptsFromAllScenes();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Fix UI References in Current Scene"))
        {
            FixUIReferencesInCurrentScene();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Enable Offline Mode"))
        {
            PlayerPrefs.SetInt("OfflineMode", 1);
            PlayerPrefs.Save();
            Debug.Log("Offline mode enabled to prevent network errors");
        }
        
        if (GUILayout.Button("Disable Offline Mode"))
        {
            PlayerPrefs.SetInt("OfflineMode", 0);
            PlayerPrefs.Save();
            Debug.Log("Offline mode disabled");
        }
    }
    
    static void RemoveMissingScriptsFromCurrentScene()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            SerializedObject serializedObject = new SerializedObject(obj);
            SerializedProperty components = serializedObject.FindProperty("m_Component");
            
            for (int i = components.arraySize - 1; i >= 0; i--)
            {
                SerializedProperty component = components.GetArrayElementAtIndex(i);
                if (component.FindPropertyRelative("component").objectReferenceValue == null)
                {
                    components.DeleteArrayElementAtIndex(i);
                    removedCount++;
                    Debug.Log($"Removed missing script from: {obj.name}");
                }
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        Debug.Log($"Removed {removedCount} missing script references from current scene");
        
        if (removedCount > 0)
        {
            EditorUtility.SetDirty(SceneManager.GetActiveScene().GetRootGameObjects()[0]);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }
    }
    
    static void RemoveMissingScriptsFromAllScenes()
    {
        string currentScene = SceneManager.GetActiveScene().path;
        
        string[] scenes = {
            "Assets/Scenes/register.unity",
            "Assets/Scenes/login.unity",
            "Assets/Scenes/titlescreen.unity",
            "Assets/Scenes/loadingscreen.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/GameplayScene.unity",
            "Assets/Scenes/gameresult.unity",
            "Assets/Scenes/classlist.unity",
            "Assets/Scenes/GPenumeration.unity",
            "Assets/Scenes/GPfillblank.unity",
            "Assets/Scenes/GPyesno.unity",
            "Assets/Scenes/gender.unity",
            "Assets/Scenes/loadingscreenF.unity"
        };
        
        int totalRemoved = 0;
        
        foreach (string scenePath in scenes)
        {
            if (System.IO.File.Exists(scenePath))
            {
                UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                
                GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
                int removedCount = 0;
                
                foreach (GameObject obj in allObjects)
                {
                    if (obj == null) continue;
                    
                    SerializedObject serializedObject = new SerializedObject(obj);
                    SerializedProperty components = serializedObject.FindProperty("m_Component");
                    
                    for (int i = components.arraySize - 1; i >= 0; i--)
                    {
                        SerializedProperty component = components.GetArrayElementAtIndex(i);
                        if (component.FindPropertyRelative("component").objectReferenceValue == null)
                        {
                            components.DeleteArrayElementAtIndex(i);
                            removedCount++;
                        }
                    }
                    
                    serializedObject.ApplyModifiedProperties();
                }
                
                if (removedCount > 0)
                {
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                    Debug.Log($"Removed {removedCount} missing scripts from {scenePath}");
                }
                
                totalRemoved += removedCount;
            }
        }
        
        // Return to original scene
        if (!string.IsNullOrEmpty(currentScene))
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(currentScene);
        }
        
        Debug.Log($"Total missing scripts removed: {totalRemoved}");
    }
    
    static void FixUIReferencesInCurrentScene()
    {
        // Fix ProfileLoader references
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (var loader in profileLoaders)
        {
            if (FixProfileLoaderReferences(loader))
            {
                fixedCount++;
                EditorUtility.SetDirty(loader);
            }
        }
        
        Debug.Log($"Fixed UI references for {fixedCount} ProfileLoader components");
    }
    
    static bool FixProfileLoaderReferences(ProfileLoader loader)
    {
        if (loader == null) return false;
        
        bool wasFixed = false;
        
        // Try to auto-assign classNameText if it's null
        if (loader.classNameText == null)
        {
            UnityEngine.UI.Text[] textComponents = loader.GetComponentsInChildren<UnityEngine.UI.Text>(true);
            foreach (var text in textComponents)
            {
                if (text.name.ToLower().Contains("class"))
                {
                    loader.classNameText = text.GetComponent<TMPro.TMP_Text>();
                    if (loader.classNameText != null)
                    {
                        Debug.Log($"Auto-assigned classNameText to: {text.name}");
                        wasFixed = true;
                        break;
                    }
                }
            }
            
            // Also try TMP_Text components
            if (loader.classNameText == null)
            {
                TMPro.TMP_Text[] tmpTextComponents = loader.GetComponentsInChildren<TMPro.TMP_Text>(true);
                foreach (var text in tmpTextComponents)
                {
                    if (text.name.ToLower().Contains("class"))
                    {
                        loader.classNameText = text;
                        Debug.Log($"Auto-assigned classNameText to TMP: {text.name}");
                        wasFixed = true;
                        break;
                    }
                }
            }
        }
        
        return wasFixed;
    }
}