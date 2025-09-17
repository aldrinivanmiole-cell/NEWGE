using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System;
using System.Linq;

public class GameMechanicDragButtons : MonoBehaviour
{
    [Header("Game Mode")]
    public GameMode gameMode = GameMode.InputField;
    public enum GameMode
    {
        DragAndDrop,
        InputField,
        MultipleChoice,
        Identification
    }

    [Header("Player & Enemy Transforms")]
    public RectTransform player;
    public RectTransform enemy1;
    public float attackOffset = 100f;
    public float movementSpeed = 2f;
    private Vector2 originalPos;

    [Header("Player & Enemy Sprites")]
    public Sprite maleSprite;
    public Sprite femaleSprite;
    public Sprite[] enemySprites;
    public Image playerImage;
    public Image enemyImage;

    [Header("Player Speech UI")]
    public GameObject playerSpeechBubble;
    public TMP_Text playerSpeechText;

    [Header("Enemy Speech UI")]
    public GameObject enemySpeechBubble;
    public TMP_Text enemySpeechText;

    [Header("UI Components")]
    public List<Button> answerButtons = new List<Button>();
    public TMP_InputField answerInputField;
    public Button submitAnswerButton;
    public TMP_Text questionText;
    public List<Toggle> answerToggles = new List<Toggle>();
    public Button submitToggleButton;
    public Slider progressBar;
    public TMP_Text textProgress;
    public GameObject resultPanel;

    [Header("Game Settings")]
    public float passingScorePercentage = 70f;
    public string currentQuestion = "Ready to start your assignment!";

    [Header("Flask Integration")]
    public string flaskURL = "https://homequest-c3k7.onrender.com";
    public bool sendToFlask = false;
    // Remove hardcoded studentId - use dynamic authentication
    public int assignmentId = 1;
    
    // Store fetched assignments for selection
    private Assignment[] fetchedAssignments;
    private string currentSubject;

    [Header("Class Code System")]
    public bool useClassCodeMode = true;
    public string currentClassCode = "";
    public string playerName = "Student1";
    public TMP_InputField classCodeInput;
    public Button joinClassButton;
    public GameObject classCodePanel;
    
    // Private variables
    private float progress = 0f;
    private float questionStartTime = 0f;
    private bool isProcessingAttack = false;
    private List<string> submittedAnswers = new List<string>();
    private HashSet<string> correctAnswers = new HashSet<string>();
    
    // Assignment management
    private int currentAssignmentIndex = 0;
    private AssignmentsResponse allAssignments;
    
    // Dynamic class and assignment data
    private List<AvailableClass> availableClasses;
    private AssignmentsResponse currentAssignments;
    
    /// <summary>
    /// Set the student ID for dynamic API calls. Call this at runtime to make the system truly dynamic.
    /// Example: SetDynamicStudentID(123);
    /// </summary>
    public void SetDynamicStudentID(int studentId)
    {
        PlayerPrefs.SetInt("DynamicStudentID", studentId);
        PlayerPrefs.Save();
        Debug.Log($"🆔 Dynamic Student ID set to: {studentId}");
    }
    
    /// <summary>
    /// Get the current dynamic student ID
    /// </summary>
    public int GetDynamicStudentID()
    {
        return PlayerPrefs.GetInt("DynamicStudentID", 1);
    }
    
    /// <summary>
    /// Test different student IDs to find one that works with the backend
    /// </summary>
    public void TestDifferentStudentIDs()
    {
        StartCoroutine(TryDifferentStudentIDs());
    }
    
    private IEnumerator TryDifferentStudentIDs()
    {
        int[] testIds = {1, 2, 3, 123, 456, 789}; // Common test IDs
        
        foreach (int testId in testIds)
        {
            Debug.Log($"🧪 Testing student ID: {testId}");
            SetDynamicStudentID(testId);
            
            // Test the subjects endpoint with this ID
            yield return StartCoroutine(TestSubjectsEndpoint(testId));
            yield return new WaitForSeconds(1f); // Wait between tests
        }
    }
    
