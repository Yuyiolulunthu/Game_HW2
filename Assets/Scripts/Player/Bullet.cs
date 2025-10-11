using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;  // 幾秒後自動銷毀
    public int damage = 1;       // 需要傷害時用得到

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // 若 Collider 是 Trigger（推薦）
    void OnTriggerEnter(Collider other)
    {
        // TODO: 這裡寫命中邏輯（例如扣血、打到牆就消失）
        // if (other.CompareTag("Enemy")) { other.GetComponent<Enemy>()?.Hit(damage); }

        Destroy(gameObject);
    }

    // 若不用 Trigger、走真碰撞，改用這個：
    // void OnCollisionEnter(Collision collision) { Destroy(gameObject); }
}
