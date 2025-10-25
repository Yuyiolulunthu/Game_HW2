using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class GameSession : MonoBehaviour
{
    public static GameSession I;

    [Header("Scene Names")]
    [SerializeField] string menuSceneName = "GameStart";
    [SerializeField] string levelOneSceneName = "GameSceneC";
    [SerializeField] string resultSceneName = "GameEnd";

    [Header("Run Data")]
    [SerializeField] private int maxHP = 100;
    [SerializeField] private int currentHP = 100;
    [SerializeField] private int score = 0;

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;
    public int Score => score;

    public event Action<int,int> OnPlayerHPChanged; // (current, max)
    public event Action<int> OnScoreChanged;

    bool eventSubscribed = false; 

    void Awake()
{
    Debug.Log($"[GameSession] Awake - existing instance? {(I != null)}  scene={SceneManager.GetActiveScene().name}");

    if (I != null && I != this)
    {
        Destroy(gameObject);
        return;
    }

    I = this;
    DontDestroyOnLoad(gameObject);

    if (!eventSubscribed)
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        eventSubscribed = true;
    }
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
        OnPlayerHPChanged?.Invoke(currentHP, maxHP);
        OnScoreChanged?.Invoke(score);
        if (scene.name == resultSceneName)
            ShowResultScore();
    }

    void ShowResultScore()
    {
        TMP_Text scoreText = GameObject.Find("ScoreText")?.GetComponent<TMP_Text>();
        if (scoreText != null)
            scoreText.text = $"<size=64><b>Score\n{Score}</b></size>";
    }

    // ---- 封裝操作：統一由這裡變更狀態 ----
    public void AddScore(int amount)
    {
        score = Mathf.Max(0, score + amount);
        OnScoreChanged?.Invoke(score);
    }

    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        OnPlayerHPChanged?.Invoke(currentHP, maxHP);
    }

    bool isRestarting = false;
    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        OnPlayerHPChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            HandlePlayerDeath();
        }
    }

    void HandlePlayerDeath()
    {
        if (isRestarting) return;
        isRestarting = true;
        StartCoroutine(RestartLevelCoroutine());
    }

    IEnumerator RestartLevelCoroutine()
    {
        // 如果想留一點死亡動畫時間可調整這裡
        yield return new WaitForSeconds(0.6f);

        // ? 回滿血（在重載前就先回滿，HUD 也能先更新一次）
        currentHP = maxHP;
        OnPlayerHPChanged?.Invoke(currentHP, maxHP);

        // ? 重新載入目前關卡（敵人血/道具都會回初始）
        string curr = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(curr);

        // ? 解除鎖
        isRestarting = false;

        // （可選）如果你希望「死亡就扣分或歸零」，在這裡調整：
        // score = 0; OnScoreChanged?.Invoke(score);
    }

    // ---- Scene flow control（沿用妳原本 API）----
    public void ResetRun()
    {
        score = 0;
        currentHP = maxHP;
        OnPlayerHPChanged?.Invoke(currentHP, maxHP);
        OnScoreChanged?.Invoke(score);
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
