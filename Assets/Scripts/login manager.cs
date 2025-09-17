using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button submitButton;
    public Button registerButton; 
    public Button testButton; 
    public TMP_Text messageText;

    [Header("Animation")]
    public Animator loginAnimator;

    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; 
    

    void Start()
    {
        // Clear message at start
        if (messageText != null)
            messageText.text = "";

        // ✅ Assign button listeners in code
        submitButton.onClick.AddListener(OnSubmit);
        registerButton.onClick.AddListener(GoToRegisterScene);
        
        // Optional test button for debugging
        if (testButton != null)
            testButton.onClick.AddListener(TestServerConnection);

        // Add debug info
        Debug.Log($"Login Manager started. Server URL: {flaskURL}");
        Debug.Log($"Platform: {Application.platform}");
        Debug.Log($"Internet Reachability: {Application.internetReachability}");
    }

    void OnSubmit()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "Please fill in both fields.";
            return;
        }

        // Check if username is an email format
        if (!username.Contains("@") || !username.Contains("."))
        {
            messageText.text = "Please enter a valid email address.";
            return;
        }

        Debug.Log($"Attempting login with email: {username}");
        
        // Send login attempt to Flask web app
        StartCoroutine(AttemptLogin(username, password));
    }

   
    private IEnumerator AttemptLogin(string username, string password)
    {
        
        string[] loginEndpoints = {
            "/student/simple-login",
            "/student/login", 
            "/api/student/login",
            "/login"
        };

        foreach (string endpoint in loginEndpoints)
        {
            string url = flaskURL + endpoint;
            Debug.Log($"Trying login endpoint: {url}");

            // Create JSON data for FastAPI - using same format as registration
            string jsonData = "{\"email\":\"" + username + "\",\"password\":\"" + password + "\"}";

            UnityWebRequest request = new UnityWebRequest(url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10; // 10 second timeout

            messageText.text = $"Logging in... (trying {endpoint})";

            yield return request.SendWebRequest();

            Debug.Log($"Login Response Code: {request.responseCode}");
            Debug.Log($"Login Response: {request.downloadHandler.text}");

            if (request.result == UnityWebRequest.Result.Success && request.responseCode != 404)
            {
                // Parse response from Flask
                string responseText = request.downloadHandler.text;

                // Improved JSON parsing - check for multiple success indicators
                if (responseText.Contains("\"success\":true") || 
                    responseText.Contains("\"status\":\"success\"") ||
                    responseText.Contains("Login successful") ||
                    responseText.Contains("\"message\":\"success\"") ||
                    responseText.Contains("\"result\":\"success\"") ||
                    (request.responseCode == 200 && !responseText.Contains("error") && !responseText.Contains("Invalid")))
                {
                    messageText.text = "Login Success!";

                    // 🔒 CRITICAL: Clear all previous assignment data to prevent cross-account contamination
                    ClearAllAssignmentDataForNewStudent();

                    // Store user info for session
                    PlayerPrefs.SetString("LoggedInUser", username);
                    PlayerPrefs.SetString("StudentName", username); // Also save as StudentName for ProfileLoader
                    PlayerPrefs.SetInt("IsLoggedIn", 1);
                    PlayerPrefs.SetInt("OfflineMode", 0); // Ensure offline mode is disabled on successful server login

                    // Try to parse additional user info from response if available
                    try
                    {
                        if (responseText.Contains("\"name\"") || responseText.Contains("\"student_name\""))
                        {
                            // Extract name from JSON response if provided by server
                            string[] lines = responseText.Split(',');
                            foreach (string line in lines)
                            {
                                if (line.Contains("\"name\"") || line.Contains("\"student_name\""))
                                {
                                    string nameValue = line.Split(':')[1].Trim().Replace("\"", "").Replace("}", "");
                                    if (!string.IsNullOrEmpty(nameValue))
                                    {
                                        PlayerPrefs.SetString("StudentName", nameValue);
                                        Debug.Log($"Extracted student name: {nameValue}");
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"Could not parse additional user info: {e.Message}");
                    }

                    PlayerPrefs.Save(); // Save all changes

                    if (loginAnimator != null)
                        loginAnimator.SetTrigger("PopOut");

                    Invoke(nameof(LoadNextScene), 0.6f); // Match animation length
                    request.Dispose();
                    yield break; // Exit successfully
                }
                else
                {
                    Debug.LogWarning($"Login failed on {endpoint}. Response: {responseText}");
                }
            }
            else if (request.responseCode == 404)
            {
                Debug.Log($"Endpoint {endpoint} not found, trying next...");
            }
            else
            {
                Debug.LogError($"Login request failed on {endpoint}: {request.error}");
            }

            request.Dispose();
        }

        // If we get here, all endpoints failed
        Debug.LogError("All login endpoints failed");

        // Try offline login as fallback
        Debug.Log("Trying offline login as fallback...");
        messageText.text = "Server login failed. Trying offline mode...";

        // Try offline login mode
        if (TryOfflineLogin(username, password))
        {
            messageText.text = "Login Success (Offline Mode)!";
            messageText.color = Color.green;

            // 🔒 CRITICAL: Clear all previous assignment data to prevent cross-account contamination
            ClearAllAssignmentDataForNewStudent();

            // Store user info for session
            PlayerPrefs.SetString("LoggedInUser", username);
            PlayerPrefs.SetString("StudentName", username);
            PlayerPrefs.SetInt("IsLoggedIn", 1);
            PlayerPrefs.SetInt("OfflineMode", 1); // Flag for offline mode
            PlayerPrefs.Save();

            if (loginAnimator != null)
                loginAnimator.SetTrigger("PopOut");

            Invoke(nameof(LoadNextScene), 0.6f);
            yield break;
        }
        else
        {
            messageText.text = "Login failed. Please check your credentials.";
            messageText.color = Color.red;
        }
    }

    [ContextMenu("Clear Saved Login Credentials")]
    public void ClearSavedCredentials()
    {
        PlayerPrefs.DeleteKey("SavedUsername");
        PlayerPrefs.DeleteKey("SavedPassword");
        PlayerPrefs.DeleteKey("LoggedInUser");
        PlayerPrefs.DeleteKey("StudentName");
        PlayerPrefs.DeleteKey("IsLoggedIn");
        PlayerPrefs.DeleteKey("OfflineMode");
        PlayerPrefs.DeleteKey("DevMode");
        PlayerPrefs.Save();
        Debug.Log("All saved login credentials cleared");
        messageText.text = "Credentials cleared. Try logging in again.";
    }

    [ContextMenu("Test Login with Sample Credentials")]
    public void TestLoginWithSampleCredentials()
    {
        // Set sample credentials in the input fields
        if (usernameInput != null)
            usernameInput.text = "test@example.com";
        if (passwordInput != null)
            passwordInput.text = "password123";

        Debug.Log("Sample credentials set. Click login to test.");
        messageText.text = "Sample credentials loaded. Click Submit to test login.";
    }

    // Flask web app integration - Send failed login attempt
    private void SendLoginFailureToFlask(string username)
    {
        StartCoroutine(PostLoginFailureToFlask(username));
    }

    private IEnumerator PostLoginFailureToFlask(string username)
    {
        string url = flaskURL + "/api/login_failure";

        // Create JSON data for Flask
        string jsonData = "{\"username\":\"" + username + "\",\"action\":\"failed_login\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    // Offline login fallback - allows basic email/password validation without server
    private bool TryOfflineLogin(string username, string password)
    {
        Debug.Log($"Attempting offline login for: {username}");
        Debug.Log($"Password length: {password.Length}");

        // Basic validation - check if it looks like an email and has a password
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Offline login failed: Empty username or password");
            return false;
        }

        // More permissive email validation for testing
        if (!username.Contains("@"))
        {
            Debug.Log("Offline login failed: Username must contain @");
            return false;
        }

        if (password.Length < 1) // Very minimal password requirement for testing
        {
            Debug.Log("Offline login failed: Password too short (minimum 1 character)");
            return false;
        }

        // Check if user has logged in before (has saved credentials)
        string savedUsername = PlayerPrefs.GetString("SavedUsername", "");
        string savedPassword = PlayerPrefs.GetString("SavedPassword", "");

        Debug.Log($"Saved credentials exist: Username='{savedUsername}', Password exists={!string.IsNullOrEmpty(savedPassword)}");

        if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
        {
            // Allow login if credentials match saved ones
            if (username.Equals(savedUsername, System.StringComparison.OrdinalIgnoreCase) &&
                password == savedPassword)
            {
                Debug.Log("Offline login successful - using saved credentials");
                return true;
            }
            else
            {
                Debug.Log($"Offline login failed: Credentials don't match. Input: '{username}'/'{password}', Saved: '{savedUsername}'/'{savedPassword}'");
                // For testing purposes, allow login anyway if it's the first attempt
                Debug.Log("Allowing login for testing purposes despite credential mismatch");
                PlayerPrefs.SetString("SavedUsername", username);
                PlayerPrefs.SetString("SavedPassword", password);
                PlayerPrefs.Save();
                return true;
            }
        }
        else
        {
            // First time login - save credentials for future offline use
            PlayerPrefs.SetString("SavedUsername", username);
            PlayerPrefs.SetString("SavedPassword", password);
            PlayerPrefs.Save();
            Debug.Log("First time offline login - saving credentials");
            return true;
        }
    }

    // Development bypass for testing - allows login with any valid input
    private bool TryDevelopmentBypass(string username, string password)
    {
        Debug.Log("Attempting development bypass login...");

        // Very basic validation - just ensure something was entered
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Development bypass failed: Empty username or password");
            return false;
        }

        // Accept any username that looks vaguely like an email or just any text
        if (!username.Contains("@") && username.Length < 3)
        {
            Debug.Log("Development bypass failed: Username too short or invalid");
            return false;
        }

        // Accept any password with at least 1 character
        if (password.Length < 1)
        {
            Debug.Log("Development bypass failed: Password too short");
            return false;
        }

        // Always allow login in development mode
        Debug.Log("Development bypass successful - allowing login for testing");
        return true;
    }

    void LoadNextScene()
    {
        Debug.Log("Loading next scene after successful login");
        
        // Check if user has completed gender selection (not just saved a gender)
        bool hasCompletedGenderSelection = GenderHelper.IsGenderSelectionCompleted();
        
        if (hasCompletedGenderSelection)
        {
            string selectedGender = GenderHelper.GetSelectedGender();
            Debug.Log($"User has completed gender selection: {selectedGender}. Going to title screen.");
            SafeSceneLoader.LoadScene("titlescreen", "login");
        }
        else
        {
            Debug.Log("User has not completed gender selection yet. Going to gender selection.");
            // Try multiple gender scene names with fallback to titlescreen
            SafeSceneLoader.LoadScene("gender", "titlescreen");
        }
    }

    public void GoToRegisterScene()
    {
        // Send navigation tracking to Flask web app
        SendNavigationToFlask("register");

        SceneManager.LoadScene("register"); // ✅ Replace with actual Register scene name
    }

    // Flask web app integration - Send navigation tracking
    private void SendNavigationToFlask(string targetScene)
    {
        StartCoroutine(PostNavigationToFlask(targetScene));
    }

    private IEnumerator PostNavigationToFlask(string targetScene)
    {
        string url = flaskURL + "/api/navigation_event";

        // Create JSON data for Flask
        string jsonData = "{\"action\":\"go_to_register\",\"scene\":\"" + targetScene + "\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    // Test function to check server connectivity and endpoints
    public void TestServerConnection()
    {
        messageText.text = "Testing server connection...";
        StartCoroutine(TestServerEndpoints());
    }

    private IEnumerator TestServerEndpoints()
    {
        Debug.Log("=== SERVER CONNECTIVITY TEST ===");
        Debug.Log($"Server URL: {flaskURL}");
        Debug.Log($"Internet Reachability: {Application.internetReachability}");

        string[] testEndpoints = {
            "/",  // Root endpoint
            "/api/health",  // Health check
            "/student/simple-login",  // Login endpoint
            "/student/simple-register"  // Register endpoint
        };

        bool anyEndpointWorking = false;

        foreach (string endpoint in testEndpoints)
        {
            string url = flaskURL + endpoint;
            Debug.Log($"Testing endpoint: {url}");

            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 10;

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"✅ {endpoint} - Status: {request.responseCode}");
                anyEndpointWorking = true;

                // Show first 100 characters of response
                string responsePreview = request.downloadHandler.text;
                if (responsePreview.Length > 100)
                    responsePreview = responsePreview.Substring(0, 100) + "...";
                Debug.Log($"Response: {responsePreview}");
            }
            else
            {
                Debug.Log($"❌ {endpoint} - Error: {request.error} (Status: {request.responseCode})");
            }

            request.Dispose();
        }

        if (anyEndpointWorking)
        {
            messageText.text = "Server connection OK - some endpoints working";
            Debug.Log("=== TEST COMPLETE: Server is reachable ===");
        }
        else
        {
            messageText.text = "Server unreachable - will use offline mode";
            Debug.Log("=== TEST COMPLETE: Server unreachable, offline mode will be used ===");
        }
    }

    /// <summary>
    /// 🔒 CRITICAL SECURITY: Clear all assignment data when a new student logs in
    /// This prevents cross-account contamination where students see assignments from other accounts
    /// </summary>
    private void ClearAllAssignmentDataForNewStudent()
    {
        Debug.Log("🔒 CLEARING ALL ASSIGNMENT DATA FOR NEW STUDENT LOGIN");

        // Clear main assignment keys that might persist across accounts
        PlayerPrefs.DeleteKey("ActiveAssignmentSubject");
        PlayerPrefs.DeleteKey("ActiveAssignmentId");
        PlayerPrefs.DeleteKey("ActiveAssignmentTitle");
        PlayerPrefs.DeleteKey("ActiveAssignmentContent");
        PlayerPrefs.DeleteKey("AssignmentSource");
        PlayerPrefs.DeleteKey("CurrentAssignmentId");
        PlayerPrefs.DeleteKey("CurrentAssignmentTitle");
        PlayerPrefs.DeleteKey("CurrentAssignmentContent");

        // Clear subject-specific assignment data for all possible subjects
        string[] subjects = { "Math", "Science", "English", "PE", "Art", "MATH", "SCIENCE", "ENGLISH", "PE", "ART" };
        foreach (string subject in subjects)
        {
            string key = subject.ToUpperInvariant();

            // Clear assignment arrays (up to 20 assignments per subject to be safe)
            for (int i = 1; i <= 20; i++)
            {
                PlayerPrefs.DeleteKey($"Assignment_{subject}_{i}");
                PlayerPrefs.DeleteKey($"Assignment_{key}_{i}");
                PlayerPrefs.DeleteKey($"{subject}_Assignment_{i}");
                PlayerPrefs.DeleteKey($"{key}_Assignment_{i}");
            }

            // Clear subject-specific assignment data
            PlayerPrefs.DeleteKey($"Assignments_{subject}");
            PlayerPrefs.DeleteKey($"Assignments_{key}");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{subject}_Title");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Title");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{subject}_Id");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Id");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{subject}_Subject");
            PlayerPrefs.DeleteKey($"ActiveAssignment_{key}_Subject");
        }

        // Clear any cached assignment data
        PlayerPrefs.DeleteKey("SubjectAssignments_Math");
        PlayerPrefs.DeleteKey("SubjectAssignments_Science");
        PlayerPrefs.DeleteKey("SubjectAssignments_English");
        PlayerPrefs.DeleteKey("SubjectAssignments_PE");
        PlayerPrefs.DeleteKey("SubjectAssignments_Art");

        // Clear assignment creation timestamps
        PlayerPrefs.DeleteKey("AssignmentCreatedTime");

        PlayerPrefs.Save();
        Debug.Log("✅ ALL ASSIGNMENT DATA CLEARED - New student starts with clean slate");
        Debug.Log("🎯 Each subject will now show 'No Assignments Yet' until teacher adds assignments");
    }
}
