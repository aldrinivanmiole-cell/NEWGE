using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

/// <summary>
/// Universal UI Reference Auto-Assigner
/// Automatically finds and assigns UI component references for any MonoBehaviour
/// </summary>
public class UniversalUIFixer : MonoBehaviour
{
    [Header("Auto-Assignment Settings")]
    public bool fixOnStart = true;
    public bool logAssignments = true;
    public bool useSmartMatching = true;
    public bool searchInChildren = true;
    public bool searchInParent = false;
    
    [Header("Search Patterns")]
    public List<string> textPatterns = new List<string> { "text", "label", "title", "name", "info" };
    public List<string> buttonPatterns = new List<string> { "button", "btn", "click" };
    public List<string> imagePatterns = new List<string> { "image", "img", "icon", "avatar", "picture" };
    public List<string> inputPatterns = new List<string> { "input", "field", "entry" };
    
    void Start()
    {
        if (fixOnStart)
        {
            StartCoroutine(FixAllUIReferencesDelayed());
        }
    }
    
    IEnumerator FixAllUIReferencesDelayed()
    {
        yield return new WaitForEndOfFrame(); // Wait for all objects to initialize
        
        FixAllUIReferences();
    }
    
    [ContextMenu("Fix All UI References")]
    public void FixAllUIReferences()
    {
        if (logAssignments)
            Debug.Log("=== UNIVERSAL UI FIXER: Starting UI reference fixing ===");
        
        MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        int totalFixed = 0;
        
        foreach (MonoBehaviour behaviour in allMonoBehaviours)
        {
            if (behaviour == null || behaviour == this) continue;
            
            int fixedCount = FixUIReferencesForComponent(behaviour);
            totalFixed += fixedCount;
        }
        
        if (logAssignments)
            Debug.Log($"UniversalUIFixer: Fixed {totalFixed} UI references across all components");
    }
    
    int FixUIReferencesForComponent(MonoBehaviour component)
    {
        if (component == null) return 0;
        
        int fixedCount = 0;
        System.Type componentType = component.GetType();
        FieldInfo[] fields = componentType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        foreach (FieldInfo field in fields)
        {
            // Skip if field is already assigned
            if (field.GetValue(component) != null) continue;
            
            bool wasFixed = false;
            
            // Try to assign based on field type
            if (field.FieldType == typeof(TMP_Text))
            {
                wasFixed = AssignTMPText(component, field);
            }
            else if (field.FieldType == typeof(Text))
            {
                wasFixed = AssignText(component, field);
            }
            else if (field.FieldType == typeof(Button))
            {
                wasFixed = AssignButton(component, field);
            }
            else if (field.FieldType == typeof(Image))
            {
                wasFixed = AssignImage(component, field);
            }
            else if (field.FieldType == typeof(TMP_InputField))
            {
                wasFixed = AssignTMPInputField(component, field);
            }
            else if (field.FieldType == typeof(InputField))
            {
                wasFixed = AssignInputField(component, field);
            }
            else if (field.FieldType == typeof(Toggle))
            {
                wasFixed = AssignToggle(component, field);
            }
            else if (field.FieldType == typeof(Slider))
            {
                wasFixed = AssignSlider(component, field);
            }
            else if (field.FieldType == typeof(Dropdown))
            {
                wasFixed = AssignDropdown(component, field);
            }
            else if (field.FieldType == typeof(TMP_Dropdown))
            {
                wasFixed = AssignTMPDropdown(component, field);
            }
            
            if (wasFixed)
            {
                fixedCount++;
                if (logAssignments)
                    Debug.Log($"Auto-assigned {field.Name} for {componentType.Name} on {component.gameObject.name}");
            }
        }
        
        return fixedCount;
    }
    
