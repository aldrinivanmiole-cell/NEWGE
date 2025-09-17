using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Specific error fixer for GameStageManager and ProfileLoader issues
/// </summary>
public class SpecificErrorFixer : MonoBehaviour
{
    [Header("Error Fixing")]
    public bool fixOnStart = true;
    
    void Start()
    {
        if (fixOnStart)
        {
            FixGameStageManagerErrors();
            FixProfileLoaderErrors();
            FixStageNavigationIssues();
        }
    }
    
    void Update()
    {
        // Continuously check for GameStageManager errors every few seconds
        if (Time.time % 3f < 0.1f) // Check every 3 seconds
        {
            FixGameStageManagerErrors();
        }
    }
    
    /// <summary>
    /// Fix stage navigation issues by ensuring FinalNavigationFix is active
    /// </summary>
    void FixStageNavigationIssues()
    {
        // Check if FinalNavigationFix exists, if not add it
        FinalNavigationFix navFixer = FindFirstObjectByType<FinalNavigationFix>();
        if (navFixer == null)
        {
            GameObject navFixerObj = new GameObject("FinalNavigationFix");
            navFixer = navFixerObj.AddComponent<FinalNavigationFix>();
            Debug.Log("SPECIFIC ERROR FIXER: Added FinalNavigationFix to fix navigation issues");
        }
        
        // Also check for the old StageNavigationFixer and disable it
        StageNavigationFixer[] oldFixers = FindObjectsByType<StageNavigationFixer>(FindObjectsSortMode.None);
        foreach (StageNavigationFixer oldFixer in oldFixers)
        {
            if (oldFixer != null)
            {
                oldFixer.enabled = false;
                Debug.Log("SPECIFIC ERROR FIXER: Disabled old StageNavigationFixer");
            }
        }
    }
    
    /// <summary>
    /// Fix GameStageManager not found errors by finding or creating GameStageManager reference
    /// </summary>
    void FixGameStageManagerErrors()
    {
        // Find all FillBlankDropZone components and assign the GameStageManager
        FillBlankDropZone[] dropZones = FindObjectsByType<FillBlankDropZone>(FindObjectsSortMode.None);
        GameStageManager manager = FindFirstObjectByType<GameStageManager>();
        
        // If no GameStageManager exists, create one
        if (manager == null)
        {
            GameObject gameStageManagerObj = new GameObject("GameStageManager");
            manager = gameStageManagerObj.AddComponent<GameStageManager>();
            Debug.Log("SPECIFIC ERROR FIXER: Created missing GameStageManager GameObject");
        }
        
        foreach (var dropZone in dropZones)
        {
            // The DropZone script uses "attackController" field for GameStageManager
            if (dropZone.attackController == null)
            {
                dropZone.attackController = manager;
                Debug.Log($"SPECIFIC ERROR FIXER: Assigned GameStageManager to {dropZone.name}");
            }
        }
    }
    
    /// <summary>
    /// Fix ProfileLoader classNameText assignment warnings
    /// </summary>
    void FixProfileLoaderErrors()
    {
        ProfileLoader[] profileLoaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        
        foreach (var profileLoader in profileLoaders)
        {
            if (profileLoader.classNameText == null)
            {
                // Try to find a suitable TMP_Text component
                TMP_Text[] textComponents = profileLoader.GetComponentsInChildren<TMP_Text>();
                
                foreach (var textComp in textComponents)
                {
                    // Look for text components that might be for class name
                    if (textComp.name.ToLower().Contains("class") || 
                        textComp.name.ToLower().Contains("name") ||
                        textComp.text.Contains("Class") ||
                        textComp.text == "")
                    {
                        profileLoader.classNameText = textComp;
                        Debug.Log($"SPECIFIC ERROR FIXER: Assigned classNameText to ProfileLoader: {textComp.name}");
                        break;
                    }
                }
                
                // If still null, create a dummy text component
                if (profileLoader.classNameText == null)
                {
                    GameObject textObj = new GameObject("ClassNameText");
                    textObj.transform.SetParent(profileLoader.transform);
                    TMP_Text newText = textObj.AddComponent<TextMeshProUGUI>();
                    newText.text = "Class Name";
                    newText.fontSize = 14;
                    
                    profileLoader.classNameText = newText;
                    Debug.Log("SPECIFIC ERROR FIXER: Created dummy classNameText for ProfileLoader");
                }
            }
        }
    }
}