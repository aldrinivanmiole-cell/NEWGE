using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages the stage panel UI for both teacher assignments and default stages
/// This script ensures the stage panel shows assignments properly
/// </summary>
public class AssignmentStageManager : MonoBehaviour
{
    [Header("Stage Panel UI")]
    public GameObject stagePanel;
    public TMP_Text stagePanelTitle;
    public Button stage1Button;
    public Button stage2Button;
    public Button stage3Button;
    public Button backButton;
    
    [Header("Assignment Display")]
    public TMP_Text assignmentCountText;
    public TMP_Text assignmentInfoText;
    
    private string currentSubject;
    
    void Start()
    {
        // Find stage panel components if not assigned
        AutoAssignComponents();
        
        // Hide stage panel initially
        if (stagePanel != null)
            stagePanel.SetActive(false);
    }
    
    void AutoAssignComponents()
    {
        if (stagePanel == null)
            stagePanel = GameObject.Find("StagePanel");
        
        if (stagePanelTitle == null)
        {
            GameObject titleObj = GameObject.Find("StagePanelTitle");
            if (titleObj != null)
                stagePanelTitle = titleObj.GetComponent<TMP_Text>();
        }
        
        if (stage1Button == null)
        {
            GameObject btn = GameObject.Find("Stage1Button");
            if (btn != null)
                stage1Button = btn.GetComponent<Button>();
        }
        
        if (stage2Button == null)
        {
            GameObject btn = GameObject.Find("Stage2Button");
            if (btn != null)
                stage2Button = btn.GetComponent<Button>();
        }
        
        if (stage3Button == null)
        {
            GameObject btn = GameObject.Find("Stage3Button");
            if (btn != null)
                stage3Button = btn.GetComponent<Button>();
        }
        
        if (backButton == null)
        {
            GameObject btn = GameObject.Find("BackButton");
            if (btn != null)
                backButton = btn.GetComponent<Button>();
        }
    }
    
    /// <summary>
    /// Show assignments for a specific subject
    /// </summary>
    public void ShowAssignmentsForSubject(string subject)
    {
        currentSubject = subject;
        
        if (stagePanel != null)
            stagePanel.SetActive(true);
        
        // Check for teacher assignments
        bool hasTeacherAssignment = CheckForTeacherAssignment(subject);
        
        if (hasTeacherAssignment)
        {
            SetupTeacherAssignmentView(subject);
        }
        else
        {
            SetupDefaultStageView(subject);
        }
        
        Debug.Log($"ASSIGNMENT STAGE MANAGER: Showing assignments for {subject}");
    }
    
    void SetupTeacherAssignmentView(string subject)
    {
        // Update title
        if (stagePanelTitle != null)
            stagePanelTitle.text = $"{subject} - Teacher Assignments";
        
        // Get teacher assignment info
        string assignmentTitle = PlayerPrefs.GetString("ActiveAssignmentTitle", "Teacher Assignment");
        string assignmentId = PlayerPrefs.GetString("ActiveAssignmentId", "");
        
        // Setup stage 1 as teacher assignment
        if (stage1Button != null)
        {
            TMP_Text btnText = stage1Button.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = assignmentTitle;
            
            stage1Button.interactable = true;
            stage1Button.onClick.RemoveAllListeners();
            stage1Button.onClick.AddListener(() => LoadTeacherAssignment());
        }
        
        // Disable other stages for now
        if (stage2Button != null)
        {
            stage2Button.interactable = false;
            TMP_Text btnText = stage2Button.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "Stage 2 (Locked)";
        }
        
        if (stage3Button != null)
        {
            stage3Button.interactable = false;
            TMP_Text btnText = stage3Button.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "Stage 3 (Locked)";
        }
        
        // Update assignment info
        if (assignmentInfoText != null)
            assignmentInfoText.text = $"Active Assignment: {assignmentTitle}";
        
        if (assignmentCountText != null)
            assignmentCountText.text = "1 Assignment Available";
        
        Debug.Log($"Setup teacher assignment view: {assignmentTitle}");
    }
    
    void SetupDefaultStageView(string subject)
    {
        // Update title
        if (stagePanelTitle != null)
            stagePanelTitle.text = $"{subject} - Default Stages";
        
        // Setup default stages
        if (stage1Button != null)
        {
            TMP_Text btnText = stage1Button.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "Stage 1";
            
            stage1Button.interactable = true;
            stage1Button.onClick.RemoveAllListeners();
            stage1Button.onClick.AddListener(() => LoadDefaultStage("Stage1"));
        }
        
        if (stage2Button != null)
        {
            TMP_Text btnText = stage2Button.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "Stage 2 (Locked)";
            stage2Button.interactable = false;
        }
        
        if (stage3Button != null)
        {
            TMP_Text btnText = stage3Button.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.text = "Stage 3 (Locked)";
            stage3Button.interactable = false;
        }
        
        // Update assignment info
        if (assignmentInfoText != null)
            assignmentInfoText.text = "Default curriculum stages";
        
        if (assignmentCountText != null)
            assignmentCountText.text = "3 Stages Available";
        
        Debug.Log($"Setup default stage view for {subject}");
    }
    
    bool CheckForTeacherAssignment(string subject)
    {
        string activeSubject = PlayerPrefs.GetString("ActiveAssignmentSubject", "");
        string activeId = PlayerPrefs.GetString("ActiveAssignmentId", "");
        
        return !string.IsNullOrEmpty(activeSubject) && 
               !string.IsNullOrEmpty(activeId) && 
               activeSubject.Equals(subject, System.StringComparison.OrdinalIgnoreCase);
    }
    
    void LoadTeacherAssignment()
    {
        string assignmentId = PlayerPrefs.GetString("ActiveAssignmentId", "");
        string assignmentTitle = PlayerPrefs.GetString("ActiveAssignmentTitle", "Assignment");
        
        // Store current assignment info
        PlayerPrefs.SetString("CurrentSubject", currentSubject);
        PlayerPrefs.SetString("CurrentAssignmentId", assignmentId);
        PlayerPrefs.SetString("CurrentAssignmentTitle", assignmentTitle);
        PlayerPrefs.SetString("AssignmentSource", "teacher");
        PlayerPrefs.Save();
        
        Debug.Log($"Loading teacher assignment: {currentSubject} - {assignmentTitle}");
        
        // Load gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
    }
    
    void LoadDefaultStage(string stageName)
    {
        // Store default stage info
        PlayerPrefs.SetString("CurrentSubject", currentSubject);
        PlayerPrefs.SetString("CurrentAssignmentId", "default_" + stageName.ToLower());
        PlayerPrefs.SetString("CurrentAssignmentTitle", $"{currentSubject} {stageName}");
        PlayerPrefs.SetString("AssignmentSource", "default");
        PlayerPrefs.Save();
        
        Debug.Log($"Loading default stage: {currentSubject} - {stageName}");
        
        // Load gameplay scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");
    }
    
    /// <summary>
    /// Hide the stage panel
    /// </summary>
    public void HideStagePanel()
    {
        if (stagePanel != null)
            stagePanel.SetActive(false);
    }
}