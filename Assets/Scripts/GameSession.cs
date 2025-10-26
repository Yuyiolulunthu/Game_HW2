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

    [Header("SFX")]
    [SerializeField] private AudioClip deathSFX;
    [SerializeField, Range(0f,1f)] private float deathSFXVolume = 1f;
    [SerializeField] private AudioClip winSFX;
    [SerializeField, Range(0f,1f)] private float winSFXVolume = 1f;

    private AudioSource sfxSource;
    private bool hasDiedSFXPlayed = false; 
    private bool hasWinSFXPlayed = false;


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

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;


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

    void PlayDeathSFXOnce()
    {
        if (hasDiedSFXPlayed) return;
        hasDiedSFXPlayed = true;

        if (deathSFX != null && sfxSource != null)
            sfxSource.PlayOneShot(deathSFX, deathSFXVolume);
    }

    public void PlayWinSFXOnce()
    {
        if (hasWinSFXPlayed) return;
        hasWinSFXPlayed = true;

        if (winSFX != null && sfxSource != null)
            sfxSource.PlayOneShot(winSFX, winSFXVolume);
    }



    // 從這裡改全局狀態
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
            PlayDeathSFXOnce();
            SafeClearSelection();
            SceneManager.LoadScene(resultSceneName);
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
        yield return new WaitForSeconds(0.6f);

        // 回滿血
        currentHP = maxHP;
        OnPlayerHPChanged?.Invoke(currentHP, maxHP);

        // 重新載入目前關卡（敵人血/道具都會回初始）
        string curr = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(curr);

        isRestarting = false;
    }

    // Scene flow control
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
