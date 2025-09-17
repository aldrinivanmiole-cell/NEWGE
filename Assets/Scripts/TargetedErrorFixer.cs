using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Targeted fixer for specific missing script and reference issues
/// Fixes: UIManager missing script, ProfileLoader missing references
/// </summary>
[DefaultExecutionOrder(-2000)] // Execute very early
public class TargetedErrorFixer : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("=== TARGETED ERROR FIXER: Starting specific fixes ===");
        
        // Fix specific issues
        FixUIManagerMissingScript();
        FixProfileLoaderReferences();
        FixAllMissingScriptsByName();
        
        Debug.Log("=== TARGETED ERROR FIXER: Completed specific fixes ===");
    }
    
    /// <summary>
    /// Fix the missing script on UIManager GameObject
    /// </summary>
    void FixUIManagerMissingScript()
    {
        GameObject uiManager = GameObject.Find("uimanager");
        if (uiManager == null)
            uiManager = GameObject.Find("UIManager");
        if (uiManager == null)
            uiManager = GameObject.Find("UI Manager");
        
        if (uiManager != null)
        {
            Debug.Log("TargetedErrorFixer: Found UIManager object, checking for missing scripts");
            
            Component[] components = uiManager.GetComponents<Component>();
            bool hasMissingScript = false;
            
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.Log($"TargetedErrorFixer: Found missing script at index {i} on UIManager");
                    hasMissingScript = true;
                }
            }
            
            if (hasMissingScript)
            {
                // Add a replacement UI manager component
                UIManagerReplacement replacement = uiManager.GetComponent<UIManagerReplacement>();
                if (replacement == null)
                {
                    replacement = uiManager.AddComponent<UIManagerReplacement>();
                    Debug.Log("TargetedErrorFixer: Added UIManagerReplacement to fix missing script");
                }
            }
        }
        else
        {
            Debug.LogWarning("TargetedErrorFixer: UIManager GameObject not found");
        }
    }
    
    /// <summary>
    /// Fix ProfileLoader missing text references
    /// </summary>
    void FixProfileLoaderReferences()
    {
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        
        foreach (ProfileLoader profileLoader in profileLoaders)
        {
            if (profileLoader != null)
            {
                Debug.Log("TargetedErrorFixer: Fixing ProfileLoader references");
                
                // Use reflection to access private fields
                var type = typeof(ProfileLoader);
                
                // Fix classNameText
                var classNameField = type.GetField("classNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (classNameField != null && classNameField.GetValue(profileLoader) == null)
                {
                    TMP_Text classNameText = FindTextComponent("classNameText", "ClassName", "Class Name");
                    if (classNameText != null)
                    {
                        classNameField.SetValue(profileLoader, classNameText);
                        Debug.Log("TargetedErrorFixer: Fixed ProfileLoader classNameText reference");
                    }
                    else
                    {
                        // Create a dummy text component
                        GameObject dummyObj = new GameObject("DummyClassNameText");
                        TMP_Text dummyText = dummyObj.AddComponent<TextMeshProUGUI>();
                        dummyText.text = "Default Class";
                        classNameField.SetValue(profileLoader, dummyText);
                        Debug.Log("TargetedErrorFixer: Created dummy classNameText for ProfileLoader");
                    }
                }
                
                // Fix studentNameText if also missing
                var studentNameField = type.GetField("studentNameText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (studentNameField != null && studentNameField.GetValue(profileLoader) == null)
                {
                    TMP_Text studentNameText = FindTextComponent("studentNameText", "StudentName", "Student Name");
                    if (studentNameText != null)
                    {
                        studentNameField.SetValue(profileLoader, studentNameText);
                        Debug.Log("TargetedErrorFixer: Fixed ProfileLoader studentNameText reference");
                    }
                }
                
                // Fix gradeLevelText if also missing
                var gradeLevelField = type.GetField("gradeLevelText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (gradeLevelField != null && gradeLevelField.GetValue(profileLoader) == null)
                {
                    TMP_Text gradeLevelText = FindTextComponent("gradeLevelText", "GradeLevel", "Grade Level");
                    if (gradeLevelText != null)
                    {
                        gradeLevelField.SetValue(profileLoader, gradeLevelText);
                        Debug.Log("TargetedErrorFixer: Fixed ProfileLoader gradeLevelText reference");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Find text component by various possible names
    /// </summary>
    TMP_Text FindTextComponent(params string[] possibleNames)
    {
        foreach (string name in possibleNames)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null)
            {
                TMP_Text text = obj.GetComponent<TMP_Text>();
                if (text != null) return text;
                
                TextMeshProUGUI textUGUI = obj.GetComponent<TextMeshProUGUI>();
                if (textUGUI != null) return textUGUI;
            }
        }
        return null;
    }
    
    /// <summary>
    /// Fix all GameObjects with missing scripts by name
    /// </summary>
    void FixAllMissingScriptsByName()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int fixedCount = 0;
        
        foreach (GameObject obj in allObjects)
        {
            Component[] components = obj.GetComponents<Component>();
            bool hasMissingScript = false;
            
            foreach (Component comp in components)
            {
                if (comp == null)
                {
                    hasMissingScript = true;
                    break;
                }
            }
            
            if (hasMissingScript)
            {
                Debug.Log($"TargetedErrorFixer: Fixing missing script on {obj.name}");
                AddTargetedReplacement(obj);
                fixedCount++;
            }
        }
        
        Debug.Log($"TargetedErrorFixer: Fixed {fixedCount} objects with missing scripts");
    }
    
    /// <summary>
    /// Add targeted replacement based on GameObject name
    /// </summary>
    void AddTargetedReplacement(GameObject obj)
    {
        string objName = obj.name.ToLower();
        
        // Specific fixes based on object names
        if (objName.Contains("uimanager") || objName.Contains("ui manager"))
        {
            if (obj.GetComponent<UIManagerReplacement>() == null)
                obj.AddComponent<UIManagerReplacement>();
        }
        else if (objName.Contains("math"))
        {
            if (obj.GetComponent<SubjectButtonReplacement>() == null)
            {
                var replacement = obj.AddComponent<SubjectButtonReplacement>();
                replacement.subjectName = "Math";
            }
        }
        else if (objName.Contains("science"))
        {
            if (obj.GetComponent<SubjectButtonReplacement>() == null)
            {
                var replacement = obj.AddComponent<SubjectButtonReplacement>();
                replacement.subjectName = "Science";
            }
        }
        else if (objName.Contains("english"))
        {
            if (obj.GetComponent<SubjectButtonReplacement>() == null)
            {
                var replacement = obj.AddComponent<SubjectButtonReplacement>();
                replacement.subjectName = "English";
            }
        }
        else if (objName.Contains("art"))
        {
            if (obj.GetComponent<SubjectButtonReplacement>() == null)
            {
                var replacement = obj.AddComponent<SubjectButtonReplacement>();
                replacement.subjectName = "Art";
            }
        }
        else if (objName.Contains("button"))
        {
            if (obj.GetComponent<Button>() == null)
                obj.AddComponent<Button>();
        }
        else if (objName.Contains("panel"))
        {
            if (obj.GetComponent<Image>() == null)
                obj.AddComponent<Image>();
        }
        else
        {
            // Generic replacement
            if (obj.GetComponent<GenericScriptReplacement>() == null)
                obj.AddComponent<GenericScriptReplacement>();
        }
    }
}

/// <summary>
/// Replacement for missing UIManager script
/// </summary>
public class UIManagerReplacement : MonoBehaviour
{
    [Header("UI Manager Replacement")]
    public string originalScript = "UIManager (missing)";
    
    void Start()
    {
        Debug.Log("UIManagerReplacement: Active - replacing missing UIManager script");
        
        // Ensure we have the navigation fix
        if (FindFirstObjectByType<FinalNavigationFix>() == null)
        {
            GameObject navObj = new GameObject("UIManager_NavigationFix");
            navObj.AddComponent<FinalNavigationFix>();
            Debug.Log("UIManagerReplacement: Added navigation fix from UIManager");
        }
    }
    
    // Add any UI management functionality that might be needed
    public void RefreshUI()
    {
        Debug.Log("UIManagerReplacement: RefreshUI called");
    }
    
    public void UpdateDisplay()
    {
        Debug.Log("UIManagerReplacement: UpdateDisplay called");
    }
}

/// <summary>
/// Replacement for missing subject button scripts
/// </summary>
public class SubjectButtonReplacement : MonoBehaviour
{
    public string subjectName = "";
    
    void Start()
    {
        var button = GetComponent<Button>();
        if (button == null)
            button = gameObject.AddComponent<Button>();
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnButtonClick);
        
        Debug.Log($"SubjectButtonReplacement: Setup {subjectName} button replacement");
    }
    
    void OnButtonClick()
    {
        Debug.Log($"SubjectButtonReplacement: {subjectName} clicked");
        
        var navFix = FindFirstObjectByType<FinalNavigationFix>();
        if (navFix != null)
        {
            navFix.ShowStagePanel(subjectName);
        }
        else
        {
            Debug.LogWarning($"SubjectButtonReplacement: No FinalNavigationFix found for {subjectName}");
        }
    }
}

/// <summary>
/// Generic replacement for any missing script
/// </summary>
public class GenericScriptReplacement : MonoBehaviour
{
    public string objectName;
    public string replacementNote = "Generic replacement for missing script";
    
    void Start()
    {
        objectName = gameObject.name;
        Debug.Log($"GenericScriptReplacement: Active on {objectName}");
    }
}