    bool AssignTMPText(MonoBehaviour component, FieldInfo field)
    {
        TMP_Text[] candidates = GetUIComponents<TMP_Text>(component);
        TMP_Text bestMatch = FindBestMatch(candidates, field.Name, textPatterns);
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignText(MonoBehaviour component, FieldInfo field)
    {
        Text[] candidates = GetUIComponents<Text>(component);
        Text bestMatch = FindBestMatch(candidates, field.Name, textPatterns);
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignButton(MonoBehaviour component, FieldInfo field)
    {
        Button[] candidates = GetUIComponents<Button>(component);
        Button bestMatch = FindBestMatch(candidates, field.Name, buttonPatterns);
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignImage(MonoBehaviour component, FieldInfo field)
    {
        Image[] candidates = GetUIComponents<Image>(component);
        Image bestMatch = FindBestMatch(candidates, field.Name, imagePatterns);
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignTMPInputField(MonoBehaviour component, FieldInfo field)
    {
        TMP_InputField[] candidates = GetUIComponents<TMP_InputField>(component);
        TMP_InputField bestMatch = FindBestMatch(candidates, field.Name, inputPatterns);
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignInputField(MonoBehaviour component, FieldInfo field)
    {
        InputField[] candidates = GetUIComponents<InputField>(component);
        InputField bestMatch = FindBestMatch(candidates, field.Name, inputPatterns);
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignToggle(MonoBehaviour component, FieldInfo field)
    {
        Toggle[] candidates = GetUIComponents<Toggle>(component);
        Toggle bestMatch = FindBestMatch(candidates, field.Name, new List<string> { "toggle", "check", "switch" });
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignSlider(MonoBehaviour component, FieldInfo field)
    {
        Slider[] candidates = GetUIComponents<Slider>(component);
        Slider bestMatch = FindBestMatch(candidates, field.Name, new List<string> { "slider", "bar", "progress" });
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignDropdown(MonoBehaviour component, FieldInfo field)
    {
        Dropdown[] candidates = GetUIComponents<Dropdown>(component);
        Dropdown bestMatch = FindBestMatch(candidates, field.Name, new List<string> { "dropdown", "select", "choice" });
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    bool AssignTMPDropdown(MonoBehaviour component, FieldInfo field)
    {
        TMP_Dropdown[] candidates = GetUIComponents<TMP_Dropdown>(component);
        TMP_Dropdown bestMatch = FindBestMatch(candidates, field.Name, new List<string> { "dropdown", "select", "choice" });
        
        if (bestMatch != null)
        {
            field.SetValue(component, bestMatch);
            return true;
        }
        
        return false;
    }
    
    T[] GetUIComponents<T>(MonoBehaviour component) where T : Component
    {
        List<T> components = new List<T>();
        
        // Search in the same GameObject
        T selfComponent = component.GetComponent<T>();
        if (selfComponent != null)
            components.Add(selfComponent);
        
        // Search in children if enabled
        if (searchInChildren)
        {
            T[] childComponents = component.GetComponentsInChildren<T>(true);
            components.AddRange(childComponents);
        }
        
        // Search in parent if enabled
        if (searchInParent && component.transform.parent != null)
        {
            T[] parentComponents = component.GetComponentsInParent<T>(true);
            components.AddRange(parentComponents);
        }
        
        return components.ToArray();
    }
    
    T FindBestMatch<T>(T[] candidates, string fieldName, List<string> patterns) where T : Component
    {
        if (candidates.Length == 0) return null;
        
        // If only one candidate, return it
        if (candidates.Length == 1) return candidates[0];
        
        if (!useSmartMatching) return candidates[0];
        
        // Score each candidate
        T bestMatch = null;
        int bestScore = -1;
        
        foreach (T candidate in candidates)
        {
            int score = CalculateMatchScore(candidate.name, fieldName, patterns);
            
            if (score > bestScore)
            {
                bestScore = score;
                bestMatch = candidate;
            }
        }
        
        return bestMatch ?? candidates[0];
    }
    
    int CalculateMatchScore(string objectName, string fieldName, List<string> patterns)
    {
        int score = 0;
        string lowerObjectName = objectName.ToLower();
        string lowerFieldName = fieldName.ToLower();
        
        // Exact match with field name gets highest score
        if (lowerObjectName.Contains(lowerFieldName))
            score += 100;
        
        // Pattern matching
        foreach (string pattern in patterns)
        {
            if (lowerObjectName.Contains(pattern.ToLower()))
                score += 50;
            
            if (lowerFieldName.Contains(pattern.ToLower()))
                score += 30;
        }
        
        // Bonus for common UI naming conventions
        if (lowerObjectName.Contains("text") && lowerFieldName.Contains("text"))
            score += 25;
        
        if (lowerObjectName.Contains("btn") && lowerFieldName.Contains("button"))
            score += 25;
        
        if (lowerObjectName.Contains("img") && lowerFieldName.Contains("image"))
            score += 25;
        
        return score;
    }
    
    [ContextMenu("Fix ProfileLoader References")]
    public void FixProfileLoaderReferences()
    {
        ProfileLoader[] loaders = FindObjectsByType<ProfileLoader>(FindObjectsSortMode.None);
        foreach (var loader in loaders)
        {
            FixUIReferencesForComponent(loader);
        }
    }
    
    [ContextMenu("Fix All GameMechanic References")]
    public void FixGameMechanicReferences()
    {
        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
        foreach (var behaviour in behaviours)
        {
            if (behaviour.GetType().Name.ToLower().Contains("game") ||
                behaviour.GetType().Name.ToLower().Contains("mechanic"))
            {
                FixUIReferencesForComponent(behaviour);
            }
        }
    }
}