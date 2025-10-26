using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = 30;
    [SerializeField] private int currentHP;
    [SerializeField] private int scoreOnKill = 100;

    [System.Serializable]
    public class HpChangedEvent : UnityEvent<int,int> {} // (cur,max)
    public HpChangedEvent OnHPChanged = new HpChangedEvent();

    public UnityEvent OnDied;  

    public int MaxHP => maxHP;
    public int CurrentHP => currentHP;

    private void Awake()
    {
        currentHP = maxHP;
        OnHPChanged.Invoke(currentHP, maxHP);
    }

    public void TakeDamage(int dmg)
    {
        if (currentHP <= 0) return;
        currentHP = Mathf.Max(0, currentHP - dmg);
        OnHPChanged.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            GameSession.I?.AddScore(scoreOnKill);
            OnDied?.Invoke();          // 對外廣播死亡
            Destroy(gameObject);       
        }
    }

    public void Heal(int val)
    {
        if (currentHP <= 0) return;
        currentHP = Mathf.Min(maxHP, currentHP + val);
        OnHPChanged.Invoke(currentHP, maxHP);
    }
}
