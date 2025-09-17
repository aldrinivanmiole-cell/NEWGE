using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoutManager : MonoBehaviour
{
    [Header("UI")]
    public Button logoutButton;
    public string loginSceneName = "Login";

    void Start()
    {
        if (logoutButton == null)
        {
            logoutButton = GetComponent<Button>();
        }
        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(LogoutPreservingClasses);
        }
    }

    public void LogoutPreservingClasses()
    {
        // Preserve classes and gameplay types
        string joinedClasses = PlayerPrefs.GetString("JoinedClasses", "");

        // Clear only login/session related data
        PlayerPrefs.DeleteKey("LoggedInUser");
        PlayerPrefs.DeleteKey("StudentName");
        PlayerPrefs.DeleteKey("StudentID");
        PlayerPrefs.DeleteKey("IsLoggedIn");
        PlayerPrefs.SetInt("OfflineMode", 0);

        // Restore preserved classes
        if (!string.IsNullOrEmpty(joinedClasses))
        {
            PlayerPrefs.SetString("JoinedClasses", joinedClasses);
        }

        PlayerPrefs.Save();

        // Navigate to login scene
        if (!string.IsNullOrEmpty(loginSceneName))
        {
            SceneManager.LoadScene(loginSceneName);
        }
    }
}


