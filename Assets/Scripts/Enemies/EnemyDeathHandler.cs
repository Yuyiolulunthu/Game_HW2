using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyDeathHandler : MonoBehaviour
{
    public enum DeathAction
    {
        None,
        LoadSceneByName,
        FinishToResult // GameSession的
    }

    [Header("Config")]
    public DeathAction action = DeathAction.None;
    public string sceneNameToLoad; // 切關

    // 給 EnemyHealth.OnDied 呼叫死後事件
    public void Perform()
    {
        switch (action)
        {
            case DeathAction.LoadSceneByName:
                if (!string.IsNullOrEmpty(sceneNameToLoad))
                    SceneManager.LoadScene(sceneNameToLoad);
                break;

            case DeathAction.FinishToResult:
                GameSession.I?.PlayWinSFXOnce();
                GameSession.I?.FinishRunAndGoResult();
                break;

            default:
                break;
        }
    }
}
