using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using TMPro;
using System.Collections;

[System.Serializable]
public class StudentProfile
{
    public int id;
    public string name;
    public string email;
    public string gender;
    public string grade_level;
    public string class_name;
}

public class ProfileLoader : MonoBehaviour
{
    [Header("Profile UI")]
    public Image avatarImage;
    public Sprite Mavatar;
    public Sprite Favatar;

    public TMP_Text studentNameText;
    public TMP_Text gradeLevelText;
    public TMP_Text classNameText;
    
    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    // For local development, change to: "http://127.0.0.1:5000"
    public bool useWebAppData = true; // Toggle between web app and local data
    public int studentId = 1; // Set this to load specific student profile
    
    [Header("Loading UI")]
    public GameObject loadingPanel;
    public TMP_Text statusText;

    void Start()
    {
        try
        {
            // Auto-assign missing UI components before validation
            AutoAssignMissingReferences();
            
            // Validate UI components
            if (avatarImage == null)
                Debug.LogWarning("ProfileLoader: avatarImage is not assigned!");
            if (studentNameText == null)
                Debug.LogWarning("ProfileLoader: studentNameText is not assigned!");
            if (gradeLevelText == null)
                Debug.LogWarning("ProfileLoader: gradeLevelText is not assigned!");
            if (classNameText == null)
                Debug.LogWarning("ProfileLoader: classNameText is not assigned!");

            // Check if we're in offline mode
            bool offlineMode = PlayerPrefs.GetInt("OfflineMode", 0) == 1;
            
            if (useWebAppData && !offlineMode)
            {
                LoadProfileFromWebApp();
            }
            else
            {
                Debug.Log("ProfileLoader: Using offline mode or local data");
                LoadProfileFromPlayerPrefs();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"ProfileLoader Start error: {e.Message}");
            // Fallback to safe local loading
            UpdateStatus("Error loading profile - using safe mode", Color.red);
            LoadProfileFromPlayerPrefs();
        }
    }

    void LoadProfileFromWebApp()
    {
        StartCoroutine(LoadStudentProfileFromFlask());
    }
    
