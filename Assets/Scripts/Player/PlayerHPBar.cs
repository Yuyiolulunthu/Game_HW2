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
        // �ͦ�����b���a�W��
        GameObject bar = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity);
        bar.transform.SetParent(transform); // ��۪��a����
        hpSlider = bar.GetComponentInChildren<Slider>();

        // �j�w GameSession ��q�ƥ�
        GameSession.I.OnPlayerHPChanged += UpdateHP;
        cam = Camera.main.transform;

        // ��l��
        UpdateHP(GameSession.I.CurrentHP, GameSession.I.MaxHP);
    }

    void LateUpdate()
    {
        // ������û����V��v��
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
