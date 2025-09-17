using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Final navigation fix to ensure subjects show stage panel first before going to assignments
/// This script will take priority over DynamicStagePanel and SafeDynamicStagePanel
/// </summary>
public class FinalNavigationFix : MonoBehaviour
{
    [Header("UI References - Auto-detected if null")]
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
    
    // Dynamic assignment list (scrolling)
    public UnityEngine.UI.ScrollRect assignmentsScroll;
    public RectTransform assignmentsContent;
    public Button assignmentButtonTemplate; // Optional: if null we clone stageButton1
    public Button refreshAssignmentsButton; // Optional refresh button
    
    [Header("Behavior")]
    public bool allowRuntimeFallback = false; // Use your existing StagePanel; no temporary panel by default
    public bool fetchAssignmentsOnOpen = true; // Fetch assignments when opening a subject panel
    
    [Header("Scene Settings")]
    public string gameplaySceneName = "GameplayScene";
    public string serverURL = "https://homequest-c3k7.onrender.com";
    
    private string currentSubject = "";
    private bool isInitialized = false;
    private Dictionary<string, GameObject> subjectPanels = new Dictionary<string, GameObject>();
    public GameObject stagePanelTemplate; // optional explicit template; if null, first found stagePanel is used
    
    void Awake()
    {
        // Ensure this script runs first and disables any conflicting scripts
        DisableConflictingScripts();
    }
    
    void Start()
    {
        InitializeNavigation();
    }
    
    void DisableConflictingScripts()
    {
        // Disable all DynamicStagePanel_TMP components
        DynamicStagePanel_TMP[] oldPanels = FindObjectsByType<DynamicStagePanel_TMP>(FindObjectsSortMode.None);
        foreach (DynamicStagePanel_TMP panel in oldPanels)
        {
            if (panel != null && panel.gameObject != this.gameObject)
            {
                panel.enabled = false;
                Debug.Log("FinalNavigationFix: Disabled conflicting DynamicStagePanel_TMP");
            }
        }
        
        // Disable all SafeDynamicStagePanel components
        SafeDynamicStagePanel[] safePanels = FindObjectsByType<SafeDynamicStagePanel>(FindObjectsSortMode.None);
        foreach (SafeDynamicStagePanel panel in safePanels)
        {
            if (panel != null && panel.gameObject != this.gameObject)
            {
                panel.enabled = false;
                Debug.Log("FinalNavigationFix: Disabled conflicting SafeDynamicStagePanel");
            }
        }
        
        // Disable StageNavigationFixer if it exists (this script replaces it)
        StageNavigationFixer[] navFixers = FindObjectsByType<StageNavigationFixer>(FindObjectsSortMode.None);
        foreach (StageNavigationFixer fixer in navFixers)
        {
            if (fixer != null && fixer.gameObject != this.gameObject)
            {
                fixer.enabled = false;
                Debug.Log("FinalNavigationFix: Disabled StageNavigationFixer");
            }
        }
    }
    
    void InitializeNavigation()
    {
        if (isInitialized) return;
        
        try
        {
            // Auto-find UI components if not assigned
            FindUIComponents();
            
            // Setup all button listeners - THE KEY FIX IS HERE
            SetupSubjectButtons();
            SetupStageButtons();
            SetupBackButton();
            
            // Initially hide the stage panel
            HideStagePanel();
            
            // Initialize stage progression
            InitializeStageProgression();
            
            isInitialized = true;
            Debug.Log("FinalNavigationFix: Navigation system initialized successfully");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FinalNavigationFix initialization error: {e.Message}");
        }
    }
    
    void FindUIComponents()
    {
        // Find stage panel
        if (stagePanel == null)
        {
            stagePanel = GameObject.Find("StagePanel");
            if (stagePanel == null)
                stagePanel = GameObject.Find("Stage Panel");
            if (stagePanel == null && allowRuntimeFallback)
            {
                // Try to create a minimal fallback panel so navigation still works
                stagePanel = CreateFallbackStagePanel();
            }
            if (stagePanel == null)
            {
                // Try to infer panel from existing known children without changing layout
                var inferred = InferStagePanelFromChildren();
                if (inferred != null) stagePanel = inferred;
            }
            if (stagePanel == null)
            {
                // Deep search including inactive children in active scene
                stagePanel = DeepFindInSceneByPred(go =>
                {
                    string n = go.name.ToLower();
                    return n == "stagepanel" || n == "stage panel" || n.Contains("stagepanel");
                });
            }
        }
        
        // Find subject buttons
        if (mathButton == null)
            mathButton = FindButtonByName("MathButton", "Math Button", "Math");
        if (scienceButton == null)
            scienceButton = FindButtonByName("ScienceButton", "Science Button", "Science");
        if (englishButton == null)
            englishButton = FindButtonByName("EnglishButton", "English Button", "English");
        if (artButton == null)
            artButton = FindButtonByName("ArtButton", "Art Button", "Art");
        
        // Find stage buttons
        if (stageButton1 == null)
            stageButton1 = FindButtonFlexible(new string[]{"StageButton1","Stage Button 1","Stage1Button","Stage 1 Button"}, "stage1");
        if (stageButton2 == null)
            stageButton2 = FindButtonFlexible(new string[]{"StageButton2","Stage Button 2","Stage2Button","Stage 2 Button"}, "stage2");
        if (stageButton3 == null)
            stageButton3 = FindButtonFlexible(new string[]{"StageButton3","Stage Button 3","Stage3Button","Stage 3 Button"}, "stage3");
        
        // Find back button
        if (backButton == null)
            backButton = FindButtonFlexible(new string[]{"BackButton","Back Button","BtnBack"}, "back");
        
        // Find title text
        if (titleText == null)
        {
            titleText = FindTitleFlexible();
        }
        
        Debug.Log($"FinalNavigationFix: Found components - StagePanel: {stagePanel != null}, Math: {mathButton != null}, Science: {scienceButton != null}, English: {englishButton != null}, Art: {artButton != null}");
    }

