using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

[Serializable]
public class TwoChoiceQuestion
{
    [TextArea(2, 3)]
    public string questionText;
    public string correctAnswer;
    public string wrongAnswer;
}

[Serializable]
public class BattleScoreUpdate
{
    public int score;
    public int levelReached;
    public string timestamp;
}

public class InsideBattleManager : MonoBehaviour
{
    [Header("Firebase Config")]
    [SerializeField] private string databaseUrl = "https://pamana-bb84d-default-rtdb.firebaseio.com/";

    [Header("Level Settings")]
    public int currentLevelNumber = 1;
    public int pointsPerCorrect = 100;
    public string nextRoomScene = "Rooms 1";

    [Header("3 Questions List")]
    public TwoChoiceQuestion[] questions = new TwoChoiceQuestion[3];
    private int currentQuestionIndex = 0;
    private int sessionScoreAccumulated = 0;
    private bool isOptionACorrect = false;
    private bool isRoomAlreadyCleared = false;

    [Header("Enemy & Artifact")]
    public Button enemyButton;
    public GameObject monsterObject;
    public GameObject artifactObject;

    [Header("Question UI")]
    public GameObject battleQuestionPanel;
    public TMP_Text questionPromptText;
    public Button optionAButton;
    public TMP_Text optionAText;
    public Button optionBButton;
    public TMP_Text optionBText;

    [Header("Game Over & Victory Panels")]
    public GameObject gameOverPanel;
    public Button retryButton;
    public GameObject victoryPanel;
    public TMP_Text victoryScoreText;
    public TMP_Text firebaseStatusText;
    public Button continueRoomsButton;

    void Start()
    {
        if (battleQuestionPanel != null) battleQuestionPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Check if player already cleared this room previously
        int savedLevel = PlayerPrefs.GetInt("CurrentPlayerLevel", 1);
        isRoomAlreadyCleared = (savedLevel > currentLevelNumber);

        if (enemyButton != null)
            enemyButton.onClick.AddListener(StartBattleQuestions);

        if (optionAButton != null)
            optionAButton.onClick.AddListener(() => OnOptionSelected(true));

        if (optionBButton != null)
            optionBButton.onClick.AddListener(() => OnOptionSelected(false));

        if (retryButton != null)
            retryButton.onClick.AddListener(RestartCurrentScene);

        if (continueRoomsButton != null)
            continueRoomsButton.onClick.AddListener(ReturnToNextRoom);
    }

    void StartBattleQuestions()
    {
        currentQuestionIndex = 0;
        sessionScoreAccumulated = 0;
        DisplayCurrentQuestion();
    }

    void DisplayCurrentQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            OnAllQuestionsCleared();
            return;
        }

        battleQuestionPanel.SetActive(true);
        TwoChoiceQuestion q = questions[currentQuestionIndex];

        if (questionPromptText != null)
            questionPromptText.text = $"({currentQuestionIndex + 1}/3) {q.questionText}";

        // Randomize button positions
        isOptionACorrect = UnityEngine.Random.value > 0.5f;

        if (isOptionACorrect)
        {
            if (optionAText != null) optionAText.text = q.correctAnswer;
            if (optionBText != null) optionBText.text = q.wrongAnswer;
        }
        else
        {
            if (optionAText != null) optionAText.text = q.wrongAnswer;
            if (optionBText != null) optionBText.text = q.correctAnswer;
        }
    }

    void OnOptionSelected(bool clickedOptionA)
    {
        bool answeredCorrectly = (clickedOptionA == isOptionACorrect);

        if (answeredCorrectly)
        {
            // Only accumulate points if the player has NOT cleared this room before
            if (!isRoomAlreadyCleared)
            {
                sessionScoreAccumulated += pointsPerCorrect;
                Debug.Log($"Correct! +{pointsPerCorrect} points.");
            }
            else
            {
                Debug.Log("Correct! (Review Mode: 0 points added)");
            }
        }
        else
        {
            Debug.Log("Wrong answer selected! 0 points awarded.");
        }

        currentQuestionIndex++;

        if (currentQuestionIndex < questions.Length)
        {
            DisplayCurrentQuestion();
        }
        else
        {
            OnAllQuestionsCleared();
        }
    }

    void OnAllQuestionsCleared()
    {
        battleQuestionPanel.SetActive(false);

        if (monsterObject != null) Destroy(monsterObject);
        if (artifactObject != null) Destroy(artifactObject);

        if (victoryPanel != null) victoryPanel.SetActive(true);

        if (isRoomAlreadyCleared)
        {
            // Room was previously completed: inform the player and do NOT update Firebase
            if (victoryScoreText != null)
            {
                victoryScoreText.text = $"Room Cleared (Review Mode)\nNo additional points awarded.\nCurrent Score: {PlayerPrefs.GetInt("CurrentPlayerScore", 0)}";
            }

            if (firebaseStatusText != null)
            {
                firebaseStatusText.text = "Already recorded in database.";
            }
        }
        else
        {
            // First-time clear: unlock next level and sync score to Firebase
            PlayerPrefs.SetInt($"Level{currentLevelNumber + 1}_Unlocked", 1);
            PlayerPrefs.Save();

            if (victoryScoreText != null)
            {
                victoryScoreText.text = $"Room Cleared!\nScore Earned: +{sessionScoreAccumulated}";
            }

            StartCoroutine(SyncLevelProgressToFirebase(sessionScoreAccumulated, currentLevelNumber));
        }
    }

    private IEnumerator SyncLevelProgressToFirebase(int pointsToAdd, int level)
    {
        if (firebaseStatusText != null)
            firebaseStatusText.text = "Syncing score to database...";

        string playerId = PlayerPrefs.GetString("CurrentPlayerId", "player_01");
        string endpoint = $"{databaseUrl}players/{playerId}.json";

        int existingScore = PlayerPrefs.GetInt("CurrentPlayerScore", 0);
        int updatedTotalScore = existingScore + pointsToAdd;
        int nextLevel = level + 1;

        BattleScoreUpdate updateData = new BattleScoreUpdate
        {
            score = updatedTotalScore,
            levelReached = nextLevel,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string jsonPayload = JsonUtility.ToJson(updateData);

        using (UnityWebRequest request = new UnityWebRequest(endpoint, "PATCH"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                PlayerPrefs.SetInt("CurrentPlayerScore", updatedTotalScore);
                PlayerPrefs.SetInt("CurrentPlayerLevel", nextLevel);
                PlayerPrefs.Save();

                // Mark as cleared for subsequent runs in the same session
                isRoomAlreadyCleared = true;

                if (firebaseStatusText != null)
                    firebaseStatusText.text = "Score saved to Firebase!";
            }
            else
            {
                if (firebaseStatusText != null)
                    firebaseStatusText.text = "Sync error: " + request.error;
            }
        }
    }

    void RestartCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ReturnToNextRoom()
    {
        SceneManager.LoadScene(nextRoomScene);
    }
}