using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] private GameObject healthBarPrefab;
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private Slider hpSlider;
    private Transform cam;

    void Start()
    {
        // 生成血條在玩家上方
        GameObject bar = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity);
        bar.transform.SetParent(transform); // 跟著玩家移動
        hpSlider = bar.GetComponentInChildren<Slider>();

        // 綁定 GameSession 血量事件
        GameSession.I.OnPlayerHPChanged += UpdateHP;
        cam = Camera.main.transform;

        // 初始化
        UpdateHP(GameSession.I.CurrentHP, GameSession.I.MaxHP);
    }

    void LateUpdate()
    {
        // 讓血條永遠面向攝影機
        if (cam && hpSlider)
            hpSlider.transform.parent.LookAt(cam);
    }

    private void UpdateHP(int cur, int max)
    {
        if (!hpSlider) return;
        hpSlider.maxValue = max;
        hpSlider.value = cur;
    }

    private void OnDestroy()
    {
        if (GameSession.I != null)
            GameSession.I.OnPlayerHPChanged -= UpdateHP;
    }
}
