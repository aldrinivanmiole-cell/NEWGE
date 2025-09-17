using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class InputFieldGameManager : BaseGameManager
{
    [Header("Input Field Components")]
    public TMP_InputField answerInputField;
    public Button submitAnswerButton;
    public Button clearAnswersButton;
    public TMP_Text answersDisplayText;
    public TMP_Text inputFeedbackText;

    [Header("Input Field Settings")]
    public int maxAnswers = 5;
    public int minimumAnswersRequired = 3;
    public bool allowDuplicateAnswers = false;
    public bool caseSensitiveAnswers = false;

    [Header("Web App Connection")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    // For local development, change to: "http://127.0.0.1:5000"
    public int studentId = 1;

    private List<string> submittedAnswers = new List<string>();

    // Updated session info
    protected override void Start()
    {
        base.Start();
        StartCoroutine(DelayedSetup());
    }

    private IEnumerator DelayedSetup()
    {
        yield return new WaitForEndOfFrame();
        SetupInputFieldComponents();
        Debug.Log("=== INPUT FIELD SCENE READY ===");
    }

    private void SetupInputFieldComponents()
    {
        Debug.Log("=== SETTING UP INPUT FIELD COMPONENTS ===");

        // Setup Input Field
        if (answerInputField != null)
        {
            answerInputField.onEndEdit.RemoveAllListeners();
            answerInputField.onValueChanged.RemoveAllListeners();

            // Add both listeners for better responsiveness
            answerInputField.onEndEdit.AddListener(OnInputFieldEndEdit);
            answerInputField.onValueChanged.AddListener(OnInputFieldValueChanged);

            answerInputField.characterLimit = 50;
            answerInputField.interactable = true;

            // Set placeholder text
            try
            {
                if (answerInputField.placeholder != null)
                {
                    TMP_Text placeholderText = answerInputField.placeholder.GetComponent<TMP_Text>();
                    if (placeholderText != null)
                    {
                        placeholderText.text = "Answer Here...";
                        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set placeholder text: {e.Message}");
            }

            Debug.Log("Input Field configured successfully");
        }

        // Setup Submit Button - FIXED LABELS
        if (submitAnswerButton != null)
        {
            Debug.Log("=== SETTING UP SUBMIT BUTTON ===");

            submitAnswerButton.onClick.RemoveAllListeners();
            submitAnswerButton.onClick.AddListener(() => {
                Debug.Log("SUBMIT BUTTON CLICKED!");
                SubmitCurrentAnswer();
            });

            submitAnswerButton.interactable = true;
            submitAnswerButton.enabled = true;

            // REMOVE HARDCODED SUBMIT BUTTON TEXT
            TMP_Text submitButtonText = submitAnswerButton.GetComponentInChildren<TMP_Text>();
            if (submitButtonText != null)
            {
                submitButtonText.text = ""; // No hardcoded text allowed
                submitButtonText.color = Color.white;
                submitButtonText.raycastTarget = false;
                Debug.Log("Submit Button Text cleared - no hardcoded content allowed");
            }
            else
            {
                Debug.LogWarning("Submit Button has no TMP_Text component!");
            }

            // Fix button image
            Image buttonImage = submitAnswerButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
                // Make button green for submit
                buttonImage.color = new Color(0.2f, 0.8f, 0.2f, 1f); // Green color
            }

            Debug.Log("Submit Button configured successfully");
        }
        else
        {
            Debug.LogError("SUBMIT BUTTON IS NULL!");
        }

        // Setup Clear Button - FIXED LABELS
        if (clearAnswersButton != null)
        {
            Debug.Log("=== SETTING UP CLEAR BUTTON ===");

            clearAnswersButton.onClick.RemoveAllListeners();
            clearAnswersButton.onClick.AddListener(() => {
                Debug.Log("CLEAR BUTTON CLICKED!");
                ClearAllInputAnswers();
            });

            clearAnswersButton.interactable = true;
            clearAnswersButton.enabled = true;

            // REMOVE HARDCODED CLEAR BUTTON TEXT
            TMP_Text clearButtonText = clearAnswersButton.GetComponentInChildren<TMP_Text>();
            if (clearButtonText != null)
            {
                clearButtonText.text = ""; // No hardcoded text allowed
                clearButtonText.color = Color.white;
                clearButtonText.raycastTarget = false;
                Debug.Log("Clear Button Text cleared - no hardcoded content allowed");
            }
            else
            {
                Debug.LogWarning("Clear Button has no TMP_Text component!");
            }

            // Fix button image
            Image buttonImage = clearAnswersButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.raycastTarget = true;
                // Make button red for clear
                buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // Red color
            }

            Debug.Log("Clear Button configured successfully");
        }
        else
        {
            Debug.LogError("CLEAR BUTTON IS NULL!");
        }

        // Initialize displays
        UpdateAnswersDisplay();
        ShowInputFeedback("Enter your answers below:", true);

        // Force update button states without changing text
        UpdateButtonInteractivity();
    }

    private void OnInputFieldEndEdit(string input)
    {
        Debug.Log($"Input completed: '{input}'");

        // Send input tracking to Flask web app
        SendInputTrackingToFlask(input, "input_completed");

        UpdateButtonInteractivity(); // Update without changing text
    }

    private void OnInputFieldValueChanged(string input)
    {
        UpdateButtonInteractivity(); // Update without changing text
    }

    public void SubmitCurrentAnswer()
    {
        Debug.Log("=== SUBMIT CURRENT ANSWER CALLED ===");

        if (isProcessingAttack)
        {
            Debug.LogWarning("Cannot submit - processing attack");
            ShowInputFeedback("Please wait, processing previous action...", false);
            return;
        }

        if (answerInputField == null)
        {
            Debug.LogError("Answer input field is null!");
            return;
        }

        string answer = answerInputField.text.Trim();
        Debug.Log($"Submitting answer: '{answer}'");

        if (string.IsNullOrEmpty(answer))
        {
            Debug.LogWarning("Empty answer");
            ShowInputFeedback("Please enter an answer!", false);
            return;
        }

        if (submittedAnswers.Count >= maxAnswers)
        {
            Debug.LogWarning($"Maximum answers reached: {maxAnswers}");
            ShowInputFeedback($"Maximum {maxAnswers} answers reached!", false);
            return;
        }

        if (!allowDuplicateAnswers && ContainsAnswer(answer))
        {
            Debug.LogWarning($"Duplicate answer: {answer}");
            ShowInputFeedback("Answer already submitted!", false);
            return;
        }

        // Add the answer
        submittedAnswers.Add(answer);
        answerInputField.text = "";

        Debug.Log($"Answer added successfully: {answer} (Total: {submittedAnswers.Count}/{maxAnswers})");

        // Send answer submission to Flask web app
        SendAnswerSubmissionToFlask(answer, submittedAnswers.Count);

        UpdateAnswersDisplay();
        ShowInputFeedback($"Answer added: {answer}", true);

        // Check if we should auto-submit
        if (submittedAnswers.Count >= maxAnswers)
        {
            ShowInputFeedback("Maximum answers reached! Submitting automatically...", true);
            StartCoroutine(DelayedSubmitAll());
        }
    }

    public void ClearAllInputAnswers()
    {
        Debug.Log("=== CLEAR ALL INPUT ANSWERS CALLED ===");

        if (isProcessingAttack)
        {
            Debug.LogWarning("Cannot clear - processing attack");
            ShowInputFeedback("Cannot clear during attack!", false);
            return;
        }

        int clearedCount = submittedAnswers.Count;
        submittedAnswers.Clear();

        if (answerInputField != null)
        {
            answerInputField.text = "";
        }

        Debug.Log($"Cleared {clearedCount} answers");

        // Send clear action to Flask web app
        SendClearActionToFlask(clearedCount);

        UpdateAnswersDisplay();
        ShowInputFeedback("All answers cleared!", true);
    }

    // Flask web app integration - Send input tracking
    private void SendInputTrackingToFlask(string input, string action)
    {
        StartCoroutine(PostInputTrackingToFlask(input, action));
    }

    private IEnumerator PostInputTrackingToFlask(string input, string action)
    {
        string url = flaskURL + "/api/input_tracking";

        // Create JSON data for Flask
        string jsonData = "{\"student_id\":" + studentId + ",\"input_text\":\"" + input + "\",\"action\":\"" + action + "\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    // Flask web app integration - Send answer submission
    private void SendAnswerSubmissionToFlask(string answer, int answerCount)
    {
        StartCoroutine(PostAnswerSubmissionToFlask(answer, answerCount));
    }

    private IEnumerator PostAnswerSubmissionToFlask(string answer, int answerCount)
    {
        string url = flaskURL + "/api/answer_submission";

        // Create JSON data for Flask
        string jsonData = "{\"student_id\":" + studentId + ",\"answer\":\"" + answer + "\",\"answer_count\":" + answerCount + ",\"max_answers\":" + maxAnswers + "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    // Flask web app integration - Send clear action
    private void SendClearActionToFlask(int clearedCount)
    {
        StartCoroutine(PostClearActionToFlask(clearedCount));
    }

    private IEnumerator PostClearActionToFlask(int clearedCount)
    {
        string url = flaskURL + "/api/clear_answers";

        // Create JSON data for Flask
        string jsonData = "{\"student_id\":" + studentId + ",\"cleared_count\":" + clearedCount + ",\"action\":\"clear_all\"}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    private bool ContainsAnswer(string answer)
    {
        foreach (string existing in submittedAnswers)
        {
            if (caseSensitiveAnswers ? existing == answer : existing.ToLower() == answer.ToLower())
                return true;
        }
        return false;
    }

    private void UpdateAnswersDisplay()
    {
        if (answersDisplayText != null)
        {
            if (submittedAnswers.Count == 0)
            {
                answersDisplayText.text = $"Your Answers (0/{maxAnswers}):\n(No answers yet)";
            }
            else
            {
                string displayText = $"Your Answers ({submittedAnswers.Count}/{maxAnswers}):\n";
                for (int i = 0; i < submittedAnswers.Count; i++)
                {
                    displayText += $"{i + 1}. {submittedAnswers[i]}\n";
                }
                answersDisplayText.text = displayText;
            }
        }

        UpdateButtonInteractivity(); // Update without changing text
    }

    // FIXED: Update button interactivity without changing text
    private void UpdateButtonInteractivity()
    {
        // Update Submit Button interactivity only
        if (submitAnswerButton != null)
        {
            bool hasInput = !string.IsNullOrEmpty(answerInputField?.text?.Trim());
            bool canAdd = submittedAnswers.Count < maxAnswers;
            bool canSubmit = !isProcessingAttack && hasInput && canAdd;

            submitAnswerButton.interactable = canSubmit;

            // Update button appearance without changing text
            Image buttonImage = submitAnswerButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Keep green color, just change opacity
                Color color = new Color(0.2f, 0.8f, 0.2f, canSubmit ? 1f : 0.6f);
                buttonImage.color = color;
            }

            // Button text remains empty - no hardcoded content allowed
            Debug.Log($"Submit button interactivity updated: {canSubmit}");
        }

        // Update Clear Button interactivity only
        if (clearAnswersButton != null)
        {
            bool canClear = !isProcessingAttack && submittedAnswers.Count > 0;
            clearAnswersButton.interactable = canClear;

            // Update button appearance without changing text
            Image buttonImage = clearAnswersButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                // Keep red color, just change opacity
                Color color = new Color(0.8f, 0.2f, 0.2f, canClear ? 1f : 0.6f);
                buttonImage.color = color;
            }

            // Button text remains empty - no hardcoded content allowed
            Debug.Log($"Clear button interactivity updated: {canClear}");
        }
    }

    private IEnumerator DelayedSubmitAll()
    {
        yield return new WaitForSeconds(1f);

        if (submittedAnswers.Count >= minimumAnswersRequired)
        {
            float scorePercentage = CalculateScorePercentage();
            string allAnswers = string.Join(", ", submittedAnswers);

            // Send final submission to Flask web app
            SendFinalSubmissionToFlask(allAnswers, scorePercentage);

            StartCoroutine(HandleAttack(allAnswers, scorePercentage));
        }
        else
        {
            ShowInputFeedback($"Need at least {minimumAnswersRequired} answers!", false);
        }
    }

    // Flask web app integration - Send final submission
    private void SendFinalSubmissionToFlask(string allAnswers, float scorePercentage)
    {
        StartCoroutine(PostFinalSubmissionToFlask(allAnswers, scorePercentage));
    }

    private IEnumerator PostFinalSubmissionToFlask(string allAnswers, float scorePercentage)
    {
        string url = flaskURL + "/api/final_submission";

        // Create JSON data for Flask
        string jsonData = "{\"student_id\":" + studentId + ",\"all_answers\":\"" + allAnswers + "\",\"score_percentage\":" + scorePercentage + ",\"answer_count\":" + submittedAnswers.Count + "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        request.Dispose();
    }

    private float CalculateScorePercentage()
    {
        if (submittedAnswers.Count == 0) return 0f;

        int correctCount = 0;
        foreach (string answer in submittedAnswers)
        {
            if (correctAnswers.Contains(answer.ToLower()))
                correctCount++;
        }

        return (float)correctCount / submittedAnswers.Count * 100f;
    }

    private void ShowInputFeedback(string message, bool isPositive)
    {
        if (inputFeedbackText != null)
        {
            inputFeedbackText.text = message;
            inputFeedbackText.color = isPositive ? Color.green : Color.red;
        }
        Debug.Log($"Input Feedback: {message} (Positive: {isPositive})");
    }

    private IEnumerator HandleAttack(string answers, float scorePercentage)
    {
        if (isProcessingAttack) yield break;

        Debug.Log($"Starting attack with answers: {answers}, Score: {scorePercentage:F1}%");

        isProcessingAttack = true;
        UpdateButtonInteractivity(); // Disable buttons during attack

        Vector2 targetPos = new Vector2(enemy1.anchoredPosition.x - attackOffset, enemy1.anchoredPosition.y);
        yield return StartCoroutine(MovePlayerTo(targetPos));

        yield return StartCoroutine(ShowSpeechBubble(playerSpeechBubble, playerSpeechText, $"My answers: {answers}", 2f));

        bool isCorrect = scorePercentage >= passingScorePercentage;
        string resultText = isCorrect ? $"Excellent! {scorePercentage:F0}% correct!" : $"Try harder! Only {scorePercentage:F0}% correct.";

        yield return StartCoroutine(ShowSpeechBubble(enemySpeechBubble, enemySpeechText, resultText, 2f));
        yield return StartCoroutine(MovePlayerTo(originalPos));

        AddProgress(isCorrect ? 25f : 10f);
        ShowInputFeedback($"{(isCorrect ? "Success" : "Keep trying")}! Score: {scorePercentage:F0}%", isCorrect);

        if (clearAnswersAfterSubmission)
        {
            yield return new WaitForSeconds(clearDelay);
            ClearAllInputAnswers();
        }

        isProcessingAttack = false;
        UpdateButtonInteractivity(); // Re-enable buttons after attack
    }

    // Force fix button text if they get changed
    [ContextMenu("Force Fix Button Text")]
    public void ForceFixButtonText()
    {
        Debug.Log("=== FORCING BUTTON TEXT FIX ===");

        if (submitAnswerButton != null)
        {
            TMP_Text submitText = submitAnswerButton.GetComponentInChildren<TMP_Text>();
            if (submitText != null)
            {
                submitText.text = ""; // No hardcoded text allowed
                Debug.Log("Submit button text cleared - no hardcoded content allowed");
            }
        }

        if (clearAnswersButton != null)
        {
            TMP_Text clearText = clearAnswersButton.GetComponentInChildren<TMP_Text>();
            if (clearText != null)
            {
                clearText.text = ""; // No hardcoded text allowed
                Debug.Log("Clear button text cleared - no hardcoded content allowed");
            }
        }
    }

    // Manual testing methods
    [ContextMenu("Test Submit Button")]
    public void TestSubmitButton()
    {
        Debug.Log("=== TESTING SUBMIT BUTTON ===");
        SubmitCurrentAnswer();
    }

    [ContextMenu("Test Clear Button")]
    public void TestClearButton()
    {
        Debug.Log("=== TESTING CLEAR BUTTON ===");
        ClearAllInputAnswers();
    }
}