    private IEnumerator TestSubjectsEndpoint(int studentId)
    {
        string url = flaskURL + "/student/subjects";
        var payload = new { student_id = studentId };
        string jsonPayload = JsonUtility.ToJson(payload);
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ SUCCESS with student ID {studentId}!");
                Debug.Log($"📋 Response: {request.downloadHandler.text}");
                
                // Set this as the working student ID
                SetDynamicStudentID(studentId);
                
                // Now load assignments with this working ID
                yield return StartCoroutine(GetAndUseFirstAvailableClass());
                yield break; // Stop testing, we found a working ID
            }
            else
            {
                Debug.Log($"❌ Failed with student ID {studentId}: {request.responseCode} - {request.downloadHandler?.text}");
            }
        }
    }
    
    /// <summary>
    /// <summary>
    /// This function has been removed to ensure all content comes from the web app.
    /// No hardcoded or static assignments are allowed.
    /// </summary>
    [System.Obsolete("Hardcoded assignments are not allowed. All content must come from web app.")]
    public void TestMultipleChoiceInterface()
    {
        Debug.LogError("❌ TestMultipleChoiceInterface is disabled!");
        Debug.LogError("📚 All assignments must come from the web app - no hardcoded content allowed!");
        ShowNoAssignmentsError("Unknown");
    }
    
    /// <summary>
    /// Called when student clicks on an assignment (like "English Assignment")
    /// This method handles the loading state and fetches teacher-created content
    /// </summary>
    public void OnAssignmentButtonPressed(string subject)
    {
        Debug.Log($"🎯 Assignment button pressed for subject: {subject}");
        currentSubject = subject;
        StartCoroutine(LoadDynamicAssignment(subject));
    }
    
    /// <summary>
    /// Called when a specific assignment is selected (with assignment ID)
    /// </summary>
    public void OnSpecificAssignmentPressed(int assignmentId)
    {
        Debug.Log($"🎯 Specific assignment pressed: Assignment {assignmentId} for {currentSubject}");
        
        if (fetchedAssignments != null && assignmentId < fetchedAssignments.Length)
        {
            LoadSpecificAssignment(fetchedAssignments[assignmentId]);
        }
        else
        {
            Debug.LogError($"❌ Assignment {assignmentId} not found in fetched assignments");
            ShowNoAssignmentsError(currentSubject);
        }
    }
    
    /// <summary>
    /// Load dynamic teacher assignment with loading state
    /// </summary>
    private IEnumerator LoadDynamicAssignment(string subject)
    {
        // Show loading state
        ShowLoadingAssignment();
        
        // Get current class code (should be saved from previous class join)
        string classCode = PlayerPrefs.GetString("CurrentClassCode", "");
        
        if (string.IsNullOrEmpty(classCode))
        {
            Debug.LogError("❌ No class code found! Cannot load assignments without a class code.");
            Debug.LogError("📚 Students must join a class to access assignments.");
            ShowNoAssignmentsError(subject);
            yield break;
        }
        
        Debug.Log($"📚 Loading {subject} assignment for student ID: {GetDynamicStudentID()}");
        
        // Fetch assignments from server
        yield return StartCoroutine(FetchTeacherAssignment(subject));
    }
    
    /// <summary>
    /// Display error message when no assignments are available
    /// </summary>
    private void ShowNoAssignmentsError(string subject)
    {
        Debug.LogError($"❌ No assignments found for {subject}!");
        Debug.LogError("📚 Teachers must create assignments in the web app first.");
        
        if (questionText != null)
        {
            questionText.text = $"No {subject} assignments available.\n\nPlease contact your teacher to create assignments for this subject.";
        }
        
        // Hide all answer UI
        HideAllAnswerUI();
    }
    
    /// <summary>
    /// Validate that assignment is from web app (not hardcoded)
    /// </summary>
    private bool ValidateAssignmentIsFromWebApp(ClassAssignment assignment)
    {
        if (assignment == null)
        {
            Debug.LogError("❌ VALIDATION FAILED: Assignment is null!");
            return false;
        }
        
        // Check for hardcoded/static content patterns
        string title = assignment.assignmentTitle?.ToLower() ?? "";
        
        if (title.Contains("demo") || title.Contains("sample") || title.Contains("test") || title.Contains("hardcoded"))
        {
            Debug.LogError($"❌ VALIDATION FAILED: Assignment '{assignment.assignmentTitle}' appears to be hardcoded!");
            Debug.LogError("📚 All assignments must come from the web app!");
            return false;
        }
        
        Debug.Log($"✅ VALIDATION PASSED: Assignment '{assignment.assignmentTitle}' appears to be from web app");
        return true;
    }
    
    /// <summary>
    /// Hide all answer-related UI elements
    /// </summary>
    private void HideAllAnswerUI()
    {
        // Hide multiple choice buttons
        if (answerButtons != null)
        {
            foreach (var button in answerButtons)
            {
                if (button != null)
                    button.gameObject.SetActive(false);
            }
        }
        
        // Hide input field
        if (answerInputField != null)
            answerInputField.gameObject.SetActive(false);
            
        // Hide submit button
        if (submitAnswerButton != null)
            submitAnswerButton.gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Show "Loading Assignment..." state
    /// </summary>
    private void ShowLoadingAssignment()
    {
        // Hide class code UI
        if (classCodePanel != null)
            classCodePanel.SetActive(false);
        if (classCodeInput != null)
            classCodeInput.gameObject.SetActive(false);
        if (joinClassButton != null)
            joinClassButton.gameObject.SetActive(false);
            
        // Show loading message
        if (questionText != null)
        {
            questionText.text = "📖 Loading Assignment...\nPlease wait...";
        }
        
        // Hide answer buttons during loading
        foreach (var button in answerButtons)
        {
            if (button != null)
                button.gameObject.SetActive(false);
        }
        
        Debug.Log("⏳ Showing loading assignment state");
    }
    
    /// <summary>
    /// Fetch actual teacher assignment from server
    /// </summary>
    private IEnumerator FetchTeacherAssignment(string subject)
    {
        Debug.Log($"🔍 FetchTeacherAssignment called with subject: '{subject}'");
        Debug.Log($"🔍 Current subject field: '{currentSubject}'");
        
        string url = $"{flaskURL}/student/assignments";
        
        // Get the dynamic student ID
        int studentId = GetDynamicStudentID();
        
        var payload = new AssignmentApiPayload
        {
            student_id = studentId,
            subject = subject
        };
        
        string jsonPayload = JsonUtility.ToJson(payload);
        Debug.Log($"📡 Fetching assignment for student {studentId}: {jsonPayload}");
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Assignment fetched successfully!");
                Debug.Log($"📄 Response: {request.downloadHandler.text}");
                
                try
                {
                    // Parse the response
                    AssignmentsResponse response = JsonUtility.FromJson<AssignmentsResponse>(request.downloadHandler.text);
                    
                    if (response != null && response.assignments != null && response.assignments.Length > 0)
                    {
                        Debug.Log($"🎓 Found {response.assignments.Length} teacher assignments");
                        
                        // Log all assignments for debugging
                        for (int i = 0; i < response.assignments.Length; i++)
                        {
                            var assignment = response.assignments[i];
                            Debug.Log($"📋 Assignment {i + 1}: '{assignment.title}' (ID: {assignment.assignment_id}) - Due: {assignment.due_date} - Created by: {assignment.created_by}");
                        }
                        
                        // Sort assignments by assignment_id in descending order (newest first)
                        // This assumes higher assignment IDs are newer assignments
                        var sortedAssignments = new Assignment[response.assignments.Length];
                        System.Array.Copy(response.assignments, sortedAssignments, response.assignments.Length);
                        System.Array.Sort(sortedAssignments, (a, b) => b.assignment_id.CompareTo(a.assignment_id));
                        
                        Debug.Log("📅 Assignments sorted by ID (newest first):");
                        for (int i = 0; i < sortedAssignments.Length; i++)
                        {
                            Debug.Log($"   {i + 1}. '{sortedAssignments[i].title}' (ID: {sortedAssignments[i].assignment_id})");
                        }
                        
                        // Store sorted assignments for selection
                        fetchedAssignments = sortedAssignments;
                        
                        // If only one assignment, load it directly
                        if (sortedAssignments.Length == 1)
                        {
                            Debug.Log("📋 Only one assignment found, loading directly...");
                            LoadSpecificAssignment(sortedAssignments[0]);
                        }
                        else
                        {
                            // Multiple assignments - show selection interface
                            Debug.Log("📋 Multiple assignments found, showing selection...");
                            ShowAssignmentSelection(sortedAssignments);
                        }
                    }
                    else
                    {
                        Debug.LogError("❌ No assignments found for this subject");
                        Debug.LogError("📚 Teachers must create assignments for this subject in the web app.");
                        ShowNoAssignmentsError(subject);
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"❌ Failed to parse assignment response: {e.Message}");
                    Debug.LogError("📚 Cannot load assignments - API response format error.");
                    ShowNoAssignmentsError(subject);
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to fetch assignment: {request.error}");
                Debug.LogError("📚 Cannot connect to assignment server.");
                ShowNoAssignmentsError(subject);
            }
        }
    }
    
    /// <summary>
    /// Helper method to find the index of the correct answer in the options array
    /// </summary>
    private int FindCorrectAnswerIndex(string[] options, string correctAnswer)
    {
        for (int i = 0; i < options.Length; i++)
        {
            if (options[i].Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        
        Debug.LogWarning($"⚠️ Correct answer '{correctAnswer}' not found in options. Defaulting to index 0.");
        return 0; // Default to first option if not found
    }
    
    /// <summary>
    /// Load a specific assignment by converting it to WebApp format
    /// </summary>
    private void LoadSpecificAssignment(Assignment assignment)
    {
        Debug.Log($"📋 Loading specific assignment: {assignment.title} (ID: {assignment.assignment_id})");
        
        // Set the current assignment ID for result submission
        assignmentId = assignment.assignment_id;
        
        // Convert teacher assignment to web app format
        var webAssignment = new WebAppAssignment
        {
            title = assignment.title,
            questions = new WebAppQuestion[assignment.questions.Length]
        };
        
        for (int i = 0; i < assignment.questions.Length; i++)
        {
            var q = assignment.questions[i];
            webAssignment.questions[i] = new WebAppQuestion
            {
                question = q.question_text,
                options = q.options,
                correct_answer = FindCorrectAnswerIndex(q.options, q.correct_answer)
            };
        }
        
        // Apply the assignment
        ApplyAssignment(webAssignment);
        Debug.Log($"✅ Assignment '{assignment.title}' loaded successfully with ID {assignmentId}!");
    }
    
    /// <summary>
    /// Show assignment selection interface when multiple assignments are available
    /// </summary>
    private void ShowAssignmentSelection(Assignment[] assignments)
    {
        Debug.Log($"📋 Showing selection for {assignments.Length} assignments");
        
        // Show assignment list in the question text area
        string assignmentList = "📚 Available Assignments (Newest First):\n\n";
        for (int i = 0; i < assignments.Length; i++)
        {
            assignmentList += $"{i + 1}. {assignments[i].title}\n";
            assignmentList += $"   📅 Due: {assignments[i].due_date}\n";
            assignmentList += $"   👩‍🏫 Created by: {assignments[i].created_by}\n";
            assignmentList += $"   🆔 ID: {assignments[i].assignment_id}\n";
            assignmentList += $"   ❓ Questions: {assignments[i].questions.Length}\n\n";
        }
        assignmentList += "Click the answer buttons below to select:\n";
        assignmentList += "A = Assignment 1, B = Assignment 2, C = Assignment 3, D = Assignment 4";
        
        // Display selection interface
        if (questionText != null)
        {
            questionText.text = assignmentList;
        }
        
        // Set up answer buttons for assignment selection
        SetupAssignmentSelectionButtons(assignments);
        
        Debug.Log("✅ Assignment selection interface ready!");
    }
    
    /// <summary>
    /// Set up answer buttons to select assignments
    /// </summary>
    private void SetupAssignmentSelectionButtons(Assignment[] assignments)
    {
        // Find and setup answer buttons
        FindUIComponents();
        
        for (int i = 0; i < answerButtons.Count && i < assignments.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].gameObject.SetActive(true);
                
                // Update button text
                var buttonText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    buttonText.text = $"{(char)('A' + i)}. {assignments[i].title}";
                }
                
                // Store assignment index in button
                int assignmentIndex = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnSpecificAssignmentPressed(assignmentIndex));
                
                Debug.Log($"🔘 Setup button {i} for assignment: {assignments[i].title}");
            }
        }
        
        // Hide unused buttons
        for (int i = assignments.Length; i < answerButtons.Count; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Dynamic method to load assignments for any subject
    /// Use this instead of hardcoded LoadEnglishAssignment, LoadMathAssignment, etc.
    /// Call this from UI buttons by passing the subject name as parameter
    /// </summary>
    public void LoadAssignmentForSubject(string subject)
    {
        Debug.Log($"🎯 Loading assignment for subject: {subject}");
        OnAssignmentButtonPressed(subject);
    }
    
    /// <summary>
    /// Load assignment using current subject (for UI buttons without parameters)
    /// </summary>
    public void LoadCurrentSubjectAssignment()
    {
        string subject = currentSubject;
        if (string.IsNullOrEmpty(subject))
        {
            // Try to get from available classes
            if (availableClasses != null && availableClasses.Count > 0)
            {
                subject = availableClasses[0].subject;
            }
            else
            {
                Debug.LogError("❌ Unable to determine subject from scene. Please ensure scene names match subject requirements.");
                return; // Exit early - no fallback subjects allowed
            }
        }
        
        Debug.Log($"🎯 Loading current subject assignment: {subject}");
        OnAssignmentButtonPressed(subject);
    }
    
    // Class code assignment
    private bool assignmentJoined = false;
    private ClassAssignment currentAssignment;
    private int currentQuestionIndex = 0;
    private List<QuestionResult> studentAnswers = new List<QuestionResult>();

    /// <summary>
    /// Check for assignments loaded by ClassCodeGate system
    /// This integrates with the existing class code infrastructure
    /// </summary>
    void CheckForClassCodeGateAssignments()
    {
        currentSubject = GetCurrentSubjectFromScene();
        string assignmentKey = $"Assignments_{currentSubject}";
        
        Debug.Log($"🔍 Checking for ClassCodeGate assignments with key: {assignmentKey}");
        Debug.Log($"🔍 Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        Debug.Log($"🔍 Detected subject: {currentSubject}");
        
        if (PlayerPrefs.HasKey(assignmentKey))
        {
            string assignmentData = PlayerPrefs.GetString(assignmentKey);
            Debug.Log($"✅ Found ClassCodeGate assignment data for {currentSubject}: {assignmentData}");
            
            try
            {
                // Try to parse as AssignmentsResponse (ClassCodeGate format)
                AssignmentsResponse response = JsonUtility.FromJson<AssignmentsResponse>(assignmentData);
                if (response != null && response.assignments != null && response.assignments.Length > 0)
                {
                    Debug.Log($"✅ Successfully loaded {response.assignments.Length} assignments from ClassCodeGate");
                    allAssignments = response;
                    LoadAssignmentByIndex(0); // Load first assignment
                    assignmentJoined = true;
                    useClassCodeMode = false; // Disable manual class code since we have assignments
                    return;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not parse ClassCodeGate assignment data: {e.Message}");
            }
        }
        else
        {
            Debug.Log($"❌ No PlayerPrefs key found: {assignmentKey}");
            
            // List all PlayerPrefs keys that start with "Assignments_"
            for (int i = 0; i < 10; i++)
            {
                string testKey = $"Assignments_MATH";
                if (PlayerPrefs.HasKey(testKey))
                {
                    Debug.Log($"📋 Found assignment key: {testKey}");
                }
            }
            
            // Also check for any class code related keys
            if (PlayerPrefs.HasKey("ClassCodeEntered"))
            {
                string classCode = PlayerPrefs.GetString("ClassCodeEntered");
                Debug.Log($"📋 Found entered class code: {classCode}");
            }
            
            if (PlayerPrefs.HasKey("JoinedClasses"))
            {
                string joinedClasses = PlayerPrefs.GetString("JoinedClasses");
                Debug.Log($"📋 Joined classes: {joinedClasses}");
            }
        }
        
        Debug.Log($"ℹ️ No ClassCodeGate assignments found for {currentSubject}, using manual class code mode");
    }
    
    /// <summary>
    /// Get current subject name from scene name or other indicators
    /// </summary>
    string GetCurrentSubjectFromScene()
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        
        // Map scene names to subject names
        if (sceneName.ToLower().Contains("math")) return "MATH";
        if (sceneName.ToLower().Contains("science")) return "SCI"; 
        if (sceneName.ToLower().Contains("english")) return "ENG";
        if (sceneName.ToLower().Contains("art")) return "ART";
        if (sceneName.ToLower().Contains("pe")) return "PE";
        
        // Default fallback - could also check for other indicators
        return "MATH";
    }
    
    /// <summary>
    /// Convert ClassCodeGate assignment format to internal game format
    /// </summary>
    void LoadAssignmentByIndex(int assignmentIndex)
    {
        if (allAssignments == null || allAssignments.assignments == null || 
            assignmentIndex >= allAssignments.assignments.Length)
        {
            Debug.LogError("Invalid assignment index or no assignments available");
            return;
        }
        
        Assignment serverAssignment = allAssignments.assignments[assignmentIndex];
        Debug.Log($"🎯 Loading assignment: {serverAssignment.title} with {serverAssignment.questions.Length} questions");
        
        // Convert to internal format
        currentAssignment = new ClassAssignment
        {
            classCode = "SERVER_LOADED", // Indicate this came from server
            assignmentTitle = serverAssignment.title,
            questions = new QuestionData[serverAssignment.questions.Length]
        };
        
        // Convert each question
        for (int i = 0; i < serverAssignment.questions.Length; i++)
        {
            var serverQ = serverAssignment.questions[i];
            
            var questionData = new QuestionData
            {
                questionId = serverQ.question_id,
                questionText = serverQ.question_text,
                questionType = serverQ.question_type,
                multipleChoiceOptions = new List<string>(),
                correctMultipleChoiceIndices = new List<int>(),
                correctAnswers = new List<string>()
            };
            
            // Add options
            for (int j = 0; j < serverQ.options.Length; j++)
            {
                questionData.multipleChoiceOptions.Add(serverQ.options[j]);
            }
            
            // Find correct answer index
            int correctIndex = FindCorrectAnswerIndex(serverQ.options, serverQ.correct_answer);
            questionData.correctMultipleChoiceIndices.Add(correctIndex);
            questionData.correctAnswers.Add(serverQ.correct_answer);
            
            currentAssignment.questions[i] = questionData;
            Debug.Log($"  Question {i + 1}: {serverQ.question_text} (Correct: {serverQ.correct_answer})");
        }
        
        // Reset progress and start the assignment
        currentQuestionIndex = 0;
        studentAnswers.Clear();
        
        // Start displaying the first question
        if (currentAssignment.questions.Length > 0)
        {
            StartAssignmentFlow();
        }
    }
    
    /// <summary>
    /// Start assignment flow with dynamic class code
    /// </summary>
    IEnumerator StartAssignmentFlow(string classCode)
    {
        Debug.Log($"🚀 Starting assignment flow for class: {classCode}");
        
        // First get dynamic class data to find the subject
        yield return StartCoroutine(GetDynamicClassData(flaskURL));
        
        if (availableClasses != null && availableClasses.Count > 0)
        {
            var targetClass = availableClasses.Find(c => c.class_code == classCode);
            if (targetClass != null)
            {
                Debug.Log($"🎯 Found class {classCode} with subject {targetClass.subject}");
                
                // Get assignments for this specific class
                yield return StartCoroutine(GetDynamicAssignments(flaskURL, classCode, targetClass.subject));
            }
            else
            {
                Debug.LogWarning($"⚠️ Class {classCode} not found in available classes");
            }
        }
    }
    
    /// <summary>
    /// Start the assignment flow with the first question
    /// </summary>
    void StartAssignmentFlow()
    {
        Debug.Log($"🚀 Starting assignment: {currentAssignment.assignmentTitle}");
        
        // IMPORTANT: Hide class code entry UI and show assignment UI
        useClassCodeMode = false;
        assignmentJoined = true;
        
        // Hide class code entry elements if they exist
        HideClassCodeUI();
        
        // Show assignment UI elements
        ShowAssignmentUI();
        
        // Update question text with actual assignment content
        string questionContent = $"📚 {currentAssignment.assignmentTitle}\n\n" +
                               $"Q{currentQuestionIndex + 1}/{currentAssignment.questions.Length}: {currentAssignment.questions[currentQuestionIndex].questionText}";
        
        questionText.text = questionContent;
        
        Debug.Log($"📝 UI Updated - Question: {questionContent}");
        
        // Update progress
        UpdateProgressUI();
        
        // Set up multiple choice buttons with the current question's options
        SetupMultipleChoiceButtons();
        
        Debug.Log($"✅ Assignment UI should now show '{currentAssignment.assignmentTitle}' with question: {currentAssignment.questions[currentQuestionIndex].questionText}");
    }

    /// <summary>
    /// Hide class code entry UI elements
    /// </summary>
    void HideClassCodeUI()
    {
        Debug.Log("🙈 Hiding class code entry UI");
        // The class code entry UI should be hidden when useClassCodeMode = false
        // This is handled automatically by the Update() method logic
    }
    
    /// <summary>
    /// Show assignment UI elements  
    /// </summary>
    void ShowAssignmentUI()
    {
        Debug.Log("👁️ Showing assignment UI");
        
        // Make sure answer buttons are visible and active
        foreach (var button in answerButtons)
        {
            if (button != null)
            {
                button.gameObject.SetActive(true);
            }
        }
        
        // Make sure question text is visible
        if (questionText != null)
        {
            questionText.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// Set up multiple choice buttons for the current question
    /// </summary>
    void SetupMultipleChoiceButtons()
    {
        if (currentAssignment == null || currentQuestionIndex >= currentAssignment.questions.Length)
            return;
            
        var currentQ = currentAssignment.questions[currentQuestionIndex];
        
        // Set up the answer buttons with the current question's options
        for (int i = 0; i < answerButtons.Count && i < currentQ.multipleChoiceOptions.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            
            // Get button text component
            TMP_Text buttonText = answerButtons[i].GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = currentQ.multipleChoiceOptions[i];
            }
            
            // Set up button click handler
            int buttonIndex = i; // Capture for closure
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => OnMultipleChoiceAnswer(buttonIndex));
        }
        
        // Hide unused buttons
        for (int i = currentQ.multipleChoiceOptions.Count; i < answerButtons.Count; i++)
        {
            answerButtons[i].gameObject.SetActive(false);
        }
        
        Debug.Log($"🔘 Set up {currentQ.multipleChoiceOptions.Count} answer options for question {currentQuestionIndex + 1}");
    }
    
    /// <summary>
    /// Handle multiple choice answer selection
    /// </summary>
    void OnMultipleChoiceAnswer(int selectedIndex)
    {
        if (currentAssignment == null || currentQuestionIndex >= currentAssignment.questions.Length)
            return;
            
        var currentQ = currentAssignment.questions[currentQuestionIndex];
        string selectedAnswer = currentQ.multipleChoiceOptions[selectedIndex];
        bool isCorrect = currentQ.correctMultipleChoiceIndices.Contains(selectedIndex);
        
        Debug.Log($"📋 Student selected: {selectedAnswer} (Index: {selectedIndex}, Correct: {isCorrect})");
        
        // Record the answer
        studentAnswers.Add(new QuestionResult
        {
            questionIndex = currentQuestionIndex,
            questionText = currentQ.questionText,
            selectedAnswer = selectedIndex,
            isCorrect = isCorrect,
            timeSpent = 0f, // Could track time if needed
            pointsEarned = isCorrect ? 1 : 0
        });
        
        // Move to next question or show results
        currentQuestionIndex++;
        
        if (currentQuestionIndex < currentAssignment.questions.Length)
        {
            // Show next question
            questionText.text = $"Assignment: {currentAssignment.assignmentTitle}\n\n{currentAssignment.questions[currentQuestionIndex].questionText}";
            SetupMultipleChoiceButtons();
            UpdateProgressUI();
        }
        else
        {
            // Assignment complete
            ShowAssignmentResults();
        }
    }
    
    /// <summary>
    /// Update progress bar and text
    /// </summary>
    void UpdateProgressUI()
    {
        if (currentAssignment == null) return;
        
        float progress = (float)currentQuestionIndex / currentAssignment.questions.Length;
        
        if (progressBar != null)
            progressBar.value = progress;
            
        if (textProgress != null)
            textProgress.text = $"Question {currentQuestionIndex + 1} of {currentAssignment.questions.Length}";
    }

    void Start()
    {
        // Clear assignment cache to ensure fresh data is loaded
        ClearAssignmentCache();
        
        // Test web app connectivity first
        StartCoroutine(TestWebAppConnection());
        
        // First, check if ClassCodeGate has loaded assignments
        CheckForClassCodeGateAssignments();
        
        InitializeGame();
        
        // Use dynamic system instead of hardcoded assignment interface
        StartCoroutine(GetAndUseFirstAvailableClass());
    }
    
    /// <summary>
    /// Clear all cached assignment data to force fresh loading
    /// </summary>
    private void ClearAssignmentCache()
    {
        Debug.Log("🧹 Clearing assignment cache to ensure fresh data...");
        
        // Clear current assignment content
        PlayerPrefs.DeleteKey("CurrentAssignmentContent");
        PlayerPrefs.DeleteKey("ActiveAssignmentSubject");
        PlayerPrefs.DeleteKey("ActiveAssignmentId");
        PlayerPrefs.DeleteKey("ActiveAssignmentTitle");
        PlayerPrefs.DeleteKey("ActiveAssignmentContent");
        PlayerPrefs.DeleteKey("AssignmentSource");
        PlayerPrefs.DeleteKey("CurrentAssignmentId");
        PlayerPrefs.DeleteKey("CurrentAssignmentTitle");
        
        // Clear subject-based caches dynamically without hardcoded subject names
        // Clear common assignment keys patterns
        string[] keyPatterns = { 
            "Assignments_", "ActiveAssignment_", "Subject_", "Assignment_", 
            "CurrentSubject", "StudentID", "ClassCode"
        };
        
        foreach (string pattern in keyPatterns)
        {
            // Clear numbered variations for each pattern
            for (int i = 0; i < 50; i++) // Check up to 50 possible variations
            {
                PlayerPrefs.DeleteKey($"{pattern}{i}");
                PlayerPrefs.DeleteKey($"{pattern}_{i}");
                PlayerPrefs.DeleteKey($"{pattern}{i}_Title");
                PlayerPrefs.DeleteKey($"{pattern}{i}_Id");
                PlayerPrefs.DeleteKey($"{pattern}{i}_Subject");
            }
            
            // Clear base pattern
            if (PlayerPrefs.HasKey(pattern))
                PlayerPrefs.DeleteKey(pattern);
        }
        
        PlayerPrefs.Save();
        Debug.Log("✅ Assignment cache cleared");
    }
    
    /// <summary>
    /// Show test question after a delay to allow UI to initialize - DISABLED to prevent hardcoded content
    /// </summary>
    private IEnumerator ShowTestQuestionAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        // TestMultipleChoiceInterface(); // DISABLED - Use dynamic assignments from web app instead
        Debug.Log("⚠️ ShowTestQuestionAfterDelay called but disabled to prevent hardcoded content. Use dynamic assignments from web app.");
    }
    
    /// <summary>
    /// Test if we can connect to the web app
    /// </summary>
    IEnumerator TestWebAppConnection()
    {
        string testUrl = "https://homequest-c3k7.onrender.com/";
        Debug.Log($"🌐 Testing connection to web app: {testUrl}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(testUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ Web app is reachable! Response code: {request.responseCode}");
            }
            else
            {
                Debug.LogError($"❌ Cannot reach web app: {request.error} (Code: {request.responseCode})");
            }
        }
    }
    
    /// <summary>
    /// Test the ClassCodeGate endpoints to see if they work
    /// </summary>
    IEnumerator TestClassCodeGateEndpoints()
    {
        string baseUrl = "https://homequest-c3k7.onrender.com";
        
        // Test 0: Get all available classes first
        Debug.Log("🧪 Getting all available classes from web app...");
        yield return StartCoroutine(GetAvailableClasses(baseUrl));
    }
    
    /// <summary>
    /// Get all available classes to see what class codes exist
    /// </summary>
    IEnumerator GetAvailableClasses(string baseUrl)
    {
        string classesUrl = baseUrl + "/classes";
        
        using (UnityWebRequest request = UnityWebRequest.Get(classesUrl))
        {
            yield return request.SendWebRequest();
            
            Debug.Log($"📡 Classes Response Code: {request.responseCode}");
            Debug.Log($"📡 Classes Response: {request.downloadHandler.text}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse the response to get available class codes
                var response = JsonUtility.FromJson<ClassesResponse>(request.downloadHandler.text);
                if (response != null && response.data != null && response.data.Length > 0)
                {
                    Debug.Log($"✅ Found {response.data.Length} classes in web app:");
                    string firstClassCode = null;
                    
                    foreach (var classData in response.data)
                    {
                        Debug.Log($"  📚 Class: {classData.name} (Code: {classData.class_code})");
                        if (firstClassCode == null)
                            firstClassCode = classData.class_code;
                    }
                    
                    // Test with the first available class code
                    if (firstClassCode != null)
                    {
                        Debug.Log($"🎯 Testing with real class code: {firstClassCode}");
                        yield return StartCoroutine(TestJoinClass(baseUrl, firstClassCode));
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ No classes found in web app. Create a class first!");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to get classes: {request.error}");
            }
        }
    }
    
    /// <summary>
    /// Test joining a class with a real class code
    /// </summary>
    IEnumerator TestJoinClass(string baseUrl, string classCode)
    {
        Debug.Log($"🧪 Testing /student/join-class with real code: {classCode}");
        string joinUrl = baseUrl + "/student/join-class";
        
        var joinPayload = new {
            class_code = classCode
            // Remove hardcoded student_id - let backend handle authentication
        };
        
        string jsonData = JsonUtility.ToJson(joinPayload);
        
        using (UnityWebRequest request = new UnityWebRequest(joinUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            Debug.Log($"📡 Join Class Response Code: {request.responseCode}");
            Debug.Log($"📡 Join Class Response: {request.downloadHandler.text}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Join Class endpoint is working!");
                
                // Parse join response to get subject
                var joinResponse = JsonUtility.FromJson<JoinClassApiResponse>(request.downloadHandler.text);
                if (joinResponse != null)
                {
                    Debug.Log($"🎯 Joined subject: {joinResponse.subject}");
                    
                    // Test getting assignments for this subject
                    yield return new WaitForSeconds(1f);
                    yield return StartCoroutine(TestGetAssignmentsEndpoint(baseUrl, joinResponse.subject));
                }
            }
            else
            {
                Debug.LogError($"❌ Join Class failed: {request.error}");
                Debug.LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }
    
    /// <summary>
    /// Test the assignments endpoint with dynamic subject
    /// </summary>
    IEnumerator TestGetAssignmentsEndpoint(string baseUrl, string subject = "")
    {
        Debug.Log($"🧪 Testing /student/assignments endpoint for subject: {subject}");
        string assignmentsUrl = baseUrl + "/student/assignments";
        
        var assignmentsPayload = new {
            subject = subject
            // Remove hardcoded student_id - let backend handle authentication
        };
        
        string jsonData = JsonUtility.ToJson(assignmentsPayload);
        
        using (UnityWebRequest request = new UnityWebRequest(assignmentsUrl, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            Debug.Log($"📡 Assignments Response Code: {request.responseCode}");
            Debug.Log($"📡 Assignments Response: {request.downloadHandler.text}");
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("✅ Assignments endpoint is working!");
                // Try to parse and use the response
                AssignmentsResponse response = JsonUtility.FromJson<AssignmentsResponse>(request.downloadHandler.text);
                if (response != null && response.assignments != null && response.assignments.Length > 0)
                {
                    Debug.Log($"🎯 Found {response.assignments.Length} assignments from web app!");
                    foreach (var assignment in response.assignments)
                    {
                        Debug.Log($"  📋 Assignment: {assignment.title} ({assignment.questions.Length} questions)");
                    }
                    
                    // If we found assignments, let's try to load them into the game!
                    Debug.Log("🚀 Attempting to load these assignments into the game...");
                    allAssignments = response;
                    LoadAssignmentByIndex(0);
                    assignmentJoined = true;
                    useClassCodeMode = false;
                }
                else
                {
                    Debug.LogWarning($"⚠️ No assignments found for subject: {subject}. Create assignments in your web app!");
                }
            }
            else
            {
                Debug.LogError($"❌ Assignments failed: {request.error}");
            }
        }
    }

    void Update()
    {
        // BASIC INPUT TEST - This should work if ANY input is working
        // Try both old and new input systems
        bool anyKeyPressed = false;
        
        try 
        {
            anyKeyPressed = Input.anyKeyDown;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Old Input system failed: {e.Message}");
        }
        
        if (anyKeyPressed)
        {
            Debug.Log("🎹 ANY KEY PRESSED! Old Input system is working!");
            
            // Check specifically for our needed keys with old system
            if (Input.GetKeyDown(KeyCode.L)) Debug.Log("🔑 L key pressed!");
            if (Input.GetKeyDown(KeyCode.T)) Debug.Log("🔑 T key pressed!");
            if (Input.GetKeyDown(KeyCode.C)) Debug.Log("🔑 C key pressed!");
            if (Input.GetKeyDown(KeyCode.R)) Debug.Log("🔑 R key pressed!");
            if (Input.GetKeyDown(KeyCode.X)) Debug.Log("🔑 X key pressed!");
            if (Input.GetKeyDown(KeyCode.D)) Debug.Log("🔑 D key pressed!");
        }
        
        // Alternative: Check for key presses without anyKeyDown
        if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.T) || 
            Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.R) ||
            Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("🎯 Direct key check detected input!");
        }
        
        // Debug: Show current state (remove this after testing)
        if (Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log($"🔍 Debug Info:");
            Debug.Log($"   useClassCodeMode: {useClassCodeMode}");
            Debug.Log($"   assignmentJoined: {assignmentJoined}");
            Debug.Log($"   Current scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        }
        
        // Press 'X' to clear PlayerPrefs and force enable class code mode
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("🧹 X key pressed - Clearing PlayerPrefs and enabling class code mode");
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            useClassCodeMode = true;
            assignmentJoined = false;
            Debug.Log("✅ Class code mode re-enabled. You can now use L, T, C, R keys.");
        }
        
        // Test ALL key presses (temporary debug)
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("🎹 L key pressed!");
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("🎹 T key pressed!");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("🎹 C key pressed!");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🎹 R key pressed!");
        }
        
        // Test different student IDs (press S key)
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("🧪 S key pressed! Testing different student IDs...");
            TestDifferentStudentIDs();
        }
        
        // Press 'R' key to refresh assignments (clear cache and reload)
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🔄 R key pressed! Refreshing assignments...");
            ClearAssignmentCache();
            StartCoroutine(GetAndUseFirstAvailableClass());
        }
        
        // Press 'M' key to test multiple choice interface (bypass class code) - DISABLED to prevent hardcoded content
        /*
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("🎮 M key pressed! Testing multiple choice interface...");
            TestMultipleChoiceInterface();
        }
        */
        
        // Test controls for class code system
        if (useClassCodeMode)
        {
            Debug.Log("🔄 useClassCodeMode is TRUE - checking keys...");
            
            // Press 'C' to test class code entry
            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("🎯 C pressed - showing class code entry");
                ShowClassCodeEntry();
            }
            
            // Press 'T' to test web app endpoints
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("🧪 T pressed - testing endpoints");
                StartCoroutine(TestClassCodeGateEndpoints());
            }
            
            // Press 'L' to list available class codes
            if (Input.GetKeyDown(KeyCode.L))
            {
                Debug.Log("📋 L pressed - getting class codes");
                StartCoroutine(GetAvailableClasses("https://homequest-c3k7.onrender.com"));
            }
            
            // Press '1' to get first available class code dynamically
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("Getting first available class code dynamically from web app");
                StartCoroutine(GetAndUseFirstAvailableClass());
            }
            
            // Press '2' to join class dynamically with first available class  
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("Getting dynamic class and joining directly");
                StartCoroutine(GetAndJoinFirstAvailableClass());
            }
            
            // Press '3' to fetch all available class codes dynamically
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                Debug.Log("Fetching all available class codes from Flask backend...");
                StartCoroutine(GetAllAvailableClassCodes());
            }
            
            // Press '4' to test working endpoints directly (bypass join-class)
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                Debug.Log("Testing working endpoints directly - bypassing join-class");
                StartCoroutine(TestWorkingEndpoints());
            }
            
            // Press 'R' to reset/clear class code
            if (Input.GetKeyDown(KeyCode.R))
            {
                ClearClassCode();
            }
            
            // Press 'N' to go to next question (for testing)
            if (Input.GetKeyDown(KeyCode.N) && currentAssignment != null)
            {
                Debug.Log("Manually advancing to next question");
                currentQuestionIndex++;
                DisplayCurrentQuestion();
            }
            
            // Press 'T' to test question display (for debugging)
            if (Input.GetKeyDown(KeyCode.T) && currentAssignment != null)
            {
                Debug.Log("Re-displaying current question");
                DisplayCurrentQuestion();
            }
            
            // Press 'S' to show assignment summary
            if (Input.GetKeyDown(KeyCode.S) && currentAssignment != null)
            {
                Debug.Log($"=== ASSIGNMENT SUMMARY ===");
                Debug.Log($"Title: {currentAssignment.assignmentTitle}");
                Debug.Log($"Questions: {currentAssignment.questions.Length}");
                Debug.Log($"Current Question: {currentQuestionIndex + 1}");
                if (currentQuestionIndex < currentAssignment.questions.Length)
                {
                    var q = currentAssignment.questions[currentQuestionIndex];
                    Debug.Log($"Current Q Text: {q.questionText}");
                    Debug.Log($"Options: {string.Join(", ", q.multipleChoiceOptions)}");
                    Debug.Log($"Correct: {string.Join(", ", q.correctMultipleChoiceIndices)}");
                }
            }
        }
    }

    private void InitializeGame()
    {
        Debug.Log("=== GAME INITIALIZATION ===");
        
        try
        {
            // Initialize collections to prevent null reference errors
            if (answerButtons == null) answerButtons = new List<Button>();
            if (answerToggles == null) answerToggles = new List<Toggle>();
            if (submittedAnswers == null) submittedAnswers = new List<string>();
            if (correctAnswers == null) correctAnswers = new HashSet<string>();
            if (studentAnswers == null) studentAnswers = new List<QuestionResult>();
            
            // Auto-find UI components if not assigned
            FindUIComponents();
            
            // Store original position
            if (player != null)
                originalPos = player.anchoredPosition;

            // Initialize UI
            if (resultPanel != null)
                resultPanel.SetActive(false);

            // Setup components
            AssignPlayerSprite();
            AssignEnemySprite();
            SetupAnswerButtons();
            UpdateUI();

            // Hide speech bubbles initially
            if (playerSpeechBubble) playerSpeechBubble.SetActive(false);
            if (enemySpeechBubble) enemySpeechBubble.SetActive(false);

            // Initialize based on mode
            if (useClassCodeMode)
            {
                InitializeClassCodeMode();
            }
            else
            {
                InitializeTraditionalMode();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Critical error during game initialization: {e.Message}");
            ShowErrorMessage("Failed to initialize game. Please restart Unity.");
        }
    }

    private void InitializeClassCodeMode()
    {
        Debug.Log("=== INITIALIZING CLASS CODE MODE ===");
        
        try
        {
            // Set player name from PlayerPrefs or use default
            playerName = PlayerPrefs.GetString("PlayerName", "Student1");
            
            // Show class code entry screen
            ShowClassCodeEntry();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing class code mode: {e.Message}");
            ShowErrorMessage("Class code mode failed. Switching to traditional mode.");
            // Fallback to traditional mode
            useClassCodeMode = false;
            InitializeTraditionalMode();
        }
    }

    private void InitializeTraditionalMode()
    {
        Debug.Log("=== INITIALIZING TRADITIONAL MODE ===");
        
        try
        {
            // FOR TESTING: Set sample student data if not already set
            if (string.IsNullOrEmpty(PlayerPrefs.GetString("StudentID", "")) || 
                string.IsNullOrEmpty(PlayerPrefs.GetString("CurrentSubject", "")))
            {
                Debug.LogError("❌ No student data found and no sample data allowed!");
                Debug.LogError("📚 All student data must be loaded from web app - no hardcoded content allowed!");
                return; // Exit early - no sample data allowed
            }

            // Load assignment content
            LoadAssignmentInfo();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error initializing traditional mode: {e.Message}");
            ShowErrorMessage("Failed to load assignments. Please check your connection.");
        }
    }

    // Helper method to show error messages
    private void ShowErrorMessage(string message)
    {
        Debug.LogError(message);
        if (questionText != null)
        {
            questionText.text = $"❌ ERROR: {message}";
        }
    }

    private void FindUIComponents()
    {
        Debug.Log("=== FINDING UI COMPONENTS ===");
        
        // Find questionText if not assigned
        if (questionText == null)
        {
            questionText = GameObject.Find("QuestionText")?.GetComponent<TMP_Text>();
            if (questionText == null)
            {
                // Find any TMP_Text with "New Text" as default text
                TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>();
                foreach (var text in allTexts)
                {
                    if (text.text == "New Text")
                    {
                        questionText = text;
                        Debug.Log($"Found questionText by default text: {text.name}");
                        break;
                    }
                }
                
                // If still not found, try to find by common names
                if (questionText == null)
                {
                    string[] commonNames = {"Question", "QuestionDisplay", "TextQuestion", "question"};
                    foreach (string name in commonNames)
                    {
                        GameObject obj = GameObject.Find(name);
                        if (obj != null)
                        {
                            questionText = obj.GetComponent<TMP_Text>();
                            if (questionText != null)
                            {
                                Debug.Log($"Found questionText by name: {name}");
                                break;
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"QuestionText found: {questionText != null}");
        
        // Find answer buttons if not assigned
        if (answerButtons == null || answerButtons.Count == 0)
        {
            answerButtons = new List<Button>();
            Button[] allButtons = FindObjectsOfType<Button>();
            
            foreach (var button in allButtons)
            {
                // Skip specific system buttons
                if (button == submitAnswerButton || button == joinClassButton || button == submitToggleButton)
                    continue;
                    
                // Look for buttons that could be answer buttons
                string buttonName = button.name.ToLower();
                TMP_Text btnText = button.GetComponentInChildren<TMP_Text>();
                
                // Include buttons with names containing "answer", "option", "choice", or default "Button" text
                if (buttonName.Contains("answer") || buttonName.Contains("option") || buttonName.Contains("choice") ||
                    buttonName.Contains("button") || (btnText != null && btnText.text == "Button"))
                {
                    answerButtons.Add(button);
                    Debug.Log($"Found potential answer button: {button.name} (text: {btnText?.text})");
                }
            }
        }
        
        Debug.Log($"Answer buttons found: {answerButtons?.Count ?? 0}");
        
        // If no answer buttons found, try to create basic ones
        if (answerButtons.Count == 0)
        {
            Debug.LogWarning("⚠️ No answer buttons found! Multiple choice questions won't work properly.");
            Debug.LogWarning("📝 Please assign Button GameObjects to the 'Answer Buttons' list in the Inspector.");
        }
        
        // Find other components
        if (progressBar == null)
        {
            progressBar = FindObjectOfType<Slider>();
            Debug.Log($"Progress bar found: {progressBar != null}");
        }
        
        if (textProgress == null)
        {
            TMP_Text[] allTexts = FindObjectsOfType<TMP_Text>();
            foreach (var text in allTexts)
            {
                if (text.text.Contains("Progress"))
                {
                    textProgress = text;
                    Debug.Log($"Progress text found: {text.name}");
                    break;
                }
            }
        }
    }

    private void LoadAssignmentInfo()
    {
        Debug.Log("=== LOADING ASSIGNMENT INFO ===");
        
        if (useClassCodeMode)
        {
            // Check if we have a class code to use
            string savedClassCode = PlayerPrefs.GetString("CurrentClassCode", "");
            if (!string.IsNullOrEmpty(savedClassCode))
            {
                Debug.Log($"Found saved class code: {savedClassCode}");
                LoadAssignmentByClassCode(savedClassCode);
            }
            else
            {
                Debug.Log("No class code found, showing class code entry");
                ShowClassCodeEntry();
            }
        }
        else
        {
            // Traditional mode - use existing assignment loading
            string assignmentContent = PlayerPrefs.GetString("CurrentAssignmentContent", "");
            Debug.Log($"Found assignment content: {(!string.IsNullOrEmpty(assignmentContent) ? "YES" : "NO")}");
            
            if (!string.IsNullOrEmpty(assignmentContent))
            {
                Debug.Log($"Processing existing assignment: {assignmentContent}");
                ProcessAssignmentContent(assignmentContent);
            }
            else
            {
                Debug.Log("No assignment found, showing waiting screen and starting API polling");
                ShowWaitingForAssignment();
            }
        }
    }

    // New method to load assignment by class code (like Kahoot PIN)
    public void LoadAssignmentByClassCode(string classCode)
    {
        Debug.Log($"=== LOADING ASSIGNMENT BY CLASS CODE: {classCode} ===");
        currentClassCode = classCode;
        
        // Save the class code for future use
        PlayerPrefs.SetString("CurrentClassCode", classCode);
        PlayerPrefs.Save();
        
        // Make direct API call to join class and load assignments
        Debug.Log($"Making direct API call to join class with code: {classCode}");
        StartCoroutine(JoinClassDirectly(classCode));
    }

    // Fetch ALL available class codes from Flask backend
    private IEnumerator GetAllAvailableClassCodes()
    {
        string url = "https://homequest-c3k7.onrender.com/api/classes";
        Debug.Log($"🌐 Fetching all class codes from: {url}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"✅ Available class codes response: {response}");
                
                try
                {
                    // Parse the response to extract class codes
                    var classData = JsonUtility.FromJson<ClassListResponse>(response);
                    if (classData != null && classData.classes != null)
                    {
                        Debug.Log($"🎯 Found {classData.classes.Length} available classes:");
                        foreach (var classInfo in classData.classes)
                        {
                            Debug.Log($"   📚 Class Code: {classInfo.class_code} - Subject: {classInfo.subject}");
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not parse class list: {e.Message}");
                    // Just log the raw response
                    Debug.Log($"Raw response: {response}");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to fetch class codes: {request.error}");
            }
        }
    }

    // Direct API call to join class (replacing classCodeGate dependency)
    private IEnumerator JoinClassDirectly(string classCode)
    {
        string serverURL = "https://homequest-c3k7.onrender.com";
        string url = serverURL + "/student/join-class";
        
        Debug.Log($"🌐 Joining class directly with API call to: {url}");
        
        // Create the payload - based on working Flask endpoints
        var payload = new JoinClassApiPayload
        {
            class_code = classCode
            // Remove hardcoded student_id - let backend handle authentication dynamically
        };
        
        string jsonPayload = JsonUtility.ToJson(payload);
        Debug.Log($"📤 Sending join-class payload: {jsonPayload}");
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"✅ Join class response: {response}");
                
                try
                {
                    var joinResponse = JsonUtility.FromJson<JoinClassApiResponse>(response);
                    if (joinResponse != null)
                    {
                        Debug.Log($"📚 Joined class - Subject: {joinResponse.subject}");
                        // Now load assignments for this subject
                        StartCoroutine(LoadAssignmentsDirectly(joinResponse.subject));
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse join response: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to join class: {request.error}");
            }
        }
    }

    // Direct API call to load assignments
    private IEnumerator LoadAssignmentsDirectly(string subject)
    {
        string serverURL = "https://homequest-c3k7.onrender.com";
        string url = serverURL + "/student/assignments";
        
        Debug.Log($"🌐 Loading assignments from: {url}");
        
        // Get the dynamic student ID
        int studentId = GetDynamicStudentID();
        
        // Create the payload
        var payload = new AssignmentApiPayload
        {
            student_id = studentId,
            subject = subject
        };
        
        string jsonPayload = JsonUtility.ToJson(payload);
        Debug.Log($"📤 Sending assignments payload: {jsonPayload}");
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"✅ Assignments response: {response}");
                
                try
                {
                    AssignmentsResponse assignmentsResponse = JsonUtility.FromJson<AssignmentsResponse>(response);
                    if (assignmentsResponse != null && assignmentsResponse.assignments != null && assignmentsResponse.assignments.Length > 0)
                    {
                        Debug.Log($"🎯 Successfully loaded {assignmentsResponse.assignments.Length} assignments!");
                        
                        // Store assignments and load first one
                        allAssignments = assignmentsResponse;
                        LoadAssignmentByIndex(0); // Load first assignment
                        assignmentJoined = true;
                        useClassCodeMode = false;
                    }
                    else
                    {
                        Debug.LogWarning("No assignments found for this class");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse assignments: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to load assignments: {request.error}");
            }
        }
    }

    // Test the working endpoints that return 200 OK
    private IEnumerator TestWorkingEndpoints()
    {
        string serverURL = "https://homequest-c3k7.onrender.com";
        
        // First, get all available classes dynamically
        yield return StartCoroutine(GetDynamicClassData(serverURL));
    }

    private IEnumerator GetDynamicClassData(string serverURL)
    {
        string url = serverURL + "/student/subjects";
        Debug.Log($"🌐 Fetching dynamic subject data from: {url}");
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            // Get the dynamic student ID
            int studentId = GetDynamicStudentID();
            
            var payload = new SubjectsApiPayload
            {
                student_id = studentId
            };
            string jsonPayload = JsonUtility.ToJson(payload);
            Debug.Log($"📤 Sending dynamic subjects request for student {studentId}: {jsonPayload}");
            
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"✅ Dynamic subjects data: {response}");
                
                SubjectsResponse subjectsData = null;
                try
                {
                    // Parse the subjects response
                    subjectsData = JsonUtility.FromJson<SubjectsResponse>(response);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not parse dynamic subjects data: {e.Message}");
                    Debug.Log($"Raw response: {response}");
                }
                
                if (subjectsData != null && subjectsData.subjects != null && subjectsData.subjects.Length > 0)
                {
                    // Convert subjects to available classes format
                    availableClasses = new List<AvailableClass>();
                    foreach (var subject in subjectsData.subjects)
                    {
                        availableClasses.Add(new AvailableClass
                        {
                            class_code = "DYNAMIC", // Use dynamic identifier since we don't have class codes from subjects endpoint
                            subject = subject.subject_name,
                            teacher_name = null // No teacher info available from subjects endpoint - will be populated from assignments
                        });
                    }
                    
                    Debug.Log($"✅ Stored {availableClasses.Count} dynamic subjects");
                    
                    // Use the first available subject dynamically
                    var firstSubject = subjectsData.subjects[0];
                    Debug.Log($"🎯 Using dynamic subject: {firstSubject.subject_name}");
                    
                    // Now get assignments for this dynamic subject
                    yield return StartCoroutine(GetDynamicAssignments(serverURL, "DYNAMIC", firstSubject.subject_name));
                }
                else
                {
                    Debug.LogWarning("No dynamic subjects found");
                    availableClasses = new List<AvailableClass>();
                }
            }
            else
            {
                Debug.LogError($"❌ Failed to fetch dynamic subjects: {request.error}");
                Debug.LogError($"📋 Response Code: {request.responseCode}");
                Debug.LogError($"📋 Response Body: {request.downloadHandler?.text}");
                
                // No hardcoded content allowed - direct user to web app
                Debug.LogError("❌ Failed to load classes from web app. No static fallback content allowed.");
                availableClasses = new List<AvailableClass>();
                
                // Show error message directing user to web app
                if (questionText != null)
                    questionText.text = "❌ Cannot connect to web app\nPlease ensure your teacher has created classes and assignments in the web application.";
            }
        }
    }

    private IEnumerator GetDynamicAssignments(string serverURL, string dynamicClassCode, string dynamicSubject)
    {
        Debug.Log($"🎯 ===== SUBJECT-SPECIFIC ASSIGNMENT LOADING =====");
        Debug.Log($"🎯 Requested Subject: '{dynamicSubject}'");
        Debug.Log($"🎯 Current Scene Subject: '{GetCurrentSubjectFromScene()}'");
        Debug.Log($"🎯 Should load ONLY '{dynamicSubject}' assignments");
        Debug.Log($"🎯 ===============================================");
        
        // Add timestamp to prevent caching
        string timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        string url = serverURL + "/student/assignments?t=" + timestamp;
        
        Debug.Log($"🔄 LOADING ASSIGNMENTS FOR SUBJECT: '{dynamicSubject}' ONLY");
        Debug.Log($"🔄 FORCING FRESH DATA - API URL: {url}");
        
        // Get the dynamic student ID
        int studentId = GetDynamicStudentID();
        
        var payload = new AssignmentApiPayload
        {
            student_id = studentId,
            subject = dynamicSubject
        };
        string jsonPayload = JsonUtility.ToJson(payload);
        Debug.Log($"📤 API Payload - Student ID: {studentId}, Subject Filter: '{dynamicSubject}'");
        Debug.Log($"📤 Full JSON Payload: {jsonPayload}");
        
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                Debug.Log($"✅ Dynamic assignments SUCCESS - Full API Response:");
                Debug.Log($"📄 Raw JSON Response: {response}");
                
                // Try to parse and load the assignments
                try
                {
                    AssignmentsResponse assignmentsResponse = JsonUtility.FromJson<AssignmentsResponse>(response);
                    if (assignmentsResponse != null && assignmentsResponse.assignments != null && assignmentsResponse.assignments.Length > 0)
                    {
                        Debug.Log($"🎯 Found {assignmentsResponse.assignments.Length} assignments from API");
                        
                        // Filter assignments to ensure they match the requested subject
                        List<Assignment> subjectAssignments = new List<Assignment>();
                        
                        for (int i = 0; i < assignmentsResponse.assignments.Length; i++)
                        {
                            var assignment = assignmentsResponse.assignments[i];
                            
                            // Check if assignment subject matches requested subject
                            bool subjectMatches = string.IsNullOrEmpty(assignment.subject) || 
                                                string.Equals(assignment.subject, dynamicSubject, System.StringComparison.OrdinalIgnoreCase);
                            
                            Debug.Log($"📋 Assignment {i}: '{assignment.title}' - ID: {assignment.assignment_id}");
                            Debug.Log($"   📚 Assignment Subject: '{assignment.subject}' | Requested: '{dynamicSubject}'");
                            Debug.Log($"   ✅ Subject Match: {subjectMatches}");
                            Debug.Log($"   📝 Questions: {assignment.questions.Length}");
                            
                            if (subjectMatches)
                            {
                                subjectAssignments.Add(assignment);
                                
                                for (int j = 0; j < assignment.questions.Length; j++)
                                {
                                    var question = assignment.questions[j];
                                    Debug.Log($"      Q{j + 1}: '{question.question_text}' - Type: '{question.question_type}'");
                                }
                            }
                            else
                            {
                                Debug.LogWarning($"⚠️ Skipping assignment '{assignment.title}' - Subject mismatch!");
                            }
                        }
                        
                        if (subjectAssignments.Count > 0)
                        {
                            // Create filtered response with only matching assignments
                            AssignmentsResponse filteredResponse = new AssignmentsResponse
                            {
                                assignments = subjectAssignments.ToArray()
                            };
                            
                            // Store both variables for different uses
                            allAssignments = filteredResponse;
                            currentAssignments = filteredResponse;
                            
                            Debug.Log($"🎯 Successfully filtered to {subjectAssignments.Count} assignments for subject '{dynamicSubject}'");
                            Debug.Log($"🔄 Before LoadAssignmentByIndex - useClassCodeMode: {useClassCodeMode}");
                            LoadAssignmentByIndex(0);
                            Debug.Log($"🔄 After LoadAssignmentByIndex - useClassCodeMode: {useClassCodeMode}");
                            
                            assignmentJoined = true;
                            useClassCodeMode = false;
                            
                            Debug.Log($"🎉 Successfully loaded {dynamicSubject} assignment: {allAssignments.assignments[0].title}");
                            Debug.Log($"📋 From class: {dynamicClassCode} - Subject: {dynamicSubject}");
                        }
                        else
                        {
                            Debug.LogWarning($"❌ No assignments found for subject '{dynamicSubject}' after filtering!");
                            Debug.LogWarning($"📚 Available subjects in API response: {string.Join(", ", assignmentsResponse.assignments.Select(a => a.subject ?? "null").Distinct())}");
                            currentAssignments = null;
                        }
                        useClassCodeMode = false;
                        
                        Debug.Log($"🎉 Successfully loaded dynamic assignment: {allAssignments.assignments[0].title}");
                        Debug.Log($"📋 From class: {dynamicClassCode} - Subject: {dynamicSubject}");
                    }
                    else
                    {
                        Debug.LogWarning($"No assignments found for dynamic class {dynamicClassCode} - {dynamicSubject}");
                        currentAssignments = null;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"Failed to parse dynamic assignments: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"❌ Dynamic assignments failed: {request.error}");
            }
        }
    }

    // Helper method to get and use first available class
    private IEnumerator GetAndUseFirstAvailableClass()
    {
        Debug.Log("🔄 Getting first available class...");
        
        yield return StartCoroutine(GetDynamicClassData(flaskURL));
        
        if (availableClasses != null && availableClasses.Count > 0)
        {
            string firstClassCode = availableClasses[0].class_code;
            string firstSubject = availableClasses[0].subject;
            
            // Set the current subject for the session
            currentSubject = firstSubject;
            
            Debug.Log($"✅ Using first available class: {firstClassCode}, Subject: {firstSubject}");
            
            // Use the first class code for the operation
            yield return StartCoroutine(GetDynamicAssignments(flaskURL, firstClassCode, firstSubject));
        }
        else
        {
            Debug.LogWarning("⚠️ No available classes found");
        }
    }

    // Helper method to get and join first available class
    private IEnumerator GetAndJoinFirstAvailableClass()
    {
        Debug.Log("🔄 Getting and joining first available class...");
        
        yield return StartCoroutine(GetDynamicClassData(flaskURL));
        
        if (availableClasses != null && availableClasses.Count > 0)
        {
            string firstClassCode = availableClasses[0].class_code;
            Debug.Log($"✅ Joining first available class: {firstClassCode}");
            
            // Start the assignment flow with the first class
            yield return StartCoroutine(StartAssignmentFlow(firstClassCode));
        }
        else
        {
            Debug.LogWarning("⚠️ No available classes found to join");
        }
    }

    // Convert web app assignment to our internal format
    private void ConvertAndApplyWebAssignment(WebAppAssignment webAssignment)
    {
        Debug.Log($"=== CONVERTING WEB ASSIGNMENT: {webAssignment.title} ===");
        
        // Create our internal assignment structure
        currentAssignment = new ClassAssignment
        {
            classCode = currentClassCode,
            assignmentTitle = webAssignment.title,
            questions = new QuestionData[webAssignment.questions.Length]
        };
        
        // Convert each question
        for (int i = 0; i < webAssignment.questions.Length; i++)
        {
            var webQ = webAssignment.questions[i];
            Debug.Log($"Converting question {i + 1}: {webQ.question}");
            
            // Create new QuestionData
            var questionData = new QuestionData
            {
                questionId = i + 1,
                questionText = webQ.question,
                questionType = "multiple_choice",
                multipleChoiceOptions = new List<string>(),
                correctMultipleChoiceIndices = new List<int>(),
                correctAnswers = new List<string>()
            };
            
            // Add all options
            for (int j = 0; j < webQ.options.Length; j++)
            {
                questionData.multipleChoiceOptions.Add(webQ.options[j]);
                Debug.Log($"  Option {j}: {webQ.options[j]}");
            }
            
            // Set correct answer
            if (webQ.correct_answer >= 0 && webQ.correct_answer < webQ.options.Length)
            {
                questionData.correctMultipleChoiceIndices.Add(webQ.correct_answer);
                questionData.correctAnswers.Add(webQ.options[webQ.correct_answer]);
                Debug.Log($"  Correct answer: {webQ.correct_answer} ({webQ.options[webQ.correct_answer]})");
            }
            else
            {
                Debug.LogError($"Invalid correct answer index: {webQ.correct_answer}");
                // Default to first option if invalid
                questionData.correctMultipleChoiceIndices.Add(0);
                questionData.correctAnswers.Add(webQ.options[0]);
            }
            
            currentAssignment.questions[i] = questionData;
        }
        
        // Reset progress and start the assignment
        currentQuestionIndex = 0;
        studentAnswers.Clear();
        
        // Show success message and start first question
        if (questionText != null)
            questionText.text = $"✅ Connected to: {webAssignment.title}\nStarting questions...";
            
        // Start the first question after a brief delay
        StartCoroutine(StartAssignmentAfterDelay(1.5f));
    }

    // Start assignment after a brief delay
    private IEnumerator StartAssignmentAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        DisplayCurrentQuestion();
    }

    // Show class code entry screen (like Kahoot join screen)
    private void ShowClassCodeEntry()
    {
        try
        {
            Debug.Log("=== SHOWING CLASS CODE ENTRY ===");
            assignmentJoined = false;
            
            if (questionText != null)
                questionText.text = "🎮 Enter Class Code\n(Dynamic codes available)";
            else
                Debug.LogWarning("questionText is null - cannot show class code entry text");
                
            // Hide answer buttons safely
            if (answerButtons != null)
            {
                foreach (var button in answerButtons)
                {
                    if (button != null) button.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogWarning("answerButtons list is null");
            }
            
            // Show input field if available
            if (classCodeInput != null)
            {
                classCodeInput.gameObject.SetActive(true);
                classCodeInput.text = "";
            }
            else
            {
                Debug.LogWarning("classCodeInput is null - students can use keyboard shortcut '1' to test");
            }
            
            // Show join button if available
            if (joinClassButton != null)
            {
                joinClassButton.gameObject.SetActive(true);
                joinClassButton.onClick.RemoveAllListeners();
                joinClassButton.onClick.AddListener(OnJoinClassClicked);
            }
            else
            {
                Debug.LogWarning("joinClassButton is null - students can use keyboard shortcut '1' to test");
            }
            
            // Show class code panel if available
            if (classCodePanel != null)
                classCodePanel.SetActive(true);
            else
                Debug.LogWarning("classCodePanel is null");
                
            Debug.Log("Class code entry screen displayed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error showing class code entry: {e.Message}");
            ShowErrorMessage("Failed to show class code entry screen");
        }
    }

    // Called when student clicks "Join Class" button
    public void OnJoinClassClicked()
    {
        string enteredCode = "";
        
        if (classCodeInput != null)
            enteredCode = classCodeInput.text.Trim().ToUpper();
        
        if (string.IsNullOrEmpty(enteredCode))
        {
            Debug.LogWarning("No class code entered!");
            if (questionText != null)
                questionText.text = "❌ Please enter a class code!";
            return;
        }
        
        Debug.Log($"Student trying to join class: {enteredCode}");
        
        // Hide class code entry UI
        if (classCodePanel != null)
            classCodePanel.SetActive(false);
        if (classCodeInput != null)
            classCodeInput.gameObject.SetActive(false);
        if (joinClassButton != null)
            joinClassButton.gameObject.SetActive(false);
            
        // Show loading message
        if (questionText != null)
            questionText.text = $"🔄 Joining class: {enteredCode}...";
        
        // Load assignment by class code
        LoadAssignmentByClassCode(enteredCode);
    }

    // Show error message for invalid class code
    private void ShowClassCodeError(string message)
    {
        Debug.LogError($"Class code error: {message}");
        
        if (questionText != null)
            questionText.text = $"❌ {message}\nTry again!";
            
        // Show class code entry again after 2 seconds
        StartCoroutine(ShowClassCodeEntryAfterDelay(2f));
    }

    private IEnumerator ShowClassCodeEntryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ShowClassCodeEntry();
    }

    // Public method to set class code directly (for testing)
    public void SetClassCode(string classCode)
    {
        Debug.Log($"Setting class code: {classCode}");
        
        // Always use real dynamic connection to Flask backend
        Debug.Log($"Using ClassCodeGate to connect with class code: {classCode}");
        LoadAssignmentByClassCode(classCode);
    }

    // Create test assignment that matches your screenshot
    private void CreateTestAssignmentFromScreenshot()
    {
        Debug.Log("=== CREATING TEST ASSIGNMENT FROM SCREENSHOT ===");
        
        // Create assignment matching your "addition" assignment
        var testAssignment = new WebAppAssignment
        {
            title = "addition",
            questions = new WebAppQuestion[]
            {
                new WebAppQuestion
                {
                    question = "1234",
                    options = new string[] { "Option A1", "Option B2", "Option C3", "Option D4" },
                    correct_answer = 0, // Option A1 is correct (as shown in screenshot)
                    question_type = "multiple_choice"
                },
                new WebAppQuestion
                {
                    question = "34231",
                    options = new string[] { "Option A1", "Option B2", "Option C3", "Option D4" },
                    correct_answer = 0, // Option A1 is correct (as shown in screenshot)
                    question_type = "multiple_choice"
                },
                new WebAppQuestion
                {
                    question = "What is 5 + 7?",
                    options = new string[] { "10", "12", "13", "15" },
                    correct_answer = 1, // 12 is correct
                    question_type = "multiple_choice"
                },
                new WebAppQuestion
                {
                    question = "What is 15 + 25?",
                    options = new string[] { "35", "40", "45", "50" },
                    correct_answer = 1, // 40 is correct
                    question_type = "multiple_choice"
                },
                new WebAppQuestion
                {
                    question = "What is 100 + 200?",
                    options = new string[] { "250", "300", "350", "400" },
                    correct_answer = 1, // 300 is correct
                    question_type = "multiple_choice"
                }
            }
        };
        
        // Convert and apply the test assignment
        ConvertAndApplyWebAssignment(testAssignment);
    }

    // Public method to clear class code and return to entry screen
    public void ClearClassCode()
    {
        Debug.Log("Clearing class code");
        PlayerPrefs.DeleteKey("CurrentClassCode");
        PlayerPrefs.Save();
        currentClassCode = "";
        assignmentJoined = false;
        ShowClassCodeEntry();
    }

    // Public method to be called when new assignment is received
    public void LoadNewAssignment(string assignmentJson)
    {
        if (!string.IsNullOrEmpty(assignmentJson))
        {
            PlayerPrefs.SetString("CurrentAssignmentContent", assignmentJson);
            PlayerPrefs.Save();
            ProcessAssignmentContent(assignmentJson);
            Debug.Log("New assignment loaded: " + assignmentJson);
        }
    }

    // Public method to clear current assignment
    public void ClearCurrentAssignment()
    {
        PlayerPrefs.DeleteKey("CurrentAssignmentContent");
        PlayerPrefs.Save();
        ShowWaitingForAssignment();
        Debug.Log("Assignment cleared, waiting for new assignment");
    }

    private void ProcessAssignmentContent(string content)
    {
        Debug.Log("=== PROCESSING ASSIGNMENT CONTENT ===");
        Debug.Log($"Content to process: {content}");
        
        // Try to parse and apply the content
        try
        {
            WebAppAssignment assignment = JsonUtility.FromJson<WebAppAssignment>(content);
            Debug.Log($"Parsed assignment: {assignment != null}");
            
            if (assignment != null && assignment.questions.Length > 0)
            {
                Debug.Log($"Assignment has {assignment.questions.Length} questions");
                ApplyAssignment(assignment);
                return;
            }
            else
            {
                Debug.LogWarning("Assignment is null or has no questions");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to parse assignment JSON: {e.Message}");
        }

        // Fallback to manual extraction
        Debug.Log("Falling back to manual extraction");
        ExtractAndApplyManually(content);
    }

    // Public method to force refresh assignments (for testing)
    public void RefreshAssignments()
    {
        Debug.Log("=== MANUALLY REFRESHING ASSIGNMENTS ===");
        PlayerPrefs.DeleteKey("CurrentAssignmentContent");
        PlayerPrefs.Save();
        currentAssignmentIndex = 0; // Reset assignment index
        ShowWaitingForAssignment();
    }

    // Public method to completely clear all cached data and fetch fresh
    public void ClearAllDataAndRefresh()
    {
        Debug.Log("=== CLEARING ALL CACHED DATA ===");
        
        // Clear all assignment-related PlayerPrefs
        PlayerPrefs.DeleteKey("CurrentAssignmentContent");
        PlayerPrefs.DeleteKey("StudentID");
        PlayerPrefs.DeleteKey("CurrentSubject");
        PlayerPrefs.Save();
        
        // Reset assignment tracking
        currentAssignmentIndex = 0;
        allAssignments = null;
        
        // Clear UI
        if (questionText != null)
            questionText.text = "Fetching fresh assignments...";
        
        // Start dynamic assignment loading without sample data
        StartCoroutine(GetAndUseFirstAvailableClass());
    }

    // Public methods to set student data (called from web app or for testing)
    public void SetStudentData(string studentId, string subject)
    {
        Debug.Log($"Setting student data - ID: {studentId}, Subject: {subject}");
        PlayerPrefs.SetString("StudentID", studentId);
        PlayerPrefs.SetString("CurrentSubject", subject);
        PlayerPrefs.Save();
        
        // Immediately try to load assignments
        RefreshAssignments();
    }

    // Public method to set just student ID
    public void SetStudentID(string studentId)
    {
        Debug.Log($"Setting student ID: {studentId}");
        PlayerPrefs.SetString("StudentID", studentId);
        PlayerPrefs.Save();
    }

    // Public method to set just current subject
    public void SetCurrentSubject(string subject)
    {
        Debug.Log($"Setting current subject: {subject}");
        PlayerPrefs.SetString("CurrentSubject", subject);
        PlayerPrefs.Save();
    }

    // Sample data is DISABLED - all content must come from web app
    [System.Obsolete("Hardcoded sample data is not allowed. All content must come from web app.")]
    public void SetSampleStudentData()
    {
        Debug.LogError("❌ SetSampleStudentData is disabled!");
        Debug.LogError("📚 All student data must come from the web app - no hardcoded content allowed!");
        return; // Exit early - no sample data allowed
    }

    // Method to try different student/subject combinations automatically
    public void TryDifferentCombinations()
    {
        Debug.Log("=== TRYING DIFFERENT STUDENT/SUBJECT COMBINATIONS ===");
        StartCoroutine(TryAllCombinations());
    }

    private IEnumerator TryAllCombinations()
    {
        // Use dynamic data from backend instead of hardcoded arrays
        Debug.Log("🔄 Using dynamic class and subject data from backend...");
        
        // Get dynamic class data first
        yield return StartCoroutine(GetDynamicClassData(flaskURL));
        
        if (availableClasses != null && availableClasses.Count > 0)
        {
            foreach (var classData in availableClasses)
            {
                // Use real class codes from backend
                Debug.Log($"Trying dynamic class: {classData.class_code}, Subject: {classData.subject}");
                
                // Try with dynamic class data
                yield return StartCoroutine(GetDynamicAssignments(flaskURL, classData.class_code, classData.subject));
                
                // Check if we found assignments
                if (currentAssignments != null && currentAssignments.assignments != null && currentAssignments.assignments.Length > 0)
                {
                    Debug.Log($"✅ SUCCESS! Found assignments for class {classData.class_code}");
                    yield break; // Stop searching, we found something
                }
                
                yield return new WaitForSeconds(1f); // Wait between attempts
            }
        }
        else
        {
            Debug.LogError("❌ No dynamic classes available from backend");
        }
        
        Debug.LogError("❌ No assignments found for any dynamic class. Make sure assignments exist in your database.");
    }

    // Public method to load next assignment
    public void LoadNextAssignment()
    {
        Debug.Log("=== LOADING NEXT ASSIGNMENT ===");
        if (allAssignments != null && allAssignments.assignments.Length > 0)
        {
            // Process the next assignment in the cycle
            int assignmentToShow = currentAssignmentIndex % allAssignments.assignments.Length;
            var selectedAssignment = allAssignments.assignments[assignmentToShow];
            
            Debug.Log($"Loading assignment {assignmentToShow + 1} of {allAssignments.assignments.Length}");
            
            var webAppAssignment = new WebAppAssignment
            {
                title = selectedAssignment.title,
                subject = PlayerPrefs.GetString("CurrentSubject", ""),
                assignment_type = "multiple_choice",
                questions = ConvertToWebAppQuestions(selectedAssignment.questions)
            };
            
            string convertedJson = JsonUtility.ToJson(webAppAssignment);
            PlayerPrefs.SetString("CurrentAssignmentContent", convertedJson);
            PlayerPrefs.Save();
            
            ApplyAssignment(webAppAssignment);
            currentAssignmentIndex++;
        }
        else
        {
            Debug.Log("No assignments available to cycle through");
            RefreshAssignments(); // Try to fetch new assignments
        }
    }

    // Public method to test API connectivity
    public void TestAPIConnection()
    {
        Debug.Log("=== TESTING API CONNECTION ===");
        StartCoroutine(TestAPIEndpoint());
    }

    private IEnumerator TestAPIEndpoint()
    {
        string testUrl = $"{flaskURL}/student/assignments";
        Debug.Log($"Testing connection to: {testUrl}");
        
        using (UnityWebRequest request = UnityWebRequest.Get(testUrl))
        {
            yield return request.SendWebRequest();
            
            Debug.Log($"Test Response Code: {request.responseCode}");
            Debug.Log($"Test Result: {request.result}");
            Debug.Log($"Test Response: {request.downloadHandler.text}");
            
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"API connection failed: {request.error}");
                if (questionText != null)
                    questionText.text = $"Connection failed: {request.error}";
            }
        }
    }

    private void ApplyAssignment(WebAppAssignment assignment)
    {
        Debug.Log("=== APPLYING DYNAMIC TEACHER ASSIGNMENT ===");
        Debug.Log($"Assignment title: {assignment.title}");
        Debug.Log($"Questions count: {assignment.questions.Length}");
        
        // Debug all question types in this assignment
        for (int i = 0; i < assignment.questions.Length; i++)
        {
            Debug.Log($"📝 Question {i + 1}: '{assignment.questions[i].question}' - Type: '{assignment.questions[i].question_type}'");
        }
        
        // Ensure UI components are found before applying assignment
        FindUIComponents();
        
        if (assignment.questions.Length > 0)
        {
            // Store the assignment for progression
            currentAssignment = new ClassAssignment
            {
                assignmentTitle = assignment.title,
                questions = new QuestionData[assignment.questions.Length]
            };
            
            Debug.Log($"📋 Converting {assignment.questions.Length} questions from assignment");
            
            // Determine game mode based on the first question type
            if (assignment.questions.Length > 0)
            {
                string firstQuestionType = assignment.questions[0].question_type.ToLower();
                Debug.Log($"🔍 First question type: {firstQuestionType}");
                
                switch (firstQuestionType)
                {
                    case "identification":
                        gameMode = GameMode.Identification;
                        Debug.Log("🎮 Game mode set to Identification");
                        break;
                    case "multiple_choice":
                        gameMode = GameMode.MultipleChoice;
                        Debug.Log("🎮 Game mode set to MultipleChoice");
                        break;
                    default:
                        gameMode = GameMode.InputField; // fallback
                        Debug.Log($"🎮 Game mode set to InputField (fallback for type: {firstQuestionType})");
                        break;
                }
            }
            else
            {
                gameMode = GameMode.MultipleChoice; // default fallback
                Debug.Log("🎮 Game mode set to MultipleChoice (no questions found)");
            }
            
            // Convert questions to our format
            for (int i = 0; i < assignment.questions.Length; i++)
            {
                var q = assignment.questions[i];
                string questionType = q.question_type.ToLower();
                
                currentAssignment.questions[i] = new QuestionData
                {
                    questionId = i + 1,
                    questionText = q.question,
                    questionType = questionType,
                    multipleChoiceOptions = new List<string>(q.options ?? new string[0]),
                    correctMultipleChoiceIndices = GetCorrectAnswerIndicesWebApp(q, questionType),
                    correctAnswers = GetCorrectAnswersForWebAppQuestion(q, questionType)
                };
                
                Debug.Log($"📝 Loaded Question {i + 1}: {q.question}");
                Debug.Log($"📝 Type: {questionType}");
                if (questionType == "identification")
                {
                    Debug.Log($"📝 Correct Answer: {(q.options != null && q.options.Length > q.correct_answer ? q.options[q.correct_answer] : "Unknown")}");
                }
                else
                {
                    Debug.Log($"📝 Options: {string.Join(", ", q.options)}");
                    Debug.Log($"📝 Correct Answer: {(q.options != null && q.options.Length > q.correct_answer ? q.options[q.correct_answer] : "Unknown")}");
                }
            }
            
            // Reset progress
            currentQuestionIndex = 0;
            studentAnswers.Clear();
            
            // Start the first question
            DisplayCurrentQuestion();
            
            Debug.Log("=== DYNAMIC TEACHER ASSIGNMENT APPLICATION COMPLETE ===");
        }
        else
        {
            Debug.LogError("Teacher assignment has no questions!");
        }
    }

    // Helper method to get correct answers based on question type (for WebAppQuestion)
    private List<string> GetCorrectAnswersForWebAppQuestion(WebAppQuestion q, string questionType)
    {
        if (questionType == "identification")
        {
            // For identification, get the correct answer from the options array using the index
            if (q.options != null && q.options.Length > q.correct_answer && q.correct_answer >= 0)
            {
                return new List<string> { q.options[q.correct_answer] };
            }
            else
            {
                Debug.LogWarning($"Invalid correct_answer index {q.correct_answer} for identification question with {q.options?.Length ?? 0} options");
                return new List<string> { "Unknown" };
            }
        }
        else
        {
            // For multiple choice, get the correct answer from the options array using the index
            if (q.options != null && q.options.Length > q.correct_answer && q.correct_answer >= 0)
            {
                return new List<string> { q.options[q.correct_answer] };
            }
            else
            {
                Debug.LogWarning($"Invalid correct_answer index {q.correct_answer} for question with {q.options?.Length ?? 0} options");
                return new List<string> { "Unknown" };
            }
        }
    }
    
    // Helper method to get correct answer indices for multiple choice (for WebAppQuestion)
    private List<int> GetCorrectAnswerIndicesWebApp(WebAppQuestion q, string questionType)
    {
        if (questionType == "identification")
        {
            // For identification, we don't use indices
            return new List<int> { -1 };
        }
        else
        {
            // For multiple choice, the correct_answer is already the index
            if (q.correct_answer >= 0 && q.options != null && q.correct_answer < q.options.Length)
            {
                return new List<int> { q.correct_answer };
            }
            else
            {
                Debug.LogWarning($"Invalid correct_answer index {q.correct_answer} for question with {q.options?.Length ?? 0} options");
                return new List<int> { 0 }; // Default to first option if invalid
            }
        }
    }

    // Helper method to get correct answers based on question type (for AssignmentQuestion)
    private List<string> GetCorrectAnswersForQuestion(AssignmentQuestion q, string questionType)
    {
        if (questionType == "identification")
        {
            // For identification, use the direct answer
            return new List<string> { q.correct_answer };
        }
        else
        {
            // For multiple choice, the correct_answer is the actual answer text
            return new List<string> { q.correct_answer };
        }
    }
    
    // Helper method to get correct answer indices for multiple choice
    private List<int> GetCorrectAnswerIndices(AssignmentQuestion q, string questionType)
    {
        if (questionType == "identification")
        {
            // For identification, we don't use indices
            return new List<int> { -1 };
        }
        else
        {
            // For multiple choice, find the index of the correct answer in options
            if (q.options != null && !string.IsNullOrEmpty(q.correct_answer))
            {
                for (int i = 0; i < q.options.Length; i++)
                {
                    if (q.options[i] == q.correct_answer)
                    {
                        return new List<int> { i };
                    }
                }
                Debug.LogWarning($"Correct answer '{q.correct_answer}' not found in options: {string.Join(", ", q.options)}");
            }
            return new List<int> { 0 }; // Default to first option if not found
        }
    }

    // Display current question like Kahoot
    private void DisplayCurrentQuestion()
    {
        try
        {
            if (currentAssignment == null)
            {
                Debug.LogError("currentAssignment is null!");
                ShowErrorMessage("No assignment loaded!");
                return;
            }
            
            if (currentAssignment.questions == null)
            {
                Debug.LogError("currentAssignment.questions is null!");
                ShowErrorMessage("Assignment has no questions!");
                return;
            }
            
            if (currentQuestionIndex >= currentAssignment.questions.Length)
            {
                Debug.Log("All questions completed, showing results");
                ShowAssignmentResults();
                return;
            }
            
            var question = currentAssignment.questions[currentQuestionIndex];
            if (question == null)
            {
                Debug.LogError($"Question {currentQuestionIndex} is null!");
                ShowErrorMessage($"Question {currentQuestionIndex + 1} is missing!");
                return;
            }
            
            // CRITICAL: Validate assignment is from web app (no hardcoded content)
            if (!ValidateAssignmentIsFromWebApp(currentAssignment))
            {
                Debug.LogError("🚫 BLOCKED: Hardcoded assignment detected! Only web app assignments allowed!");
                ShowNoAssignmentsError(currentSubject ?? "Unknown");
                return;
            }
            
            Debug.Log($"=== DISPLAYING QUESTION {currentQuestionIndex + 1}/{currentAssignment.questions.Length} ===");
            Debug.Log($"Question: {question.questionText}");
            
            // Record question start time for timing
            questionStartTime = Time.time;
            
            // Update question text
            if (questionText != null)
            {
                questionText.text = $"Q{currentQuestionIndex + 1}/{currentAssignment.questions.Length}: {question.questionText}";
            }
            else
            {
                Debug.LogWarning("questionText is null - cannot display question text");
            }
            
            // Update progress bar
            if (progressBar != null)
            {
                float progressValue = (float)(currentQuestionIndex + 1) / currentAssignment.questions.Length;
                progressBar.value = progressValue;
            }
            
            if (textProgress != null)
            {
                textProgress.text = $"{currentQuestionIndex + 1}/{currentAssignment.questions.Length}";
            }
            
            // Ensure answer buttons list is not null
            if (answerButtons == null)
            {
                Debug.LogError("answerButtons list is null!");
                ShowErrorMessage("Answer buttons not found!");
                return;
            }
            
            Debug.Log($"🔘 answerButtons count: {answerButtons.Count}");
            Debug.Log($"🎮 Current game mode: {gameMode}");
            
            // Show a clear error if no answer buttons are available
            if (answerButtons.Count == 0)
            {
                Debug.LogError("❌ CRITICAL: No answer buttons available! Multiple choice won't work!");
                Debug.LogError("📝 SOLUTION: Assign Button GameObjects to 'Answer Buttons' list in Inspector");
                ShowErrorMessage("No answer buttons found! Check Inspector settings.");
                return;
            }
            
            // Ensure question has options (only required for multiple choice)
            if (gameMode == GameMode.MultipleChoice && question.multipleChoiceOptions == null)
            {
                Debug.LogError("Multiple choice question has no options!");
                ShowErrorMessage("Question options missing!");
                return;
            }
            
            if (gameMode == GameMode.MultipleChoice)
            {
                Debug.Log($"🔘 Question options count: {question.multipleChoiceOptions.Count}");
            }
            Debug.Log($"📝 Question type: {question.questionType}");
            Debug.Log($"🎮 Game mode: {gameMode}");
            
            // Configure UI based on game mode
            Debug.Log($"🎮 Setting up UI for game mode: {gameMode}");
            if (gameMode == GameMode.MultipleChoice)
            {
                Debug.Log("🔘 Setting up Multiple Choice UI");
                SetupMultipleChoiceUI(question);
            }
            else if (gameMode == GameMode.Identification)
            {
                Debug.Log("✏️ Setting up Identification UI");
                SetupIdentificationUI(question);
            }
            else
            {
                Debug.Log("📝 Setting up Input Field UI (fallback)");
                // Default to input field mode
                SetupInputFieldUI(question);
            }
            
            // Hide class code UI when showing questions
            if (classCodePanel != null)
                classCodePanel.SetActive(false);
            if (classCodeInput != null)
                classCodeInput.gameObject.SetActive(false);
            if (joinClassButton != null)
                joinClassButton.gameObject.SetActive(false);
                
            Debug.Log("🎮 Hidden class code UI for question display");
            
            // Show student avatar/icon
            SetupStudentAvatar();
            
            // Show enemy speech bubble with question
            if (enemySpeechBubble && enemySpeechText)
            {
                enemySpeechBubble.SetActive(true);
                enemySpeechText.text = question.questionText;
            }

            Debug.Log($"Question {currentQuestionIndex + 1} displayed for {gameMode} mode");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error displaying question: {e.Message}");
            ShowErrorMessage($"Failed to display question {currentQuestionIndex + 1}");
        }
    }
    
    private void SetupMultipleChoiceUI(QuestionData question)
    {
        Debug.Log("🎮 Setting up Multiple Choice UI");
        
        // Hide input field UI elements
        if (answerInputField != null)
            answerInputField.gameObject.SetActive(false);
        if (submitAnswerButton != null)
            submitAnswerButton.gameObject.SetActive(false);
        
        // Ensure question has options
        if (question.multipleChoiceOptions == null)
        {
            Debug.LogError("Question has no multiple choice options!");
            ShowErrorMessage("Question options missing!");
            return;
        }
        
        // Setup answer buttons with options
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (answerButtons[i] == null)
            {
                Debug.LogWarning($"Answer button {i} is null, skipping");
                continue;
            }
            
            if (i < question.multipleChoiceOptions.Count)
            {
                // Show and setup button
                answerButtons[i].gameObject.SetActive(true);
                
                var btnText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = $"{(char)('A' + i)}: {question.multipleChoiceOptions[i]}";
                }
                else
                {
                    Debug.LogWarning($"Button {i} has no TMP_Text component");
                }
                
                // Remove old listeners and add new one
                answerButtons[i].onClick.RemoveAllListeners();
                int answerIndex = i; // Capture for closure
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(answerIndex));
                
                Debug.Log($"Button {i} setup: {question.multipleChoiceOptions[i]}");
            }
            else
            {
                // Hide unused buttons
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }
    
    private void SetupIdentificationUI(QuestionData question)
    {
        Debug.Log("🎮 Setting up Identification UI");
        Debug.Log($"📝 Question: '{question.questionText}'");
        Debug.Log($"📝 Question Type: '{question.questionType}'");
        
        // Show and update progress bar
        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            // Set progress based on current question
            if (currentAssignment != null && currentAssignment.questions.Length > 0)
            {
                float progress = (float)currentQuestionIndex / currentAssignment.questions.Length;
                progressBar.value = progress;
                Debug.Log($"📊 Progress bar updated: {progress:P0}");
            }
        }
        
        // Update progress text dynamically from web app data
        if (textProgress != null)
        {
            textProgress.gameObject.SetActive(true);
            // Get progress text from assignment or use empty
            string progressText = "";
            if (currentAssignment != null && !string.IsNullOrEmpty(currentAssignment.assignmentTitle))
            {
                progressText = currentAssignment.assignmentTitle.ToUpper();
            }
            textProgress.text = progressText;
            Debug.Log($"✅ Progress text set dynamically: '{progressText}'");
        }
        
        // Ensure question text is visible and displays the question
        if (questionText != null)
        {
            questionText.gameObject.SetActive(true);
            // Format the question display like "Q1/1: [question text]"
            if (currentAssignment != null && currentAssignment.questions.Length > 0)
            {
                string formattedQuestion = $"Q{currentQuestionIndex + 1}/{currentAssignment.questions.Length}: {question.questionText}";
                questionText.text = formattedQuestion;
                Debug.Log($"✅ Question text displayed: '{formattedQuestion}'");
            }
            else
            {
                questionText.text = question.questionText;
                Debug.Log($"✅ Question text displayed: '{question.questionText}'");
            }
        }
        else
        {
            Debug.LogError("❌ questionText UI element is null! Cannot display question text.");
        }
        
        // Hide all answer buttons (not needed for identification)
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (answerButtons[i] != null)
                answerButtons[i].gameObject.SetActive(false);
        }
        
        // Show input field UI elements
        if (answerInputField != null)
        {
            answerInputField.gameObject.SetActive(true);
            answerInputField.text = ""; // Clear any previous input
            answerInputField.interactable = true; // Make sure it's interactable
            answerInputField.readOnly = false; // Make sure it's not read-only
            
            // Set placeholder text dynamically from assignment or keep empty
            if (answerInputField.placeholder != null)
            {
                var placeholderComponent = answerInputField.placeholder.GetComponent<TMP_Text>();
                if (placeholderComponent != null)
                {
                    // Keep placeholder empty to avoid hardcoded text
                    placeholderComponent.text = "";
                }
            }
            
            // Add Enter key support
            answerInputField.onSubmit.RemoveAllListeners();
            answerInputField.onSubmit.AddListener((string value) => OnIdentificationAnswerSubmitted());
            
            // Focus the input field so user can start typing immediately
            answerInputField.Select();
            answerInputField.ActivateInputField();
            
            Debug.Log("🎯 IDENTIFICATION INPUT FIELD IS READY FOR TYPING!");
        }
        else
        {
            Debug.LogError("answerInputField is null! Cannot show identification input.");
        }
        
        // Setup SUBMIT button
        if (submitAnswerButton != null)
        {
            submitAnswerButton.gameObject.SetActive(true);
            submitAnswerButton.onClick.RemoveAllListeners();
            submitAnswerButton.onClick.AddListener(() => OnIdentificationAnswerSubmitted());
            
            var buttonText = submitAnswerButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                // Keep button text empty to avoid hardcoded content
                buttonText.text = "";
            }
            
            Debug.Log("✅ SUBMIT button activated");
        }
        else
        {
            Debug.LogError("submitAnswerButton is null! Cannot show identification submit button.");
        }
        
        // Try to find and setup CLEAR button if it exists
        SetupClearButton();
    }
    
    private void SetupClearButton()
    {
        // Try to find a CLEAR button by checking button names (no hardcoded text matching)
        Button[] allButtons = FindObjectsOfType<Button>();
        Button clearButton = null;
        
        foreach (Button btn in allButtons)
        {
            // Only check button names, not text content to avoid hardcoded references
            if (btn.name.ToLower().Contains("clear"))
            {
                clearButton = btn;
                break;
            }
        }
        
        if (clearButton != null)
        {
            clearButton.gameObject.SetActive(true);
            clearButton.onClick.RemoveAllListeners();
            clearButton.onClick.AddListener(() => ClearAnswerInput());
            
            var buttonText = clearButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                // Keep button text empty to avoid hardcoded content
                buttonText.text = "";
            }
                
            Debug.Log("✅ Clear button found and activated (no hardcoded text)");
        }
        else
        {
            Debug.Log("ℹ️ No clear button found in scene");
        }
    }
    
    private void ClearAnswerInput()
    {
        if (answerInputField != null)
        {
            answerInputField.text = "";
            answerInputField.Select();
            answerInputField.ActivateInputField();
            Debug.Log("🧹 Answer input cleared");
        }
    }
    
    private void SetupInputFieldUI(QuestionData question)
    {
        Debug.Log("🎮 Setting up Input Field UI (fallback mode)");
        
        // Hide all answer buttons
        for (int i = 0; i < answerButtons.Count; i++)
        {
            if (answerButtons[i] != null)
                answerButtons[i].gameObject.SetActive(false);
        }
        
        // Show input field UI elements
        if (answerInputField != null)
        {
            answerInputField.gameObject.SetActive(true);
            answerInputField.text = "";
            answerInputField.interactable = true;
            answerInputField.readOnly = false;
            
            if (answerInputField.placeholder != null)
            {
                answerInputField.placeholder.GetComponent<TMP_Text>().text = "Type your answer here...";
            }
            
            // Add Enter key support
            answerInputField.onSubmit.RemoveAllListeners();
            answerInputField.onSubmit.AddListener((string value) => OnIdentificationAnswerSubmitted());
            
            // Focus the input field
            answerInputField.Select();
            answerInputField.ActivateInputField();
        }
        
        if (submitAnswerButton != null)
        {
            submitAnswerButton.gameObject.SetActive(true);
            submitAnswerButton.onClick.RemoveAllListeners();
            submitAnswerButton.onClick.AddListener(() => OnIdentificationAnswerSubmitted());
            
            var buttonText = submitAnswerButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = "🎯 SUBMIT ANSWER 🎯";
            
            Debug.Log("🎯 SUBMIT BUTTON IS NOW VISIBLE AND READY!");
            Debug.Log($"🔍 Submit button position: {submitAnswerButton.transform.position}");
            Debug.Log($"🔍 Submit button active: {submitAnswerButton.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ SUBMIT BUTTON IS NULL! No submit button available!");
        }
    }
    
    // Handle identification answer submission
    private void OnIdentificationAnswerSubmitted()
    {
        try
        {
            if (currentAssignment == null)
            {
                Debug.LogError("currentAssignment is null in OnIdentificationAnswerSubmitted!");
                return;
            }
            
            if (currentQuestionIndex >= currentAssignment.questions.Length)
            {
                Debug.LogWarning("Question index out of range in OnIdentificationAnswerSubmitted!");
                return;
            }
            
            if (answerInputField == null)
            {
                Debug.LogError("answerInputField is null in OnIdentificationAnswerSubmitted!");
                return;
            }
            
            string userAnswer = answerInputField.text.Trim();
            if (string.IsNullOrEmpty(userAnswer))
            {
                Debug.LogWarning("Empty answer submitted for identification question");
                ShowErrorMessage("Please enter an answer!");
                return;
            }
            
            var question = currentAssignment.questions[currentQuestionIndex];
            bool isCorrect = ValidateIdentificationAnswer(userAnswer, question.correctAnswers);
            
            Debug.Log($"🧠 Identification Answer: '{userAnswer}' | Correct: {isCorrect}");
            
            // Record the answer
            var questionResult = new QuestionResult
            {
                questionIndex = currentQuestionIndex,
                questionText = question.questionText,
                selectedAnswer = -1, // For identification, we don't have a selected index
                isCorrect = isCorrect,
                timeSpent = Time.time - questionStartTime,
                pointsEarned = isCorrect ? 1 : 0
            };
            
            studentAnswers.Add(questionResult);
            
            // Show feedback
            if (isCorrect)
            {
                Debug.Log("✅ Correct identification answer!");
                // PlayCorrectAnswerFeedback(); // TODO: Add sound feedback if needed
            }
            else
            {
                Debug.Log("❌ Incorrect identification answer!");
                // PlayIncorrectAnswerFeedback(); // TODO: Add sound feedback if needed
                string correctAnswer = question.correctAnswers.Count > 0 ? question.correctAnswers[0] : "Unknown";
                Debug.Log($"Correct answer was: {correctAnswer}");
            }
            
            // Send to web app
            SendIdentificationAnswerToWebApp(userAnswer, isCorrect);
            
            // Move to next question after delay
            StartCoroutine(ProgressToNextQuestion(2f));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnIdentificationAnswerSubmitted: {e.Message}");
            ShowErrorMessage("Failed to submit identification answer");
        }
    }
    
    // Validate identification answer with smart matching
    private bool ValidateIdentificationAnswer(string userAnswer, List<string> correctAnswers)
    {
        if (correctAnswers == null || correctAnswers.Count == 0)
        {
            Debug.LogWarning("No correct answers provided for identification question");
            return false;
        }
        
        // Normalize user answer (trim, lower case, remove extra spaces)
        string normalizedUserAnswer = userAnswer.Trim().ToLower().Replace("  ", " ");
        
        foreach (string correctAnswer in correctAnswers)
        {
            if (string.IsNullOrEmpty(correctAnswer)) continue;
            
            string normalizedCorrectAnswer = correctAnswer.Trim().ToLower().Replace("  ", " ");
            
            // Exact match (case-insensitive)
            if (normalizedUserAnswer == normalizedCorrectAnswer)
            {
                Debug.Log($"✅ Exact match: '{userAnswer}' = '{correctAnswer}'");
                return true;
            }
            
            // Contains match (user answer contains the correct answer)
            if (normalizedUserAnswer.Contains(normalizedCorrectAnswer))
            {
                Debug.Log($"✅ Contains match: '{userAnswer}' contains '{correctAnswer}'");
                return true;
            }
            
            // Reverse contains match (correct answer contains user answer)
            if (normalizedCorrectAnswer.Contains(normalizedUserAnswer))
            {
                Debug.Log($"✅ Reverse contains match: '{correctAnswer}' contains '{userAnswer}'");
                return true;
            }
            
            // Partial match using Levenshtein distance (for typos)
            if (CalculateLevenshteinDistance(normalizedUserAnswer, normalizedCorrectAnswer) <= 2 && 
                normalizedCorrectAnswer.Length > 3) // Only for words longer than 3 characters
            {
                Debug.Log($"✅ Fuzzy match: '{userAnswer}' ≈ '{correctAnswer}' (typo tolerance)");
                return true;
            }
        }
        
        Debug.Log($"❌ No match found for '{userAnswer}' among correct answers: {string.Join(", ", correctAnswers)}");
        return false;
    }
    
    // Calculate Levenshtein distance for fuzzy matching
    private int CalculateLevenshteinDistance(string source, string target)
    {
        if (string.IsNullOrEmpty(source)) return target?.Length ?? 0;
        if (string.IsNullOrEmpty(target)) return source.Length;
        
        int[,] distance = new int[source.Length + 1, target.Length + 1];
        
        for (int i = 0; i <= source.Length; i++)
            distance[i, 0] = i;
        for (int j = 0; j <= target.Length; j++)
            distance[0, j] = j;
        
        for (int i = 1; i <= source.Length; i++)
        {
            for (int j = 1; j <= target.Length; j++)
            {
                int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                distance[i, j] = System.Math.Min(
                    System.Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }
        
        return distance[source.Length, target.Length];
    }
    
    // Send identification answer to web app
    private void SendIdentificationAnswerToWebApp(string answer, bool isCorrect)
    {
        try
        {
            Debug.Log($"📤 Sending identification answer to web app: '{answer}' (Correct: {isCorrect})");
            // Implement web app submission logic here if needed
            // This follows the same pattern as SendAnswerToWebApp for multiple choice
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error sending identification answer to web app: {e.Message}");
        }
    }
    
    // Handle answer selection (like Kahoot)
    private void OnAnswerSelected(int selectedAnswer)
    {
        try
        {
            if (currentAssignment == null)
            {
                Debug.LogError("currentAssignment is null in OnAnswerSelected!");
                return;
            }
            
            if (currentQuestionIndex >= currentAssignment.questions.Length)
            {
                Debug.LogWarning("Question index out of range in OnAnswerSelected!");
                return;
            }
                
            var question = currentAssignment.questions[currentQuestionIndex];
            if (question == null)
            {
                Debug.LogError($"Question {currentQuestionIndex} is null in OnAnswerSelected!");
                return;
            }
            
            if (question.correctMultipleChoiceIndices == null)
            {
                Debug.LogError("Question has no correct answer indices!");
                return;
            }
            
            bool isCorrect = question.correctMultipleChoiceIndices.Contains(selectedAnswer);
            
            Debug.Log($"=== ANSWER SELECTED ===");
            Debug.Log($"Question {currentQuestionIndex + 1}: Selected {selectedAnswer}, Correct: {string.Join(",", question.correctMultipleChoiceIndices)}");
            Debug.Log($"Answer is: {(isCorrect ? "CORRECT" : "WRONG")}");
            
            // Record the answer
            var result = new QuestionResult
            {
                questionIndex = currentQuestionIndex,
                questionText = question.questionText,
                selectedAnswer = selectedAnswer,
                isCorrect = isCorrect,
                timeSpent = Time.time, // Simple timing
                pointsEarned = isCorrect ? 1 : 0
            };
            
            if (studentAnswers != null)
            {
                studentAnswers.Add(result);
            }
            else
            {
                Debug.LogError("studentAnswers list is null!");
                studentAnswers = new List<QuestionResult> { result };
            }
            
            // Show immediate feedback like Kahoot
            ShowAnswerFeedback(isCorrect, selectedAnswer);
            
            // Disable all buttons to prevent multiple clicks
            if (answerButtons != null)
            {
                foreach (var btn in answerButtons)
                {
                    if (btn != null)
                        btn.interactable = false;
                }
            }
            
            // Move to next question after delay
            StartCoroutine(ProgressToNextQuestion(2f));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in OnAnswerSelected: {e.Message}");
            ShowErrorMessage("Failed to process answer");
        }
    }
    
    // Show answer feedback like Kahoot
    private void ShowAnswerFeedback(bool isCorrect, int selectedAnswer)
    {
        Debug.Log($"=== SHOWING FEEDBACK: {(isCorrect ? "CORRECT" : "WRONG")} ===");
        
        // Highlight the selected button
        if (selectedAnswer < answerButtons.Count)
        {
            var button = answerButtons[selectedAnswer];
            ColorBlock colors = button.colors;
            colors.normalColor = isCorrect ? Color.green : Color.red;
            button.colors = colors;
        }
        
        // Show player speech bubble with feedback
        if (playerSpeechBubble && playerSpeechText)
        {
            playerSpeechBubble.SetActive(true);
            
            if (isCorrect)
            {
                playerSpeechText.text = "🎉 Correct!";
                playerSpeechText.color = Color.green;
            }
            else
            {
                var question = currentAssignment.questions[currentQuestionIndex];
                int correctIndex = question.correctMultipleChoiceIndices[0]; // Get first correct answer
                string correctOption = question.multipleChoiceOptions[correctIndex];
                playerSpeechText.text = $"❌ Wrong!\nCorrect: {(char)('A' + correctIndex)}: {correctOption}";
                playerSpeechText.color = Color.red;
            }
        }
    }
    
    /// <summary>
    /// Setup and show student avatar based on selected gender
    /// </summary>
    private void SetupStudentAvatar()
    {
        // Get saved gender preference
        string savedGender = PlayerPrefs.GetString("SelectedGender", "");
        
        // Show player image
        if (playerImage != null)
        {
            playerImage.gameObject.SetActive(true);
            
            // Set sprite based on gender
            if (savedGender.ToLower() == "female" && femaleSprite != null)
            {
                playerImage.sprite = femaleSprite;
                Debug.Log("👩 Showing female student avatar");
            }
            else if (savedGender.ToLower() == "male" && maleSprite != null)
            {
                playerImage.sprite = maleSprite;
                Debug.Log("👨 Showing male student avatar");
            }
            else if (maleSprite != null)
            {
                // Default to male if no preference saved
                playerImage.sprite = maleSprite;
                Debug.Log("👨 Showing default male student avatar");
            }
            else
            {
                Debug.LogWarning("⚠️ No student avatar sprites assigned in Inspector");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ playerImage is null - cannot show student avatar");
        }
        
        // Position player in visible area if needed
        if (player != null)
        {
            player.gameObject.SetActive(true);
            Debug.Log("👤 Student player GameObject activated");
        }
    }
    
    // Progress to next question like Kahoot
    private IEnumerator ProgressToNextQuestion(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Reset button colors and interactability
        foreach (var btn in answerButtons)
        {
            btn.interactable = true;
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }
        
        // Hide feedback
        if (playerSpeechBubble) playerSpeechBubble.SetActive(false);
        if (enemySpeechBubble) enemySpeechBubble.SetActive(false);
        
        // Move to next question
        currentQuestionIndex++;
        DisplayCurrentQuestion();
    }
    
    // Show final results like Kahoot
    private void ShowAssignmentResults()
    {
        Debug.Log("=== SHOWING ASSIGNMENT RESULTS ===");
        
        int totalQuestions = currentAssignment.questions.Length;
        int correctAnswers = studentAnswers.Count(a => a.isCorrect);
        int totalPoints = studentAnswers.Sum(a => a.pointsEarned);
        float percentage = (float)correctAnswers / totalQuestions * 100f;
        
        // Update UI with results
        if (questionText != null)
        {
            questionText.text = $"🎉 Assignment Complete!\n\nScore: {correctAnswers}/{totalQuestions}\nPercentage: {percentage:F1}%\nPoints: {totalPoints}";
        }
        
        // Hide answer buttons
        foreach (var btn in answerButtons)
        {
            btn.gameObject.SetActive(false);
        }
        
        // Show result panel if available
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
        
        // Convert submit button to "Play Again" button
        if (submitAnswerButton != null)
        {
            submitAnswerButton.gameObject.SetActive(true);
            var buttonText = submitAnswerButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "🔄 Play Again";
            }
            
            // Clear old listeners and add restart functionality
            submitAnswerButton.onClick.RemoveAllListeners();
            submitAnswerButton.onClick.AddListener(() => RestartAssignment());
        }
        
        // Convert toggle button to "Home" button  
        if (submitToggleButton != null)
        {
            submitToggleButton.gameObject.SetActive(true);
            var buttonText = submitToggleButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = "🏠 Home";
            }
            
            // Clear old listeners and add home functionality
            submitToggleButton.onClick.RemoveAllListeners();
            submitToggleButton.onClick.AddListener(() => GoHome());
        }
        
        // Show final speech
        if (playerSpeechBubble && playerSpeechText)
        {
            playerSpeechBubble.SetActive(true);
            
            if (percentage >= passingScorePercentage)
            {
                playerSpeechText.text = $"🎉 Great job!\nYou passed with {percentage:F1}%!";
                playerSpeechText.color = Color.green;
            }
            else
            {
                playerSpeechText.text = $"📚 Keep practicing!\nYou got {percentage:F1}%";
                playerSpeechText.color = Color.yellow;
            }
        }
        
        Debug.Log($"Final Results: {correctAnswers}/{totalQuestions} correct ({percentage:F1}%)");
        
        // Send results to web app if enabled
        if (sendToFlask)
        {
            StartCoroutine(SubmitAssignmentResults());
        }
    }
    
    /// <summary>
    /// Restart the current assignment from the beginning
    /// </summary>
    private void RestartAssignment()
    {
        Debug.Log("🔄 Restarting assignment...");
        
        // Reset progress
        currentQuestionIndex = 0;
        studentAnswers.Clear();
        
        // Hide results UI
        if (resultPanel != null)
            resultPanel.SetActive(false);
        if (playerSpeechBubble != null)
            playerSpeechBubble.SetActive(false);
        
        // Show answer buttons again
        foreach (var btn in answerButtons)
        {
            btn.gameObject.SetActive(true);
        }
        
        // Hide navigation buttons
        if (submitAnswerButton != null)
            submitAnswerButton.gameObject.SetActive(false);
        if (submitToggleButton != null)
            submitToggleButton.gameObject.SetActive(false);
        
        // Restart the assignment
        FindUIComponents();
        DisplayCurrentQuestion();
        
        Debug.Log("✅ Assignment restarted!");
    }
    
    /// <summary>
    /// Go back to subject selection (home)
    /// </summary>
    private void GoHome()
    {
        Debug.Log("🏠 Going home...");
        
        // Reset the game state
        currentQuestionIndex = 0;
        studentAnswers.Clear();
        
        // Hide results UI
        if (resultPanel != null)
            resultPanel.SetActive(false);
        if (playerSpeechBubble != null)
            playerSpeechBubble.SetActive(false);
        
        // Hide navigation buttons
        if (submitAnswerButton != null)
            submitAnswerButton.gameObject.SetActive(false);
        if (submitToggleButton != null)
            submitToggleButton.gameObject.SetActive(false);
        
        // Show loading state and restart dynamic system
        if (questionText != null)
            questionText.text = "🏠 Returning to subject selection...\nLoading your classes...";
        
        // Restart the dynamic class system
        StartCoroutine(GoHomeCoroutine());
    }
    
    /// <summary>
    /// Coroutine to handle going back to subject selection
    /// </summary>
    private IEnumerator GoHomeCoroutine()
    {
        yield return new WaitForSeconds(1f); // Brief loading animation
        
        // Restart the dynamic system
        yield return StartCoroutine(GetAndUseFirstAvailableClass());
        
        Debug.Log("✅ Returned to home (subject selection)!");
    }
    
    // Submit results to web app
    private IEnumerator SubmitAssignmentResults()
    {
        Debug.Log("📊 Submitting assignment results to backend...");
        
        int correctAnswers = studentAnswers.Count(a => a.isCorrect);
        int totalQuestions = currentAssignment.questions.Length;
        float percentage = totalQuestions > 0 ? (float)correctAnswers / totalQuestions * 100f : 0f;
        
        var results = new AssignmentResultsPayload
        {
            student_id = GetDynamicStudentID(),
            assignment_id = assignmentId, // Use the tracked assignment ID
            subject = currentSubject ?? "Unknown",
            score = correctAnswers,
            total_questions = totalQuestions,
            percentage = percentage,
            completed_at = System.DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss"),
            answers = studentAnswers.ToArray()
        };
        
        Debug.Log($"📤 Submitting results: Student {results.student_id}, Assignment {results.assignment_id}, Score {results.score}/{results.total_questions} ({results.percentage:F1}%)");
        
        // Try common result submission endpoints
        string[] possibleEndpoints = {
            "/student/results",
            "/student/submit-results", 
            "/assignment/results",
            "/api/assignment-results"
        };
        
        string jsonData = JsonUtility.ToJson(results);
        Debug.Log($"📤 Results payload: {jsonData}");
        
        bool submitted = false;
        
        foreach (string endpoint in possibleEndpoints)
        {
            string url = $"{flaskURL}{endpoint}";
            Debug.Log($"🔄 Trying endpoint: {url}");
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"✅ Assignment results submitted successfully to {endpoint}!");
                    Debug.Log($"📋 Response: {request.downloadHandler.text}");
                    submitted = true;
                    break;
                }
                else
                {
                    Debug.Log($"❌ Failed {endpoint}: {request.error} (Code: {request.responseCode})");
                }
            }
        }
        
        if (!submitted)
        {
            Debug.LogWarning("⚠️ Could not submit to any endpoint. Results saved locally only.");
            // Save results locally as backup
            SaveResultsLocally(results);
        }
    }
    
    /// <summary>
    /// Save results locally as backup when server submission fails
    /// </summary>
    private void SaveResultsLocally(AssignmentResultsPayload results)
    {
        try
        {
            string assignmentId = results?.assignment_id.ToString() ?? "unknown";
            string key = $"AssignmentResults_{assignmentId}_{System.DateTime.Now:yyyyMMdd_HHmmss}";
            string jsonResults = JsonUtility.ToJson(results);
            PlayerPrefs.SetString(key, jsonResults);
            PlayerPrefs.Save();
            Debug.Log($"💾 Results saved locally with key: {key}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error saving results locally: {ex.Message}");
        }
    }

    private void ExtractAndApplyManually(string content)
    {
        Debug.Log("Using manual extraction...");
        
        // Simple manual extraction for common formats
        if (content.Contains("question"))
        {
            int start = content.IndexOf("\"question\":\"") + 12;
            int end = content.IndexOf("\"", start);
            if (end > start && questionText != null)
            {
                string question = content.Substring(start, end - start);
                questionText.text = question;
            }
        }

        // Extract options
        string[] optionKeys = {"option_a", "option_b", "option_c", "option_d"};
        for (int i = 0; i < optionKeys.Length && i < answerButtons.Count; i++)
        {
            string key = optionKeys[i];
            int start = content.IndexOf($"\"{key}\":\"");
            if (start >= 0)
            {
                start += key.Length + 4;
                int end = content.IndexOf("\"", start);
                if (end > start)
                {
                    string option = content.Substring(start, end - start);
                    var btnText = answerButtons[i].GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                        btnText.text = option;
                }
            }
        }
    }

    private void ShowWaitingForAssignment()
    {
        Debug.Log("=== SHOWING WAITING FOR ASSIGNMENT ===");
        if (questionText != null)
            questionText.text = "Loading dynamic assignments...";
        
        // Use dynamic system instead of legacy polling
        Debug.Log("Starting dynamic assignment loading");
        StartCoroutine(GetAndUseFirstAvailableClass());
    }

    private IEnumerator PollForAssignments()
    {
        Debug.Log("=== STARTING ASSIGNMENT POLLING ===");
        
        while (string.IsNullOrEmpty(PlayerPrefs.GetString("CurrentAssignmentContent", "")))
        {
            Debug.Log("Polling for assignments from Flask API...");
            yield return new WaitForSeconds(2f); // Check every 2 seconds
            
            // Try to fetch from Flask API
            yield return StartCoroutine(FetchAssignmentFromAPI());
            
            // Check if we got new content
            string content = PlayerPrefs.GetString("CurrentAssignmentContent", "");
            if (!string.IsNullOrEmpty(content))
            {
                Debug.Log("Found new assignment content from API, processing...");
                ProcessAssignmentContent(content);
                break;
            }
            else
            {
                Debug.Log("No assignment found yet, will continue polling...");
            }
        }
    }

    private IEnumerator FetchAssignmentFromAPI()
    {
        Debug.Log("=== USING DYNAMIC ASSIGNMENT SYSTEM ===");
        
        // Use dynamic system instead of PlayerPrefs
        if (questionText != null)
            questionText.text = "🎯 Loading dynamic assignments from backend...";
        
        // Start dynamic assignment loading
        yield return StartCoroutine(GetAndUseFirstAvailableClass());
        
        Debug.Log("Dynamic assignment loading completed");
    }

    private IEnumerator TryFetchAssignments(string studentId, string subject)
    {
        string apiUrl = $"{flaskURL}/student/assignments";
        Debug.Log($"API URL: {apiUrl}");
        Debug.Log($"Flask URL: {flaskURL}");
        
        // Create request body matching your API spec
        string jsonBody = $"{{\"student_id\": {studentId}, \"subject\": \"{subject}\"}}";
        Debug.Log($"Request Body: {jsonBody}");
        
        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, jsonBody, "application/json"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseData = request.downloadHandler.text;
                Debug.Log($"API Response for {studentId}/{subject}: {responseData}");
                
                if (!string.IsNullOrEmpty(responseData) && responseData != "{}" && responseData != "null")
                {
                    // Check if response has assignments
                    if (!responseData.Contains("\"assignments\": []") && !responseData.Contains("\"assignments\":[]"))
                    {
                        // Found assignments! Process them
                        ProcessAssignmentsResponse(responseData);
                    }
                }
            }
            else
            {
                Debug.Log($"API request failed for {studentId}/{subject}: {request.error}");
            }
        }
    }

    private void ProcessAssignmentsResponse(string responseData)
    {
        Debug.Log("=== PROCESSING ASSIGNMENTS RESPONSE ===");
        Debug.Log($"Raw response: {responseData}");
        
        try
        {
            // Parse the assignments response from your API
            var response = JsonUtility.FromJson<AssignmentsResponse>(responseData);
            Debug.Log($"Parsed response: {response != null}");
            Debug.Log($"Assignments count: {(response?.assignments?.Length ?? 0)}");
            
            if (response.assignments != null && response.assignments.Length > 0)
            {
                // Store all assignments
                allAssignments = response;
                
                // Determine which assignment to show
                int assignmentToShow = currentAssignmentIndex % response.assignments.Length;
                var selectedAssignment = response.assignments[assignmentToShow];
                
                Debug.Log($"Showing assignment {assignmentToShow + 1} of {response.assignments.Length}");
                Debug.Log($"Assignment title: {selectedAssignment.title}");
                Debug.Log($"Questions count: {selectedAssignment.questions?.Length ?? 0}");
                
                var webAppAssignment = new WebAppAssignment
                {
                    title = selectedAssignment.title,
                    subject = PlayerPrefs.GetString("CurrentSubject", ""),
                    assignment_type = "multiple_choice",
                    questions = ConvertToWebAppQuestions(selectedAssignment.questions)
                };
                
                string convertedJson = JsonUtility.ToJson(webAppAssignment);
                Debug.Log($"Converted assignment JSON: {convertedJson}");
                
                PlayerPrefs.SetString("CurrentAssignmentContent", convertedJson);
                PlayerPrefs.Save();
                
                ProcessAssignmentContent(convertedJson);
                Debug.Log($"Successfully loaded assignment: {selectedAssignment.title}");
                
                // Increment for next time (will cycle through all assignments)
                currentAssignmentIndex++;
            }
            else
            {
                Debug.LogWarning("No assignments found for this subject");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to process assignments response: {e.Message}");
            Debug.LogError($"Stack trace: {e.StackTrace}");
        }
    }

    private WebAppQuestion[] ConvertToWebAppQuestions(AssignmentQuestion[] apiQuestions)
    {
        var webAppQuestions = new WebAppQuestion[apiQuestions.Length];
        
        for (int i = 0; i < apiQuestions.Length; i++)
        {
            var apiQuestion = apiQuestions[i];
            webAppQuestions[i] = new WebAppQuestion
            {
                question = apiQuestion.question_text,
                options = apiQuestion.options ?? new string[0],
                correct_answer = FindCorrectAnswerIndex(apiQuestion.options, apiQuestion.correct_answer),
                question_type = apiQuestion.question_type.ToLower()
            };
        }
        
        return webAppQuestions;
    }

    private void SetupAnswerButtons()
    {
        foreach (var button in answerButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnAnswerButtonClick(button));
            }
        }
    }

    private void OnAnswerButtonClick(Button clickedButton)
    {
        var btnText = clickedButton.GetComponentInChildren<TMP_Text>();
        if (btnText != null)
        {
            string answer = btnText.text;
            bool isCorrect = correctAnswers.Contains(answer);
            StartCoroutine(HandleAnswerAttack(answer, isCorrect));
        }
    }

    private IEnumerator HandleAnswerAttack(string answer, bool isCorrect)
    {
        if (isProcessingAttack || player == null || enemy1 == null)
            yield break;

        isProcessingAttack = true;

        // Move to enemy
        Vector2 targetPos = new Vector2(enemy1.anchoredPosition.x - attackOffset, enemy1.anchoredPosition.y);
        yield return StartCoroutine(MovePlayerTo(targetPos));

        // Show answer
        yield return StartCoroutine(ShowSpeechBubble(playerSpeechBubble, playerSpeechText, $"I choose: {answer}", 1.5f));

        // Show result
        string resultText = isCorrect ? "Correct! Well done!" : "Wrong! Try again!";
        yield return StartCoroutine(ShowSpeechBubble(enemySpeechBubble, enemySpeechText, resultText, 2f));

        // Return to original position
        yield return StartCoroutine(MovePlayerTo(originalPos));

        if (isCorrect)
        {
            progress += 25f; // Simple progression
            UpdateUI();
        }

        isProcessingAttack = false;
    }

    private IEnumerator MovePlayerTo(Vector2 targetPosition)
    {
        if (player == null) yield break;

        Vector2 startPosition = player.anchoredPosition;
        float elapsedTime = 0f;
        float moveTime = 0.5f;

        while (elapsedTime < moveTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveTime;
            player.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        player.anchoredPosition = targetPosition;
    }

    private IEnumerator ShowSpeechBubble(GameObject bubble, TMP_Text text, string message, float duration)
    {
        if (bubble != null && text != null)
        {
            text.text = message;
            bubble.SetActive(true);
            yield return new WaitForSeconds(duration);
            bubble.SetActive(false);
        }
    }

    private void UpdateUI()
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (textProgress != null)
            textProgress.text = $"Progress: {progress:F0}%";

        if (progress >= 100f && resultPanel != null)
        {
            resultPanel.SetActive(true);
        }
    }

    private void AssignPlayerSprite()
    {
        if (playerImage != null && maleSprite != null)
            playerImage.sprite = maleSprite;
    }

    private void AssignEnemySprite()
    {
        if (enemyImage != null && enemySprites.Length > 0)
            enemyImage.sprite = enemySprites[0];
    }
}

