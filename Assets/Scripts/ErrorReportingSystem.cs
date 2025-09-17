using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;

/// <summary>
/// Comprehensive error reporting and logging system
/// Tracks, logs, and provides solutions for Unity errors
/// </summary>
public class ErrorReportingSystem : MonoBehaviour
{
    [Header("Logging Settings")]
    public bool enableErrorLogging = true;
    public bool saveLogsToFile = true;
    public bool showErrorSolutions = true;
    public int maxLogEntries = 1000;
    
    [Header("File Settings")]
    public string logFileName = "unity_error_log.txt";
    public bool includeDateInFileName = true;
    
    private List<ErrorEntry> errorLog = new List<ErrorEntry>();
    private Dictionary<string, string> errorSolutions = new Dictionary<string, string>();
    private string logFilePath;
    
    [System.Serializable]
    public class ErrorEntry
    {
        public string timestamp;
        public string errorType;
        public string message;
        public string stackTrace;
        public string sceneName;
        public string suggestedSolution;
        
        public ErrorEntry(string type, string msg, string stack, string solution = "")
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            errorType = type;
            message = msg;
            stackTrace = stack;
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            suggestedSolution = solution;
        }
    }
    
    void Awake()
    {
        InitializeErrorSolutions();
        InitializeLogFile();
        
        if (enableErrorLogging)
        {
            Application.logMessageReceived += OnLogMessageReceived;
        }
    }
    
    void OnDestroy()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
        
        if (saveLogsToFile)
        {
            SaveLogToFile();
        }
    }
    
    void InitializeErrorSolutions()
    {
        errorSolutions = new Dictionary<string, string>
        {
            // Missing script solutions
            { "missing script", "Use Tools → Fix Missing Scripts in the Unity Editor, or add MasterErrorFixer component to scene" },
            { "referenced script (unknown)", "Remove missing script references using EditorMissingScriptsFixer" },
            
            // UI reference solutions
            { "classNameText is not assigned", "Use UniversalUIFixer to auto-assign UI references, or manually assign in inspector" },
            { "is not assigned", "Check component references in inspector or use auto-assignment tools" },
            
            // Network solutions
            { "failed to send data to flask", "Enable offline mode: PlayerPrefs.SetInt('OfflineMode', 1)" },
            { "404 not found", "Server may be down. Enable offline mode to continue without network features" },
            { "connection error", "Check internet connection or enable offline mode" },
            
            // Null reference solutions
            { "nullreferenceexception", "Check for null object references. Use null checks before accessing objects" },
            { "object reference not set", "Initialize objects before use or add null checks" },
            
            // Component solutions
            { "component not found", "Ensure required components are attached to GameObjects" },
            { "missing component", "Add required components using AddComponent() or manually in inspector" },
            
            // Scene solutions
            { "scene not found", "Check scene is added to Build Settings" },
            { "cannot load scene", "Verify scene path and build settings" }
        };
    }
    
    void InitializeLogFile()
    {
        if (!saveLogsToFile) return;
        
        string fileName = logFileName;
        if (includeDateInFileName)
        {
            string dateStr = DateTime.Now.ToString("yyyyMMdd");
            fileName = Path.GetFileNameWithoutExtension(logFileName) + "_" + dateStr + Path.GetExtension(logFileName);
        }
        
        logFilePath = Path.Combine(Application.persistentDataPath, fileName);
        
        // Create log file header
        try
        {
            StringBuilder header = new StringBuilder();
            header.AppendLine("=== UNITY ERROR LOG ===");
            header.AppendLine($"Application: {Application.productName}");
            header.AppendLine($"Version: {Application.version}");
            header.AppendLine($"Unity Version: {Application.unityVersion}");
            header.AppendLine($"Platform: {Application.platform}");
            header.AppendLine($"Started: {DateTime.Now}");
            header.AppendLine("========================");
            header.AppendLine();
            
            File.WriteAllText(logFilePath, header.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to create log file: {e.Message}");
        }
    }
    
    void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (!enableErrorLogging) return;
        
        // Only log errors and warnings
        if (type != LogType.Error && type != LogType.Exception && type != LogType.Warning)
            return;
        
        string solution = FindSolution(logString);
        ErrorEntry entry = new ErrorEntry(type.ToString(), logString, stackTrace, solution);
        
        // Add to memory log
        errorLog.Add(entry);
        
        // Limit log size
        if (errorLog.Count > maxLogEntries)
        {
            errorLog.RemoveAt(0);
        }
        
        // Show solution if available
        if (showErrorSolutions && !string.IsNullOrEmpty(solution))
        {
            Debug.Log($"💡 Solution: {solution}");
        }
        
        // Write to file immediately for critical errors
        if (type == LogType.Error || type == LogType.Exception)
        {
            WriteEntryToFile(entry);
        }
    }
    
    string FindSolution(string errorMessage)
    {
        string lowerError = errorMessage.ToLower();
        
        foreach (var kvp in errorSolutions)
        {
            if (lowerError.Contains(kvp.Key.ToLower()))
            {
                return kvp.Value;
            }
        }
        
        // Generic solutions based on error patterns
        if (lowerError.Contains("null"))
        {
            return "Check for null references and add null checks before accessing objects";
        }
        
        if (lowerError.Contains("network") || lowerError.Contains("http"))
        {
            return "Check network connection or enable offline mode";
        }
        
        if (lowerError.Contains("component"))
        {
            return "Check component references and ensure all required components are attached";
        }
        
        return "";
    }
    
    void WriteEntryToFile(ErrorEntry entry)
    {
        if (!saveLogsToFile || string.IsNullOrEmpty(logFilePath)) return;
        
        try
        {
            StringBuilder logEntry = new StringBuilder();
            logEntry.AppendLine($"[{entry.timestamp}] {entry.errorType} in {entry.sceneName}");
            logEntry.AppendLine($"Message: {entry.message}");
            
            if (!string.IsNullOrEmpty(entry.suggestedSolution))
            {
                logEntry.AppendLine($"Solution: {entry.suggestedSolution}");
            }
            
            if (!string.IsNullOrEmpty(entry.stackTrace))
            {
                logEntry.AppendLine("Stack Trace:");
                logEntry.AppendLine(entry.stackTrace);
            }
            
            logEntry.AppendLine(new string('-', 50));
            logEntry.AppendLine();
            
            File.AppendAllText(logFilePath, logEntry.ToString());
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }
    
    void SaveLogToFile()
    {
        if (!saveLogsToFile || errorLog.Count == 0) return;
        
        try
        {
            StringBuilder fullLog = new StringBuilder();
            fullLog.AppendLine("=== COMPLETE ERROR LOG ===");
            fullLog.AppendLine($"Total Entries: {errorLog.Count}");
            fullLog.AppendLine($"Session End: {DateTime.Now}");
            fullLog.AppendLine();
            
            foreach (var entry in errorLog)
            {
                fullLog.AppendLine($"[{entry.timestamp}] {entry.errorType} in {entry.sceneName}");
                fullLog.AppendLine($"Message: {entry.message}");
                
                if (!string.IsNullOrEmpty(entry.suggestedSolution))
                {
                    fullLog.AppendLine($"Solution: {entry.suggestedSolution}");
                }
                
                fullLog.AppendLine(new string('-', 30));
                fullLog.AppendLine();
            }
            
            File.AppendAllText(logFilePath, fullLog.ToString());
            Debug.Log($"Error log saved to: {logFilePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save complete log: {e.Message}");
        }
    }
    
    [ContextMenu("Generate Error Report")]
    public void GenerateErrorReport()
    {
        StringBuilder report = new StringBuilder();
        report.AppendLine("=== ERROR REPORT ===");
        report.AppendLine($"Generated: {DateTime.Now}");
        report.AppendLine($"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        report.AppendLine($"Total Errors Logged: {errorLog.Count}");
        report.AppendLine();
        
        // Count error types
        Dictionary<string, int> errorCounts = new Dictionary<string, int>();
        foreach (var entry in errorLog)
        {
            if (errorCounts.ContainsKey(entry.errorType))
                errorCounts[entry.errorType]++;
            else
                errorCounts[entry.errorType] = 1;
        }
        
        report.AppendLine("Error Type Summary:");
        foreach (var kvp in errorCounts)
        {
            report.AppendLine($"  {kvp.Key}: {kvp.Value}");
        }
        
        report.AppendLine();
        report.AppendLine("Recent Errors (Last 10):");
        
        int startIndex = Mathf.Max(0, errorLog.Count - 10);
        for (int i = startIndex; i < errorLog.Count; i++)
        {
            var entry = errorLog[i];
            report.AppendLine($"  [{entry.timestamp}] {entry.errorType}: {entry.message}");
            if (!string.IsNullOrEmpty(entry.suggestedSolution))
            {
                report.AppendLine($"    💡 {entry.suggestedSolution}");
            }
        }
        
        Debug.Log(report.ToString());
    }
    
    [ContextMenu("Clear Error Log")]
    public void ClearErrorLog()
    {
        errorLog.Clear();
        Debug.Log("Error log cleared");
    }
    
    [ContextMenu("Open Log File")]
    public void OpenLogFile()
    {
        if (File.Exists(logFilePath))
        {
            Application.OpenURL("file://" + logFilePath);
        }
        else
        {
            Debug.LogWarning("Log file not found: " + logFilePath);
        }
    }
    
    public void AddCustomError(string message, string solution = "")
    {
        ErrorEntry entry = new ErrorEntry("Custom", message, "", solution);
        errorLog.Add(entry);
        
        if (showErrorSolutions && !string.IsNullOrEmpty(solution))
        {
            Debug.Log($"💡 Solution: {solution}");
        }
    }
    
    public List<ErrorEntry> GetRecentErrors(int count = 10)
    {
        int startIndex = Mathf.Max(0, errorLog.Count - count);
        return errorLog.GetRange(startIndex, errorLog.Count - startIndex);
    }
    
    public int GetErrorCount(string errorType = "")
    {
        if (string.IsNullOrEmpty(errorType))
            return errorLog.Count;
        
        int count = 0;
        foreach (var entry in errorLog)
        {
            if (entry.errorType.Equals(errorType, StringComparison.OrdinalIgnoreCase))
                count++;
        }
        
        return count;
    }
}