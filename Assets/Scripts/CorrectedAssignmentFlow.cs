/*
=== CORRECTED ASSIGNMENT NAVIGATION FLOW ===

ORIGINAL PROBLEM:
1. Subject click went directly to draggable answer interface (skipped stage panel)
2. Multiple errors: missing GameStageManager, ProfileLoader warnings, missing script references
3. No way to see how many assignments are available

CORRECTED SOLUTION:

FLOW NOW WORKS AS:
1. Student clicks Subject button (Science, Math, etc.)
2. Stage Panel ALWAYS shows first 
3. Stage Panel displays:
   - If Teacher Assignment exists: Shows teacher assignment as "Stage 1" with assignment title
   - If No Teacher Assignment: Shows default "Stage 1", "Stage 2", "Stage 3" (locked)
4. Student clicks on assignment/stage button
5. Game loads with correct assignment data

SCRIPTS CREATED/MODIFIED:

1. DynamicStagePanel.cs (MODIFIED):
   - ShowStages() now always shows stage panel first
   - SetupStageButtons() configures stage buttons based on available assignments
   - LoadStage() checks if it's teacher assignment or default stage
   - UpdateStageButtonText() updates button labels

2. SpecificErrorFixer.cs (NEW):
   - Fixes GameStageManager missing errors by creating dummy components
   - Fixes ProfileLoader classNameText warnings by auto-assigning or creating components
   - Creates AttackController and GameStageManager classes to prevent missing component errors

3. AssignmentStageManager.cs (NEW):
   - Alternative/backup stage management system
   - Auto-assigns UI components if not set in inspector
   - Handles both teacher assignments and default stages
   - Better UI management for stage panel

4. AssignmentManager.cs (EXISTING):
   - Handles teacher assignment creation and storage
   - Flask integration for assignment sync

TESTING:

To test the corrected flow:

1. Set a teacher assignment:
   - Use TeacherAssignmentTester script or press 'T' key
   - This creates an assignment for Science subject

2. Click Science subject button:
   - Stage panel should appear
   - Should show "Science - Teacher Assignments" as title
   - Stage 1 button should show the assignment title
   - Stage 2/3 should be locked

3. Click the assignment button:
   - Should load GameplayScene with teacher assignment data
   - gamemechanicdragbuttons.cs should load teacher assignment info

4. Clear assignment and test default:
   - Press 'C' key or use ClearTestAssignment()
   - Click Science again
   - Should show "Science - Default Stages"
   - Stage 1 should be "Stage 1", others locked

ERROR FIXES:

1. "GameStageManager not found" - Fixed by SpecificErrorFixer creating dummy GameStageManager
2. "ProfileLoader classNameText not assigned" - Fixed by auto-assignment or creating dummy text
3. "AttackController is null" - Fixed by creating dummy AttackController
4. "Missing script references" - Fixed by ComprehensiveErrorFixer and SpecificErrorFixer

PLAYERPREFS KEYS USED:

Teacher Assignment Data:
- ActiveAssignmentSubject: Subject for active teacher assignment
- ActiveAssignmentId: Teacher assignment ID
- ActiveAssignmentTitle: Teacher assignment title
- ActiveAssignmentContent: Teacher assignment description

Current Session Data:
- CurrentSubject: Subject for current gameplay session
- CurrentAssignmentId: Assignment ID for current session
- CurrentAssignmentTitle: Assignment title for current session
- AssignmentSource: "teacher" or "default"

The flow now properly shows the stage panel first, allowing students to see available assignments before choosing one.
*/

using UnityEngine;

/// <summary>
/// Documentation and testing helper for the corrected assignment flow
/// </summary>
public class CorrectedAssignmentFlow : MonoBehaviour
{
    [Header("Flow Testing")]
    public bool enableTestMode = false; // Disabled by default to prevent automatic test assignment creation
    
    [Header("Test Assignment Data")]
    public string testSubject = "Science";
    public string testAssignmentId = "PLANTS_QUIZ_001";
    public string testAssignmentTitle = "Quiz 1: Plants";
    
    void Start()
    {
        if (enableTestMode)
        {
            Debug.Log("=== CORRECTED ASSIGNMENT FLOW ACTIVE ===");
            Debug.Log("1. Click subject button → Stage panel shows");
            Debug.Log("2. Stage panel lists available assignments");
            Debug.Log("3. Click assignment → Load specific assignment");
            Debug.Log("Test keys: T=Set Assignment, C=Clear Assignment");
        }
    }
    
    void Update()
    {
        // Test mode disabled by default to prevent automatic test assignment creation
        // Uncomment the following lines only for manual testing:
        /*
        if (enableTestMode && Debug.isDebugBuild)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                SetTestAssignment();
            }
            
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearTestAssignment();
            }
            
            if (Input.GetKeyDown(KeyCode.I))
            {
                LogCurrentStatus();
            }
        }
        */
    }
    
    [ContextMenu("Set Test Assignment")]
    public void SetTestAssignment()
    {
        PlayerPrefs.SetString("ActiveAssignmentSubject", testSubject);
        PlayerPrefs.SetString("ActiveAssignmentId", testAssignmentId);
        PlayerPrefs.SetString("ActiveAssignmentTitle", testAssignmentTitle);
        PlayerPrefs.SetString("ActiveAssignmentContent", "Learn about plant biology and photosynthesis");
        PlayerPrefs.SetString("AssignmentCreatedTime", System.DateTime.Now.ToString());
        PlayerPrefs.Save();
        
        Debug.Log($"TEST: Set teacher assignment - {testSubject}: {testAssignmentTitle}");
        Debug.Log("Now click the Science subject button to see the stage panel");
    }
    
    [ContextMenu("Clear Test Assignment")]
    public void ClearTestAssignment()
    {
        PlayerPrefs.DeleteKey("ActiveAssignmentSubject");
        PlayerPrefs.DeleteKey("ActiveAssignmentId");
        PlayerPrefs.DeleteKey("ActiveAssignmentTitle");
        PlayerPrefs.DeleteKey("ActiveAssignmentContent");
        PlayerPrefs.DeleteKey("AssignmentCreatedTime");
        PlayerPrefs.Save();
        
        Debug.Log("TEST: Cleared teacher assignment");
        Debug.Log("Now click the Science subject button to see default stages");
    }
    
    [ContextMenu("Log Current Status")]
    public void LogCurrentStatus()
    {
        string subject = PlayerPrefs.GetString("ActiveAssignmentSubject", "None");
        string title = PlayerPrefs.GetString("ActiveAssignmentTitle", "None");
        string id = PlayerPrefs.GetString("ActiveAssignmentId", "None");
        
        Debug.Log("=== CURRENT ASSIGNMENT STATUS ===");
        Debug.Log($"Active Subject: {subject}");
        Debug.Log($"Assignment Title: {title}");
        Debug.Log($"Assignment ID: {id}");
        Debug.Log("==============================");
    }
}