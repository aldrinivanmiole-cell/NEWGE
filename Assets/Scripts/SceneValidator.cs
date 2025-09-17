using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Scene validator that ensures all critical components are properly configured
/// </summary>
public class SceneValidator : MonoBehaviour
{
    [Header("Validation Settings")]
    public bool validateOnSceneLoad = true;
    public bool autoFixIssues = true;
    public bool logValidationResults = true;
    
    [Header("Required Components")]
    public List<string> requiredScriptNames = new List<string>
    {
        "ProfileLoader",
        "GameMechanicDragButtons",
        "MasterErrorFixer"
    };
    
    private Dictionary<string, List<string>> validationResults = new Dictionary<string, List<string>>();
    
    void Start()
    {
        if (validateOnSceneLoad)
        {
            StartCoroutine(ValidateSceneDelayed());
        }
    }
    
    IEnumerator ValidateSceneDelayed()
    {
        // Wait for all objects to initialize
        yield return new WaitForSeconds(1f);
        
        ValidateScene();
    }
    
    [ContextMenu("Validate Scene")]
    public void ValidateScene()
    {
        if (logValidationResults)
            Debug.Log("=== SCENE VALIDATOR: Starting validation ===");
        
        validationResults.Clear();
        
        // Validate UI references
        ValidateUIReferences();
        
        // Validate network components
        ValidateNetworkComponents();
        
        // Validate missing scripts
        ValidateMissingScripts();
        
        // Validate critical game objects
        ValidateCriticalGameObjects();
        
        // Apply fixes if enabled
        if (autoFixIssues)
        {
            StartCoroutine(ApplyFixes());
        }
        
        // Log results
        LogValidationResults();
    }
    
    void ValidateUIReferences()
    {
        List<string> issues = new List<string>();
        
        // Check ProfileLoader components
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        foreach (var loader in profileLoaders)
        {
            if (loader.classNameText == null)
                issues.Add($"ProfileLoader on {loader.gameObject.name}: classNameText not assigned");
            if (loader.studentNameText == null)
                issues.Add($"ProfileLoader on {loader.gameObject.name}: studentNameText not assigned");
            if (loader.gradeLevelText == null)
                issues.Add($"ProfileLoader on {loader.gameObject.name}: gradeLevelText not assigned");
            if (loader.avatarImage == null)
                issues.Add($"ProfileLoader on {loader.gameObject.name}: avatarImage not assigned");
        }
        
        // Check Button components
        UnityEngine.UI.Button[] buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            if (button.targetGraphic == null)
                issues.Add($"Button on {button.gameObject.name}: targetGraphic not assigned");
        }
        
