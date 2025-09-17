using UnityEngine;

/// <summary>
/// Utility script to find and remove the mysterious "da" assignment
/// </summary>
public class DaAssignmentFixer : MonoBehaviour
{
    [ContextMenu("Find the 'da' Assignment")]
    public void FindDaAssignment()
    {
        Debug.Log("=== SEARCHING FOR 'da' ASSIGNMENT ===");
        
        // Check common PlayerPrefs keys that might contain "da"
        string[] possibleKeys = {
            "da", "DA", "Da",
            "ActiveAssignmentTitle", "CurrentAssignmentTitle",
            "Assignment_English_1", "Assignment_ENGLISH_1", "Assignment_english_1",
            "Assignment_English_2", "Assignment_ENGLISH_2", "Assignment_english_2", 
            "Assignment_English_3", "Assignment_ENGLISH_3", "Assignment_english_3",
            "English_Assignment_1", "ENGLISH_Assignment_1", "english_Assignment_1",
            "English_da", "ENGLISH_da", "english_da",
            "SubjectAssignments_English", "SubjectAssignments_ENGLISH",
            "Assignments_English", "Assignments_ENGLISH",
            "ActiveAssignment_ENGLISH_Title", "ActiveAssignment_ENGLISH_Id",
            "ActiveAssignment_English_Title", "ActiveAssignment_English_Id"
        };

        bool foundDa = false;
        foreach (string key in possibleKeys)
        {
            string value = PlayerPrefs.GetString(key, "");
            if (!string.IsNullOrEmpty(value))
            {
                Debug.LogError($"🔍 FOUND DATA: {key} = '{value}'");
                if (value.Contains("da") || value.Equals("da", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError($"🎯 FOUND THE 'da' SOURCE: {key} = '{value}'");
                    foundDa = true;
                }
            }
        }

        if (!foundDa)
        {
            Debug.Log("❓ No 'da' found in PlayerPrefs. It might be:");
            Debug.Log("   • Hardcoded in UI elements");
            Debug.Log("   • Coming from server/API");
            Debug.Log("   • In a different storage location");
        }
    }

    [ContextMenu("Delete All 'da' Related Data")]
    public void DeleteDaRelatedData()
    {
        Debug.Log("=== DELETING ALL 'da' RELATED DATA ===");
        
        string[] keysToDelete = {
            "da", "DA", "Da",
            "ActiveAssignmentTitle", "CurrentAssignmentTitle",
            "Assignment_English_1", "Assignment_ENGLISH_1", "Assignment_english_1",
            "Assignment_English_2", "Assignment_ENGLISH_2", "Assignment_english_2",
            "Assignment_English_3", "Assignment_ENGLISH_3", "Assignment_english_3",
            "English_Assignment_1", "ENGLISH_Assignment_1", "english_Assignment_1",
            "English_da", "ENGLISH_da", "english_da",
            "SubjectAssignments_English", "SubjectAssignments_ENGLISH",
            "Assignments_English", "Assignments_ENGLISH",
            "ActiveAssignment_ENGLISH_Title", "ActiveAssignment_ENGLISH_Id",
            "ActiveAssignment_English_Title", "ActiveAssignment_English_Id",
            "ActiveAssignmentSubject", "ActiveAssignmentId", "ActiveAssignmentContent"
        };

        int deletedCount = 0;
        foreach (string key in keysToDelete)
        {
            if (PlayerPrefs.HasKey(key))
            {
                string value = PlayerPrefs.GetString(key, "");
                Debug.Log($"🗑️ Deleting: {key} = '{value}'");
                PlayerPrefs.DeleteKey(key);
                deletedCount++;
            }
        }

        PlayerPrefs.Save();
        Debug.LogError($"✅ Deleted {deletedCount} keys. The 'da' assignment should now be gone!");
        Debug.LogError("🎯 Test English subject now - should show 'No Assignments Yet'");
    }

    [ContextMenu("NUCLEAR - Delete ALL PlayerPrefs")]
    public void NuclearDeleteAll()
    {
        Debug.LogError("=== NUCLEAR OPTION - DELETING ALL PLAYERPREFS ===");
        Debug.LogError("⚠️⚠️⚠️ WARNING: This deletes ALL saved data including login! ⚠️⚠️⚠️");
        
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        Debug.LogError("💥 ALL PLAYERPREFS DELETED!");
        Debug.LogError("🔄 You will need to log in again");
        Debug.LogError("✅ The 'da' assignment is definitely gone now!");
    }

    [ContextMenu("Debug - List English Assignment Data")]
    public void DebugEnglishAssignmentData()
    {
        Debug.Log("=== ENGLISH ASSIGNMENT DATA DEBUG ===");
        
        // Check what the HasAnyAssignmentsForSubject method would find
        string subject = "English";
        
        // Check active assignment
        string activeSubject = PlayerPrefs.GetString("ActiveAssignmentSubject", "");
        if (!string.IsNullOrEmpty(activeSubject))
        {
            Debug.Log($"ActiveAssignmentSubject: {activeSubject}");
            if (activeSubject.Equals(subject, System.StringComparison.OrdinalIgnoreCase))
            {
                string title = PlayerPrefs.GetString("ActiveAssignmentTitle", "");
                Debug.LogError($"🎯 ACTIVE ASSIGNMENT FOUND: {title}");
            }
        }

        // Check JSON assignments
        string assignmentsJson = PlayerPrefs.GetString("SubjectAssignments_" + subject, "");
        if (!string.IsNullOrEmpty(assignmentsJson))
        {
            Debug.LogError($"🎯 JSON ASSIGNMENTS FOUND: {assignmentsJson}");
        }

        // Check assignment arrays
        for (int i = 1; i <= 10; i++)
        {
            string assignmentKey = $"Assignment_{subject}_{i}";
            string assignmentData = PlayerPrefs.GetString(assignmentKey, "");
            if (!string.IsNullOrEmpty(assignmentData))
            {
                Debug.LogError($"🎯 ARRAY ASSIGNMENT FOUND: {assignmentKey} = {assignmentData}");
            }
        }
        
        Debug.Log("=== END DEBUG ===");
    }
}