using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMManager : MonoBehaviour
{
    public static BGMManager I;

    [Header("BGM Clips")]
    public AudioClip menuBGM;
    public AudioClip levelBGM;
    public AudioClip resultBGM;

    private AudioSource audioSource;

    void Awake()
    {
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.volume = 0.5f;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "GameStart":
                PlayBGM(menuBGM);
                break;
            case "Gameplay":
            case "GameSceneC":
                PlayBGM(levelBGM);
                break;
            case "GameEnd":
                PlayBGM(resultBGM);
                break;
            default:
                PlayBGM(levelBGM);
                break;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource.clip == clip && audioSource.isPlaying) return; 

        audioSource.clip = clip;
        audioSource.Play();
    }

    public void StopBGM()
    {
        audioSource.Stop();
    }

    public void SetVolume(float v)
    {
        audioSource.volume = Mathf.Clamp01(v);
    }
}
