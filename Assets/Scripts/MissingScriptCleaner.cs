using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Advanced missing script cleaner and reference fixer
/// Fixes the "The referenced script (Unknown) on this Behaviour is missing!" error
/// </summary>
public class MissingScriptCleaner : MonoBehaviour
{
    [Header("Cleanup Settings")]
    public bool removeOnStart = true;
    public bool logRemovals = true;
    public bool createReplacementComponents = true;
    
    [Header("Scene Management")]
    public bool cleanAllScenes = true;
    public string[] specificScenes = {"MainMenu", "GameplayScene"};
    
    void Start()
    {
        if (removeOnStart)
        {
            CleanupMissingScripts();
        }
    }
    
    [ContextMenu("Clean Missing Scripts Now")]
    public void CleanupMissingScripts()
    {
        Debug.Log("=== MISSING SCRIPT CLEANER: Starting cleanup ===");
        
        if (cleanAllScenes)
        {
            CleanCurrentScene();
        }
        else
        {
            foreach (string sceneName in specificScenes)
            {
                CleanSceneByName(sceneName);
            }
        }
        
        Debug.Log("=== MISSING SCRIPT CLEANER: Cleanup completed ===");
    }
    
    void CleanCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        CleanScene(currentScene);
    }
    
    void CleanSceneByName(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (scene.isLoaded)
        {
            CleanScene(scene);
        }
        else if (logRemovals)
        {
            Debug.LogWarning($"Scene '{sceneName}' is not loaded, skipping cleanup");
        }
    }
    
    void CleanScene(Scene scene)
    {
        if (logRemovals)
            Debug.Log($"Cleaning missing scripts in scene: {scene.name}");
        
        GameObject[] rootObjects = scene.GetRootGameObjects();
        int totalRemovals = 0;
        
        foreach (GameObject rootObject in rootObjects)
        {
            totalRemovals += CleanGameObjectAndChildren(rootObject);
        }
        
        if (logRemovals)
            Debug.Log($"Removed {totalRemovals} missing script references from scene '{scene.name}'");
    }
    
    int CleanGameObjectAndChildren(GameObject gameObject)
    {
        int removals = 0;
        
        // Clean this GameObject
        removals += CleanGameObject(gameObject);
        
        // Clean all children recursively
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            Transform child = gameObject.transform.GetChild(i);
            removals += CleanGameObjectAndChildren(child.gameObject);
        }
        
        return removals;
    }
    
    int CleanGameObject(GameObject gameObject)
    {
        int removals = 0;
        Component[] components = gameObject.GetComponents<Component>();
        
        for (int i = components.Length - 1; i >= 0; i--)
        {
            Component component = components[i];
            
            // Check if component is null (missing script)
            if (component == null)
            {
                if (logRemovals)
                    Debug.Log($"Removing missing script from GameObject: {gameObject.name}");
                
                // Attempt to remove the missing component
                RemoveMissingComponent(gameObject, i);
                removals++;
                
                // Create replacement if needed
                if (createReplacementComponents)
                {
                    CreateReplacementComponent(gameObject);
                }
            }
        }
        
        return removals;
    }
    
    void RemoveMissingComponent(GameObject gameObject, int componentIndex)
    {
        try
        {
            // Use SerializedObject to safely remove missing components
            var serializedObject = new UnityEditor.SerializedObject(gameObject);
            var componentsProperty = serializedObject.FindProperty("m_Component");
            
            if (componentIndex < componentsProperty.arraySize)
            {
                componentsProperty.DeleteArrayElementAtIndex(componentIndex);
                serializedObject.ApplyModifiedProperties();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not remove missing component from {gameObject.name}: {e.Message}");
            
            // Alternative method - destroy the GameObject and recreate it
            TryAlternativeCleanup(gameObject);
        }
    }
    
    void TryAlternativeCleanup(GameObject gameObject)
    {
        try
        {
            // Get all valid components
            List<Component> validComponents = new List<Component>();
            Component[] allComponents = gameObject.GetComponents<Component>();
            
            foreach (Component comp in allComponents)
            {
                if (comp != null)
                {
                    validComponents.Add(comp);
                }
            }
            
            // Store GameObject info
            string objectName = gameObject.name;
            Transform parent = gameObject.transform.parent;
            Vector3 position = gameObject.transform.position;
            Quaternion rotation = gameObject.transform.rotation;
            Vector3 scale = gameObject.transform.localScale;
            
            // Store children
            List<Transform> children = new List<Transform>();
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                children.Add(gameObject.transform.GetChild(i));
            }
            
            // Create new GameObject
            GameObject newObject = new GameObject(objectName);
            newObject.transform.SetParent(parent);
            newObject.transform.position = position;
            newObject.transform.rotation = rotation;
            newObject.transform.localScale = scale;
            
            // Copy valid components (this is complex and may not work for all component types)
            // For now, just add essential components
            CreateEssentialComponents(newObject);
            
            // Move children to new object
            foreach (Transform child in children)
            {
                child.SetParent(newObject.transform);
            }
            
            // Destroy old object
            if (Application.isPlaying)
            {
                Destroy(gameObject);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
            
            if (logRemovals)
                Debug.Log($"Recreated GameObject '{objectName}' to remove missing scripts");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Alternative cleanup failed for {gameObject.name}: {e.Message}");
        }
    }
    
    void CreateReplacementComponent(GameObject gameObject)
    {
        // Add a simple MonoBehaviour to replace missing scripts
        if (gameObject.GetComponent<MissingScriptReplacement>() == null)
        {
            gameObject.AddComponent<MissingScriptReplacement>();
            
            if (logRemovals)
                Debug.Log($"Added replacement component to {gameObject.name}");
        }
    }
    
    void CreateEssentialComponents(GameObject gameObject)
    {
        // Add commonly needed components based on GameObject name patterns
        string name = gameObject.name.ToLower();
        
        if (name.Contains("button"))
        {
            if (gameObject.GetComponent<UnityEngine.UI.Button>() == null)
                gameObject.AddComponent<UnityEngine.UI.Button>();
        }
        else if (name.Contains("text"))
        {
            if (gameObject.GetComponent<TMPro.TextMeshProUGUI>() == null)
                gameObject.AddComponent<TMPro.TextMeshProUGUI>();
        }
        else if (name.Contains("image"))
        {
            if (gameObject.GetComponent<UnityEngine.UI.Image>() == null)
                gameObject.AddComponent<UnityEngine.UI.Image>();
        }
        else if (name.Contains("panel"))
        {
            if (gameObject.GetComponent<UnityEngine.UI.Image>() == null)
                gameObject.AddComponent<UnityEngine.UI.Image>();
        }
        
        // Add the navigation fix for stage-related objects
        if (name.Contains("stage") || name.Contains("subject") || name.Contains("math") || 
            name.Contains("science") || name.Contains("english") || name.Contains("art"))
        {
            if (gameObject.GetComponent<FinalNavigationFix>() == null)
                gameObject.AddComponent<FinalNavigationFix>();
        }
    }
    
    [ContextMenu("Force Clean All Missing Scripts")]
    public void ForceCleanAllMissingScripts()
    {
        // More aggressive cleanup
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int totalRemovals = 0;
        
        foreach (GameObject obj in allObjects)
        {
            totalRemovals += CleanGameObject(obj);
        }
        
        Debug.Log($"Force cleanup removed {totalRemovals} missing script references");
    }
    
    [ContextMenu("Validate Scene")]
    public void ValidateScene()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int missingScriptCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    missingScriptCount++;
                    Debug.LogWarning($"Missing script found on: {obj.name}");
                }
            }
        }
        
        if (missingScriptCount == 0)
        {
            Debug.Log("Scene validation complete - no missing scripts found!");
        }
        else
        {
            Debug.LogWarning($"Scene validation found {missingScriptCount} missing script references");
        }
    }
}

/// <summary>
/// Simple replacement component for missing scripts
/// </summary>
public class MissingScriptReplacement : MonoBehaviour
{
    [Header("Replacement Info")]
    public string originalScriptName = "Unknown";
    public string replacementNote = "This component replaced a missing script reference";
    
    void Start()
    {
        Debug.Log($"MissingScriptReplacement active on {gameObject.name}");
    }
}