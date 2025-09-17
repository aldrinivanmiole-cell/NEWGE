using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Scene initializer that fixes missing scripts before they cause errors
/// This script uses Unity's earliest execution hooks
/// </summary>
public static class SceneInitializer
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void OnSubsystemRegistration()
    {
        Debug.Log("SceneInitializer: Subsystem registration - preparing for cleanup");
        
        // Register for scene loading events
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void OnAfterAssembliesLoaded()
    {
        Debug.Log("SceneInitializer: Assemblies loaded - checking for missing scripts");
        PerformEmergencyCleanup();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        Debug.Log("SceneInitializer: Before scene load - final cleanup");
        PerformEmergencyCleanup();
    }
    
    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SceneInitializer: Scene '{scene.name}' loaded - cleaning up");
        CleanSceneImmediately(scene);
    }
    
    static void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        Debug.Log($"SceneInitializer: Active scene changed to '{newScene.name}' - cleaning up");
        CleanSceneImmediately(newScene);
    }
    
    static void PerformEmergencyCleanup()
    {
        try
        {
            // Get all loaded scenes
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    CleanSceneImmediately(scene);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SceneInitializer: Emergency cleanup failed: {e.Message}");
        }
    }
    
    static void CleanSceneImmediately(Scene scene)
    {
        try
        {
            GameObject[] rootObjects = scene.GetRootGameObjects();
            int cleanedCount = 0;
            
            foreach (GameObject rootObj in rootObjects)
            {
                cleanedCount += CleanObjectAndChildren(rootObj);
            }
            
            if (cleanedCount > 0)
            {
                Debug.Log($"SceneInitializer: Cleaned {cleanedCount} objects in scene '{scene.name}'");
                
                // Ensure essential components exist after cleanup
                EnsureSceneEssentials(scene);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"SceneInitializer: Scene cleanup failed for '{scene.name}': {e.Message}");
        }
    }
    
    static int CleanObjectAndChildren(GameObject obj)
    {
        int cleanedCount = 0;
        
        // Clean this object
        if (CleanSingleObject(obj))
        {
            cleanedCount++;
        }
        
        // Clean children
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            cleanedCount += CleanObjectAndChildren(obj.transform.GetChild(i).gameObject);
        }
        
        return cleanedCount;
    }
    
    static bool CleanSingleObject(GameObject obj)
    {
        Component[] components = obj.GetComponents<Component>();
        bool foundMissing = false;
        
        // Check for missing components
        foreach (Component comp in components)
        {
            if (comp == null)
            {
                foundMissing = true;
                break;
            }
        }
        
        if (foundMissing)
        {
            AddQuickReplacement(obj);
            return true;
        }
        
        return false;
    }
    
    static void AddQuickReplacement(GameObject obj)
    {
        string objName = obj.name.ToLower();
        
        // Quick and dirty replacement based on name
        if (objName.Contains("math") || objName.Contains("science") || 
            objName.Contains("english") || objName.Contains("art"))
        {
            // Subject button
            if (obj.GetComponent<QuickSubjectFix>() == null)
            {
                QuickSubjectFix fix = obj.AddComponent<QuickSubjectFix>();
                fix.subjectName = ExtractSubjectName(objName);
            }
        }
        else if (objName.Contains("button"))
        {
            // Regular button
            if (obj.GetComponent<UnityEngine.UI.Button>() == null)
                obj.AddComponent<UnityEngine.UI.Button>();
        }
        else if (objName.Contains("panel"))
        {
            // Panel
            if (obj.GetComponent<UnityEngine.UI.Image>() == null)
                obj.AddComponent<UnityEngine.UI.Image>();
        }
        
        // Always add a marker to show this was cleaned
        if (obj.GetComponent<QuickCleanMarker>() == null)
            obj.AddComponent<QuickCleanMarker>();
    }
    
    static string ExtractSubjectName(string objName)
    {
        if (objName.Contains("math")) return "Math";
        if (objName.Contains("science")) return "Science";
        if (objName.Contains("english")) return "English";
        if (objName.Contains("art")) return "Art";
        return "Unknown";
    }
    
    static void EnsureSceneEssentials(Scene scene)
    {
        GameObject[] rootObjects = scene.GetRootGameObjects();
        bool hasNavFix = false;
        bool hasErrorFixer = false;
        
        // Check if essential components exist
        foreach (GameObject rootObj in rootObjects)
        {
            if (rootObj.GetComponentInChildren<FinalNavigationFix>() != null)
                hasNavFix = true;
            if (rootObj.GetComponentInChildren<ComprehensiveErrorFixer>() != null)
                hasErrorFixer = true;
        }
        
        // Add missing essential components
        if (!hasNavFix)
        {
            GameObject navObj = new GameObject("SceneInitializer_NavigationFix");
            navObj.AddComponent<FinalNavigationFix>();
            SceneManager.MoveGameObjectToScene(navObj, scene);
            Debug.Log($"SceneInitializer: Added navigation fix to scene '{scene.name}'");
        }
        
        if (!hasErrorFixer)
        {
            GameObject errorObj = new GameObject("SceneInitializer_ErrorFixer");
            errorObj.AddComponent<ComprehensiveErrorFixer>();
            SceneManager.MoveGameObjectToScene(errorObj, scene);
            Debug.Log($"SceneInitializer: Added error fixer to scene '{scene.name}'");
        }
    }
}

/// <summary>
/// Quick subject button fix for emergency situations
/// </summary>
public class QuickSubjectFix : MonoBehaviour
{
    public string subjectName = "";
    
    void Start()
    {
        var button = GetComponent<UnityEngine.UI.Button>();
        if (button == null)
            button = gameObject.AddComponent<UnityEngine.UI.Button>();
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnSubjectClick);
        
        Debug.Log($"QuickSubjectFix: Fixed {subjectName} button");
    }
    
    void OnSubjectClick()
    {
        Debug.Log($"QuickSubjectFix: {subjectName} clicked");
        
        // Find navigation fix and use it
        var navFix = FindFirstObjectByType<FinalNavigationFix>();
        if (navFix != null)
        {
            navFix.ShowStagePanel(subjectName);
        }
        else
        {
            Debug.LogWarning($"QuickSubjectFix: No navigation fix found for {subjectName}");
        }
    }
}

/// <summary>
/// Marker to show an object was quickly cleaned
/// </summary>
public class QuickCleanMarker : MonoBehaviour
{
    public string cleanedAt;
    
    void Awake()
    {
        cleanedAt = System.DateTime.Now.ToString();
    }
}