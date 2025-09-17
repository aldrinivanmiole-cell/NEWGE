using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// One-click installer for the complete error fixing system
/// This script automatically sets up all error prevention and fixing components
/// </summary>
public class ErrorFixSystemInstaller : MonoBehaviour
{
    [Header("Installation Settings")]
    public bool installOnStart = false; // Set to true to auto-install
    public bool installInAllScenes = false;
    public bool createPersistentErrorHandler = true;
    public bool enableDebugLogging = true;
    
    [Header("Installation Status")]
    public bool isInstalled = false;
    public int installedComponents = 0;
    
    void Start()
    {
        if (installOnStart && !isInstalled)
        {
            StartCoroutine(InstallErrorFixingSystemDelayed());
        }
    }
    
    IEnumerator InstallErrorFixingSystemDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        InstallErrorFixingSystem();
    }
    
    [ContextMenu("Install Error Fixing System")]
    public void InstallErrorFixingSystem()
    {
        if (enableDebugLogging)
            Debug.Log("=== ERROR FIX SYSTEM INSTALLER: Starting installation ===");
        
        installedComponents = 0;
        
        // Step 1: Install core error handling
        InstallCoreErrorHandling();
        
        // Step 2: Install UI fixing system
        InstallUIFixingSystem();
        
        // Step 3: Install network error handling
        InstallNetworkErrorHandling();
        
        // Step 4: Install scene validation
        InstallSceneValidation();
        
        // Step 5: Install error reporting
        InstallErrorReporting();
        
        // Step 6: Install test suite
        InstallTestSuite();
        
        // Step 7: Configure offline mode
        ConfigureOfflineMode();
        
        // Step 8: Create persistent error handler
        if (createPersistentErrorHandler)
        {
            CreatePersistentErrorHandler();
        }
        
        // Final configuration
        FinalizeInstallation();
        
        isInstalled = true;
        
        if (enableDebugLogging)
        {
            Debug.Log($"=== ERROR FIX SYSTEM INSTALLER: Installation complete! ===");
            Debug.Log($"Installed {installedComponents} components");
            Debug.Log("Your Unity project is now protected against common errors!");
        }
    }
    
    void InstallCoreErrorHandling()
    {
        if (enableDebugLogging)
            Debug.Log("Installing core error handling...");
        
        // Install MasterErrorFixer
        if (FindFirstObjectByType<MasterErrorFixer>() == null)
        {
            GameObject masterFixer = new GameObject("MasterErrorFixer");
            masterFixer.AddComponent<MasterErrorFixer>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ MasterErrorFixer installed");
        }
        
        // Install GlobalErrorHandler
        if (FindFirstObjectByType<GlobalErrorHandler>() == null)
        {
            GameObject globalHandler = new GameObject("GlobalErrorHandler");
            globalHandler.AddComponent<GlobalErrorHandler>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ GlobalErrorHandler installed");
        }
        
        // Install AutoCleanupMissingScripts
        if (FindFirstObjectByType<AutoCleanupMissingScripts>() == null)
        {
            GameObject cleanup = new GameObject("AutoCleanupMissingScripts");
            cleanup.AddComponent<AutoCleanupMissingScripts>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ AutoCleanupMissingScripts installed");
        }
    }
    
    void InstallUIFixingSystem()
    {
        if (enableDebugLogging)
            Debug.Log("Installing UI fixing system...");
        
        // Install UniversalUIFixer
        if (FindFirstObjectByType<UniversalUIFixer>() == null)
        {
            GameObject uiFixer = new GameObject("UniversalUIFixer");
            uiFixer.AddComponent<UniversalUIFixer>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ UniversalUIFixer installed");
        }
        
        // Install ComprehensiveErrorFixer
        if (FindFirstObjectByType<ComprehensiveErrorFixer>() == null)
        {
            GameObject compFixer = new GameObject("ComprehensiveErrorFixer");
            compFixer.AddComponent<ComprehensiveErrorFixer>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ ComprehensiveErrorFixer installed");
        }
    }
    
    void InstallNetworkErrorHandling()
    {
        if (enableDebugLogging)
            Debug.Log("Installing network error handling...");
        
        // Install NetworkErrorHandler
        if (FindFirstObjectByType<NetworkErrorHandler>() == null)
        {
            GameObject networkHandler = new GameObject("NetworkErrorHandler");
            networkHandler.AddComponent<NetworkErrorHandler>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ NetworkErrorHandler installed");
        }
    }
    
    void InstallSceneValidation()
    {
        if (enableDebugLogging)
            Debug.Log("Installing scene validation...");
        
        // Install SceneValidator
        if (FindFirstObjectByType<SceneValidator>() == null)
        {
            GameObject validator = new GameObject("SceneValidator");
            validator.AddComponent<SceneValidator>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ SceneValidator installed");
        }
    }
    
    void InstallErrorReporting()
    {
        if (enableDebugLogging)
            Debug.Log("Installing error reporting system...");
        
        // Install ErrorReportingSystem
        if (FindFirstObjectByType<ErrorReportingSystem>() == null)
        {
            GameObject reporter = new GameObject("ErrorReportingSystem");
            reporter.AddComponent<ErrorReportingSystem>();
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ ErrorReportingSystem installed");
        }
    }
    
    void InstallTestSuite()
    {
        if (enableDebugLogging)
            Debug.Log("Installing test suite...");
        
        // Install ErrorFixTestSuite
        if (FindFirstObjectByType<ErrorFixTestSuite>() == null)
        {
            GameObject testSuite = new GameObject("ErrorFixTestSuite");
            ErrorFixTestSuite suite = testSuite.AddComponent<ErrorFixTestSuite>();
            suite.runTestsOnStart = false; // Don't run automatically during installation
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ ErrorFixTestSuite installed");
        }
    }
    
    void ConfigureOfflineMode()
    {
        if (enableDebugLogging)
            Debug.Log("Configuring offline mode...");
        
        // Enable offline mode by default to prevent network errors
        PlayerPrefs.SetInt("OfflineMode", 1);
        PlayerPrefs.SetInt("OfflineModeConfigured", 1);
        PlayerPrefs.SetInt("ErrorFixSystemInstalled", 1);
        PlayerPrefs.Save();
        
        if (enableDebugLogging) Debug.Log("✓ Offline mode configured");
    }
    
    void CreatePersistentErrorHandler()
    {
        if (enableDebugLogging)
            Debug.Log("Creating persistent error handler...");
        
        GameObject persistentHandler = GameObject.Find("PersistentErrorHandler");
        if (persistentHandler == null)
        {
            persistentHandler = new GameObject("PersistentErrorHandler");
            DontDestroyOnLoad(persistentHandler);
            
            // Add essential persistent components
            persistentHandler.AddComponent<GlobalErrorHandler>();
            persistentHandler.AddComponent<NetworkErrorHandler>();
            persistentHandler.AddComponent<ErrorReportingSystem>();
            
            installedComponents++;
            if (enableDebugLogging) Debug.Log("✓ Persistent error handler created");
        }
    }
    
    void FinalizeInstallation()
    {
        // Add installer info to all major components
        MasterErrorFixer masterFixer = FindFirstObjectByType<MasterErrorFixer>();
        if (masterFixer != null)
        {
            masterFixer.fixOnSceneLoad = true;
            masterFixer.logAllActions = enableDebugLogging;
        }
        
        UniversalUIFixer uiFixer = FindFirstObjectByType<UniversalUIFixer>();
        if (uiFixer != null)
        {
            uiFixer.fixOnStart = true;
            uiFixer.logAssignments = enableDebugLogging;
        }
        
        SceneValidator validator = FindFirstObjectByType<SceneValidator>();
        if (validator != null)
        {
            validator.validateOnSceneLoad = true;
            validator.autoFixIssues = true;
            validator.logValidationResults = enableDebugLogging;
        }
        
        // Run initial fixes
        StartCoroutine(RunInitialFixes());
    }
    
    IEnumerator RunInitialFixes()
    {
        yield return new WaitForSeconds(1f);
        
        if (enableDebugLogging)
            Debug.Log("Running initial error fixes...");
        
        // Trigger all fixers
        MasterErrorFixer masterFixer = FindFirstObjectByType<MasterErrorFixer>();
        if (masterFixer != null)
        {
            masterFixer.FixAllErrorsNow();
        }
        
        UniversalUIFixer uiFixer = FindFirstObjectByType<UniversalUIFixer>();
        if (uiFixer != null)
        {
            uiFixer.FixAllUIReferences();
        }
        
        yield return new WaitForSeconds(1f);
        
        // Run validation test
        ErrorFixTestSuite testSuite = FindFirstObjectByType<ErrorFixTestSuite>();
        if (testSuite != null)
        {
            testSuite.autoSetupIfFailed = true;
            testSuite.logTestResults = enableDebugLogging;
            testSuite.RunAllTests();
        }
        
        if (enableDebugLogging)
            Debug.Log("✅ Error fixing system is now active and protecting your project!");
    }
    
    [ContextMenu("Uninstall Error Fixing System")]
    public void UninstallErrorFixingSystem()
    {
        if (enableDebugLogging)
            Debug.Log("Uninstalling error fixing system...");
        
        // Find and destroy all error fixing components
        string[] componentNames = {
            "MasterErrorFixer", "GlobalErrorHandler", "UniversalUIFixer",
            "SceneValidator", "ErrorReportingSystem", "NetworkErrorHandler",
            "ComprehensiveErrorFixer", "AutoCleanupMissingScripts",
            "ErrorFixTestSuite", "PersistentErrorHandler"
        };
        
        int removed = 0;
        foreach (string componentName in componentNames)
        {
            GameObject obj = GameObject.Find(componentName);
            if (obj != null)
            {
                DestroyImmediate(obj);
                removed++;
            }
        }
        
        // Clear PlayerPrefs
        PlayerPrefs.DeleteKey("ErrorFixSystemInstalled");
        PlayerPrefs.Save();
        
        isInstalled = false;
        installedComponents = 0;
        
        if (enableDebugLogging)
            Debug.Log($"Uninstalled {removed} components. Error fixing system removed.");
    }
    
    [ContextMenu("Check Installation Status")]
    public void CheckInstallationStatus()
    {
        bool systemInstalled = PlayerPrefs.GetInt("ErrorFixSystemInstalled", 0) == 1;
        
        if (systemInstalled)
        {
            Debug.Log("✅ Error fixing system is installed and active");
        }
        else
        {
            Debug.Log("❌ Error fixing system is not installed. Run 'Install Error Fixing System' to set up.");
        }
        
        // Count active components
        string[] componentNames = {
            "MasterErrorFixer", "GlobalErrorHandler", "UniversalUIFixer",
            "SceneValidator", "ErrorReportingSystem", "NetworkErrorHandler"
        };
        
        int activeComponents = 0;
        foreach (string componentName in componentNames)
        {
            if (GameObject.Find(componentName) != null)
                activeComponents++;
        }
        
        Debug.Log($"Active error fixing components: {activeComponents}/{componentNames.Length}");
    }
    
    [ContextMenu("Reinstall Error Fixing System")]
    public void ReinstallErrorFixingSystem()
    {
        UninstallErrorFixingSystem();
        InstallErrorFixingSystem();
    }
}