        validationResults["UI References"] = issues;
    }
    
    void ValidateNetworkComponents()
    {
        List<string> issues = new List<string>();
        
        // Check if offline mode is properly configured
        bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
        if (!offlineMode)
        {
            issues.Add("Offline mode not enabled - may cause network errors");
        }
        
        // Check for network error handlers
        NetworkErrorHandler handler = FindFirstObjectByType<NetworkErrorHandler>();
        if (handler == null)
        {
            issues.Add("NetworkErrorHandler not found in scene");
        }
        
        validationResults["Network Components"] = issues;
    }
    
    void ValidateMissingScripts()
    {
        List<string> issues = new List<string>();
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    issues.Add($"Missing script on {GetGameObjectPath(obj)}");
                }
            }
        }
        
        validationResults["Missing Scripts"] = issues;
    }
    
    void ValidateCriticalGameObjects()
    {
        List<string> issues = new List<string>();
        
        // Check for error handling components
        if (FindFirstObjectByType<MasterErrorFixer>() == null)
        {
            issues.Add("MasterErrorFixer not found in scene");
        }
        
        if (FindFirstObjectByType<GlobalErrorHandler>() == null)
        {
            issues.Add("GlobalErrorHandler not found in scene");
        }
        
        // Check for essential UI elements based on scene name
        string sceneName = SceneManager.GetActiveScene().name.ToLower();
        
        if (sceneName.Contains("login"))
        {
            ValidateLoginScene(issues);
        }
        else if (sceneName.Contains("game") || sceneName.Contains("gp"))
        {
            ValidateGameScene(issues);
        }
        else if (sceneName.Contains("menu"))
        {
            ValidateMenuScene(issues);
        }
        
        validationResults["Critical GameObjects"] = issues;
    }
    
    void ValidateLoginScene(List<string> issues)
    {
        // Check for login manager
        if (GameObject.Find("LoginManager") == null && FindFirstObjectByType<MonoBehaviour>() == null)
        {
            issues.Add("Login scene missing LoginManager component");
        }
        
        // Check for input fields
        TMPro.TMP_InputField[] inputFields = FindObjectsByType<TMPro.TMP_InputField>(FindObjectsSortMode.None);
        if (inputFields.Length == 0)
        {
            UnityEngine.UI.InputField[] legacyInputs = FindObjectsByType<UnityEngine.UI.InputField>(FindObjectsSortMode.None);
            if (legacyInputs.Length == 0)
            {
                issues.Add("Login scene missing input fields");
            }
        }
    }
    
    void ValidateGameScene(List<string> issues)
    {
        // Check for game mechanics
        MonoBehaviour[] scripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        bool hasGameMechanic = false;
        
        foreach (var script in scripts)
        {
            if (script.GetType().Name.ToLower().Contains("game") || 
                script.GetType().Name.ToLower().Contains("mechanic"))
            {
                hasGameMechanic = true;
                break;
            }
        }
        
        if (!hasGameMechanic)
        {
            issues.Add("Game scene missing game mechanic components");
        }
    }
    
    void ValidateMenuScene(List<string> issues)
    {
        // Check for essential menu buttons
        UnityEngine.UI.Button[] buttons = FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None);
        if (buttons.Length == 0)
        {
            issues.Add("Menu scene has no buttons");
        }
    }
    
    IEnumerator ApplyFixes()
    {
        if (logValidationResults)
            Debug.Log("SceneValidator: Applying automatic fixes...");
        
        yield return null;
        
        // Add MasterErrorFixer if missing
        if (FindFirstObjectByType<MasterErrorFixer>() == null)
        {
            GameObject fixerObj = new GameObject("MasterErrorFixer");
            fixerObj.AddComponent<MasterErrorFixer>();
            if (logValidationResults)
                Debug.Log("Added MasterErrorFixer to scene");
        }
        
        // Add GlobalErrorHandler if missing
        if (FindFirstObjectByType<GlobalErrorHandler>() == null)
        {
            GameObject handlerObj = new GameObject("GlobalErrorHandler");
            handlerObj.AddComponent<GlobalErrorHandler>();
            handlerObj.AddComponent<NetworkErrorHandler>();
            if (logValidationResults)
                Debug.Log("Added GlobalErrorHandler to scene");
        }
        
        // Enable offline mode if not set
        if (PlayerPrefs.GetInt("OfflineMode", 0) == 0)
        {
            PlayerPrefs.SetInt("OfflineMode", 1);
            PlayerPrefs.Save();
            if (logValidationResults)
                Debug.Log("Enabled offline mode for safety");
        }
        
        yield return null;
        
        // Trigger UI reference fixing
        MasterErrorFixer fixer = FindFirstObjectByType<MasterErrorFixer>();
        if (fixer != null)
        {
            fixer.FixAllErrorsNow();
        }
    }
    
    void LogValidationResults()
    {
        if (!logValidationResults) return;
        
        int totalIssues = 0;
        foreach (var category in validationResults)
        {
            totalIssues += category.Value.Count;
            
            if (category.Value.Count > 0)
            {
                Debug.LogWarning($"SceneValidator - {category.Key} Issues ({category.Value.Count}):");
                foreach (var issue in category.Value)
                {
                    Debug.LogWarning($"  • {issue}");
                }
            }
        }
        
        if (totalIssues == 0)
        {
            Debug.Log("SceneValidator: ✓ Scene validation passed - no issues found!");
        }
        else
        {
            Debug.LogWarning($"SceneValidator: Found {totalIssues} total issues" + 
                           (autoFixIssues ? " (fixes applied automatically)" : " (enable autoFixIssues to fix automatically)"));
        }
        
        Debug.Log("=== SCENE VALIDATOR: Validation complete ===");
    }
    
    string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    public void EnableOfflineMode()
    {
        PlayerPrefs.SetInt("OfflineMode", 1);
        PlayerPrefs.Save();
        Debug.Log("SceneValidator: Offline mode enabled");
    }
    
    public void DisableOfflineMode()
    {
        PlayerPrefs.SetInt("OfflineMode", 0);
        PlayerPrefs.Save();
        Debug.Log("SceneValidator: Offline mode disabled");
    }
}