using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    [Header("Canvas References")]
    [SerializeField] Canvas mainMenuCanvas;   
    [SerializeField] Canvas rulesCanvas;      

    [Header("Gameplay Scene")]
    [SerializeField] string gameplaySceneName = "SampleScene"; 

    void Awake()
    {
        if (mainMenuCanvas != null) mainMenuCanvas.enabled = true;
        if (rulesCanvas != null) rulesCanvas.enabled = false;
    }

    void Update()
    {
        // press esc to go back to menu
        if (rulesCanvas != null && rulesCanvas.enabled && Input.GetKeyDown(KeyCode.Escape))
        {
            BackToMenu();
        }
    }

    // for PlayBtn
    public void GoToGame()
    {
        SceneManager.LoadScene(gameplaySceneName);
    }

    // for RuleBtn
    public void ShowRules()
    {
        if (rulesCanvas == null || mainMenuCanvas == null) return;
        rulesCanvas.enabled = true;
        mainMenuCanvas.enabled = false; 
    }

    public void BackToMenu()
    {
        if (rulesCanvas == null || mainMenuCanvas == null) return;
        rulesCanvas.enabled = false;
        mainMenuCanvas.enabled = true;
        EventSystem.current.SetSelectedGameObject(null);
    }

    // for ResultBtn
     public void GoResult() { GameSession.I.FinishRunAndGoResult();}
}