using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;

public class GameSession : MonoBehaviour
{
    public static GameSession I;

    [Header("Scene Names")]
    [SerializeField] string menuSceneName = "GameStart";
    [SerializeField] string levelOneSceneName = "Gameplay";
    [SerializeField] string resultSceneName = "GameEnd";

    [Header("Run Data")]
    public int Score;

    bool eventSubscribed = false; 

    void Awake()
{
    Debug.Log($"[GameSession] Awake - existing instance? {(I != null)}  scene={SceneManager.GetActiveScene().name}");

    if (I != null)
    {
        Destroy(gameObject);
        return;
    }

    I = this;
    DontDestroyOnLoad(gameObject);

    SceneManager.sceneLoaded += OnSceneLoaded;
}


    void OnDestroy()
    {
        if (eventSubscribed)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            eventSubscribed = false;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == resultSceneName)
            ShowResultScore();
    }

    void ShowResultScore()
    {
        TMP_Text scoreText = GameObject.Find("ScoreText")?.GetComponent<TMP_Text>();
        if (scoreText != null)
            scoreText.text = $"<size=64><b>Score\n{Score}</b></size>";
    }

    // scene flow control
    public void ResetRun()
    {
        Score = 0;
    }

    public void FinishRunAndGoResult()
    {
        SafeClearSelection();
        SceneManager.LoadScene(resultSceneName);
    }

    public void Replay()
    {
        ResetRun();
        SafeClearSelection();
        SceneManager.LoadScene(levelOneSceneName);
    }

    public void ToMenu()
    {
        ResetRun();
        SafeClearSelection();
        SceneManager.LoadScene(menuSceneName);
    }

    void SafeClearSelection()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }
}
