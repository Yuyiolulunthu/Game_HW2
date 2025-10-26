using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMove : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;
    public float turnSpeed = 12f;

    [Header("Shoot")]
    public GameObject bulletPrefab;   // 指到 Sphere 子彈 Prefab（需有 Rigidbody）
    public Transform firePoint;       // 槍口/發射點（玩家子物件，放前方）
    public float bulletSpeed = 16f;
    public float bulletLife = 3f;
    public float cooldown = 0.25f;    // 射速間隔
    public bool useAnimationEvent = false; // 勾選則由動畫事件呼叫 AnimFire()

    [Header("Audio")]
    public AudioClip shootSFX;
    [Range(0f,1f)] public float shootVolume = 1f;
    private AudioSource audioSource;

    [Header("Animator (optional)")]
    public Animator animator;         // 可留空；若指定會驅動參數
    public string speedParam = "Speed";
    public string shootTrigger = "ShootTrig";

    Rigidbody rb;
    Vector3 lastFacingDir = Vector3.forward;
    float nextFireTime = 0f;

    void Start(){
        int L = LayerMask.NameToLayer("Projectile");
        if (L != -1) Physics.IgnoreLayerCollision(L, L, true);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!animator) animator = GetComponent<Animator>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var f = transform.forward; f.y = 0;
        if (f.sqrMagnitude > 1e-6f) lastFacingDir = f.normalized;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; 
            audioSource.loop = false;
        }
    }

    void Update()
    {
        // 射擊輸入（不鎖移動、不打斷跑步）
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + cooldown;

            if (animator && !string.IsNullOrEmpty(shootTrigger))
            {
                animator.ResetTrigger(shootTrigger);
                animator.SetTrigger(shootTrigger);
            }

            if (!useAnimationEvent)
                FireNow(); // 直接發射；若改用動畫事件就取消這行
        }
    }

    void FixedUpdate()
    {
        // 讀取 WASD
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0f, v);

        if (dir.sqrMagnitude > 1e-4f)
        {
            dir.Normalize();

            // 移動（保留 y 速度）
            Vector3 vel = dir * moveSpeed; vel.y = rb.velocity.y;
            rb.velocity = vel;

            // 轉向
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);

            // 記錄最後面向
            lastFacingDir = dir;
        }
        else
        {
            // 停止水平速度
            Vector3 vel = rb.velocity; vel.x = 0; vel.z = 0; rb.velocity = vel;
        }

        // 動畫 Speed 參數
        if (animator && !string.IsNullOrEmpty(speedParam))
        {
            float hs = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
            animator.SetFloat(speedParam, hs);
        }
    }

    void SetLayerRecursively(GameObject root, int layer){
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }

    // 立即產生子彈（可由動畫事件呼叫）
    public void FireNow()
    {
        if (shootSFX != null && audioSource != null) audioSource.PlayOneShot(shootSFX, shootVolume);
        if (!bulletPrefab) { Debug.LogWarning("bulletPrefab 未指定！"); return; }

        // 射擊方向
        Vector3 dir = lastFacingDir.sqrMagnitude > 1e-6f
                        ? lastFacingDir
                        : new Vector3(transform.forward.x, 0f, transform.forward.z);
        if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;
        dir.Normalize();

        // 槍口與生成點（向前推避免卡在自己體內）
        Vector3 basePos = firePoint ? firePoint.position : (transform.position + Vector3.up * 1.0f);
        float spawnOffset = 0.6f; // 至少大於子彈半徑*2
        Vector3 spawnPos = (firePoint ? firePoint.position : transform.position + Vector3.up*1.0f)
                        + dir * spawnOffset;

        // 生成
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        GameObject go = Instantiate(bulletPrefab, spawnPos, rot);
        if (!go.activeInHierarchy) go.SetActive(true);
        int bulletLayer = LayerMask.NameToLayer("Projectile");
        if (bulletLayer != -1) SetLayerRecursively(go, bulletLayer);

        // 確保有 Renderer（不然看起來像「閃一下」）
        if (!go.GetComponentInChildren<Renderer>())
            Debug.LogWarning("子彈 Prefab 沒有 Renderer（加個 Sphere Mesh/材質）。");

        // 剛體設定（保底）
        Rigidbody brb = go.GetComponent<Rigidbody>();
        if (!brb) brb = go.AddComponent<Rigidbody>();
        brb.useGravity = false;
        brb.isKinematic = false;
        brb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        brb.velocity = dir * bulletSpeed;

        // Collider 設定（保底）
        Collider bulletCol = go.GetComponent<Collider>();
        if (!bulletCol) bulletCol = go.AddComponent<SphereCollider>(); // 半徑可在 Prefab 調
        // 立刻忽略與玩家所有 Collider 的碰撞，避免一生成就被打掉
        foreach (var myCol in GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(bulletCol, myCol, true);

        // 生存時間（確保 >= 1 秒）
        float life = Mathf.Max(bulletLife, 1.0f);
        Destroy(go, life);

        // Scene 視窗可視化
        Debug.DrawRay(spawnPos, dir * 2f, Color.yellow, 1.0f);
    }

    // 動畫事件在開槍幀呼叫這個（勾 useAnimationEvent 才使用）
    public void AnimFire()
    {
        FireNow();
    }

}