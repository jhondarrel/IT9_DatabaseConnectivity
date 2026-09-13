using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;

[Serializable]
public class UserAccount
{
    public string playerId;
    public string playerName;
    public string password;
    public int score;
    public int levelReached;
    public string timestamp;
}

public class PlayerAuthManager : MonoBehaviour
{
    [Header("Firebase Config")]
    [SerializeField] private string databaseUrl = "https://pamana-bb84d-default-rtdb.firebaseio.com/";

    [Header("UI Inputs")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_Text statusText;

    [Header("Target Scene")]
    [SerializeField] private string nextSceneName = "Missing";

    // Hook this to the "Register / Create Account" Button
    public void OnRegisterClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Username and password cannot be empty.";
            return;
        }

        StartCoroutine(RegisterUser(username, password));
    }

    // Hook this to the "Log In" Button
    public void OnLoginClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            statusText.text = "Username and password cannot be empty.";
            return;
        }

        StartCoroutine(LoginUser(username, password));
    }

    // --- REGISTER NEW ACCOUNT ---
    private IEnumerator RegisterUser(string username, string password)
    {
        statusText.text = "Checking username availability...";
        string cleanId = username.ToLower().Replace(" ", "_");
        string endpoint = $"{databaseUrl}players/{cleanId}.json";

        // Step 1: Check if user already exists
        using (UnityWebRequest checkReq = UnityWebRequest.Get(endpoint))
        {
            yield return checkReq.SendWebRequest();

            if (checkReq.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Network error: " + checkReq.error;
                yield break;
            }

            // If it doesn't return "null", the user already exists!
            if (checkReq.downloadHandler.text != "null")
            {
                statusText.text = "Username already taken! Please log in.";
                yield break;
            }
        }

        // Step 2: Create new user
        statusText.text = "Creating new account...";

        UserAccount newAccount = new UserAccount
        {
            playerId = cleanId,
            playerName = username,
            password = password,
            score = 0,
            levelReached = 1,
            timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
        };

        string jsonPayload = JsonUtility.ToJson(newAccount);

        using (UnityWebRequest putReq = new UnityWebRequest(endpoint, "PUT"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            putReq.uploadHandler = new UploadHandlerRaw(bodyRaw);
            putReq.downloadHandler = new DownloadHandlerBuffer();
            putReq.SetRequestHeader("Content-Type", "application/json");

            yield return putReq.SendWebRequest();

            if (putReq.result == UnityWebRequest.Result.Success)
            {
                // Save locally to track this session
                CachePlayerData(newAccount);

                statusText.text = "Account created! Entering game...";
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                statusText.text = "Failed to register: " + putReq.error;
            }
        }
    }

    // --- LOG IN EXISTING USER & RETRIEVE DATA ---
    private IEnumerator LoginUser(string username, string password)
    {
        statusText.text = "Logging in...";
        string cleanId = username.ToLower().Replace(" ", "_");
        string endpoint = $"{databaseUrl}players/{cleanId}.json";

        using (UnityWebRequest getReq = UnityWebRequest.Get(endpoint))
        {
            yield return getReq.SendWebRequest();

            if (getReq.result != UnityWebRequest.Result.Success)
            {
                statusText.text = "Network error: " + getReq.error;
                yield break;
            }

            if (getReq.downloadHandler.text == "null")
            {
                statusText.text = "User does not exist! Please register.";
                yield break;
            }

            // Parse fetched JSON data
            UserAccount account = JsonUtility.FromJson<UserAccount>(getReq.downloadHandler.text);

            // Verify password
            if (account.password != password)
            {
                statusText.text = "Incorrect password. Try again.";
                yield break;
            }

            // SUCCESS: Recover progress into PlayerPrefs so all scenes have it
            CachePlayerData(account);

            statusText.text = $"Welcome back, {account.playerName}!";
            SceneManager.LoadScene(nextSceneName);
        }
    }

    private void CachePlayerData(UserAccount acc)
    {
        PlayerPrefs.SetString("CurrentPlayerId", acc.playerId);
        PlayerPrefs.SetString("CurrentPlayerName", acc.playerName);
        PlayerPrefs.SetInt("CurrentPlayerScore", acc.score);
        PlayerPrefs.SetInt("CurrentPlayerLevel", acc.levelReached);
        PlayerPrefs.Save();
    }
}