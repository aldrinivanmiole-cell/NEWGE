using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Test script to simulate teacher assignment creation
/// Add this to a button in your main menu for testing
/// </summary>
public class TeacherAssignmentTester : MonoBehaviour
{
    [Header("Test Assignment Settings")]
    public Button setAssignmentButton;
    public Button clearAssignmentButton;
    
    [Header("Test Assignment Data")]
    public string testSubject = "Science";
    public string testAssignmentId = "PLANTS_QUIZ_001";
    public string testAssignmentTitle = "Quiz 1: Plants";
    public string testAssignmentContent = "Learn about plant biology and photosynthesis";

    void Start()
    {
        // Set up button listeners
        if (setAssignmentButton != null)
        {
            setAssignmentButton.onClick.AddListener(SetTestAssignment);
        }
        
        if (clearAssignmentButton != null)
        {
            clearAssignmentButton.onClick.AddListener(ClearTestAssignment);
        }
        
        // Log current assignment status
        LogCurrentAssignmentStatus();
    }

    /// <summary>
    /// Set a test teacher assignment for Science subject
    /// </summary>
    public void SetTestAssignment()
    {
        Debug.Log("=== SETTING TEST TEACHER ASSIGNMENT ===");
        
        // Use AssignmentManager if available, otherwise set directly
        AssignmentManager manager = AssignmentManager.Instance;
        if (manager != null)
        {
            manager.SetTeacherAssignment(testSubject, testAssignmentId, testAssignmentTitle, testAssignmentContent);
        }
        else
        {
            // Set directly via PlayerPrefs
            PlayerPrefs.SetString("ActiveAssignmentSubject", testSubject);
            PlayerPrefs.SetString("ActiveAssignmentId", testAssignmentId);
            PlayerPrefs.SetString("ActiveAssignmentTitle", testAssignmentTitle);
            PlayerPrefs.SetString("ActiveAssignmentContent", testAssignmentContent);
            PlayerPrefs.SetString("AssignmentCreatedTime", System.DateTime.Now.ToString());
            
            Debug.Log($"Test assignment set directly: {testSubject} - {testAssignmentTitle}");
        }
        
        LogCurrentAssignmentStatus();
    }

    /// <summary>
    /// Clear the current teacher assignment
    /// </summary>
    public void ClearTestAssignment()
    {
        Debug.Log("=== CLEARING TEACHER ASSIGNMENT ===");
        
        // Use AssignmentManager if available, otherwise clear directly
        AssignmentManager manager = AssignmentManager.Instance;
        if (manager != null)
        {
            manager.ClearTeacherAssignment();
        }
        else
        {
            // Clear directly via PlayerPrefs
            PlayerPrefs.DeleteKey("ActiveAssignmentSubject");
            PlayerPrefs.DeleteKey("ActiveAssignmentId");
            PlayerPrefs.DeleteKey("ActiveAssignmentTitle");
            PlayerPrefs.DeleteKey("ActiveAssignmentContent");
            PlayerPrefs.DeleteKey("AssignmentCreatedTime");
            
            Debug.Log("Test assignment cleared directly");
        }
        
        LogCurrentAssignmentStatus();
    }

    /// <summary>
    /// Log the current assignment status for debugging
    /// </summary>
    public void LogCurrentAssignmentStatus()
    {
        string subject = PlayerPrefs.GetString("ActiveAssignmentSubject", "");
        string id = PlayerPrefs.GetString("ActiveAssignmentId", "");
        string title = PlayerPrefs.GetString("ActiveAssignmentTitle", "");
        string content = PlayerPrefs.GetString("ActiveAssignmentContent", "");
        string created = PlayerPrefs.GetString("AssignmentCreatedTime", "");
        
        Debug.Log("=== CURRENT ASSIGNMENT STATUS ===");
        if (string.IsNullOrEmpty(subject))
        {
            Debug.Log("NO ACTIVE ASSIGNMENT");
        }
        else
        {
            Debug.Log($"Active Assignment Found:");
            Debug.Log($"  Subject: {subject}");
            Debug.Log($"  ID: {id}");
            Debug.Log($"  Title: {title}");
            Debug.Log($"  Content: {content}");
            Debug.Log($"  Created: {created}");
        }
        Debug.Log("================================");
    }

    /// <summary>
    /// Set a custom assignment with specified parameters
    /// </summary>
    public void SetCustomAssignment(string subject, string assignmentId, string title, string content = "")
    {
        testSubject = subject;
        testAssignmentId = assignmentId;
        testAssignmentTitle = title;
        testAssignmentContent = content;
        
        SetTestAssignment();
    }

    void Update()
    {
        // Debug hotkeys disabled by default to prevent automatic test assignment creation
        // Uncomment the following lines only for manual testing:
        /*
        // Debug hotkeys for testing (only in development builds)
        if (Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                SetTestAssignment();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearTestAssignment();
            }
            
            if (Input.GetKeyDown(KeyCode.L))
            {
                LogCurrentAssignmentStatus();
            }
        }
        */
    }
}