    private IEnumerator LoadStudentProfileFromFlask()
    {
        UpdateStatus("Loading student profile...", Color.yellow);
        
        string url = flaskURL + "/student/" + studentId + "/profile";
        UnityWebRequest request = UnityWebRequest.Get(url);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                string jsonResponse = request.downloadHandler.text;
                StudentProfile profile = JsonUtility.FromJson<StudentProfile>(jsonResponse);
                ApplyProfile(profile);
                UpdateStatus("Profile loaded successfully!", Color.green);
                
                // Hide status after 2 seconds
                StartCoroutine(HideStatusAfterDelay(2f));
            }
            catch (System.Exception e)
            {
                Debug.LogError("Failed to parse profile data: " + e.Message);
                UpdateStatus("Failed to load profile", Color.red);
                // Fallback to local data
                LoadProfileFromPlayerPrefs();
            }
        }
        else
        {
            Debug.LogWarning($"Profile loading failed: {request.error} (Code: {request.responseCode})");
            UpdateStatus("Web app offline - Using local data", Color.yellow);
            // Fallback to local data
            LoadProfileFromPlayerPrefs();
        }
        
        request.Dispose();
    }
    
    void ApplyProfile(StudentProfile profile)
    {
        // Set avatar based on gender using safe helper system
        if (avatarImage != null)
        {
            // If profile has gender info, save it and use it
            if (!string.IsNullOrEmpty(profile.gender))
            {
                GenderHelper.SaveGender(profile.gender);
            }
            
            // Update avatar using the safe helper
            GenderHelper.UpdateAvatarImage(avatarImage, Mavatar, Favatar);
        }

        // Set profile information with null checks
        if (studentNameText != null && !string.IsNullOrEmpty(profile.name)) 
            studentNameText.text = profile.name;
        if (gradeLevelText != null && !string.IsNullOrEmpty(profile.grade_level)) 
            gradeLevelText.text = profile.grade_level;
        if (classNameText != null && !string.IsNullOrEmpty(profile.class_name)) 
            classNameText.text = profile.class_name;
        
        // Save to PlayerPrefs for offline use
        if (!string.IsNullOrEmpty(profile.name))
            PlayerPrefs.SetString("StudentName", profile.name);
        if (!string.IsNullOrEmpty(profile.gender))
            PlayerPrefs.SetString("SelectedGender", profile.gender);
        if (!string.IsNullOrEmpty(profile.grade_level))
            PlayerPrefs.SetString("GradeLevel", profile.grade_level);
        if (!string.IsNullOrEmpty(profile.class_name))
            PlayerPrefs.SetString("ClassName", profile.class_name);
        PlayerPrefs.SetInt("StudentId", profile.id);
        PlayerPrefs.Save();
        
        Debug.Log($"Profile loaded: {profile.name} ({profile.gender}) - {profile.grade_level} in {profile.class_name}");
    }

    void LoadProfileFromPlayerPrefs()
    {
        UpdateStatus("Loading local profile...", Color.blue);
        
        // Load from PlayerPrefs (fallback/offline mode)
        string gender = PlayerPrefs.GetString("SelectedGender", "Male");
        
        // Get the actual logged-in user's name, not hardcoded
        string studentName = PlayerPrefs.GetString("StudentName", "");
        if (string.IsNullOrEmpty(studentName))
        {
            studentName = PlayerPrefs.GetString("LoggedInUser", "");
        }
        if (string.IsNullOrEmpty(studentName))
        {
            studentName = "Student"; // Generic fallback instead of hardcoded name
        }
        
        string gradeLevel = PlayerPrefs.GetString("GradeLevel", "Grade 7");
        string className = PlayerPrefs.GetString("ClassName", "Sample Class");

        // Set avatar based on gender using safe helper system
        if (avatarImage != null)
        {
            // Use GenderHelper for safe avatar updating
            GenderHelper.UpdateAvatarImage(avatarImage, Mavatar, Favatar);
        }

        // Set profile information with null checks
        if (studentNameText != null) studentNameText.text = studentName;
        if (gradeLevelText != null) gradeLevelText.text = gradeLevel;
        if (classNameText != null) classNameText.text = className;
        
        UpdateStatus("Local profile loaded", Color.green);
        
        // Hide status after 1 second
        StartCoroutine(HideStatusAfterDelay(1f));
        
        Debug.Log($"Local profile loaded: {studentName} ({gender}) - {gradeLevel}");
    }
    
    private IEnumerator HideStatusAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (loadingPanel) loadingPanel.SetActive(false);
    }
    
    void UpdateStatus(string message, Color color)
    {
        if (statusText)
        {
            statusText.text = message;
            statusText.color = color;
        }
        
        if (loadingPanel) loadingPanel.SetActive(true);
        
        Debug.Log($"ProfileLoader: {message}");
    }
    
    // Public method to refresh profile (can be called from buttons)
    public void RefreshProfile()
    {
        if (useWebAppData)
        {
            LoadProfileFromWebApp();
        }
        else
        {
            LoadProfileFromPlayerPrefs();
        }
    }
    
    // Public method to toggle data source
    public void ToggleDataSource()
    {
        useWebAppData = !useWebAppData;
        RefreshProfile();
    }
    
    // Auto-assign missing UI references
    void AutoAssignMissingReferences()
    {
        Debug.Log("ProfileLoader: Auto-assigning missing references");
        
        // Try to auto-assign classNameText if it's null
        if (classNameText == null)
        {
            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in textComponents)
            {
                if (text.name.ToLower().Contains("class"))
                {
                    classNameText = text;
                    Debug.Log($"ProfileLoader: Auto-assigned classNameText to: {text.name}");
                    break;
                }
            }
            
            // If still null, create a dummy component
            if (classNameText == null)
            {
                GameObject dummyObj = new GameObject("DummyClassNameText");
                dummyObj.transform.SetParent(this.transform);
                classNameText = dummyObj.AddComponent<TextMeshProUGUI>();
                classNameText.text = "Default Class";
                Debug.Log("ProfileLoader: Created dummy classNameText");
            }
        }
        
        // Try to auto-assign studentNameText if it's null
        if (studentNameText == null)
        {
            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in textComponents)
            {
                if (text.name.ToLower().Contains("name") || text.name.ToLower().Contains("student"))
                {
                    studentNameText = text;
                    Debug.Log($"ProfileLoader: Auto-assigned studentNameText to: {text.name}");
                    break;
                }
            }
            
            // If still null, create a dummy component
            if (studentNameText == null)
            {
                GameObject dummyObj = new GameObject("DummyStudentNameText");
                dummyObj.transform.SetParent(this.transform);
                studentNameText = dummyObj.AddComponent<TextMeshProUGUI>();
                studentNameText.text = "Default Student";
                Debug.Log("ProfileLoader: Created dummy studentNameText");
            }
        }
        
        // Try to auto-assign gradeLevelText if it's null
        if (gradeLevelText == null)
        {
            TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);
            foreach (var text in textComponents)
            {
                if (text.name.ToLower().Contains("grade") || text.name.ToLower().Contains("level"))
                {
                    gradeLevelText = text;
                    Debug.Log($"ProfileLoader: Auto-assigned gradeLevelText to: {text.name}");
                    break;
                }
            }
            
            // If still null, create a dummy component
            if (gradeLevelText == null)
            {
                GameObject dummyObj = new GameObject("DummyGradeLevelText");
                dummyObj.transform.SetParent(this.transform);
                gradeLevelText = dummyObj.AddComponent<TextMeshProUGUI>();
                gradeLevelText.text = "Default Grade";
                Debug.Log("ProfileLoader: Created dummy gradeLevelText");
            }
        }
        
        // Try to auto-assign avatarImage if it's null
        if (avatarImage == null)
        {
            Image[] imageComponents = GetComponentsInChildren<Image>(true);
            foreach (var img in imageComponents)
            {
                if (img.name.ToLower().Contains("avatar") || img.name.ToLower().Contains("profile"))
                {
                    avatarImage = img;
                    Debug.Log($"ProfileLoader: Auto-assigned avatarImage to: {img.name}");
                    break;
                }
            }
        }
    }
}
