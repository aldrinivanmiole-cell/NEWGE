using UnityEngine;

/// <summary>
/// Immediate script reference fixer that runs before other scripts
/// Handles missing script errors during Unity compilation/reload
/// </summary>
[DefaultExecutionOrder(-1000)] // Execute very early
public class ImmediateScriptFixer : MonoBehaviour
{
    static bool hasRunCleanup = false;
    
    void Awake()
    {
        // Run cleanup immediately on awake, before other scripts
        if (!hasRunCleanup)
        {
            PerformImmediateCleanup();
            hasRunCleanup = true;
        }
    }
    
    void Start()
    {
        // Double-check cleanup on start
        PerformImmediateCleanup();
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeOnLoad()
    {
        // This runs before any scene loads
        Debug.Log("ImmediateScriptFixer: Running before scene load");
        
        // Reset the cleanup flag for new scene loads
        hasRunCleanup = false;
    }
    
    static void PerformImmediateCleanup()
    {
        try
        {
            Debug.Log("=== IMMEDIATE SCRIPT FIXER: Starting emergency cleanup ===");
            
            // Find all GameObjects in the scene
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            int fixedObjects = 0;
            
            foreach (GameObject obj in allObjects)
            {
                // Skip objects that are part of prefabs or not in current scene
                if (obj.scene.name == null || obj.hideFlags != HideFlags.None)
                    continue;
                
                if (CleanGameObjectImmediate(obj))
                {
                    fixedObjects++;
                }
            }
            
            Debug.Log($"IMMEDIATE SCRIPT FIXER: Fixed {fixedObjects} objects");
            
            // Ensure essential components exist
            EnsureEssentialComponents();
            
            Debug.Log("=== IMMEDIATE SCRIPT FIXER: Emergency cleanup completed ===");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"IMMEDIATE SCRIPT FIXER: Error during cleanup: {e.Message}");
        }
    }
    
    static bool CleanGameObjectImmediate(GameObject obj)
    {
        bool wasFixed = false;
        Component[] components = obj.GetComponents<Component>();
        
        // Check for null/missing components
        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] == null)
            {
                Debug.Log($"IMMEDIATE SCRIPT FIXER: Found missing script on {obj.name}");
                
                // Add replacement based on object name/purpose
                AddEmergencyReplacement(obj);
                wasFixed = true;
                break; // Only need to fix once per object
            }
        }
        
        return wasFixed;
    }
    
    static void AddEmergencyReplacement(GameObject obj)
    {
        string objName = obj.name.ToLower();
        
        // Add appropriate replacement component
        if (objName.Contains("math"))
        {
            AddSubjectButtonReplacement(obj, "Math");
        }
        else if (objName.Contains("science"))
        {
            AddSubjectButtonReplacement(obj, "Science");
        }
        else if (objName.Contains("english"))
        {
            AddSubjectButtonReplacement(obj, "English");
        }
        else if (objName.Contains("art"))
        {
            AddSubjectButtonReplacement(obj, "Art");
        }
        else if (objName.Contains("button"))
        {
            // Generic button
            if (obj.GetComponent<UnityEngine.UI.Button>() == null)
                obj.AddComponent<UnityEngine.UI.Button>();
        }
        else if (objName.Contains("panel"))
        {
            // Panel object
            if (obj.GetComponent<UnityEngine.UI.Image>() == null)
                obj.AddComponent<UnityEngine.UI.Image>();
        }
        else
        {
            // Generic replacement
            if (obj.GetComponent<EmergencyScriptReplacement>() == null)
                obj.AddComponent<EmergencyScriptReplacement>();
        }
        
        Debug.Log($"IMMEDIATE SCRIPT FIXER: Added replacement to {obj.name}");
    }
    
    static void AddSubjectButtonReplacement(GameObject obj, string subject)
    {
        // Ensure it has a Button component
        if (obj.GetComponent<UnityEngine.UI.Button>() == null)
            obj.AddComponent<UnityEngine.UI.Button>();
        
        // Add our emergency subject button handler
        EmergencySubjectButton subjectBtn = obj.GetComponent<EmergencySubjectButton>();
        if (subjectBtn == null)
        {
            subjectBtn = obj.AddComponent<EmergencySubjectButton>();
            subjectBtn.subjectName = subject;
        }
    }
    
    static void EnsureEssentialComponents()
    {
        // Make sure we have a navigation fixer in the scene
        if (FindFirstObjectByType<FinalNavigationFix>() == null)
        {
            GameObject navObj = new GameObject("Emergency_FinalNavigationFix");
            navObj.AddComponent<FinalNavigationFix>();
            Debug.Log("IMMEDIATE SCRIPT FIXER: Added emergency FinalNavigationFix");
        }
        
        // Make sure we have the main error fixer
        if (FindFirstObjectByType<ComprehensiveErrorFixer>() == null)
        {
            GameObject errorObj = new GameObject("Emergency_ComprehensiveErrorFixer");
            errorObj.AddComponent<ComprehensiveErrorFixer>();
            Debug.Log("IMMEDIATE SCRIPT FIXER: Added emergency ComprehensiveErrorFixer");
        }
    }
}

/// <summary>
/// Emergency replacement for missing scripts
/// </summary>
public class EmergencyScriptReplacement : MonoBehaviour
{
    public string replacementInfo = "Emergency replacement for missing script";
    
    void Start()
    {
        Debug.Log($"EmergencyScriptReplacement active on {gameObject.name}");
    }
}

/// <summary>
/// Emergency subject button handler
/// </summary>
public class EmergencySubjectButton : MonoBehaviour
{
    public string subjectName = "";
    
    void Start()
    {
        SetupButton();
    }
    
    void SetupButton()
    {
        UnityEngine.UI.Button button = GetComponent<UnityEngine.UI.Button>();
        if (button != null && !string.IsNullOrEmpty(subjectName))
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => ShowSubjectStages());
            Debug.Log($"EmergencySubjectButton: Setup {subjectName} button");
        }
    }
    
    void ShowSubjectStages()
    {
        Debug.Log($"EmergencySubjectButton: {subjectName} clicked - looking for navigation fix");
        
        // Try to find the navigation fixer
        FinalNavigationFix navFix = FindFirstObjectByType<FinalNavigationFix>();
        if (navFix != null)
        {
            navFix.ShowStagePanel(subjectName);
            Debug.Log($"EmergencySubjectButton: Showed {subjectName} stage panel");
        }
        else
        {
            Debug.LogWarning($"EmergencySubjectButton: No FinalNavigationFix found for {subjectName}");
            
            // Fallback - create one
            GameObject navObj = new GameObject("Fallback_FinalNavigationFix");
            FinalNavigationFix newNavFix = navObj.AddComponent<FinalNavigationFix>();
            newNavFix.ShowStagePanel(subjectName);
        }
    }
}