    GameObject CreateFallbackStagePanel()
    {
        try
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("FinalNavigationFix: No Canvas found. Cannot create fallback stage panel.");
                return null;
            }

            GameObject panel = new GameObject("StagePanel");
            panel.transform.SetParent(canvas.transform, false);
            var image = panel.AddComponent<UnityEngine.UI.Image>();
            image.color = new Color(0, 0, 0, 0.5f);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            panel.SetActive(false);

            // Title
            GameObject titleObj = new GameObject("TitleText");
            titleObj.transform.SetParent(panel.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0, -30);
            var tmp = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.fontSize = 28;
            tmp.text = "Assignments";
            titleText = tmp;

            // Helper to create a button with label
            Button MakeButton(string name, Vector2 anchoredPos)
            {
                GameObject btnObj = new GameObject(name);
                btnObj.transform.SetParent(panel.transform, false);
                var btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(300, 60);
                btnRect.anchorMin = btnRect.anchorMax = new Vector2(0.5f, 0.5f);
                btnRect.anchoredPosition = anchoredPos;
                var img = btnObj.AddComponent<UnityEngine.UI.Image>();
                img.color = Color.white;
                var btn = btnObj.AddComponent<UnityEngine.UI.Button>();
                // label
                GameObject lbl = new GameObject("Text");
                lbl.transform.SetParent(btnObj.transform, false);
                var lrect = lbl.AddComponent<RectTransform>();
                lrect.anchorMin = lrect.anchorMax = new Vector2(0.5f, 0.5f);
                lrect.sizeDelta = new Vector2(280, 40);
                var ltmp = lbl.AddComponent<TMPro.TextMeshProUGUI>();
                ltmp.alignment = TMPro.TextAlignmentOptions.Center;
                ltmp.fontSize = 24;
                ltmp.text = name;
                return btn;
            }

            stageButton1 = MakeButton("StageButton1", new Vector2(0, 60));
            stageButton2 = MakeButton("StageButton2", new Vector2(0, -20));
            stageButton3 = MakeButton("StageButton3", new Vector2(0, -100));

            backButton = MakeButton("BackButton", new Vector2(0, -180));
            var backLabel = backButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (backLabel != null) backLabel.text = "Back";

