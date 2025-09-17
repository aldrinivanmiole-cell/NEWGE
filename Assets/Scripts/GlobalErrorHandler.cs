using UnityEngine;
using System.Collections;

/// <summary>
/// Global error handler to catch and manage all Unity errors
/// </summary>
public class GlobalErrorHandler : MonoBehaviour
{
    [Header("Error Handling Settings")]
    public bool logAllErrors = true;
    public bool autoEnableOfflineModeOnNetworkError = true;
    public int maxConsecutiveErrors = 5;
    
    private int consecutiveErrorCount = 0;
    private float lastErrorTime = 0f;
    private const float errorResetTime = 30f; // Reset error count after 30 seconds
    
    void Awake()
    {
        // Subscribe to Unity's log message received event
        Application.logMessageReceived += HandleLog;
        
        if (logAllErrors)
            Debug.Log("GlobalErrorHandler: Error monitoring started");
    }
    
    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
    
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Reset error count if enough time has passed
        if (Time.time - lastErrorTime > errorResetTime)
        {
            consecutiveErrorCount = 0;
        }
        
        switch (type)
        {
            case LogType.Error:
            case LogType.Exception:
                HandleError(logString, stackTrace);
                break;
            case LogType.Warning:
                HandleWarning(logString);
                break;
        }
    }
    
    void HandleError(string error, string stackTrace)
    {
        consecutiveErrorCount++;
        lastErrorTime = Time.time;
        
        if (logAllErrors)
            Debug.Log($"GlobalErrorHandler: Caught error #{consecutiveErrorCount}: {error}");
        
        // Handle specific error types
        if (error.Contains("missing script") || error.Contains("Missing script"))
        {
            HandleMissingScriptError();
        }
        else if (error.Contains("Flask") || error.Contains("HTTP") || error.Contains("404") || error.Contains("Connection"))
        {
            HandleNetworkError();
        }
        else if (error.Contains("NullReferenceException") || error.Contains("null"))
        {
            HandleNullReferenceError();
        }
        
        // If too many consecutive errors, take defensive action
        if (consecutiveErrorCount >= maxConsecutiveErrors)
        {
            EnableEmergencyMode();
        }
    }
    
    void HandleWarning(string warning)
    {
        if (warning.Contains("classNameText is not assigned") || 
            warning.Contains("is not assigned"))
        {
            StartCoroutine(AttemptUIFixDelayed());
        }
        
        if (warning.Contains("Failed to send data to Flask"))
        {
            HandleNetworkError();
        }
    }
    
    void HandleMissingScriptError()
    {
        if (logAllErrors)
            Debug.Log("GlobalErrorHandler: Attempting to fix missing script references");
        
        // Try to add cleanup component to scene
        GameObject cleanupObj = GameObject.Find("AutoCleanup");
        if (cleanupObj == null)
        {
            cleanupObj = new GameObject("AutoCleanup");
            cleanupObj.AddComponent<AutoCleanupMissingScripts>();
        }
    }
    
    void HandleNetworkError()
    {
        if (autoEnableOfflineModeOnNetworkError)
        {
            if (logAllErrors)
                Debug.Log("GlobalErrorHandler: Network error detected, enabling offline mode");
            
            PlayerPrefs.SetInt("OfflineMode", 1);
            PlayerPrefs.Save();
        }
    }
    
    void HandleNullReferenceError()
    {
        if (logAllErrors)
            Debug.Log("GlobalErrorHandler: Null reference detected, attempting UI fix");
        
        StartCoroutine(AttemptUIFixDelayed());
    }
    
    IEnumerator AttemptUIFixDelayed()
    {
        yield return new WaitForSeconds(0.5f); // Wait for scene to settle
        
        // Try to fix UI references
        MasterErrorFixer fixer = FindFirstObjectByType<MasterErrorFixer>();
        if (fixer != null)
        {
            fixer.FixAllErrorsNow();
        }
        else
        {
            // Create a temporary fixer
            GameObject tempFixer = new GameObject("TempErrorFixer");
            tempFixer.AddComponent<MasterErrorFixer>();
        }
    }
    
    void EnableEmergencyMode()
    {
        if (logAllErrors)
            Debug.LogWarning($"GlobalErrorHandler: Too many consecutive errors ({consecutiveErrorCount}). Enabling emergency mode.");
        
        // Enable offline mode
        PlayerPrefs.SetInt("OfflineMode", 1);
        PlayerPrefs.SetInt("EmergencyMode", 1);
        PlayerPrefs.Save();
        
        // Reset error count
        consecutiveErrorCount = 0;
        
        // Try to clean up problematic components
        StartCoroutine(EmergencyCleanup());
    }
    
    IEnumerator EmergencyCleanup()
    {
        yield return new WaitForSeconds(1f);
        
        // Add comprehensive error fixing
        GameObject emergencyFixer = new GameObject("EmergencyFixer");
        emergencyFixer.AddComponent<MasterErrorFixer>();
        emergencyFixer.AddComponent<AutoCleanupMissingScripts>();
        
        Debug.LogWarning("Emergency cleanup initiated. The application is now in safe mode.");
    }
}

/// <summary>
/// Specialized network error handler
/// </summary>
public class NetworkErrorHandler : MonoBehaviour
{
    [Header("Network Settings")]
    public bool autoDetectNetworkIssues = true;
    public float networkCheckInterval = 30f;
    public string testUrl = "https://www.google.com";
    
    private bool isCheckingNetwork = false;
    
    void Start()
    {
        if (autoDetectNetworkIssues)
        {
            InvokeRepeating(nameof(CheckNetworkStatus), 5f, networkCheckInterval);
        }
    }
    
    void CheckNetworkStatus()
    {
        if (!isCheckingNetwork)
        {
            StartCoroutine(CheckNetworkCoroutine());
        }
    }
    
    IEnumerator CheckNetworkCoroutine()
    {
        isCheckingNetwork = true;
        
        // Simple network check
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("NetworkErrorHandler: No internet connection detected, enabling offline mode");
            PlayerPrefs.SetInt("OfflineMode", 1);
            PlayerPrefs.Save();
        }
        else
        {
            // More detailed check with actual web request
            UnityEngine.Networking.UnityWebRequest request = UnityEngine.Networking.UnityWebRequest.Get(testUrl);
            request.timeout = 5;
            
            yield return request.SendWebRequest();
            
            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.Log("NetworkErrorHandler: Network connectivity issues detected, enabling offline mode");
                PlayerPrefs.SetInt("OfflineMode", 1);
                PlayerPrefs.Save();
            }
            
            request.Dispose();
        }
        
        isCheckingNetwork = false;
    }
    
    public void ForceOfflineMode()
    {
        PlayerPrefs.SetInt("OfflineMode", 1);
        PlayerPrefs.Save();
        Debug.Log("NetworkErrorHandler: Offline mode manually enabled");
    }
    
    public void ForceOnlineMode()
    {
        PlayerPrefs.SetInt("OfflineMode", 0);
        PlayerPrefs.Save();
        Debug.Log("NetworkErrorHandler: Online mode manually enabled");
    }
}