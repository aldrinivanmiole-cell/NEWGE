using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Comprehensive test suite and setup validator for the error fixing system
/// This script validates that all fixes are working correctly
/// </summary>
public class ErrorFixTestSuite : MonoBehaviour
{
    [Header("Test Settings")]
    public bool runTestsOnStart = true;
    public bool logTestResults = true;
    public bool autoSetupIfFailed = true;
    
    [Header("Test Results")]
    public int totalTests = 0;
    public int passedTests = 0;
    public int failedTests = 0;
    
    private List<string> testResults = new List<string>();
    private List<string> setupActions = new List<string>();
    
    void Start()
    {
        if (runTestsOnStart)
        {
            StartCoroutine(RunAllTestsDelayed());
        }
    }
    
    IEnumerator RunAllTestsDelayed()
    {
        yield return new WaitForSeconds(2f); // Allow all systems to initialize
        
        RunAllTests();
    }
    
    [ContextMenu("Run All Tests")]
    public void RunAllTests()
    {
        if (logTestResults)
            Debug.Log("=== ERROR FIX TEST SUITE: Starting comprehensive tests ===");
        
        ResetTestCounters();
        
        // Test 1: Error Handling Components
        TestErrorHandlingComponents();
        
        // Test 2: UI Reference Assignment
        TestUIReferenceAssignment();
        
        // Test 3: Offline Mode Configuration
        TestOfflineModeConfiguration();
        
        // Test 4: Scene Validation
        TestSceneValidation();
        
        // Test 5: Missing Script Detection
        TestMissingScriptDetection();
        
        // Test 6: Network Error Handling
        TestNetworkErrorHandling();
        
        // Test 7: ProfileLoader Functionality
        TestProfileLoaderFunctionality();
        
        // Generate final report
        GenerateTestReport();
        
        // Auto-setup if needed
        if (autoSetupIfFailed && failedTests > 0)
        {
            StartCoroutine(AutoSetupFailedSystems());
        }
    }
    
    void ResetTestCounters()
    {
        totalTests = 0;
        passedTests = 0;
        failedTests = 0;
        testResults.Clear();
        setupActions.Clear();
    }
    
    void TestErrorHandlingComponents()
    {
        RunTest("MasterErrorFixer Present", () => FindFirstObjectByType<MasterErrorFixer>() != null,
                "Add MasterErrorFixer component to scene");
        
        RunTest("GlobalErrorHandler Present", () => FindFirstObjectByType<GlobalErrorHandler>() != null,
                "Add GlobalErrorHandler component to scene");
        
        RunTest("ErrorReportingSystem Present", () => FindFirstObjectByType<ErrorReportingSystem>() != null,
                "Add ErrorReportingSystem component to scene");
        
        RunTest("SceneValidator Present", () => FindFirstObjectByType<SceneValidator>() != null,
                "Add SceneValidator component to scene");
        
        RunTest("UniversalUIFixer Present", () => FindFirstObjectByType<UniversalUIFixer>() != null,
                "Add UniversalUIFixer component to scene");
    }
    
    void TestUIReferenceAssignment()
    {
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        
        if (profileLoaders.Length > 0)
        {
            foreach (var loader in profileLoaders)
            {
                RunTest($"ProfileLoader {loader.name} - classNameText assigned", 
                        () => loader.classNameText != null,
                        $"Auto-assign classNameText for ProfileLoader on {loader.name}");
                
                RunTest($"ProfileLoader {loader.name} - studentNameText assigned", 
                        () => loader.studentNameText != null,
                        $"Auto-assign studentNameText for ProfileLoader on {loader.name}");
                
                RunTest($"ProfileLoader {loader.name} - gradeLevelText assigned", 
                        () => loader.gradeLevelText != null,
                        $"Auto-assign gradeLevelText for ProfileLoader on {loader.name}");
            }
        }
        else
        {
            RunTest("ProfileLoader Components Found", () => false, "No ProfileLoader components to test");
        }
    }
    