// Class-based assignment data structures (like Kahoot)
[System.Serializable]
public class ClassAssignment
{
    public string classCode;
    public string className;
    public string subject;
    public string assignmentTitle;
    public QuestionData[] questions;
    public string teacherName;
    public string createdDate;
}

[System.Serializable]
public class QuestionResult
{
    public int questionIndex;
    public string questionText;
    public int selectedAnswer;
    public bool isCorrect;
    public float timeSpent;
    public int pointsEarned;
}

// Additional API response classes for testing
[System.Serializable]
public class ClassesResponse
{
    public string status;
    public string message;
    public ClassInfo[] data;
}

[System.Serializable]
public class ClassInfo
{
    public int id;
    public string name;
    public string section;
    public string class_code;
    public string teacher;
}

[System.Serializable]
public class JoinClassApiResponse
{
    public string subject;
    public string gameplay_type;
}

[System.Serializable]
public class ClassListResponse
{
    public AvailableClass[] classes;
}

[System.Serializable]
public class AvailableClass
{
    public string class_code;
    public string subject;
    public string teacher_name;
}

[System.Serializable]
public class SubjectsResponse
{
    public Subject[] subjects;
}

[System.Serializable]
public class Subject
{
    public string subject_name;
    public string gameplay_type;
}

[System.Serializable]
public class JoinClassApiPayload
{
    public string class_code;
    // Remove static student_id - let backend handle authentication dynamically
}

[System.Serializable]
public class AssignmentApiPayload
{
    public int student_id;
    public string subject;
}

[System.Serializable]
public class SubjectsApiPayload
{
    public int student_id;
}

[System.Serializable]
public class AssignmentResultsPayload
{
    public int student_id;
    public int assignment_id;
    public string subject;
    public int score;
    public int total_questions;
    public float percentage;
    public string completed_at;
    public QuestionResult[] answers;
}