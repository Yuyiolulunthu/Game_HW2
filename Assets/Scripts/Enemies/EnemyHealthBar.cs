using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private EnemyHealth target;
    [SerializeField] private Slider slider;
    [SerializeField] private Vector3 worldOffset = new Vector3(0, 2f, 0);
    Transform cam;

    private void Start()
    {
        if (!target) target = GetComponentInParent<EnemyHealth>();
        cam = Camera.main ? Camera.main.transform : null;

        // ªì©l¦P¨B
        if (target)
            target.OnHPChanged.AddListener(UpdateBar);
        if (target) UpdateBar(target.CurrentHP, target.MaxHP);
    }

    private void LateUpdate()
    {
        if (target) transform.position = target.transform.position + worldOffset;
        if (cam) transform.LookAt(transform.position + cam.forward); 
    }

    private void OnDestroy()
    {
        if (target)
            target.OnHPChanged.RemoveListener(UpdateBar);
    }

    private void UpdateBar(int cur, int max)
    {
        if (!slider) return;
        slider.maxValue = max;
        slider.value = cur;
    }
}
