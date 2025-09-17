using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StageNavigationFixer : MonoBehaviour
{
    [Header("Stage Panel Components")]
    public GameObject stagePanel;
    public Button mathButton;
    public Button scienceButton;
    public Button englishButton;
    public Button artButton;
    public Button stageButton1;
    public Button stageButton2;
    public Button stageButton3;
    public Button backButton;
    public TMP_Text titleText;
    
    [Header("Settings")]
    public string gameplaySceneName = "GameplayScene";
    public bool enableOfflineMode = true;
    
    private string currentSubject = "";
    
    void Start()
    {
        Debug.Log("StageNavigationFixer: Starting navigation fix");
        
        // Disable any existing stage panel scripts to avoid conflicts
        DisableOtherStageScripts();
        
        // Initialize this script
        InitializeNavigation();
    }
    
    void DisableOtherStageScripts()
    {
        // Disable DynamicStagePanel_TMP components
        DynamicStagePanel_TMP[] oldPanels = FindObjectsByType<DynamicStagePanel_TMP>(FindObjectsSortMode.None);
        foreach (DynamicStagePanel_TMP panel in oldPanels)
        {
            panel.enabled = false;
            Debug.Log("Disabled DynamicStagePanel_TMP component");
        }
        
        // Disable SafeDynamicStagePanel components
        SafeDynamicStagePanel[] safePanels = FindObjectsByType<SafeDynamicStagePanel>(FindObjectsSortMode.None);
        foreach (SafeDynamicStagePanel panel in safePanels)
        {
            panel.enabled = false;
            Debug.Log("Disabled SafeDynamicStagePanel component");
        }
    }
    
    void InitializeNavigation()
    {
        try
        {
            // Auto-find components if not assigned
            if (stagePanel == null)
                stagePanel = GameObject.Find("StagePanel");
            
            if (mathButton == null)
                mathButton = GameObject.Find("MathButton")?.GetComponent<Button>();
            if (scienceButton == null)
                scienceButton = GameObject.Find("ScienceButton")?.GetComponent<Button>();
            if (englishButton == null)
                englishButton = GameObject.Find("EnglishButton")?.GetComponent<Button>();
            if (artButton == null)
                artButton = GameObject.Find("ArtButton")?.GetComponent<Button>();
            
            if (stageButton1 == null)
                stageButton1 = GameObject.Find("StageButton1")?.GetComponent<Button>();
            if (stageButton2 == null)
                stageButton2 = GameObject.Find("StageButton2")?.GetComponent<Button>();
            if (stageButton3 == null)
                stageButton3 = GameObject.Find("StageButton3")?.GetComponent<Button>();
            
            if (backButton == null)
                backButton = GameObject.Find("BackButton")?.GetComponent<Button>();
            
            if (titleText == null)
                titleText = GameObject.Find("TitleText")?.GetComponent<TMP_Text>();
            
            // Setup button listeners
            SetupButton(mathButton, () => ShowStagePanel("Math"));
            SetupButton(scienceButton, () => ShowStagePanel("Science"));
            SetupButton(englishButton, () => ShowStagePanel("English"));
            SetupButton(artButton, () => ShowStagePanel("Art"));
            SetupButton(backButton, HideStagePanel);
            SetupButton(stageButton1, () => LoadStage("Stage1"));
            SetupButton(stageButton2, () => LoadStage("Stage2"));
            SetupButton(stageButton3, () => LoadStage("Stage3"));
            
            // Initially hide stage panel
            if (stagePanel != null)
                stagePanel.SetActive(false);
            
            // Initialize stage buttons
            SetButtonInteractable(stageButton1, true);
            SetButtonInteractable(stageButton2, false);
            SetButtonInteractable(stageButton3, false);
            
            Debug.Log("StageNavigationFixer: Navigation setup completed successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"StageNavigationFixer setup error: {e.Message}");
        }
    }
    
    void SetupButton(Button button, System.Action action)
    {
        if (button != null && action != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action());
            Debug.Log($"Setup button: {button.name}");
        }
        else if (button == null)
        {
            Debug.LogWarning($"Button is null, cannot setup listener");
        }
    }
    
    void ShowStagePanel(string subject)
    {
        try
        {
            Debug.Log($"StageNavigationFixer: Showing stage panel for {subject}");
            
            currentSubject = subject;
            
            // ALWAYS show the stage panel first - this is the key fix
            if (stagePanel != null)
            {
                stagePanel.SetActive(true);
                Debug.Log("Stage panel activated");
            }
            else
            {
                Debug.LogError("Stage panel is null!");
                return;
            }
            
            // Update title
            if (titleText != null)
            {
                titleText.text = subject + " Stages";
            }
            
            // Save subject for later use
            PlayerPrefs.SetString("CurrentSubject", subject);
            PlayerPrefs.Save();
            
            // Log for offline mode
            bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
            if (offlineMode || enableOfflineMode)
            {
                Debug.Log($"Offline mode: Subject {subject} selected, stage panel shown");
            }
            
            Debug.Log($"Successfully showed stages for subject: {subject}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ShowStagePanel error: {e.Message}");
        }
    }
    
    void HideStagePanel()
    {
        if (stagePanel != null)
        {
            stagePanel.SetActive(false);
            Debug.Log("Stage panel hidden");
        }
    }
    
    void LoadStage(string stageID)
    {
        try
        {
            Debug.Log($"StageNavigationFixer: Loading {currentSubject} - {stageID}");
            
            // Save stage info for next scene
            if (!string.IsNullOrEmpty(currentSubject))
            {
                PlayerPrefs.SetString("CurrentSubject", currentSubject);
                PlayerPrefs.SetString("CurrentStage", stageID);
                PlayerPrefs.Save();
                Debug.Log($"Saved to PlayerPrefs: Subject={currentSubject}, Stage={stageID}");
            }
            
            // Log for offline mode
            bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
            if (offlineMode || enableOfflineMode)
            {
                Debug.Log($"Offline mode: Stage {stageID} selected for {currentSubject}");
            }
            
            // Unlock next stage
            if (stageID == "Stage1")
            {
                SetButtonInteractable(stageButton2, true);
                Debug.Log("Stage 2 unlocked");
            }
            else if (stageID == "Stage2")
            {
                SetButtonInteractable(stageButton3, true);
                Debug.Log("Stage 3 unlocked");
            }
            
            // Load gameplay scene
            LoadSceneSafely(gameplaySceneName);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LoadStage error: {e.Message}");
        }
    }
    
    void LoadSceneSafely(string sceneName)
    {
        try
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                Debug.Log($"Loading scene: {sceneName}");
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogWarning("Scene name is empty, staying in current scene");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load scene '{sceneName}': {e.Message}");
        }
    }
    
    void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
            
            // Visual feedback
            CanvasGroup cg = button.GetComponent<CanvasGroup>();
            if (cg == null) cg = button.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = interactable ? 1f : 0.5f;
            
            Debug.Log($"Button {button.name} interactable: {interactable}");
        }
    }
    
    // Public methods for external access
    public void ForceShowMath() => ShowStagePanel("Math");
    public void ForceShowScience() => ShowStagePanel("Science");
    public void ForceShowEnglish() => ShowStagePanel("English");
    public void ForceShowArt() => ShowStagePanel("Art");
    public void ForceHidePanel() => HideStagePanel();
    
    // Debug method
    [ContextMenu("Test All Subjects")]
    void TestAllSubjects()
    {
        Debug.Log("Testing all subjects...");
        ShowStagePanel("Math");
        Invoke(nameof(ForceHidePanel), 2f);
    }
    
    // Method to check current state
    [ContextMenu("Check Current State")]
    void CheckCurrentState()
    {
        Debug.Log($"Current Subject: {currentSubject}");
        Debug.Log($"Stage Panel Active: {(stagePanel != null ? stagePanel.activeSelf : "null")}");
        Debug.Log($"Components found: Math={mathButton != null}, Science={scienceButton != null}, English={englishButton != null}, Art={artButton != null}");
        Debug.Log($"Stage buttons: 1={stageButton1 != null}, 2={stageButton2 != null}, 3={stageButton3 != null}");
    }
}