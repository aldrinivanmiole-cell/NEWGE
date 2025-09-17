using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

/// <summary>
/// Manages teacher assignments and student assignment loading
/// </summary>
public class AssignmentManager : MonoBehaviour
{
    [Header("Assignment Configuration")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    
    private static AssignmentManager instance;
    public static AssignmentManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AssignmentManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("AssignmentManager");
                    instance = go.AddComponent<AssignmentManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Check for active assignments when the game starts
        CheckForActiveAssignments();
    }

    /// <summary>
    /// Set a teacher assignment - DYNAMIC VERSION that supports multiple assignments per subject
    /// This would typically be called from the teacher's interface
    /// assignmentType examples: "MultipleChoice", "Enumeration", "FillInBlank", "YesNo"
    /// </summary>
    public void SetTeacherAssignment(string subject, string assignmentId, string assignmentTitle, string assignmentContent = "", string assignmentType = "")
    {
        Debug.Log($"📝 DYNAMIC: Setting teacher assignment: {subject} - {assignmentTitle} (ID: {assignmentId})");
        
        string keySubject = NormalizeSubjectKey(subject);
        if (string.IsNullOrEmpty(keySubject))
        {
            Debug.LogError($"❌ Invalid subject key for: {subject}");
            return;
        }

        // Find the next available assignment slot for this subject
        int assignmentIndex = GetNextAssignmentIndex(keySubject);
        
        Debug.Log($"🔢 Next assignment index for {subject}: {assignmentIndex}");
        
        // Store assignment with indexed keys - DYNAMIC STORAGE
        string assignmentKey = $"Assignment_{keySubject}_{assignmentIndex}";
        PlayerPrefs.SetString($"{assignmentKey}_Id", assignmentId);
        PlayerPrefs.SetString($"{assignmentKey}_Title", assignmentTitle);
        PlayerPrefs.SetString($"{assignmentKey}_Subject", subject);
        PlayerPrefs.SetString($"{assignmentKey}_Content", assignmentContent);
        PlayerPrefs.SetString($"{assignmentKey}_CreatedTime", System.DateTime.Now.ToString());
        if (!string.IsNullOrEmpty(assignmentType))
            PlayerPrefs.SetString($"{assignmentKey}_Type", assignmentType);
            
        // Update assignment count for this subject
        PlayerPrefs.SetInt($"AssignmentCount_{keySubject}", assignmentIndex);
        
        Debug.Log($"✅ DYNAMIC: Assignment stored as {assignmentKey}");
        Debug.Log($"📊 Total assignments for {subject}: {assignmentIndex}");
        
        // Also maintain backward compatibility with single assignment keys for currently active one
        PlayerPrefs.SetString("ActiveAssignmentSubject", subject);
        PlayerPrefs.SetString("ActiveAssignmentId", assignmentId);
        PlayerPrefs.SetString("ActiveAssignmentTitle", assignmentTitle);
        PlayerPrefs.SetString("ActiveAssignmentContent", assignmentContent);
        if (!string.IsNullOrEmpty(assignmentType))
            PlayerPrefs.SetString("ActiveAssignmentType", assignmentType);
        
        PlayerPrefs.Save();
        
        // Send assignment creation to Flask
        StartCoroutine(SendAssignmentToFlask(subject, assignmentId, assignmentTitle, assignmentContent));
    }

    /// <summary>
    /// Clear active teacher assignment
    /// </summary>
    public void ClearTeacherAssignment()
    {
        Debug.Log("Clearing active teacher assignment");
        
        PlayerPrefs.DeleteKey("ActiveAssignmentSubject");
        PlayerPrefs.DeleteKey("ActiveAssignmentId");
        PlayerPrefs.DeleteKey("ActiveAssignmentTitle");
        PlayerPrefs.DeleteKey("ActiveAssignmentContent");
        PlayerPrefs.DeleteKey("AssignmentCreatedTime");
    }

    /// <summary>
    /// Check if there are any active assignments from the server
    /// </summary>
    public void CheckForActiveAssignments()
    {
        StartCoroutine(FetchActiveAssignments());
    }

    /// <summary>
    /// Get current active assignment info
    /// </summary>
    public string GetActiveAssignmentInfo()
    {
        string subject = PlayerPrefs.GetString("ActiveAssignmentSubject", "");
        string title = PlayerPrefs.GetString("ActiveAssignmentTitle", "");
        string id = PlayerPrefs.GetString("ActiveAssignmentId", "");
        
        if (string.IsNullOrEmpty(subject))
            return "No active assignment";
        
        return $"{subject}: {title} (ID: {id})";
    }

    private IEnumerator SendAssignmentToFlask(string subject, string assignmentId, string assignmentTitle, string assignmentContent)
    {
        string url = flaskURL + "/api/teacher_assignment";
        
        // Get student info
        int studentId = PlayerPrefs.GetInt("StudentID", 1);
        string studentName = PlayerPrefs.GetString("LoggedInUser", "");
        
        // Create JSON data
        string jsonData = "{" +
            "\"student_id\":" + studentId + "," +
            "\"student_name\":\"" + studentName + "\"," +
            "\"subject\":\"" + subject + "\"," +
            "\"assignment_id\":\"" + assignmentId + "\"," +
            "\"assignment_title\":\"" + assignmentTitle + "\"," +
            "\"assignment_content\":\"" + assignmentContent + "\"," +
            "\"action\":\"set_assignment\"" +
            "}";
        
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Assignment sent to Flask successfully");
        }
        else
        {
            Debug.LogWarning($"Failed to send assignment to Flask: {request.error}");
        }
        
