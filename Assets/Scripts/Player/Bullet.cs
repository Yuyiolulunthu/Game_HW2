using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;  // 幾秒後自動銷毀
    public int damage = 10;       // 需要傷害時用得到

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // 若 Collider 是 Trigger（推薦）
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 拿敵人的 EnemyHealth 扣血
            EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }


    // 若不用 Trigger、走真碰撞，改用這個：
    // void OnCollisionEnter(Collision collision) { Destroy(gameObject); }
}
