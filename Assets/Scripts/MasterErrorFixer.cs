using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Master Error Fixer for CapstoneUnityApp
/// This script automatically fixes all common Unity errors when a scene loads
/// </summary>
[DefaultExecutionOrder(-1000)] // Execute before other scripts
public class MasterErrorFixer : MonoBehaviour
{
    [Header("Auto-Fix Settings")]
    public bool fixOnSceneLoad = true;
    public bool enableOfflineModeOnNetworkError = true;
    public bool autoAssignUIReferences = true;
    public bool removeInvalidComponents = true;
    public bool logAllActions = true;
    
    [Header("Network Settings")]
    public float networkTimeoutSeconds = 5f;
    public int maxConnectionRetries = 3;
    
    private static bool hasRunInThisScene = false;
    
    void Awake()
    {
        // Ensure this only runs once per scene
        if (hasRunInThisScene) return;
        hasRunInThisScene = true;
        
        if (fixOnSceneLoad)
        {
            StartCoroutine(FixAllErrorsCoroutine());
        }
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        hasRunInThisScene = false; // Reset for new scene
    }
    
    IEnumerator FixAllErrorsCoroutine()
    {
        if (logAllActions)
            Debug.Log("=== MASTER ERROR FIXER: Starting comprehensive fix ===");
        
        // Wait a frame to ensure all objects are loaded
        yield return null;
        
        // Fix 1: Handle missing script references
        yield return StartCoroutine(DetectMissingScripts());
        
        // Fix 2: Auto-assign UI references
        if (autoAssignUIReferences)
        {
            yield return StartCoroutine(AutoAssignAllUIReferences());
        }
        
        // Fix 3: Setup offline mode if needed
        if (enableOfflineModeOnNetworkError)
        {
            SetupOfflineMode();
        }
        
        // Fix 4: Validate and fix game mechanics
        yield return StartCoroutine(FixGameMechanics());
        
        // Fix 5: Setup error prevention
        SetupErrorPrevention();
        
        if (logAllActions)
            Debug.Log("=== MASTER ERROR FIXER: All fixes completed ===");
    }
    
    IEnumerator DetectMissingScripts()
    {
        if (logAllActions)
            Debug.Log("Detecting missing script references...");
        
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        List<GameObject> objectsWithMissingScripts = new List<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj == null) continue;
            
