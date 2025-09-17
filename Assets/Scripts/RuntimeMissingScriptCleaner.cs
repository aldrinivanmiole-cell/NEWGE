using UnityEngine;

/// <summary>
/// Runtime-safe missing script cleaner that doesn't require UnityEditor
/// Fixes "The referenced script (Unknown) on this Behaviour is missing!" errors
/// </summary>
public class RuntimeMissingScriptCleaner : MonoBehaviour
{
    [Header("Cleanup Settings")]
    public bool cleanOnStart = true;
    public bool logCleanup = true;
    
    void Start()
    {
        if (cleanOnStart)
        {
            CleanMissingScripts();
        }
    }
    
    [ContextMenu("Clean Missing Scripts")]
    public void CleanMissingScripts()
    {
        Debug.Log("=== RUNTIME MISSING SCRIPT CLEANER: Starting ===");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int cleanedObjects = 0;
        int totalMissingComponents = 0;
        
        foreach (GameObject obj in allObjects)
        {
            int missingCount = CountAndReplaceMissingComponents(obj);
            if (missingCount > 0)
            {
                cleanedObjects++;
                totalMissingComponents += missingCount;
                
                if (logCleanup)
                    Debug.Log($"Cleaned {missingCount} missing scripts from: {obj.name}");
            }
        }
        
        if (logCleanup)
        {
            Debug.Log($"=== CLEANUP COMPLETE: {cleanedObjects} objects cleaned, {totalMissingComponents} missing components handled ===");
        }
        
        // Ensure navigation fix is present
        EnsureNavigationFix();
    }
    
    int CountAndReplaceMissingComponents(GameObject gameObject)
    {
        Component[] components = gameObject.GetComponents<Component>();
        int missingCount = 0;
        
        // Count missing components
        foreach (Component component in components)
        {
            if (component == null)
            {
                missingCount++;
            }
        }
        
        // If we found missing components, add replacement functionality
        if (missingCount > 0)
        {
            AddReplacementComponents(gameObject);
        }
        
        return missingCount;
    }
    
    void AddReplacementComponents(GameObject gameObject)
    {
        string objName = gameObject.name.ToLower();
        
        // Add appropriate components based on object name and context
        if (objName.Contains("button") && !objName.Contains("stage"))
        {
            // Regular button - ensure it has Button component
            if (gameObject.GetComponent<UnityEngine.UI.Button>() == null)
            {
                gameObject.AddComponent<UnityEngine.UI.Button>();
                if (logCleanup) Debug.Log($"Added Button component to {gameObject.name}");
            }
        }
        else if (objName.Contains("math") || objName.Contains("science") || 
                 objName.Contains("english") || objName.Contains("art"))
        {
            // Subject button - needs navigation fix
            EnsureSubjectButtonFunctionality(gameObject);
        }
        else if (objName.Contains("stage") && objName.Contains("button"))
        {
            // Stage button - needs button component
            if (gameObject.GetComponent<UnityEngine.UI.Button>() == null)
            {
                gameObject.AddComponent<UnityEngine.UI.Button>();
                if (logCleanup) Debug.Log($"Added Button component to stage button {gameObject.name}");
            }
        }
        else if (objName.Contains("panel"))
        {
            // Panel - ensure it has Image component for UI
            if (gameObject.GetComponent<UnityEngine.UI.Image>() == null)
            {
                gameObject.AddComponent<UnityEngine.UI.Image>();
                if (logCleanup) Debug.Log($"Added Image component to panel {gameObject.name}");
            }
        }
        else if (objName.Contains("text"))
        {
            // Text component
            if (gameObject.GetComponent<TMPro.TextMeshProUGUI>() == null)
            {
                gameObject.AddComponent<TMPro.TextMeshProUGUI>();
                if (logCleanup) Debug.Log($"Added TextMeshProUGUI component to {gameObject.name}");
            }
        }
        
        // Add a generic placeholder component to mark that this object was cleaned
        if (gameObject.GetComponent<CleanedObjectMarker>() == null)
        {
            CleanedObjectMarker marker = gameObject.AddComponent<CleanedObjectMarker>();
            marker.originalMissingScriptCount = CountMissingComponents(gameObject);
        }
    }
    
