using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class GameStageManager : MonoBehaviour
{
    [Header("Game Mode")]
    public GameMode gameMode = GameMode.InputField;
    public enum GameMode
    {
        DragAndDrop,
        InputField,
        MultipleChoice
    }

    [Header("Player & Enemy Transforms")]
    public RectTransform player;
    public RectTransform enemy1;
    public float attackOffset = 100f;
    public float movementSpeed = 2f;
    private Vector2 originalPos;

    [Header("Player & Enemy Sprites")]
    public Sprite maleSprite;
    public Sprite femaleSprite;
    public Sprite[] enemySprites;
    public Image playerImage;
    public Image enemyImage;

    [Header("Player Speech UI")]
    public GameObject playerSpeechBubble;
    public TMP_Text playerSpeechText;

    [Header("Enemy Speech UI")]
    public GameObject enemySpeechBubble;
    public TMP_Text enemySpeechText;

    [Header("Drag & Drop Components (Scene-Specific)")]
    public List<Button> answerButtons = new List<Button>();
    public FillBlankDropZone dropZone;

    [Header("Input Field Components (Scene-Specific)")]
    public TMP_InputField answerInputField;
    public Button submitAnswerButton;
    public Button clearAnswersButton;
    public TMP_Text answersDisplayText;
    public TMP_Text inputFeedbackText;
    public TMP_Text questionText;
    public int maxAnswers = 5;
    public int minimumAnswersRequired = 3;
    public bool allowDuplicateAnswers = false;
    public bool caseSensitiveAnswers = false;

    [Header("Multiple Choice Components (Scene-Specific)")]
    public List<Toggle> answerToggles = new List<Toggle>();
    public Button submitToggleButton;
    public TMP_Text multipleChoiceFeedbackText;

    [Header("Game Settings")]
    public float passingScorePercentage = 70f;
    public string currentQuestion = "Name 5 programming languages:";
    public bool clearAnswersAfterSubmission = true;
    public float clearDelay = 1f;

    [Header("Progress UI")]
    public Slider progressBar;
    public TMP_Text textProgress;
    public GameObject resultPanel;

    [Header("Progress Dialogues")]
    [TextArea] public string dialogueAt0 = "Let's begin the challenge!";
    [TextArea] public string dialogueAt25 = "Good start, keep going!";
    [TextArea] public string dialogueAt50 = "Halfway there!";
    [TextArea] public string dialogueAt75 = "Almost done, stay focused!";
    [TextArea] public string dialogueAt100 = "Congratulations, you did it!";
    public TMP_Text dialogueText;

    [Header("Flask Integration")]
    public string flaskURL = "https://homequest-c3k7.onrender.com"; // Production FastAPI+Flask server URL
    // For local development, change to: "http://127.0.0.1:5000"
    public bool sendToFlask = true;
    public int studentId = 1;
    public int assignmentId = 1;

    // Private variables
    private float progress = 0f;
    private float maxProgress = 100f;
    private bool isProcessingAttack = false;
    private List<string> submittedAnswers = new List<string>();
    private HashSet<string> correctAnswers = new HashSet<string>();

    // Scene detection
    private bool hasInputFieldComponents = false;
    private bool hasDragDropComponents = false;
    private bool hasMultipleChoiceComponents = false;

    // User session info - UPDATED
    private string currentUser = "aldrinivanmiole-cell";
    private string sessionTime = "2025-09-02 22:56:40";

    void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        Debug.Log("=== MOBILE GAME INITIALIZATION ===");
        Debug.Log($"Scene: {SceneManager.GetActiveScene().name}");
        Debug.Log($"Game Mode: {gameMode}");
        Debug.Log($"User: {currentUser} | Session: {sessionTime} UTC");
        Debug.Log($"Platform: {Application.platform} | Mobile: {Application.isMobilePlatform}");

        // Store original position with safety check
        if (player != null)
        {
            originalPos = player.anchoredPosition;
            Debug.Log($"Player original position: {originalPos}");
        }
        else
        {
            Debug.LogError("Player RectTransform is not assigned!");
            originalPos = Vector2.zero;
        }

        if (enemy1 != null)
        {
            Debug.Log($"Enemy position: {enemy1.anchoredPosition}");
        }
        else
        {
            Debug.LogError("Enemy1 RectTransform is not assigned!");
        }

        // Initialize UI safely
        if (resultPanel != null)
            resultPanel.SetActive(false);

        // Setup game components
        AssignPlayerSprite();
        AssignEnemySprite();
        InitializeTemporaryAnswers();

        // Detect what components exist in this scene and setup accordingly
        DetectSceneComponents();
        SetupSceneSpecificComponents();

        UpdateUI();
        ShowDialogueForProgress();

        // Hide speech bubbles initially
        if (playerSpeechBubble) playerSpeechBubble.SetActive(false);
        if (enemySpeechBubble) enemySpeechBubble.SetActive(false);

        // Set up the question
        if (questionText != null)
        {
            questionText.text = currentQuestion;
        }

        // Send game initialization to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendGameInitToFlask());
        }

        Debug.Log("=== MOBILE INITIALIZATION COMPLETE ===");
        LogSceneComponentStatus();
    }

    private void DetectSceneComponents()
    {
        // Detect Input Field components
        hasInputFieldComponents = (answerInputField != null);

        // Detect Drag & Drop components
        hasDragDropComponents = (dropZone != null || answerButtons.Count > 0);

        // Detect Multiple Choice components
        hasMultipleChoiceComponents = (submitToggleButton != null || answerToggles.Count > 0);

        Debug.Log($"=== MOBILE SCENE COMPONENT DETECTION ===");
        Debug.Log($"Input Field Scene: {hasInputFieldComponents} (InputField: {answerInputField != null}, Submit: {submitAnswerButton != null})");
        Debug.Log($"Drag & Drop Scene: {hasDragDropComponents} (DropZone: {dropZone != null}, Buttons: {answerButtons.Count})");
        Debug.Log($"Multiple Choice Scene: {hasMultipleChoiceComponents} (SubmitToggle: {submitToggleButton != null}, Toggles: {answerToggles.Count})");
    }

    private void SetupSceneSpecificComponents()
    {
        Debug.Log("Setting up mobile-optimized scene components...");

        if (hasInputFieldComponents)
        {
            SetupInputFieldComponents();
        }

        if (hasDragDropComponents)
        {
            SetupDragAndDropComponents();
        }

        if (hasMultipleChoiceComponents)
        {
            SetupMultipleChoiceComponents();
        }
    }

    private void LogSceneComponentStatus()
    {
        Debug.Log("=== MOBILE COMPONENT STATUS ===");
        Debug.Log($"Scene: {SceneManager.GetActiveScene().name}");

        if (hasInputFieldComponents)
        {
            Debug.Log($"INPUT FIELD COMPONENTS (Mobile Optimized):");
            Debug.Log($"  Input Field: {(answerInputField != null ? $"OK Active: {answerInputField.gameObject.activeInHierarchy}, Interactable: {answerInputField.interactable}" : "NULL")}");
            Debug.Log($"  Submit Button: {(submitAnswerButton != null ? $"OK Active: {submitAnswerButton.gameObject.activeInHierarchy}, Interactable: {submitAnswerButton.interactable}" : "NULL")}");
            Debug.Log($"  Clear Button: {(clearAnswersButton != null ? $"OK Active: {clearAnswersButton.gameObject.activeInHierarchy}, Interactable: {clearAnswersButton.interactable}" : "NULL")}");
            Debug.Log($"  Answers Display: {(answersDisplayText != null ? "OK Assigned" : "NULL")}");
            Debug.Log($"  Feedback Text: {(inputFeedbackText != null ? "OK Assigned" : "NULL")}");
        }

        if (hasDragDropComponents)
        {
            Debug.Log($"DRAG & DROP COMPONENTS (Touch Optimized):");
            Debug.Log($"  DropZone: {(dropZone != null ? $"OK Active: {dropZone.gameObject.activeInHierarchy}" : "NULL")}");
            Debug.Log($"  Answer Buttons: {answerButtons.Count}");
        }

        if (hasMultipleChoiceComponents)
        {
            Debug.Log($"MULTIPLE CHOICE COMPONENTS (Touch Optimized):");
            Debug.Log($"  Submit Toggle Button: {(submitToggleButton != null ? $"OK Active: {submitToggleButton.gameObject.activeInHierarchy}" : "NULL")}");
            Debug.Log($"  Answer Toggles: {answerToggles.Count}");
            Debug.Log($"  Feedback Text: {(multipleChoiceFeedbackText != null ? "OK Assigned" : "NULL")}");
        }
    }

    private void InitializeTemporaryAnswers()
    {
        // TEMPORARY ANSWERS - Will be replaced with API endpoints and database integration
        correctAnswers.Clear();

        string[] programmingLanguages = {
            "python", "javascript", "c#", "java", "c++",
            "ruby", "go", "swift", "php", "kotlin",
            "typescript", "rust", "dart", "scala", "perl",
            "html", "css", "sql", "r", "matlab",
            "c", "objective-c", "shell", "powershell", "bash",
            "vb.net", "f#", "haskell", "lua", "assembly",
            "cobol", "fortran", "pascal", "delphi", "prolog"
        };

        foreach (string lang in programmingLanguages)
        {
            correctAnswers.Add(lang.ToLower());
        }

        Debug.Log($"Loaded {correctAnswers.Count} temporary correct answers (to be replaced with database)");
    }

    #region Input Field Components Setup - MOBILE OPTIMIZED
    private void SetupInputFieldComponents()
    {
        Debug.Log("=== Setting up Mobile Input Field components ===");

        if (answerInputField != null)
        {
            answerInputField.onEndEdit.RemoveAllListeners();
            answerInputField.onValueChanged.RemoveAllListeners();
            answerInputField.characterLimit = 50;
            answerInputField.interactable = true;
            answerInputField.onEndEdit.AddListener(OnMobileInputFieldEndEdit);

            try
            {
                if (answerInputField.placeholder != null)
                {
                    TMP_Text placeholderText = answerInputField.placeholder.GetComponent<TMP_Text>();
                    if (placeholderText != null)
                    {
                        placeholderText.text = "Tap here to enter answer...";
                        placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Could not set placeholder text: {e.Message}");
            }

            Debug.Log("OK Mobile Input Field configured successfully");
        }

        if (submitAnswerButton != null)
        {
            submitAnswerButton.onClick.RemoveAllListeners();
            submitAnswerButton.onClick.AddListener(() => {
                Debug.Log("Mobile Submit button tapped");
                SubmitCurrentAnswer();
            });
            submitAnswerButton.interactable = true;
            Debug.Log("OK Mobile Submit Button configured successfully");
        }

        if (clearAnswersButton != null)
        {
            clearAnswersButton.onClick.RemoveAllListeners();
            clearAnswersButton.onClick.AddListener(() => {
                Debug.Log("Mobile Clear button tapped");
                ClearAllInputAnswers();
            });
            clearAnswersButton.interactable = true;
            Debug.Log("OK Mobile Clear Button configured successfully");
        }

        UpdateAnswersDisplay();
        ShowInputFeedback("Tap the input field and use the submit button:", true);
        Debug.Log("OK Mobile Input Field components setup complete");
    }
    #endregion

    #region Drag and Drop Components Setup - TOUCH OPTIMIZED
    private void SetupDragAndDropComponents()
    {
        Debug.Log("=== Setting up Mobile Drag & Drop components ===");

        if (dropZone != null)
        {
            Debug.Log("OK Mobile DropZone found and ready for touch input");
        }

        foreach (var btn in answerButtons)
        {
            if (btn != null)
            {
                string btnText = GetButtonText(btn);
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    if (!isProcessingAttack)
                    {
                        Debug.Log($"Mobile Drag & Drop Button tapped: {btnText}");
                        TriggerAttackWithEnemy(btnText);
                    }
                });

                btn.interactable = true;
                Debug.Log($"OK Mobile Drag & Drop button configured: {btnText}");
            }
        }

        Debug.Log($"OK Mobile Drag & Drop setup complete - {answerButtons.Count} buttons configured for touch");
    }

    public void TriggerAttackWithEnemy(string answer)
    {
        Debug.Log($"=== MOBILE DRAG & DROP ATTACK TRIGGERED ===");
        Debug.Log($"Answer: {answer}");

        if (!isProcessingAttack && enemy1 != null && player != null)
        {
            Debug.Log("Starting HandleSingleAnswerAttack coroutine...");
            StartCoroutine(HandleSingleAnswerAttack(answer));
        }
        else
        {
            Debug.LogWarning($"Cannot trigger attack - Processing: {isProcessingAttack}, Enemy: {enemy1 != null}, Player: {player != null}");
        }
    }
    #endregion

    #region Multiple Choice Components Setup - BULLETPROOF FIX
    private void SetupMultipleChoiceComponents()
    {
        Debug.Log("=== Setting up Mobile Multiple Choice components ===");

        if (submitToggleButton != null)
        {
            submitToggleButton.onClick.RemoveAllListeners();
            submitToggleButton.onClick.AddListener(() => {
                Debug.Log("Mobile Submit toggle button tapped");
                SubmitToggleAnswers();
            });
            submitToggleButton.interactable = true;
            Debug.Log("OK Mobile Submit Toggle Button configured successfully");
        }
        else
        {
            Debug.Log("WARNING: Submit Toggle Button is NULL - this may not be a multiple choice scene");
        }

        foreach (var toggle in answerToggles)
        {
            if (toggle != null)
            {
                toggle.interactable = true;
                Debug.Log("OK Mobile Toggle configured successfully");
            }
        }

        Debug.Log($"OK Mobile Multiple Choice setup complete - {answerToggles.Count} toggles configured for touch");
    }

    public void SubmitToggleAnswers()
    {
        Debug.Log("=== MOBILE SUBMITTING TOGGLE ANSWERS - BULLETPROOF VERSION ===");

        // BULLETPROOF VALIDATION - Multiple layers of checking

        // CHECK 1: Scene components validation
        if (!hasMultipleChoiceComponents)
        {
            Debug.LogError("CRITICAL ERROR: SubmitToggleAnswers called but this scene doesn't have multiple choice components!");
            Debug.LogError("This should not happen - check your UI setup");
            return;
        }

        // CHECK 2: Processing state validation
        if (isProcessingAttack)
        {
            Debug.LogWarning("Cannot submit toggles - attack in progress");
            ShowMultipleChoiceFeedback("Please wait, processing previous action...", false);
            return;
        }

        // CHECK 3: Button validation
        if (submitToggleButton == null)
        {
            Debug.LogError("CRITICAL ERROR: Submit toggle button is null but method was called!");
            return;
        }

        // CHECK 4: Toggle collection validation
        if (answerToggles == null || answerToggles.Count == 0)
        {
            Debug.LogError("CRITICAL ERROR: No toggle collection found!");
            ShowMultipleChoiceFeedback("Game error - no answer options available!", false);
            return;
        }

        // COUNT SELECTED ANSWERS WITH DETAILED LOGGING
        List<string> selectedAnswers = new List<string>();
        int totalToggles = 0;
        int activeToggles = 0;
        int selectedToggles = 0;

        Debug.Log("Scanning all toggles for selections...");

        for (int i = 0; i < answerToggles.Count; i++)
        {
            Toggle toggle = answerToggles[i];
            if (toggle != null)
            {
                totalToggles++;

                if (toggle.gameObject.activeInHierarchy)
                {
                    activeToggles++;

                    if (toggle.isOn)
                    {
                        selectedToggles++;
                        TMP_Text toggleText = toggle.GetComponentInChildren<TMP_Text>();
                        if (toggleText != null)
                        {
                            selectedAnswers.Add(toggleText.text);
                            Debug.Log($"  Toggle {i}: SELECTED - '{toggleText.text}'");
                        }
                        else
                        {
                            Debug.LogWarning($"  Toggle {i}: SELECTED but no text component found!");
                        }
                    }
                    else
                    {
                        Debug.Log($"  Toggle {i}: Not selected");
                    }
                }
                else
                {
                    Debug.Log($"  Toggle {i}: Inactive/Hidden");
                }
            }
            else
            {
                Debug.LogWarning($"  Toggle {i}: NULL reference");
            }
        }

        Debug.Log($"Toggle scan complete: {selectedToggles}/{activeToggles} active toggles selected (Total: {totalToggles})");
        Debug.Log($"Selected answers collected: {selectedAnswers.Count}");

        // BULLETPROOF CHECK: Multiple validation for no selections
        bool hasNoSelections = (selectedToggles == 0) || (selectedAnswers.Count == 0);

        if (hasNoSelections)
        {
            Debug.LogWarning("No answers selected in mobile multiple choice!");
            Debug.LogWarning($"Validation details: selectedToggles={selectedToggles}, selectedAnswers.Count={selectedAnswers.Count}");
            ShowMultipleChoiceFeedback("Please select at least one answer!", false);
            Debug.LogWarning("EXITING SubmitToggleAnswers - no selections found - METHOD TERMINATED");
            return; // CRITICAL: This MUST exit the method
        }

        // If we reach here, we have valid selections
        Debug.Log($"VALIDATION PASSED: Processing {selectedAnswers.Count} mobile selected answers");

        float scorePercentage = CalculateMultipleChoiceScore(selectedAnswers);
        string allAnswers = string.Join(", ", selectedAnswers);

        Debug.Log($"Mobile multiple choice submission: {selectedAnswers.Count} answers, {scorePercentage:F1}% score");
        ShowMultipleChoiceFeedback($"Submitting {selectedAnswers.Count} answers...", true);

        // Send to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendMultipleChoiceToFlask(selectedAnswers, scorePercentage));
        }

        StartCoroutine(HandleMultipleAnswerAttack(allAnswers, scorePercentage));
    }

    private float CalculateMultipleChoiceScore(List<string> selectedAnswers)
    {
        if (selectedAnswers.Count == 0) return 0f;

        int correctCount = 0;
        foreach (string answer in selectedAnswers)
        {
            if (correctAnswers.Contains(answer.ToLower()))
            {
                correctCount++;
                Debug.Log($"Correct mobile multiple choice answer: {answer}");
            }
            else
            {
                Debug.Log($"Incorrect mobile multiple choice answer: {answer}");
            }
        }

        float percentage = (float)correctCount / selectedAnswers.Count * 100f;
        Debug.Log($"Mobile multiple choice score: {correctCount}/{selectedAnswers.Count} = {percentage:F1}%");
        return percentage;
    }

    private void ShowMultipleChoiceFeedback(string message, bool isPositive)
    {
        if (multipleChoiceFeedbackText != null)
        {
            multipleChoiceFeedbackText.text = message;
            multipleChoiceFeedbackText.color = isPositive ? Color.green : Color.red;
        }
        Debug.Log($"Mobile Multiple Choice Feedback: {message} (Positive: {isPositive})");
    }
    #endregion

    #region Input Field Methods - MOBILE OPTIMIZED
    private void OnMobileInputFieldEndEdit(string input)
    {
        Debug.Log($"Mobile OnInputFieldEndEdit called with input: '{input}'");
        Debug.Log("Mobile input completed - user should tap Submit button");
    }

    public void SubmitCurrentAnswer()
    {
        Debug.Log("=== MOBILE SUBMIT CURRENT ANSWER CALLED ===");

        if (!hasInputFieldComponents)
        {
            Debug.LogWarning("SubmitCurrentAnswer called but this scene doesn't have input field components!");
            return;
        }

        if (isProcessingAttack)
        {
            Debug.LogWarning("Cannot submit - attack in progress");
            ShowInputFeedback("Please wait, processing previous action...", false);
            return;
        }

        if (answerInputField == null)
        {
            Debug.LogError("Answer input field is null!");
            return;
        }

        string answer = answerInputField.text.Trim();
        Debug.Log($"Attempting to submit mobile answer: '{answer}'");

        if (string.IsNullOrEmpty(answer))
        {
            Debug.LogWarning("Empty answer submitted");
            ShowInputFeedback("Please enter an answer!", false);
            return;
        }

        if (submittedAnswers.Count >= maxAnswers)
        {
            Debug.LogWarning($"Maximum answers ({maxAnswers}) already reached");
            ShowInputFeedback($"Maximum {maxAnswers} answers reached!", false);
            return;
        }

        if (!allowDuplicateAnswers && ContainsAnswer(answer))
        {
            Debug.LogWarning($"Duplicate answer: {answer}");
            ShowInputFeedback("Answer already submitted!", false);
            return;
        }

        submittedAnswers.Add(answer);
        Debug.Log($"Mobile answer added successfully: {answer} (Total: {submittedAnswers.Count}/{maxAnswers})");

        // Send to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendInputAnswerToFlask(answer));
        }

        answerInputField.text = "";
        UpdateAnswersDisplay();
        ShowInputFeedback($"Answer added: {answer}", true);
        CheckInputCompletionStatus();
    }

    private bool ContainsAnswer(string answer)
    {
        foreach (string existing in submittedAnswers)
        {
            if (caseSensitiveAnswers)
            {
                if (existing == answer) return true;
            }
            else
            {
                if (existing.ToLower() == answer.ToLower()) return true;
            }
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
            Debug.Log($"Updated mobile answers display: {submittedAnswers.Count} answers shown");
        }

        UpdateComponentStates();
    }

    private void UpdateComponentStates()
    {
        if (hasInputFieldComponents)
        {
            if (submitAnswerButton != null)
            {
                bool canSubmit = !isProcessingAttack && !string.IsNullOrEmpty(answerInputField?.text?.Trim()) && submittedAnswers.Count < maxAnswers;
                submitAnswerButton.interactable = canSubmit;

                TMP_Text buttonText = submitAnswerButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null)
                {
                    if (submittedAnswers.Count >= maxAnswers)
                    {
                        buttonText.text = "Complete!";
                    }
                    else
                    {
                        buttonText.text = ""; // No hardcoded text allowed
                    }
                }
            }

            if (clearAnswersButton != null)
            {
                clearAnswersButton.interactable = !isProcessingAttack && submittedAnswers.Count > 0;
            }

            if (answerInputField != null)
            {
                answerInputField.interactable = !isProcessingAttack;
            }
        }

        if (hasDragDropComponents)
        {
            foreach (var btn in answerButtons)
            {
                if (btn != null)
                {
                    btn.interactable = !isProcessingAttack;
                }
            }
        }

        if (hasMultipleChoiceComponents)
        {
            if (submitToggleButton != null)
            {
                submitToggleButton.interactable = !isProcessingAttack;
            }

            foreach (var toggle in answerToggles)
            {
                if (toggle != null)
                {
                    toggle.interactable = !isProcessingAttack;
                }
            }
        }
    }

    public void ClearAllInputAnswers()
    {
        Debug.Log("=== MOBILE CLEARING ALL INPUT ANSWERS ===");

        if (!hasInputFieldComponents)
        {
            Debug.LogWarning("ClearAllInputAnswers called but this scene doesn't have input field components!");
            return;
        }

        if (isProcessingAttack)
        {
            ShowInputFeedback("Cannot clear during attack!", false);
            return;
        }

        int clearedCount = submittedAnswers.Count;
        submittedAnswers.Clear();
        Debug.Log($"Cleared {clearedCount} mobile answers");

        // Send to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendClearAnswersToFlask(clearedCount));
        }

        UpdateAnswersDisplay();
        ShowInputFeedback("All answers cleared!", true);

        if (answerInputField != null)
        {
            answerInputField.text = "";
        }
    }

    private void CheckInputCompletionStatus()
    {
        Debug.Log($"Checking mobile completion status: {submittedAnswers.Count}/{maxAnswers} answers, minimum: {minimumAnswersRequired}");

        if (submittedAnswers.Count >= minimumAnswersRequired && submittedAnswers.Count < maxAnswers)
        {
            ShowInputFeedback($"Great! You have {submittedAnswers.Count}/{minimumAnswersRequired} answers. Keep going!", true);
        }

        if (submittedAnswers.Count >= maxAnswers)
        {
            ShowInputFeedback("Maximum answers reached! Submitting automatically...", true);
            StartCoroutine(DelayedInputSubmission());
        }
    }

    private IEnumerator DelayedInputSubmission()
    {
        Debug.Log("Starting delayed mobile submission...");
        yield return new WaitForSeconds(1f);
        SubmitAllInputAnswers();
    }

    public void SubmitAllInputAnswers()
    {
        Debug.Log("=== MOBILE SUBMITTING ALL INPUT ANSWERS ===");

        if (submittedAnswers.Count < minimumAnswersRequired)
        {
            Debug.LogWarning($"Not enough answers: {submittedAnswers.Count}/{minimumAnswersRequired}");
            ShowInputFeedback($"Please provide at least {minimumAnswersRequired} answers!", false);
            return;
        }

        float scorePercentage = CalculateScorePercentage();
        string allAnswers = string.Join(", ", submittedAnswers);

        Debug.Log($"Submitting {submittedAnswers.Count} mobile answers with {scorePercentage:F1}% score");

        // Send to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendAllInputAnswersToFlask(submittedAnswers, scorePercentage));
        }

        StartCoroutine(HandleMultipleAnswerAttack(allAnswers, scorePercentage));
    }

    private float CalculateScorePercentage()
    {
        if (submittedAnswers.Count == 0) return 0f;

        int correctCount = 0;
        foreach (string answer in submittedAnswers)
        {
            if (correctAnswers.Contains(answer.ToLower()))
            {
                correctCount++;
                Debug.Log($"Correct mobile answer: {answer}");
            }
            else
            {
                Debug.Log($"Incorrect mobile answer: {answer}");
            }
        }

        float percentage = (float)correctCount / submittedAnswers.Count * 100f;
        Debug.Log($"Mobile score calculation: {correctCount}/{submittedAnswers.Count} = {percentage:F1}%");
        return percentage;
    }

    private void ShowInputFeedback(string message, bool isPositive)
    {
        if (inputFeedbackText != null)
        {
            inputFeedbackText.text = message;
            inputFeedbackText.color = isPositive ? Color.green : Color.red;
        }
        Debug.Log($"Mobile Feedback: {message} (Positive: {isPositive})");
    }
    #endregion

    #region Attack Handling Methods
    private IEnumerator HandleSingleAnswerAttack(string answer)
    {
        if (enemy1 == null || isProcessingAttack || player == null)
        {
            yield break;
        }

        isProcessingAttack = true;
        UpdateComponentStates();

        // Send to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendSingleAnswerToFlask(answer));
        }

        Vector2 targetPos = new Vector2(enemy1.anchoredPosition.x - attackOffset, enemy1.anchoredPosition.y);
        yield return StartCoroutine(MovePlayerTo(targetPos));

        yield return StartCoroutine(ShowSpeechBubble(playerSpeechBubble, playerSpeechText, $"I choose: {answer}", 1.5f));

        bool isCorrect = correctAnswers.Contains(answer.ToLower());
        string resultText = isCorrect ? "Correct! Well done!" : "Wrong! Try again!";

        yield return StartCoroutine(ShowSpeechBubble(enemySpeechBubble, enemySpeechText, resultText, 2f));

        yield return StartCoroutine(MovePlayerTo(originalPos));

        if (isCorrect)
        {
            AddProgress(25f);
        }

        if (clearAnswersAfterSubmission && dropZone != null)
        {
            yield return new WaitForSeconds(clearDelay);
            dropZone.ClearDropZone();
        }

        isProcessingAttack = false;
        UpdateComponentStates();
    }

    IEnumerator HandleMultipleAnswerAttack(string answers, float scorePercentage)
    {
        if (enemy1 == null || isProcessingAttack || player == null)
        {
            Debug.LogWarning("Cannot process attack - missing components or already processing");
            yield break;
        }

        isProcessingAttack = true;
        UpdateComponentStates();

        Debug.Log($"Processing mobile multiple answer attack with score: {scorePercentage:F1}%");

        Vector2 targetPos = new Vector2(enemy1.anchoredPosition.x - attackOffset, enemy1.anchoredPosition.y);
        yield return StartCoroutine(MovePlayerTo(targetPos));

        yield return StartCoroutine(ShowSpeechBubble(playerSpeechBubble, playerSpeechText, $"My answers: {answers}", 2f));

        bool isCorrect = scorePercentage >= passingScorePercentage;
        string resultText = isCorrect ?
            $"Excellent! {scorePercentage:F0}% correct!" :
            $"Try harder! Only {scorePercentage:F0}% correct.";

        yield return StartCoroutine(ShowSpeechBubble(enemySpeechBubble, enemySpeechText, resultText, 2f));

        yield return StartCoroutine(MovePlayerTo(originalPos));

        if (isCorrect)
        {
            AddProgress(25f);
            if (hasInputFieldComponents)
            {
                ShowInputFeedback($"Success! +25 progress. Score: {scorePercentage:F0}%", true);
            }
            if (hasMultipleChoiceComponents)
            {
                ShowMultipleChoiceFeedback($"Success! +25 progress. Score: {scorePercentage:F0}%", true);
            }
        }
        else
        {
            AddProgress(10f);
            if (hasInputFieldComponents)
            {
                ShowInputFeedback($"Keep trying! +10 progress. Score: {scorePercentage:F0}%", false);
            }
            if (hasMultipleChoiceComponents)
            {
                ShowMultipleChoiceFeedback($"Keep trying! +10 progress. Score: {scorePercentage:F0}%", false);
            }
        }

        if (clearAnswersAfterSubmission)
        {
            yield return new WaitForSeconds(clearDelay);

            if (hasInputFieldComponents)
            {
                ClearAllInputAnswers();
            }
            else if (hasDragDropComponents && dropZone != null)
            {
                dropZone.ClearDropZone();
            }
            else if (hasMultipleChoiceComponents)
            {
                foreach (var toggle in answerToggles)
                {
                    if (toggle != null) toggle.isOn = false;
                }
                ShowMultipleChoiceFeedback("Answers cleared. Select new answers to continue.", true);
            }
        }

        isProcessingAttack = false;
        UpdateComponentStates();
    }
    #endregion

    #region Flask Integration Methods
    private IEnumerator SendGameInitToFlask()
    {
        string jsonData = "{\"action\":\"game_init\",\"scene\":\"" + SceneManager.GetActiveScene().name + "\",\"game_mode\":\"" + gameMode.ToString() + "\",\"student_id\":" + studentId + ",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_event", jsonData));
    }

    private IEnumerator SendSingleAnswerToFlask(string answer)
    {
        bool isCorrect = correctAnswers.Contains(answer.ToLower());
        string jsonData = "{\"action\":\"single_answer\",\"answer\":\"" + answer + "\",\"is_correct\":" + isCorrect.ToString().ToLower() + ",\"student_id\":" + studentId + ",\"assignment_id\":" + assignmentId + ",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_answer", jsonData));
    }

    private IEnumerator SendInputAnswerToFlask(string answer)
    {
        bool isCorrect = correctAnswers.Contains(answer.ToLower());
        string jsonData = "{\"action\":\"input_answer\",\"answer\":\"" + answer + "\",\"is_correct\":" + isCorrect.ToString().ToLower() + ",\"total_answers\":" + submittedAnswers.Count + ",\"student_id\":" + studentId + ",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_answer", jsonData));
    }

    private IEnumerator SendMultipleChoiceToFlask(List<string> answers, float score)
    {
        string answersJson = "[\"" + string.Join("\",\"", answers) + "\"]";
        string jsonData = "{\"action\":\"multiple_choice\",\"answers\":" + answersJson + ",\"score\":" + score + ",\"student_id\":" + studentId + ",\"assignment_id\":" + assignmentId + ",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_answer", jsonData));
    }

    private IEnumerator SendAllInputAnswersToFlask(List<string> answers, float score)
    {
        string answersJson = "[\"" + string.Join("\",\"", answers) + "\"]";
        string jsonData = "{\"action\":\"submit_all_input\",\"answers\":" + answersJson + ",\"score\":" + score + ",\"student_id\":" + studentId + ",\"assignment_id\":" + assignmentId + ",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_answer", jsonData));
    }

    private IEnumerator SendClearAnswersToFlask(int clearedCount)
    {
        string jsonData = "{\"action\":\"clear_answers\",\"cleared_count\":" + clearedCount + ",\"student_id\":" + studentId + ",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_event", jsonData));
    }

    private IEnumerator SendDataToFlask(string endpoint, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(flaskURL + endpoint, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Successfully sent data to Flask: " + endpoint);
        }
        else
        {
            Debug.LogWarning("Failed to send data to Flask: " + request.error);
        }
        
        request.Dispose();
    }
    #endregion

    #region Utility Methods
    private string GetButtonText(Button button)
    {
        if (button == null) return "Unknown";

        TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>();
        if (tmpText != null) return tmpText.text;

        Text regularText = button.GetComponentInChildren<Text>();
        if (regularText != null) return regularText.text;

        return button.name;
    }

    private IEnumerator MovePlayerTo(Vector2 targetPosition)
    {
        if (player == null)
        {
            Debug.LogWarning("Player is null, cannot move");
            yield break;
        }

        Vector2 startPos = player.anchoredPosition;
        float journey = 0f;
        float distance = Vector2.Distance(startPos, targetPosition);

        if (distance < 0.1f) yield break;

        while (journey <= 1f)
        {
            journey += Time.deltaTime * movementSpeed;
            Vector2 newPos = Vector2.Lerp(startPos, targetPosition, journey);

            if (player != null)
            {
                player.anchoredPosition = newPos;
            }
            yield return null;
        }

        if (player != null)
        {
            player.anchoredPosition = targetPosition;
        }
    }

    private IEnumerator ShowSpeechBubble(GameObject bubble, TMP_Text text, string message, float duration)
    {
        if (bubble && text)
        {
            text.text = message;
            bubble.SetActive(true);
            yield return new WaitForSeconds(duration);
            bubble.SetActive(false);
        }
    }

    void AssignPlayerSprite()
    {
        try
        {
            string gender = PlayerPrefs.GetString("PlayerGender", "Male");
            Sprite chosenSprite = (gender.ToLower() == "female" && femaleSprite != null) ? femaleSprite : maleSprite;

            if (playerImage && chosenSprite)
            {
                playerImage.sprite = chosenSprite;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not assign player sprite: {e.Message}");
        }
    }

    void AssignEnemySprite()
    {
        try
        {
            if (enemySprites != null && enemySprites.Length > 0 && enemyImage)
            {
                int randomIndex = Random.Range(0, enemySprites.Length);
                enemyImage.sprite = enemySprites[randomIndex];
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not assign enemy sprite: {e.Message}");
        }
    }

    void AddProgress(float value)
    {
        progress += value;
        if (progress > maxProgress) progress = maxProgress;

        // Send progress to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendProgressToFlask(progress, value));
        }

        UpdateUI();
        ShowDialogueForProgress();

        if (progress >= maxProgress)
            StartCoroutine(ShowResultThenNextScene());
    }

    private IEnumerator SendProgressToFlask(float currentProgress, float addedValue)
    {
        string jsonData = "{\"action\":\"progress_update\",\"current_progress\":" + currentProgress + ",\"added_value\":" + addedValue + ",\"student_id\":" + studentId + ",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_progress", jsonData));
    }

    void UpdateUI()
    {
        if (progressBar != null)
            progressBar.value = progress / maxProgress;

        if (textProgress != null)
            textProgress.text = Mathf.RoundToInt((progress / maxProgress) * 100f) + "%";
    }

    void ShowDialogueForProgress()
    {
        if (dialogueText == null) return;

        string dialogue = dialogueAt0;

        if (progress >= 100) dialogue = dialogueAt100;
        else if (progress >= 75) dialogue = dialogueAt75;
        else if (progress >= 50) dialogue = dialogueAt50;
        else if (progress >= 25) dialogue = dialogueAt25;

        dialogueText.text = dialogue;
    }

    IEnumerator ShowResultThenNextScene()
    {
        if (resultPanel != null) resultPanel.SetActive(true);

        int score = Mathf.RoundToInt((progress / maxProgress) * 100f);
        PlayerPrefs.SetInt("PlayerScore", score);
        PlayerPrefs.SetString("LastPlayedBy", currentUser);
        PlayerPrefs.SetString("LastPlayedTime", sessionTime);
        PlayerPrefs.Save();

        // Send game completion to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendGameCompletionToFlask(score));
        }

        yield return new WaitForSeconds(2f);

        try
        {
            SceneManager.LoadScene("gameresult");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Could not load gameresult scene: {e.Message}");
        }
    }

    private IEnumerator SendGameCompletionToFlask(int finalScore)
    {
        string jsonData = "{\"action\":\"game_completion\",\"final_score\":" + finalScore + ",\"student_id\":" + studentId + ",\"assignment_id\":" + assignmentId + ",\"scene\":\"" + SceneManager.GetActiveScene().name + "\",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_completion", jsonData));
    }
    #endregion

    #region Public Methods & Database Integration Points
    public void RestartGame()
    {
        progress = 0f;
        submittedAnswers.Clear();
        isProcessingAttack = false;

        // Send restart to Flask
        if (sendToFlask)
        {
            StartCoroutine(SendGameRestartToFlask());
        }

        UpdateUI();
        if (hasInputFieldComponents)
        {
            UpdateAnswersDisplay();
            ShowInputFeedback("Game restarted! Tap to enter your answers:", true);
        }

        if (hasMultipleChoiceComponents)
        {
            foreach (var toggle in answerToggles)
            {
                if (toggle != null) toggle.isOn = false;
            }
            ShowMultipleChoiceFeedback("Game restarted! Tap to select your answers:", true);
        }

        ShowDialogueForProgress();

        if (resultPanel != null) resultPanel.SetActive(false);
    }

    private IEnumerator SendGameRestartToFlask()
    {
        string jsonData = "{\"action\":\"game_restart\",\"student_id\":" + studentId + ",\"scene\":\"" + SceneManager.GetActiveScene().name + "\",\"timestamp\":\"" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\"}";
        
        yield return StartCoroutine(SendDataToFlask("/api/game_event", jsonData));
    }

    [System.Obsolete("This is temporary - replace with API endpoint")]
    public void LoadAnswersFromDatabase()
    {
        Debug.Log("=== TEMPORARY METHOD - TO BE REPLACED ===");
        Debug.Log($"Mobile Scene: {SceneManager.GetActiveScene().name} | User: {currentUser}");
        InitializeTemporaryAnswers();
    }

    [System.Obsolete("This is temporary - replace with API endpoint")]
    public void SubmitScoreToDatabase(int score, string answers)
    {
        Debug.Log("=== TEMPORARY METHOD - TO BE REPLACED ===");
        Debug.Log($"Mobile Score: {score}, Answers: {answers}, User: {currentUser}");
        Debug.Log($"Timestamp: {sessionTime} UTC");
    }

    [ContextMenu("Test Mobile Multiple Choice Validation")]
    public void TestMultipleChoiceValidation()
    {
        Debug.Log("=== TESTING MOBILE MULTIPLE CHOICE VALIDATION ===");
        Debug.Log($"Has MC Components: {hasMultipleChoiceComponents}");
        Debug.Log($"Submit Toggle Button: {submitToggleButton != null}");
        Debug.Log($"Toggles Count: {answerToggles.Count}");
        Debug.Log($"Is Processing Attack: {isProcessingAttack}");
        Debug.Log($"Platform: {Application.platform} | Mobile: {Application.isMobilePlatform}");

        if (answerToggles != null)
        {
            for (int i = 0; i < answerToggles.Count; i++)
            {
                Toggle toggle = answerToggles[i];
                if (toggle != null)
                {
                    Debug.Log($"Toggle {i}: Active={toggle.gameObject.activeInHierarchy}, Selected={toggle.isOn}");
                }
                else
                {
                    Debug.Log($"Toggle {i}: NULL");
                }
            }
        }
    }
    #endregion

    #region Question Management
    // Method to update correct answers from QuestionManager
    public void UpdateCorrectAnswers(List<string> newCorrectAnswers)
    {
        if (correctAnswers == null)
            correctAnswers = new HashSet<string>();
        
        correctAnswers.Clear();
        
        if (newCorrectAnswers != null)
        {
            foreach (string answer in newCorrectAnswers)
            {
                if (!string.IsNullOrEmpty(answer))
                {
                    correctAnswers.Add(caseSensitiveAnswers ? answer : answer.ToLower());
                }
            }
        }
        
        Debug.Log($"Updated correct answers: {string.Join(", ", correctAnswers)}");
    }
    #endregion
}
