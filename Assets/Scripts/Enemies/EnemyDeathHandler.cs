using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyDeathHandler : MonoBehaviour
{
    public enum DeathAction
    {
        None,
        LoadSceneByName,
        FinishToResult // GameSession��
    }

    [Header("Config")]
    public DeathAction action = DeathAction.None;
    public string sceneNameToLoad; // ����

    // �� EnemyHealth.OnDied �I�s����ƥ�
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