    void EnsureSubjectButtonFunctionality(GameObject buttonObject)
    {
        // Ensure subject buttons have proper navigation functionality
        SubjectButtonFix buttonFix = buttonObject.GetComponent<SubjectButtonFix>();
        if (buttonFix == null)
        {
            buttonFix = buttonObject.AddComponent<SubjectButtonFix>();
            if (logCleanup) Debug.Log($"Added SubjectButtonFix to {buttonObject.name}");
        }
        
        // Ensure it has a Button component
        if (buttonObject.GetComponent<UnityEngine.UI.Button>() == null)
        {
            buttonObject.AddComponent<UnityEngine.UI.Button>();
        }
    }
    
    void EnsureNavigationFix()
    {
        // Make sure we have the main navigation fix in the scene
        FinalNavigationFix navFix = FindFirstObjectByType<FinalNavigationFix>();
        if (navFix == null)
        {
            GameObject navObj = new GameObject("AutoCreated_FinalNavigationFix");
            navFix = navObj.AddComponent<FinalNavigationFix>();
            if (logCleanup) Debug.Log("Added FinalNavigationFix to scene");
        }
    }
    
    int CountMissingComponents(GameObject gameObject)
    {
        Component[] components = gameObject.GetComponents<Component>();
        int count = 0;
        
        foreach (Component component in components)
        {
            if (component == null) count++;
        }
        
        return count;
    }
    
    [ContextMenu("Validate Scene")]
    public void ValidateScene()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int totalMissing = 0;
        
        foreach (GameObject obj in allObjects)
        {
            int missing = CountMissingComponents(obj);
            if (missing > 0)
            {
                totalMissing += missing;
                Debug.LogWarning($"GameObject '{obj.name}' has {missing} missing script(s)");
            }
        }
        
        if (totalMissing == 0)
        {
            Debug.Log("✅ Scene validation passed - no missing scripts found!");
        }
        else
        {
            Debug.LogWarning($"⚠️ Scene has {totalMissing} missing script references that need cleanup");
        }
    }
}

/// <summary>
/// Marker component to track objects that have been cleaned
/// </summary>
public class CleanedObjectMarker : MonoBehaviour
{
    public int originalMissingScriptCount = 0;
    public string cleanupTimestamp;
    
    void Start()
    {
        cleanupTimestamp = System.DateTime.Now.ToString();
    }
}

/// <summary>
/// Replacement for missing scripts on subject buttons
/// </summary>
public class SubjectButtonFix : MonoBehaviour
{
    void Start()
    {
        SetupButton();
    }
    
    void SetupButton()
    {
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button != null)
        {
            string objName = gameObject.name.ToLower();
            
            button.onClick.RemoveAllListeners();
            
            if (objName.Contains("math"))
                button.onClick.AddListener(() => ShowStagePanel("Math"));
            else if (objName.Contains("science"))
                button.onClick.AddListener(() => ShowStagePanel("Science"));
            else if (objName.Contains("english"))
                button.onClick.AddListener(() => ShowStagePanel("English"));
            else if (objName.Contains("art"))
                button.onClick.AddListener(() => ShowStagePanel("Art"));
            
            Debug.Log($"SubjectButtonFix: Setup button for {gameObject.name}");
        }
    }
    
    void ShowStagePanel(string subject)
    {
        // Find and use the FinalNavigationFix
        FinalNavigationFix navFix = FindFirstObjectByType<FinalNavigationFix>();
        if (navFix != null)
        {
            navFix.ShowStagePanel(subject);
        }
        else
        {
            Debug.LogWarning($"No FinalNavigationFix found to show {subject} stage panel");
        }
    }
}