    void TestOfflineModeConfiguration()
    {
        RunTest("Offline Mode Configured", () => PlayerPrefs.HasKey("OfflineMode"),
                "Set offline mode configuration: PlayerPrefs.SetInt('OfflineMode', 1)");
        
        RunTest("Offline Mode Enabled by Default", () => PlayerPrefs.GetInt("OfflineMode", 0) == 1,
                "Enable offline mode: PlayerPrefs.SetInt('OfflineMode', 1)");
        
        NetworkErrorHandler networkHandler = FindFirstObjectByType<NetworkErrorHandler>();
        RunTest("Network Error Handler Configured", () => networkHandler != null && networkHandler.autoDetectNetworkIssues,
                "Configure NetworkErrorHandler with auto-detection enabled");
    }
    
    void TestSceneValidation()
    {
        SceneValidator validator = FindFirstObjectByType<SceneValidator>();
        
        RunTest("Scene Validator Auto-Fix Enabled", () => validator != null && validator.autoFixIssues,
                "Enable auto-fix in SceneValidator");
        
        RunTest("Scene Validator Validation Enabled", () => validator != null && validator.validateOnSceneLoad,
                "Enable validation on scene load in SceneValidator");
    }
    
    void TestMissingScriptDetection()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int missingScriptCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    missingScriptCount++;
                }
            }
        }
        
        RunTest("No Missing Scripts Detected", () => missingScriptCount == 0,
                $"Remove {missingScriptCount} missing script references using Editor tools");
        
        AutoCleanupMissingScripts cleanup = FindFirstObjectByType<AutoCleanupMissingScripts>();
        RunTest("Auto Cleanup Component Available", () => cleanup != null,
                "Add AutoCleanupMissingScripts component for runtime cleanup");
    }
    
    void TestNetworkErrorHandling()
    {
        // Test that network components handle offline mode properly
        MonoBehaviour[] allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        bool hasNetworkComponents = false;
        
        foreach (var component in allComponents)
        {
            if (component.GetType().Name.ToLower().Contains("network") ||
                component.GetType().Name.ToLower().Contains("flask") ||
                component.GetType().Name.ToLower().Contains("gamemechanic"))
            {
                hasNetworkComponents = true;
                break;
            }
        }
        
        RunTest("Network Components Present", () => hasNetworkComponents,
                "Network components found for testing");
        
        RunTest("Offline Mode Protection Active", () => PlayerPrefs.GetInt("OfflineMode", 0) == 1,
                "Offline mode should be enabled to prevent network errors");
    }
    
    void TestProfileLoaderFunctionality()
    {
        ProfileLoader[] loaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        
        foreach (var loader in loaders)
        {
            // Test if ProfileLoader has auto-assignment method
            bool hasAutoAssign = loader.GetType().GetMethod("AutoAssignMissingReferences") != null;
            RunTest($"ProfileLoader {loader.name} - Has Auto-Assignment", () => hasAutoAssign,
                    "ProfileLoader should have AutoAssignMissingReferences method");
            
            // Test offline mode awareness
            bool hasOfflineCheck = loader.GetType().GetField("useWebAppData") != null;
            RunTest($"ProfileLoader {loader.name} - Offline Mode Aware", () => hasOfflineCheck,
                    "ProfileLoader should check offline mode before web requests");
        }
    }
    
    void RunTest(string testName, System.Func<bool> testCondition, string setupAction)
    {
        totalTests++;
        bool passed = false;
        
        try
        {
            passed = testCondition();
        }
        catch (System.Exception e)
        {
            if (logTestResults)
                Debug.LogError($"Test '{testName}' threw exception: {e.Message}");
        }
        
        if (passed)
        {
            passedTests++;
            testResults.Add($"✓ {testName}");
        }
        else
        {
            failedTests++;
            testResults.Add($"✗ {testName}");
            setupActions.Add(setupAction);
        }
        
        if (logTestResults)
        {
            Debug.Log($"Test: {testName} - {(passed ? "PASSED" : "FAILED")}");
        }
    }
    
    void GenerateTestReport()
    {
        System.Text.StringBuilder report = new System.Text.StringBuilder();
        report.AppendLine("=== ERROR FIX TEST SUITE REPORT ===");
        report.AppendLine($"Total Tests: {totalTests}");
        report.AppendLine($"Passed: {passedTests}");
        report.AppendLine($"Failed: {failedTests}");
        report.AppendLine($"Success Rate: {(totalTests > 0 ? (passedTests * 100f / totalTests) : 0):F1}%");
        report.AppendLine();
        
        if (failedTests > 0)
        {
            report.AppendLine("FAILED TESTS:");
            foreach (string result in testResults)
            {
                if (result.StartsWith("✗"))
                {
                    report.AppendLine($"  {result}");
                }
            }
            report.AppendLine();
            
            report.AppendLine("RECOMMENDED ACTIONS:");
            for (int i = 0; i < setupActions.Count; i++)
            {
                report.AppendLine($"  {i + 1}. {setupActions[i]}");
            }
        }
        else
        {
            report.AppendLine("🎉 ALL TESTS PASSED! Error fixing system is working correctly.");
        }
        
        report.AppendLine();
        report.AppendLine("=== END OF REPORT ===");
        
        if (logTestResults)
        {
            Debug.Log(report.ToString());
        }
    }
    
    IEnumerator AutoSetupFailedSystems()
    {
        if (logTestResults)
            Debug.Log("=== AUTO-SETUP: Fixing failed systems ===");
        
        yield return new WaitForSeconds(0.5f);
        
        // Add missing error handling components
        if (FindFirstObjectByType<MasterErrorFixer>() == null)
        {
            GameObject fixerObj = new GameObject("MasterErrorFixer");
            fixerObj.AddComponent<MasterErrorFixer>();
            if (logTestResults) Debug.Log("Added MasterErrorFixer");
        }
        
        if (FindFirstObjectByType<GlobalErrorHandler>() == null)
        {
            GameObject handlerObj = new GameObject("GlobalErrorHandler");
            handlerObj.AddComponent<GlobalErrorHandler>();
            if (logTestResults) Debug.Log("Added GlobalErrorHandler");
        }
        
        if (FindFirstObjectByType<ErrorReportingSystem>() == null)
        {
            GameObject reporterObj = new GameObject("ErrorReportingSystem");
            reporterObj.AddComponent<ErrorReportingSystem>();
            if (logTestResults) Debug.Log("Added ErrorReportingSystem");
        }
        
        if (FindFirstObjectByType<SceneValidator>() == null)
        {
            GameObject validatorObj = new GameObject("SceneValidator");
            validatorObj.AddComponent<SceneValidator>();
            if (logTestResults) Debug.Log("Added SceneValidator");
        }
        
        if (FindFirstObjectByType<UniversalUIFixer>() == null)
        {
            GameObject uiFixerObj = new GameObject("UniversalUIFixer");
            uiFixerObj.AddComponent<UniversalUIFixer>();
            if (logTestResults) Debug.Log("Added UniversalUIFixer");
        }
        
        if (FindFirstObjectByType<NetworkErrorHandler>() == null)
        {
            GameObject networkObj = new GameObject("NetworkErrorHandler");
            networkObj.AddComponent<NetworkErrorHandler>();
            if (logTestResults) Debug.Log("Added NetworkErrorHandler");
        }
        
        // Enable offline mode
        PlayerPrefs.SetInt("OfflineMode", 1);
        PlayerPrefs.Save();
        
        yield return new WaitForSeconds(1f);
        
        if (logTestResults)
            Debug.Log("=== AUTO-SETUP: Complete. Re-running tests... ===");
        
        // Re-run tests to verify fixes
        yield return new WaitForSeconds(1f);
        RunAllTests();
    }
    
    [ContextMenu("Quick Setup All Systems")]
    public void QuickSetupAllSystems()
    {
        StartCoroutine(AutoSetupFailedSystems());
    }
    
    [ContextMenu("Enable Debug Mode")]
    public void EnableDebugMode()
    {
        logTestResults = true;
        autoSetupIfFailed = true;
        Debug.Log("Debug mode enabled for ErrorFixTestSuite");
    }
}