        request.Dispose();
    }

    private IEnumerator FetchActiveAssignments()
    {
        string url = flaskURL + "/api/get_active_assignments";
        
        // Get student info
        int studentId = PlayerPrefs.GetInt("StudentID", 1);
        
        string requestUrl = url + "?student_id=" + studentId;
        
        UnityWebRequest request = UnityWebRequest.Get(requestUrl);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success && request.responseCode == 200)
        {
            try
            {
                string response = request.downloadHandler.text;
                Debug.Log($"Active assignments response: {response}");
                
                // Parse the response and set active assignment if found
                // This is a lightweight implementation to avoid tight schema coupling
                if (!string.IsNullOrEmpty(response) && response != "null" && response != "{}")
                {
                    bool hasSubject = response.Contains("\"subject\"");
                    bool hasId = response.Contains("\"assignment_id\"");
                    bool hasTitle = response.Contains("\"assignment_title\"") || response.Contains("\"title\"");
                    bool hasType = response.Contains("\"assignment_type\"") || response.Contains("\"type\"");

                    if (hasSubject && hasId)
                    {
                        string subject = ExtractJsonValue(response, "subject");
                        string assignmentId = ExtractJsonValue(response, "assignment_id");
                        string assignmentTitle = ExtractJsonValue(response, "assignment_title");
                        if (string.IsNullOrEmpty(assignmentTitle))
                            assignmentTitle = ExtractJsonValue(response, "title");
                        if (string.IsNullOrEmpty(assignmentTitle))
                            assignmentTitle = "Teacher Assignment";
                        string assignmentType = hasType ? ExtractJsonValue(response, "assignment_type") : "";
                        if (string.IsNullOrEmpty(assignmentType))
                            assignmentType = ExtractJsonValue(response, "type");

                        if (!string.IsNullOrEmpty(subject) && !string.IsNullOrEmpty(assignmentId))
                        {
                            Debug.Log($"Found active assignment from server: {subject} - {assignmentTitle} ({assignmentId})");
                            PlayerPrefs.SetString("ActiveAssignmentSubject", subject);
                            PlayerPrefs.SetString("ActiveAssignmentId", assignmentId);
                            PlayerPrefs.SetString("ActiveAssignmentTitle", assignmentTitle);
                            PlayerPrefs.SetString("AssignmentCreatedTime", System.DateTime.Now.ToString());
                            if (!string.IsNullOrEmpty(assignmentType))
                                PlayerPrefs.SetString("ActiveAssignmentType", assignmentType);
                            // Store per-subject copies
                            string keySubject = NormalizeSubjectKey(subject);
                            if (!string.IsNullOrEmpty(keySubject))
                            {
                                PlayerPrefs.SetString($"ActiveAssignment_{keySubject}_Subject", subject);
                                PlayerPrefs.SetString($"ActiveAssignment_{keySubject}_Id", assignmentId);
                                PlayerPrefs.SetString($"ActiveAssignment_{keySubject}_Title", assignmentTitle);
                                if (!string.IsNullOrEmpty(assignmentType))
                                    PlayerPrefs.SetString($"ActiveAssignment_{keySubject}_Type", assignmentType);
                            }
                            PlayerPrefs.Save();
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing active assignments: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Failed to fetch active assignments: {(int)request.responseCode} {request.error}");
        }
        
        request.Dispose();
    }

    // Lightweight JSON value extractor (non-nested, string values)
    string ExtractJsonValue(string json, string key)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(key)) return "";
        string pattern = "\"" + key + "\"";
        int idx = json.IndexOf(pattern);
        if (idx < 0) return "";
        int colon = json.IndexOf(':', idx + pattern.Length);
        if (colon < 0) return "";
        int quoteStart = json.IndexOf('"', colon + 1);
        if (quoteStart < 0) return "";
        int quoteEnd = json.IndexOf('"', quoteStart + 1);
        if (quoteEnd < 0) return "";
        return json.Substring(quoteStart + 1, quoteEnd - quoteStart - 1);
    }

    /// <summary>
    /// Get the next available assignment index for a subject (dynamic assignment support)
    /// </summary>
    private int GetNextAssignmentIndex(string keySubject)
    {
        int currentCount = PlayerPrefs.GetInt($"AssignmentCount_{keySubject}", 0);
        return currentCount + 1;
    }

    /// <summary>
    /// Get all assignments for a specific subject - DYNAMIC VERSION
    /// </summary>
    public List<AssignmentInfo> GetAssignmentsForSubject(string subject)
    {
        List<AssignmentInfo> assignments = new List<AssignmentInfo>();
        string keySubject = NormalizeSubjectKey(subject);
        if (string.IsNullOrEmpty(keySubject)) return assignments;
        
        int assignmentCount = PlayerPrefs.GetInt($"AssignmentCount_{keySubject}", 0);
        
        for (int i = 1; i <= assignmentCount; i++)
        {
            string assignmentKey = $"Assignment_{keySubject}_{i}";
            string id = PlayerPrefs.GetString($"{assignmentKey}_Id", "");
            string title = PlayerPrefs.GetString($"{assignmentKey}_Title", "");
            string content = PlayerPrefs.GetString($"{assignmentKey}_Content", "");
            string type = PlayerPrefs.GetString($"{assignmentKey}_Type", "");
            
            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(title))
            {
                assignments.Add(new AssignmentInfo
                {
                    id = id,
                    title = title,
                    subject = subject,
                    content = content,
                    type = type,
                    index = i
                });
            }
        }
        
        Debug.Log($"📚 DYNAMIC: Found {assignments.Count} assignments for {subject}");
        return assignments;
    }

    /// <summary>
    /// Set a specific assignment as active for gameplay
    /// </summary>
    public void SetActiveAssignment(string subject, int assignmentIndex)
    {
        string keySubject = NormalizeSubjectKey(subject);
        if (string.IsNullOrEmpty(keySubject)) return;
        
        string assignmentKey = $"Assignment_{keySubject}_{assignmentIndex}";
        string id = PlayerPrefs.GetString($"{assignmentKey}_Id", "");
        string title = PlayerPrefs.GetString($"{assignmentKey}_Title", "");
        string content = PlayerPrefs.GetString($"{assignmentKey}_Content", "");
        string type = PlayerPrefs.GetString($"{assignmentKey}_Type", "");
        
        if (!string.IsNullOrEmpty(id))
        {
            PlayerPrefs.SetString("CurrentAssignmentId", id);
            PlayerPrefs.SetString("CurrentAssignmentTitle", title);
            PlayerPrefs.SetString("CurrentAssignmentContent", content);
            PlayerPrefs.SetString("CurrentSubject", subject);
            PlayerPrefs.SetString("AssignmentSource", "teacher");
            if (!string.IsNullOrEmpty(type))
                PlayerPrefs.SetString("CurrentGameplayType", type);
                
            PlayerPrefs.Save();
            Debug.Log($"🎯 DYNAMIC: Set active assignment: {title} (Index {assignmentIndex}) for {subject}");
        }
    }

    /// <summary>
    /// Assignment info structure
    /// </summary>
    [System.Serializable]
    public class AssignmentInfo
    {
        public string id;
        public string title;
        public string subject;
        public string content;
        public string type;
        public int index;
    }

    string NormalizeSubjectKey(string subject)
    {
        if (string.IsNullOrEmpty(subject)) return "";
        string s = subject.Trim().ToUpperInvariant();
        if (s.Contains("MATH")) return "MATH";
        if (s.Contains("SCI")) return "SCIENCE";
        if (s.Contains("ENG")) return "ENGLISH";
        if (s == "PE" || s.Contains("PHYSICAL") || s.Contains("ED")) return "PE";
        if (s.Contains("ART")) return "ART";
        return s.Replace(' ', '_');
    }

    /// <summary>
    /// Demo assignments are DISABLED - all content must come from web app
    /// </summary>
    [System.Obsolete("Hardcoded assignments are not allowed. All content must come from web app.")]
    [ContextMenu("Create Demo Assignment")]
    public void SetDemoAssignment()
    {
        Debug.LogError("❌ SetDemoAssignment is disabled!");
        Debug.LogError("📚 All assignments must come from the web app - no hardcoded content allowed!");
        return;
    }

    [ContextMenu("Clear All Math Assignments")]
    public void ClearMathAssignments()
    {
        string keySubject = NormalizeSubjectKey("Math");
        int count = PlayerPrefs.GetInt($"AssignmentCount_{keySubject}", 0);
        
        for (int i = 1; i <= count; i++)
        {
            string assignmentKey = $"Assignment_{keySubject}_{i}";
            PlayerPrefs.DeleteKey($"{assignmentKey}_Id");
            PlayerPrefs.DeleteKey($"{assignmentKey}_Title");
            PlayerPrefs.DeleteKey($"{assignmentKey}_Subject");
            PlayerPrefs.DeleteKey($"{assignmentKey}_Content");
            PlayerPrefs.DeleteKey($"{assignmentKey}_Type");
            PlayerPrefs.DeleteKey($"{assignmentKey}_CreatedTime");
        }
        
        PlayerPrefs.DeleteKey($"AssignmentCount_{keySubject}");
        PlayerPrefs.Save();
        
        Debug.Log($"✅ Cleared all Math assignments ({count} removed)");
    }
    public void SetTestAssignment()
    {
        SetTeacherAssignment("Science", "PLANT_QUIZ_001", "Quiz 1: Plants", "Learn about plant biology and photosynthesis");
    }

    /// <summary>
    /// Public method to clear assignment (for debugging)
    /// </summary>
    public void ClearTestAssignment()
    {
        ClearTeacherAssignment();
    }
}