            Debug.Log("FinalNavigationFix: Created fallback StagePanel at runtime");
            return panel;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"FinalNavigationFix: Failed to create fallback StagePanel: {e.Message}");
            return null;
        }
    }
    
    Button FindButtonByName(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                Button btn = obj.GetComponent<Button>();
                if (btn != null) return btn;
            }
        }
        return null;
    }

    Button FindButtonFlexible(string[] names, string containsKeyword)
    {
        // Try exact names first
        var btn = FindButtonByName(names);
        if (btn != null) return btn;
        // Then try search within stagePanel by contains keyword
        if (stagePanel != null && !string.IsNullOrEmpty(containsKeyword))
        {
            var buttons = stagePanel.GetComponentsInChildren<Button>(true);
            foreach (var b in buttons)
            {
                if (b != null && b.name.ToLower().Contains(containsKeyword.ToLower()))
                    return b;
            }
        }
        // Finally deep search whole scene
        var deep = DeepFindInSceneByPred(go =>
        {
            if (go == null) return false;
            var bt = go.GetComponent<Button>();
            return bt != null && go.name.ToLower().Contains(containsKeyword.ToLower());
        });
        if (deep != null)
            return deep.GetComponent<Button>();
        return null;
    }

    TMP_Text FindTitleFlexible()
    {
        // Try common names
        string[] names = {"TitleText","Title Text","StagePanelTitle","Stage Panel Title"};
        foreach (var n in names)
        {
            var obj = GameObject.Find(n);
            if (obj != null)
            {
                var t = obj.GetComponent<TMP_Text>();
                if (t != null) return t;
            }
        }
        // Then search under the panel for any TMP_Text that looks like a title
        if (stagePanel != null)
        {
            var texts = stagePanel.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in texts)
            {
                string nm = t.name.ToLower();
                if (nm.Contains("title") || nm.Contains("header"))
                    return t;
            }
        }
        return null;
    }

    GameObject InferStagePanelFromChildren()
    {
        // Look for common stage button names and return their parent container
        string[] names = {"StageButton1","Stage Button 1","Stage1Button","Stage 1 Button","BackButton","Back Button"};
        foreach (var n in names)
        {
            var obj = GameObject.Find(n);
            if (obj != null && obj.transform.parent != null)
            {
                return obj.transform.parent.gameObject;
            }
        }
        return null;
    }

    GameObject DeepFindInSceneByPred(System.Func<GameObject, bool> predicate)
    {
        try
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            foreach (var root in roots)
            {
                var result = DeepFindRecursive(root, predicate);
                if (result != null) return result;
            }
        }
        catch {}
        return null;
    }

    GameObject DeepFindRecursive(GameObject current, System.Func<GameObject, bool> predicate)
    {
        if (predicate(current)) return current;
        for (int i = 0; i < current.transform.childCount; i++)
        {
            var child = current.transform.GetChild(i).gameObject;
            var res = DeepFindRecursive(child, predicate);
            if (res != null) return res;
        }
        return null;
    }
    
    void SetupSubjectButtons()
    {
        // THE CRITICAL FIX: These buttons now show stage panel instead of going directly to assignments
        if (mathButton != null)
        {
            mathButton.onClick.RemoveAllListeners();
            mathButton.onClick.AddListener(() => ShowStagePanel("Math"));
            Debug.Log("FinalNavigationFix: Math button setup to show stage panel first");
        }
        
        if (scienceButton != null)
        {
            scienceButton.onClick.RemoveAllListeners();
            scienceButton.onClick.AddListener(() => ShowStagePanel("Science"));
            Debug.Log("FinalNavigationFix: Science button setup to show stage panel first");
        }
        
        if (englishButton != null)
        {
            englishButton.onClick.RemoveAllListeners();
            englishButton.onClick.AddListener(() => ShowStagePanel("English"));
            Debug.Log("FinalNavigationFix: English button setup to show stage panel first");
        }
        
        if (artButton != null)
        {
            artButton.onClick.RemoveAllListeners();
            artButton.onClick.AddListener(() => ShowStagePanel("Art"));
            Debug.Log("FinalNavigationFix: Art button setup to show stage panel first");
        }
    }
    
    void SetupStageButtons()
    {
        if (stageButton1 != null)
        {
            stageButton1.onClick.RemoveAllListeners();
            stageButton1.onClick.AddListener(() => LoadStage("Stage1"));
        }
        
        if (stageButton2 != null)
        {
            stageButton2.onClick.RemoveAllListeners();
            stageButton2.onClick.AddListener(() => LoadStage("Stage2"));
        }
        
        if (stageButton3 != null)
        {
            stageButton3.onClick.RemoveAllListeners();
            stageButton3.onClick.AddListener(() => LoadStage("Stage3"));
        }
    }
    
    void SetupBackButton()
    {
        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(HideStagePanel);
        }
    }
    
    /// <summary>
    /// THE KEY METHOD: Shows stage panel when subject is clicked
    /// This ensures users see "Assignment 1", "Assignment 2", etc. before selecting
    /// </summary>
    public void ShowStagePanel(string subject)
    {
        try
        {
            Debug.Log($"FinalNavigationFix: Showing stage panel for {subject}");
            
            currentSubject = subject;
            
            // Get or create a dedicated panel instance for this subject
            GameObject panelForSubject = GetOrCreateSubjectPanel(subject);
            if (panelForSubject == null)
            {
                Debug.LogError("FinalNavigationFix: Stage panel not found! Cannot show assignments.");
                return;
            }
            // Hide all other subject panels
            foreach (var kv in subjectPanels)
            {
                if (kv.Value != null) kv.Value.SetActive(false);
            }
            panelForSubject.SetActive(true);
            stagePanel = panelForSubject; // bind references to currently active panel
            RebindPanelChildren(stagePanel);
            // Wire refresh button if present
            if (refreshAssignmentsButton != null)
            {
                refreshAssignmentsButton.onClick.RemoveAllListeners();
                refreshAssignmentsButton.onClick.AddListener(() => { StartCoroutine(FetchSubjectAssignmentsAndUpdateUI(subject)); });
            }
            BringPanelToFrontAndBlockInput(stagePanel);
            Debug.Log("FinalNavigationFix: Stage panel activated - user will see assignments first");
            
            // Update the title to show which subject's stages (force re-assert on each call)
            if (titleText != null)
            {
                titleText.text = subject + " Assignments";
                Debug.Log($"FinalNavigationFix: Title updated to '{subject} Assignments'");
            }

            // Hide static stage buttons and show dynamic scrollable list instead
            HideStaticStageButtons();
            BuildAssignmentListUI(stagePanel);
            PopulateAssignmentsForSubject(subject);
            
            // Save subject for later use
            PlayerPrefs.SetString("CurrentSubject", subject);
            PlayerPrefs.Save();
            
            Debug.Log($"FinalNavigationFix: Successfully showed stage panel for {subject}");

            // Optionally fetch latest assignments from server
            if (fetchAssignmentsOnOpen)
                StartCoroutine(FetchSubjectAssignmentsAndUpdateUI(subject));
            StartCoroutine(RefreshAssignmentAfterDelay(subject, 0.75f));
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FinalNavigationFix ShowStagePanel error: {e.Message}");
        }
    }
    
    public void HideStagePanel()
    {
        if (stagePanel != null)
        {
            stagePanel.SetActive(false);
            Debug.Log("FinalNavigationFix: Stage panel hidden");
        }
        currentSubject = "";
    }
    
    public void LoadStage(string stageID)
    {
        try
        {
            Debug.Log($"FinalNavigationFix: Loading {currentSubject} - {stageID}");
            
            // Save both subject and stage for the next scene
            if (!string.IsNullOrEmpty(currentSubject))
            {
                PlayerPrefs.SetString("CurrentSubject", currentSubject);
                PlayerPrefs.SetString("CurrentStage", stageID);
                PlayerPrefs.Save();
                Debug.Log($"FinalNavigationFix: Saved - Subject: {currentSubject}, Stage: {stageID}");
            }
            
            // If teacher assignment available for this subject, populate current assignment details for any stage
            if (HasTeacherAssignmentForSubject(currentSubject))
            {
                string key = NormalizeSubjectName(currentSubject).ToUpperInvariant();
                string id = PlayerPrefs.GetString($"ActiveAssignment_{key}_Id", PlayerPrefs.GetString("ActiveAssignmentId", ""));
                string title = PlayerPrefs.GetString($"ActiveAssignment_{key}_Title", PlayerPrefs.GetString("ActiveAssignmentTitle", "Teacher Assignment"));
                string content = PlayerPrefs.GetString($"ActiveAssignment_{key}_Content", PlayerPrefs.GetString("ActiveAssignmentContent", ""));
                string aType = PlayerPrefs.GetString($"ActiveAssignment_{key}_Type", PlayerPrefs.GetString("ActiveAssignmentType", PlayerPrefs.GetString("CurrentGameplayType", "")));

                PlayerPrefs.SetString("CurrentAssignmentId", id);
                PlayerPrefs.SetString("CurrentAssignmentTitle", title);
                if (!string.IsNullOrEmpty(content))
                    PlayerPrefs.SetString("CurrentAssignmentContent", content);
                PlayerPrefs.SetString("AssignmentSource", "teacher");
                if (!string.IsNullOrEmpty(aType))
                    PlayerPrefs.SetString("CurrentGameplayType", aType);
                PlayerPrefs.Save();
                Debug.Log($"FinalNavigationFix: Teacher assignment set for gameplay -> {currentSubject} | {title} ({id}) | Stage: {stageID}");
            }

            // Update stage progression
            UnlockNextStage(stageID);
            
            // Load the gameplay scene
            if (!string.IsNullOrEmpty(gameplaySceneName))
            {
                SceneManager.LoadScene(gameplaySceneName);
                Debug.Log($"FinalNavigationFix: Loading scene: {gameplaySceneName}");
            }
            else
            {
                Debug.LogWarning("FinalNavigationFix: Gameplay scene name not set");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"FinalNavigationFix LoadStage error: {e.Message}");
        }
    }

    // --- Assignment-aware stage button text and detection ---
    void UpdateStageButtonsForSubject(string subject)
    {
        string normalized = NormalizeSubjectName(subject);
        string key = NormalizeSubjectName(subject).ToUpperInvariant();

        // 1) Strongest signal: per-subject stored title
        string perSubTitle = PlayerPrefs.GetString($"ActiveAssignment_{key}_Title", "");
        if (!string.IsNullOrEmpty(perSubTitle))
        {
            SetStageButtonText(stageButton1, perSubTitle);
            SetButtonInteractable(stageButton1, true);
            // Make all stages available for additional assignments - show same assignment title
            SetStageButtonText(stageButton2, perSubTitle);
            SetStageButtonText(stageButton3, perSubTitle);
            SetButtonInteractable(stageButton2, true);
            SetButtonInteractable(stageButton3, true);
            Debug.Log($"FinalNavigationFix: Stage 1 title set from per-subject key: '{perSubTitle}' for {normalized}");
            return;
        }

        // 2) Fallback: global active assignment if subject matches
        string activeSubj = PlayerPrefs.GetString("ActiveAssignmentSubject", "");
        string activeTitle = PlayerPrefs.GetString("ActiveAssignmentTitle", "");
        if (!string.IsNullOrEmpty(activeSubj) && SubjectNamesMatch(normalized, activeSubj) && !string.IsNullOrEmpty(activeTitle))
        {
            SetStageButtonText(stageButton1, activeTitle);
            SetButtonInteractable(stageButton1, true);
            // Make all stages available for additional assignments - show same assignment title
            SetStageButtonText(stageButton2, activeTitle);
            SetStageButtonText(stageButton3, activeTitle);
            SetButtonInteractable(stageButton2, true);
            SetButtonInteractable(stageButton3, true);
            Debug.Log($"FinalNavigationFix: Stage 1 title set from global keys: '{activeTitle}' for {normalized}");
            return;
        }

        // 3) Fallback: parse server assignments cache for this subject (saved by ClassCodeGate)
        string rawAssignments = PlayerPrefs.GetString($"Assignments_{normalized}", PlayerPrefs.GetString($"Assignments_{key}", ""));
        string parsedTitle = GetFirstAssignmentTitleFromJson(rawAssignments);
        if (!string.IsNullOrEmpty(parsedTitle))
        {
            SetStageButtonText(stageButton1, parsedTitle);
            SetButtonInteractable(stageButton1, true);
            // Make all stages available for additional assignments - show same assignment title
            SetStageButtonText(stageButton2, parsedTitle);
            SetStageButtonText(stageButton3, parsedTitle);
            SetButtonInteractable(stageButton2, true);
            SetButtonInteractable(stageButton3, true);
            Debug.Log($"FinalNavigationFix: Stage 1 title parsed from assignments cache: '{parsedTitle}' for {normalized}");
            return;
        }

        // 4) No assignment yet → show that no content exists and disable clicks
        SetStageButtonText(stageButton1, "No Assignments Yet");
        SetStageButtonText(stageButton2, "No Assignments Yet");
        SetStageButtonText(stageButton3, "No Assignments Yet");
        SetButtonInteractable(stageButton1, false);
        SetButtonInteractable(stageButton2, false);
        SetButtonInteractable(stageButton3, false);
        Debug.Log($"FinalNavigationFix: No assignment found for {normalized}. Showing 'No Assignments Yet' and disabling buttons");
    }

    string GetFirstAssignmentTitleFromJson(string json)
    {
        if (string.IsNullOrEmpty(json)) return "";
        // try common keys
        string title = ExtractJsonValueSimple(json, "assignment_title");
        if (!string.IsNullOrEmpty(title)) return title;
        title = ExtractJsonValueSimple(json, "title");
        return title;
    }

    string ExtractJsonValueSimple(string json, string key)
    {
        try
        {
            string pattern = "\"" + key + "\"";
            int idx = json.IndexOf(pattern);
            if (idx < 0) return "";
            int colon = json.IndexOf(':', idx + pattern.Length);
            if (colon < 0) return "";
            int q1 = json.IndexOf('"', colon + 1);
            if (q1 < 0) return "";
            int q2 = json.IndexOf('"', q1 + 1);
            if (q2 < 0) return "";
            return json.Substring(q1 + 1, q2 - q1 - 1);
        }
        catch { return ""; }
    }

    bool HasTeacherAssignmentForSubject(string subject)
    {
        string normalized = NormalizeSubjectName(subject);
        string activeSubj = PlayerPrefs.GetString("ActiveAssignmentSubject", "");
        if (string.IsNullOrEmpty(activeSubj)) return false;
        return SubjectNamesMatch(normalized, activeSubj);
    }

    bool SubjectNamesMatch(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b)) return false;
        string na = NormalizeSubjectName(a);
        string nb = NormalizeSubjectName(b);
        return string.Equals(na, nb, System.StringComparison.OrdinalIgnoreCase);
    }

    string NormalizeSubjectName(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        string u = s.Trim();
        // Map common abbreviations to canonical names
        string upper = u.ToUpperInvariant();
        if (upper.Contains("MATH")) return "Math";
        if (upper.Contains("SCI")) return "Science";
        if (upper.Contains("ENG")) return "English";
        if (upper == "PE" || upper.Contains("PHYSICAL") || upper.Contains("ED")) return "PE";
        if (upper.Contains("ART")) return "Art";
        return u;
    }

    void SetStageButtonText(Button button, string text)
    {
        if (button == null) return;
        var tmp = button.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }
        var legacy = button.GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (legacy != null)
        {
            legacy.text = text;
        }
    }

    void BringPanelToFrontAndBlockInput(GameObject panel)
    {
        try
        {
            // Ensure on top
            panel.transform.SetAsLastSibling();
            // Ensure an image exists to receive raycasts without changing current color if present
            var img = panel.GetComponent<UnityEngine.UI.Image>();
            if (img == null)
            {
                img = panel.AddComponent<UnityEngine.UI.Image>();
                img.color = img.color; // no-op keeps default
            }
            img.raycastTarget = true;
            // Ensure CanvasGroup blocks raycasts
            var cg = panel.GetComponent<CanvasGroup>();
            if (cg == null) cg = panel.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }
        catch {}
    }

    GameObject GetOrCreateSubjectPanel(string subject)
    {
        string key = NormalizeSubjectName(subject);
        if (subjectPanels.TryGetValue(key, out var existing) && existing != null)
            return existing;

        // Use explicit template if provided; otherwise, duplicate first discovered panel
        GameObject template = stagePanelTemplate != null ? stagePanelTemplate : stagePanel;
        if (template == null)
        {
            // Try to find a panel once more
            FindUIComponents();
            template = stagePanel;
        }
        if (template == null) return null;

        // Instantiate a clone for this subject so each subject has its own UI instance
        GameObject clone = Instantiate(template, template.transform.parent);
        clone.name = $"StagePanel_{key}";
        clone.SetActive(false);
        subjectPanels[key] = clone;
        return clone;
    }

    void RebindPanelChildren(GameObject panel)
    {
        if (panel == null) return;
        // Rebind title and buttons from the active panel instance
        titleText = FindTitleFlexibleIn(panel) ?? titleText;
        stageButton1 = FindButtonFlexibleIn(panel, new string[]{"StageButton1","Stage Button 1","Stage1Button","Stage 1 Button"}, "stage1") ?? stageButton1;
        stageButton2 = FindButtonFlexibleIn(panel, new string[]{"StageButton2","Stage Button 2","Stage2Button","Stage 2 Button"}, "stage2") ?? stageButton2;
        stageButton3 = FindButtonFlexibleIn(panel, new string[]{"StageButton3","Stage Button 3","Stage3Button","Stage 3 Button"}, "stage3") ?? stageButton3;
        backButton = FindButtonFlexibleIn(panel, new string[]{"BackButton","Back Button","BtnBack"}, "back") ?? backButton;
        // Re-attach listeners to the new buttons
        SetupStageButtons();
        SetupBackButton();
    }

    TMP_Text FindTitleFlexibleIn(GameObject root)
    {
        if (root == null) return null;
        var texts = root.GetComponentsInChildren<TMP_Text>(true);
        foreach (var t in texts)
        {
            string nm = t.name.ToLower();
            if (nm.Contains("title") || nm.Contains("header"))
                return t;
        }
        return null;
    }

    Button FindButtonFlexibleIn(GameObject root, string[] names, string containsKeyword)
    {
        if (root == null) return null;
        // Exact names under root
        foreach (var n in names)
        {
            var child = DeepFindRecursive(root, go => go.name == n);
            if (child != null)
            {
                var b = child.GetComponent<Button>();
                if (b != null) return b;
            }
        }
        // Contains keyword under root
        var buttons = root.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b != null && b.name.ToLower().Contains(containsKeyword.ToLower()))
                return b;
        }
        return null;
    }

    // Ask AssignmentManager to refresh active assignments from server
    void TryFetchAssignments()
    {
        try
        {
            var mgr = FindFirstObjectByType<AssignmentManager>();
            if (mgr == null)
            {
                // Touch singleton to ensure creation
                var _ = AssignmentManager.Instance;
                mgr = FindFirstObjectByType<AssignmentManager>();
            }
            if (mgr != null)
            {
                mgr.CheckForActiveAssignments();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"FinalNavigationFix: Unable to fetch assignments: {e.Message}");
        }
    }

    System.Collections.IEnumerator RefreshAssignmentAfterDelay(string subject, float delay)
    {
        yield return new WaitForSeconds(delay);
        UpdateStageButtonsForSubject(subject);
        PopulateAssignmentsForSubject(subject);
    }

    System.Collections.IEnumerator FetchSubjectAssignmentsAndUpdateUI(string subject)
    {
        int studentId = PlayerPrefs.GetInt("StudentID", 1);
        string key = NormalizeSubjectName(subject).ToUpperInvariant();
        string jsonBody = "{\"student_id\":" + studentId + ",\"subject\":\"" + subject + "\"}";
        string[][] attempts = new string[][] {
            new string[]{"POST", serverURL + "/student/assignments"},
            new string[]{"POST", serverURL + "/api/student/assignments"},
            new string[]{"GET", serverURL + "/student/assignments?student_id=" + studentId + "&subject=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(subject)},
            new string[]{"GET", serverURL + "/api/get_active_assignments?student_id=" + studentId + "&subject=" + UnityEngine.Networking.UnityWebRequest.EscapeURL(subject)}
        };

        bool fetched = false;
        string response = "";
        for (int i = 0; i < attempts.Length && !fetched; i++)
        {
            string method = attempts[i][0];
            string url = attempts[i][1];
            var req = new UnityEngine.Networking.UnityWebRequest(url, method);
            if (method == "POST")
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                req.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
                req.SetRequestHeader("Content-Type", "application/json");
            }
            req.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            Debug.Log($"FinalNavigationFix: Fetching assignments via {method} {url}");
            yield return req.SendWebRequest();
            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success && req.responseCode == 200)
            {
                response = req.downloadHandler.text;
                fetched = true;
            }
            else
            {
                Debug.LogWarning($"FinalNavigationFix: Fetch attempt failed ({(int)req.responseCode}) {req.error} at {url}");
            }
            req.Dispose();
        }

        if (fetched)
        {
            PlayerPrefs.SetString($"Assignments_{key}", response);
            string title = GetFirstAssignmentTitleFromJson(response);
            // robust id extraction
            string id = ExtractJsonValueSimple(response, "assignment_id");
            if (string.IsNullOrEmpty(id)) id = ExtractJsonValueSimple(response, "id");
            if (string.IsNullOrEmpty(id)) id = ExtractJsonValueSimple(response, "assignmentId");
            if (!string.IsNullOrEmpty(title))
            {
                PlayerPrefs.SetString($"ActiveAssignment_{key}_Title", title);
                if (!string.IsNullOrEmpty(id)) PlayerPrefs.SetString($"ActiveAssignment_{key}_Id", id);
                PlayerPrefs.SetString($"ActiveAssignment_{key}_Subject", subject);
                PlayerPrefs.Save();
            }
            UpdateStageButtonsForSubject(subject);
        }
    }
    
    void InitializeStageProgression()
    {
        // Note: Removed initial locking of stages 2 and 3
        // All stages will be enabled/disabled dynamically based on available assignments
        // This ensures that when teachers add assignments, all stages are accessible

        SetButtonInteractable(stageButton1, true);
        SetButtonInteractable(stageButton2, true);
        SetButtonInteractable(stageButton3, true);
    }
    
    void UnlockNextStage(string completedStage)
    {
        // Note: Removed progressive unlocking since all stages should be available
        // when teachers add assignments. This method is kept for compatibility
        // but no longer locks/unlocks stages.
        Debug.Log($"FinalNavigationFix: Completed {completedStage} - all stages remain available");
    }
    
    void SetButtonInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
            
            // Visual feedback
            CanvasGroup cg = button.GetComponent<CanvasGroup>();
            if (cg == null)
                cg = button.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = interactable ? 1f : 0.5f;
        }
    }

    [ContextMenu("Clear All Assignments (FinalNavigationFix)")]
    public void ClearAllAssignments()
    {
        Debug.Log("=== CLEARING ALL ASSIGNMENTS (FinalNavigationFix) ===");

        // Clear main assignment keys
        PlayerPrefs.DeleteKey("ActiveAssignmentSubject");
        PlayerPrefs.DeleteKey("ActiveAssignmentId");
        PlayerPrefs.DeleteKey("ActiveAssignmentTitle");
        PlayerPrefs.DeleteKey("ActiveAssignmentContent");
        PlayerPrefs.DeleteKey("ActiveAssignmentType");
        PlayerPrefs.DeleteKey("AssignmentCreatedTime");
        PlayerPrefs.DeleteKey("AssignmentSource");
        PlayerPrefs.DeleteKey("CurrentAssignmentId");
        PlayerPrefs.DeleteKey("CurrentAssignmentTitle");
        PlayerPrefs.DeleteKey("CurrentAssignmentContent");
        PlayerPrefs.DeleteKey("CurrentGameplayType");

        // Clear subject-specific assignments
        string[] subjects = { "Math", "Science", "English", "PE", "Art", "Mathematics", "History", "Geography" };
        foreach (string subject in subjects)
        {
            string key = NormalizeSubjectName(subject).ToUpperInvariant();
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Subject");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Id");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Title");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Content");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Type");
            PlayerPrefs.DeleteKey($"Assignments_{subject}");
            PlayerPrefs.DeleteKey($"Assignments_{key}");
            PlayerPrefs.DeleteKey($"SubjectAssignments_{subject}");
            PlayerPrefs.DeleteKey($"SubjectAssignments_{key}");

            // Clear assignment arrays (up to 20 assignments per subject)
            for (int i = 1; i <= 20; i++)
            {
                PlayerPrefs.DeleteKey($"Assignment_{subject}_{i}");
                PlayerPrefs.DeleteKey($"Assignment_{key}_{i}");
            }

            // Clear any other assignment-related keys for this subject
            PlayerPrefs.DeleteKey($"{subject}_Assignments");
            PlayerPrefs.DeleteKey($"{key}_Assignments");
            PlayerPrefs.DeleteKey($"{subject}_AssignmentCount");
            PlayerPrefs.DeleteKey($"{key}_AssignmentCount");
        }

        // Clear teacher assignments data
        PlayerPrefs.DeleteKey("TeacherAssignments");

        // Clear any remaining assignment-related keys
        PlayerPrefs.DeleteKey("AssignmentData");
        PlayerPrefs.DeleteKey("AllAssignments");
        PlayerPrefs.DeleteKey("StudentAssignments");

        // Clear any cached assignment data
        for (int i = 0; i < 50; i++)
        {
            PlayerPrefs.DeleteKey($"CachedAssignment_{i}");
            PlayerPrefs.DeleteKey($"AssignmentCache_{i}");
        }

        PlayerPrefs.Save();
        Debug.Log("✅ All assignments cleared from FinalNavigationFix!");
    }

    // --------------------- Dynamic Assignment List ---------------------
    void HideStaticStageButtons()
    {
        // Hide the old Stage 1/2/3 buttons to make room for dynamic list
        if (stageButton1 != null) stageButton1.gameObject.SetActive(false);
        if (stageButton2 != null) stageButton2.gameObject.SetActive(false);
        if (stageButton3 != null) stageButton3.gameObject.SetActive(false);
    }

    void BuildAssignmentListUI(GameObject panel)
    {
        if (panel == null) return;
        // Try find existing scroll/content
        if (assignmentsScroll == null)
        {
            assignmentsScroll = panel.GetComponentInChildren<UnityEngine.UI.ScrollRect>(true);
        }
        if (assignmentsScroll == null)
        {
            // Create scroll rect to replace static stage buttons
            GameObject scrollObj = new GameObject("AssignmentsScroll");
            scrollObj.transform.SetParent(panel.transform, false);
            var rect = scrollObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.1f, 0.2f);
            rect.anchorMax = new Vector2(0.9f, 0.8f);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var img = scrollObj.AddComponent<UnityEngine.UI.Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // visible background
            assignmentsScroll = scrollObj.AddComponent<UnityEngine.UI.ScrollRect>();
            assignmentsScroll.horizontal = false;
            assignmentsScroll.vertical = true;
            assignmentsScroll.scrollSensitivity = 30f;
            var mask = scrollObj.AddComponent<UnityEngine.UI.Mask>();
            mask.showMaskGraphic = false;
        }
        if (assignmentsContent == null)
        {
            GameObject contentObj = new GameObject("AssignmentsContent");
            contentObj.transform.SetParent(assignmentsScroll.transform, false);
            assignmentsContent = contentObj.AddComponent<RectTransform>();
            assignmentsScroll.content = assignmentsContent;
            assignmentsContent.anchorMin = new Vector2(0, 1);
            assignmentsContent.anchorMax = new Vector2(1, 1);
            assignmentsContent.pivot = new Vector2(0.5f, 1);
            var layout = contentObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            layout.spacing = 10f;
            layout.padding = new RectOffset(10, 10, 10, 10);
            var fitter = contentObj.AddComponent<UnityEngine.UI.ContentSizeFitter>();
            fitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        }
        if (assignmentButtonTemplate == null)
        {
            // Use stageButton1 as a visual template if available
            assignmentButtonTemplate = stageButton1;
        }
    }

    class SimpleAssignment
    {
        public string id;
        public string title;
        public string type;
        public string content;
    }

    [System.Serializable]
    class RawAssignment
    {
        public string subject;
        public string assignment_id;
        public string assignment_title;
        public string assignment_type;
        public string assignment_content;
        public string id; // alt
        public string title; // alt
        public string type; // alt
        public string content; // alt
    }

    [System.Serializable]
    class AssignmentsResponse
    {
        public RawAssignment[] assignments;
    }

    void PopulateAssignmentsForSubject(string subject)
    {
        if (assignmentsContent == null) return;
        // Clear existing children
        for (int i = assignmentsContent.childCount - 1; i >= 0; i--)
        {
            var child = assignmentsContent.GetChild(i);
            if (child != null) GameObject.Destroy(child.gameObject);
        }

        string key = NormalizeSubjectName(subject).ToUpperInvariant();
        string json = PlayerPrefs.GetString($"Assignments_{key}", "");
        var list = ExtractAssignmentsList(json);
        if (list.Count == 0)
        {
            // Fall back to per-subject single active assignment
            string singleTitle = PlayerPrefs.GetString($"ActiveAssignment_{key}_Title", "");
            string singleId = PlayerPrefs.GetString($"ActiveAssignment_{key}_Id", "");
            string singleType = PlayerPrefs.GetString($"ActiveAssignment_{key}_Type", "");
            string singleContent = PlayerPrefs.GetString($"ActiveAssignment_{key}_Content", "");
            if (!string.IsNullOrEmpty(singleTitle))
            {
                list.Add(new SimpleAssignment{ id = singleId, title = singleTitle, type = singleType, content = singleContent });
            }
        }

        if (list.Count == 0)
        {
            // Show "No assignments yet" message
            Button emptyBtn = CreateAssignmentButton("No assignments yet - teacher will add soon");
            emptyBtn.interactable = false;
            var btnImg = emptyBtn.GetComponent<UnityEngine.UI.Image>();
            if (btnImg != null) btnImg.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }
        else
        {
            foreach (var a in list)
            {
                Button btn = CreateAssignmentButton(a.title);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => SelectAssignmentAndLoad(subject, a));
            }
        }

    }

    Button CreateAssignmentButton(string title)
    {
        GameObject obj = new GameObject("AssignmentButton");
        obj.transform.SetParent(assignmentsContent, false);
        
        // Create button with visible styling
        var rect = obj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 60);
        
        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.2f, 0.7f, 0.2f, 0.9f); // Green background
        
        var btn = obj.AddComponent<Button>();
        
        // Create text label
        GameObject txtObj = new GameObject("Text");
        txtObj.transform.SetParent(obj.transform, false);
        var txtRect = txtObj.AddComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(10, 5);
        txtRect.offsetMax = new Vector2(-10, -5);
        
        var text = txtObj.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = string.IsNullOrEmpty(title) ? "Untitled Assignment" : title;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.fontSize = 18;
        text.color = Color.white;
        text.fontStyle = TMPro.FontStyles.Bold;
        
        // Add layout element for proper scrolling
        var layoutElement = obj.AddComponent<UnityEngine.UI.LayoutElement>();
        layoutElement.minHeight = 60;
        layoutElement.preferredHeight = 60;
        
        btn.interactable = true;
        Debug.Log($"Created assignment button: {title}");
        return btn;
    }

    void SetButtonLabel(Button button, string text)
    {
        if (button == null) return;
        var tmp = button.GetComponentInChildren<TMPro.TMP_Text>(true);
        if (tmp != null) { tmp.text = text; return; }
        var legacy = button.GetComponentInChildren<UnityEngine.UI.Text>(true);
        if (legacy != null) legacy.text = text;
    }

    void SelectAssignmentAndLoad(string subject, SimpleAssignment a)
    {
        PlayerPrefs.SetString("CurrentSubject", subject);
        if (!string.IsNullOrEmpty(a.id)) PlayerPrefs.SetString("CurrentAssignmentId", a.id);
        if (!string.IsNullOrEmpty(a.title)) PlayerPrefs.SetString("CurrentAssignmentTitle", a.title);
        if (!string.IsNullOrEmpty(a.content)) PlayerPrefs.SetString("CurrentAssignmentContent", a.content);
        if (!string.IsNullOrEmpty(a.type)) PlayerPrefs.SetString("CurrentGameplayType", a.type);
        PlayerPrefs.SetString("AssignmentSource", "teacher");
        PlayerPrefs.Save();
        LoadStage("Stage1");
    }

    void SetStageButtonsVisible(bool visible)
    {
        if (stageButton1 != null) stageButton1.gameObject.SetActive(visible);
        if (stageButton2 != null) stageButton2.gameObject.SetActive(visible);
        if (stageButton3 != null) stageButton3.gameObject.SetActive(visible);
    }

    List<SimpleAssignment> ExtractAssignmentsList(string json)
    {
        List<SimpleAssignment> res = new List<SimpleAssignment>();
        if (string.IsNullOrEmpty(json)) return res;
        // 1) Try structured parse via JsonUtility with wrapper
        try
        {
            AssignmentsResponse wrap = JsonUtility.FromJson<AssignmentsResponse>(json);
            if (wrap != null && wrap.assignments != null && wrap.assignments.Length > 0)
            {
                foreach (var ra in wrap.assignments)
                {
                    if (ra == null) continue;
                    string title = !string.IsNullOrEmpty(ra.assignment_title) ? ra.assignment_title : ra.title;
                    if (string.IsNullOrEmpty(title)) continue;
                    string id = !string.IsNullOrEmpty(ra.assignment_id) ? ra.assignment_id : ra.id;
                    string type = !string.IsNullOrEmpty(ra.assignment_type) ? ra.assignment_type : ra.type;
                    string content = !string.IsNullOrEmpty(ra.assignment_content) ? ra.assignment_content : ra.content;
                    res.Add(new SimpleAssignment { id = id, title = title, type = type, content = content });
                }
                if (res.Count > 0) return res;
            }
        }
        catch {}

        // 2) Fallback: naive parse - split by `{` ... `}` blocks containing title
        int idx = 0;
        while (idx < json.Length)
        {
            int start = json.IndexOf('{', idx);
            if (start < 0) break;
            int end = json.IndexOf('}', start + 1);
            if (end < 0) break;
            string chunk = json.Substring(start, end - start + 1);
            string title = ExtractJsonValueSimple(chunk, "assignment_title");
            if (string.IsNullOrEmpty(title)) title = ExtractJsonValueSimple(chunk, "title");
            if (!string.IsNullOrEmpty(title))
            {
                string id = ExtractJsonValueSimple(chunk, "assignment_id"); if (string.IsNullOrEmpty(id)) id = ExtractJsonValueSimple(chunk, "id");
                string type = ExtractJsonValueSimple(chunk, "assignment_type"); if (string.IsNullOrEmpty(type)) type = ExtractJsonValueSimple(chunk, "type");
                string content = ExtractJsonValueSimple(chunk, "assignment_content"); if (string.IsNullOrEmpty(content)) content = ExtractJsonValueSimple(chunk, "content");
                res.Add(new SimpleAssignment{ id = id, title = title, type = type, content = content });
            }
            idx = end + 1;
        }
        return res;
    }
    
    // Public methods for testing
    [ContextMenu("Test Math Navigation")]
    public void TestMath() => ShowStagePanel("Math");
    
    [ContextMenu("Test Science Navigation")]
    public void TestScience() => ShowStagePanel("Science");
    
    [ContextMenu("Hide Panel")]
    public void TestHide() => HideStagePanel();
    
    // Method to manually trigger setup if needed
    [ContextMenu("Reinitialize Navigation")]
    public void ReinitializeNavigation()
    {
        isInitialized = false;
        InitializeNavigation();
    }
}