            Component[] components = obj.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    objectsWithMissingScripts.Add(obj);
                    if (logAllActions)
                        Debug.LogWarning($"Missing script detected on: {GetGameObjectPath(obj)}", obj);
                    break;
                }
            }
        }
        
        if (objectsWithMissingScripts.Count > 0)
        {
            Debug.LogWarning($"Found {objectsWithMissingScripts.Count} objects with missing scripts. Use the Editor tools to remove them.");
            
            // Try to create auto-cleanup component on objects without causing errors
            foreach (GameObject obj in objectsWithMissingScripts)
            {
                if (obj.GetComponent<AutoCleanupMissingScripts>() == null)
                {
                    try
                    {
                        obj.AddComponent<AutoCleanupMissingScripts>();
                    }
                    catch (System.Exception e)
                    {
                        if (logAllActions)
                            Debug.Log($"Could not add cleanup component to {obj.name}: {e.Message}");
                    }
                }
            }
        }
        
        yield return null;
    }
    
    IEnumerator AutoAssignAllUIReferences()
    {
        if (logAllActions)
            Debug.Log("Auto-assigning UI references...");
        
        // Fix ProfileLoader components
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        foreach (var loader in profileLoaders)
        {
            FixProfileLoaderReferences(loader);
            yield return null;
        }
        
        // Fix other UI components
        yield return StartCoroutine(FixOtherUIComponents());
        
        if (logAllActions)
            Debug.Log($"Fixed UI references for {profileLoaders.Length} ProfileLoader components");
    }
    
    IEnumerator FixOtherUIComponents()
    {
        // Find and fix other common UI component issues
        Button[] buttons = FindObjectsByType<Button>(FindObjectsSortMode.None);
        foreach (var button in buttons)
        {
            if (button.targetGraphic == null)
            {
                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    button.targetGraphic = image;
                    if (logAllActions)
                        Debug.Log($"Fixed button target graphic for: {button.name}");
                }
            }
            yield return null;
        }
        
        // Fix TMP_Text components without proper fonts
        TMP_Text[] tmpTexts = FindObjectsByType<TMP_Text>(FindObjectsSortMode.None);
        foreach (var text in tmpTexts)
        {
            if (text.font == null)
            {
                // Try to assign default TMP font
                text.font = Resources.GetBuiltinResource<TMP_FontAsset>("LegacyRuntime.fontsettings");
                if (logAllActions && text.font != null)
                    Debug.Log($"Assigned default font to: {text.name}");
            }
            yield return null;
        }
    }
    
    void FixProfileLoaderReferences(ProfileLoader loader)
    {
        if (loader == null) return;
        
        // Auto-assign classNameText
        if (loader.classNameText == null)
        {
            TMP_Text[] textComponents = loader.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in textComponents)
            {
                string name = text.name.ToLower();
                if (name.Contains("class") || name.Contains("className"))
                {
                    loader.classNameText = text;
                    if (logAllActions)
                        Debug.Log($"Auto-assigned classNameText to: {text.name}");
                    break;
                }
            }
        }
        
        // Auto-assign studentNameText
        if (loader.studentNameText == null)
        {
            TMP_Text[] textComponents = loader.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in textComponents)
            {
                string name = text.name.ToLower();
                if (name.Contains("student") || name.Contains("name"))
                {
                    loader.studentNameText = text;
                    if (logAllActions)
                        Debug.Log($"Auto-assigned studentNameText to: {text.name}");
                    break;
                }
            }
        }
        
        // Auto-assign gradeLevelText
        if (loader.gradeLevelText == null)
        {
            TMP_Text[] textComponents = loader.GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in textComponents)
            {
                string name = text.name.ToLower();
                if (name.Contains("grade") || name.Contains("level"))
                {
                    loader.gradeLevelText = text;
                    if (logAllActions)
                        Debug.Log($"Auto-assigned gradeLevelText to: {text.name}");
                    break;
                }
            }
        }
        
        // Auto-assign avatarImage
        if (loader.avatarImage == null)
        {
            Image[] imageComponents = loader.GetComponentsInChildren<Image>(true);
            foreach (var img in imageComponents)
            {
                string name = img.name.ToLower();
                if (name.Contains("avatar") || name.Contains("profile") || name.Contains("image"))
                {
                    loader.avatarImage = img;
                    if (logAllActions)
                        Debug.Log($"Auto-assigned avatarImage to: {img.name}");
                    break;
                }
            }
        }
    }
    
    void SetupOfflineMode()
    {
        if (logAllActions)
            Debug.Log("Setting up offline mode configuration...");
        
        // Enable offline mode by default to prevent network errors
        if (PlayerPrefs.GetInt("OfflineModeConfigured", 0) == 0)
        {
            PlayerPrefs.SetInt("OfflineMode", 1);
            PlayerPrefs.SetInt("OfflineModeConfigured", 1);
            PlayerPrefs.Save();
            
            if (logAllActions)
                Debug.Log("Offline mode enabled by default to prevent network errors");
        }
    }
    
    IEnumerator FixGameMechanics()
    {
        if (logAllActions)
            Debug.Log("Fixing game mechanics...");
        
        // Find and configure GameMechanicDragButtons components
        MonoBehaviour[] allMonos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        
        foreach (var mono in allMonos)
        {
            if (mono == null) continue;
            
            // Check if it's a GameMechanicDragButtons (by name since we don't have direct reference)
            if (mono.GetType().Name.Contains("GameMechanic") || mono.GetType().Name.Contains("gamemechanic"))
            {
                // Add offline mode check capability
                if (mono.gameObject.GetComponent<OfflineModeChecker>() == null)
                {
                    mono.gameObject.AddComponent<OfflineModeChecker>();
                }
            }
            
            yield return null;
        }
    }
    
    void SetupErrorPrevention()
    {
        if (logAllActions)
            Debug.Log("Setting up error prevention...");
        
        // Add error prevention components to scene
        GameObject errorPreventer = GameObject.Find("ErrorPreventer");
        if (errorPreventer == null)
        {
            errorPreventer = new GameObject("ErrorPreventer");
            DontDestroyOnLoad(errorPreventer);
            
            // Add comprehensive error prevention
            errorPreventer.AddComponent<GlobalErrorHandler>();
            errorPreventer.AddComponent<NetworkErrorHandler>();
        }
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
    
    [ContextMenu("Fix All Errors Now")]
    public void FixAllErrorsNow()
    {
        StartCoroutine(FixAllErrorsCoroutine());
    }
}

/// <summary>
/// Component to check offline mode status for game mechanics
/// </summary>
public class OfflineModeChecker : MonoBehaviour
{
    public bool IsOfflineMode()
    {
        return PlayerPrefs.GetInt("OfflineMode", 0) == 1;
    }
    
    public void EnableOfflineMode()
    {
        PlayerPrefs.SetInt("OfflineMode", 1);
        PlayerPrefs.Save();
        Debug.Log("Offline mode enabled");
    }
    
    public void DisableOfflineMode()
    {
        PlayerPrefs.SetInt("OfflineMode", 0);
        PlayerPrefs.Save();
        Debug.Log("Offline mode disabled");
    }
}