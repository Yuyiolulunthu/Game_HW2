using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUD : MonoBehaviour
{
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TMP_Text hpText; 

    private void Awake()
    {
        GameSession.I.OnPlayerHPChanged += UpdateHP;

        UpdateHP(GameSession.I.CurrentHP, GameSession.I.MaxHP);
    }

    private void OnDestroy()
    {
        if (GameSession.I != null)
            GameSession.I.OnPlayerHPChanged -= UpdateHP;
    }

    private void UpdateHP(int cur, int max)
    {
        if (playerHealthSlider)
        {
            playerHealthSlider.maxValue = max;
            playerHealthSlider.value = cur;
        }

        if (hpText)
        {
            hpText.text = $"{cur}/{max}";
        }
    }
}
