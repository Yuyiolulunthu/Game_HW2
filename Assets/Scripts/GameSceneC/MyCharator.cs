using UnityEngine;
using System.Linq; // 為了用 LINQ 篩選子彈名稱

namespace EnemyC
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class MyCharator : MonoBehaviour
    {
        [Header("Tags")]
        public string groundTag = "Ground";
        public string playerTag = "Player";

        [Header("Movement")]
        public float moveSpeed = 3.0f;
        public float rotateSpeedDegPerSec = 360f;
        public Vector2 rotateIntervalRange = new Vector2(5.0f, 8.0f);
        public float yRayHeight = 15f;

        [Header("Edge Guard")]
        public float forwardProbeDistance = 1.2f;
        public float groundSnapSkin = 0.01f;
        public float repickBigTurnMin = 135f;
        public float repickBigTurnMax = 225f;

        [Header("Combat / Detection")]
        public float attackRange = 2.0f;
        public float facingBoostWhileAttack = 3f;

        [Header("Animator")]
        public float animatorPlaybackSpeed = 1.0f;
        static readonly int AttackState = Animator.StringToHash("Base Layer.Attack");
        static readonly int WalkState = Animator.StringToHash("Base Layer.Locomotion");
        static readonly int IdleState = Animator.StringToHash("Base Layer.Idle");

        [Header("Debug")]
        public bool debugLogs = true;

        [Header("Hit VFX")]
        public GameObject enemyCBloodPrefab;     // 指到你的 EnemyCBlood Prefab
        public Vector3 bloodOffset = new Vector3(0f, 0.05f, 0f);
        public bool destroyBulletAfterHit = true;

        [Header("Layers")]
        public LayerMask groundMask;
        public LayerMask selfMask;

        // ---- private fields ----
        private Animator anim;
        private Rigidbody rb;
        private CapsuleCollider col;
        private Transform playerT;
        private Quaternion targetFacing;
        private float rotateTimer;

        [Header("Attack Effect")]
        public GameObject enemyCAttackEffectPrefab;   // 指向 EnemyCAttackEffect 預製體
        public Vector3 attackEffectOffset = new Vector3(0f, 1f, 0.5f);  // 生成位置偏移
        public float attackEffectCooldown = 1.0f;     // 冷卻時間（秒）
        private float attackEffectTimer = 0f;         // 計時器

        [Header("Attack Sound")]
        public AudioClip attackAudioClip;             // 指定 04_Fire_explosion_04_medium
        public float attackAudioVolume = 0.0f;        // 音量（可在 Inspector 調整）
        private AudioSource audioSource;              // 內部音源

        [Header("Hit Sound")]
        public AudioClip hitAudioClip;     // 指向 51_Flee_02 音效
        public float hitAudioVolume = 1.0f;
        private AudioSource audioSourceHit; // 專用音源


        // 嘗試生成攻擊特效與音效（有冷卻）
        private void TrySpawnAttackEffect()
        {
            attackEffectTimer -= Time.fixedDeltaTime;
            if (attackEffectTimer > 0f) return; // 尚未冷卻

            // --- 攻擊特效 ---
            if (enemyCAttackEffectPrefab != null)
            {
                Vector3 spawnPos = transform.position + transform.TransformDirection(attackEffectOffset);
                spawnPos.y -= 1.0f;
                Instantiate(enemyCAttackEffectPrefab, spawnPos, transform.rotation);

                if (debugLogs)
                    Debug.Log($"[AttackEffect] Spawned at {spawnPos}");
            }

            // --- 攻擊音效 ---
            if (attackAudioClip != null && audioSource != null)
            {
                audioSource.volume = attackAudioVolume;
                audioSource.PlayOneShot(attackAudioClip);

                if (debugLogs)
                    Debug.Log("[AttackSound] Played attack sound effect.");
            }

            // 重置冷卻時間
            attackEffectTimer = attackEffectCooldown;
        }

        void Start()
        {
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            col = GetComponent<CapsuleCollider>();
            rb.useGravity = true;

            //自動建立 AudioSource（如果物件上沒有的話）
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D 音效
            audioSource.volume = attackAudioVolume;

            //受擊音效的 AudioSource（獨立一個，避免疊音被打斷）
            audioSourceHit = gameObject.AddComponent<AudioSource>(); // 直接新增第二個
            audioSourceHit.playOnAwake = false;
            audioSourceHit.spatialBlend = 1f; // 3D
            audioSourceHit.volume = hitAudioVolume;

            targetFacing = transform.rotation;
            rotateTimer = UnityEngine.Random.Range(rotateIntervalRange.x, rotateIntervalRange.y);

            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go != null) playerT = go.transform;
        }

        void Update()
        {
            anim.speed = animatorPlaybackSpeed;
            Vector3 pos = transform.position;
            pos.y = 0f;
            transform.position = pos;
            if (playerT == null)
            {
                var go = GameObject.FindGameObjectWithTag(playerTag);
                if (go != null) playerT = go.transform;
            }
        }

        void FixedUpdate()
        {
            StickToGround();

            bool inAttack = false;
            Vector3 forwardMove = Vector3.zero;

            // 玩家偵測與攻擊
            if (playerT != null)
            {
                float planarDist = PlanarDistance(transform.position, playerT.position);
                if (planarDist <= attackRange)
                {
                    FaceTowards(playerT.position, rotateSpeedDegPerSec * facingBoostWhileAttack);
                    inAttack = true;
                }
            }

            if (!inAttack)
            {
                DoRandomTurn();

                Vector3 desired = transform.forward * moveSpeed * Time.fixedDeltaTime;
                
                if (CanMoveForward(desired))
                {
                    forwardMove = desired;
                    Debug.Log("TurnOrNot(desired)=true");
                }
                else
                {
                    Debug.Log("TurnOrNot(desired)=false");
                    BigTurnAway();
                     Debug.Log("[EdgeGuard] Turned away from edge.");
                }
            }

            // 位移
            if (forwardMove.sqrMagnitude > 0f)
                rb.MovePosition(rb.position + forwardMove);

            // 動畫參數
            anim.SetBool("Attack", inAttack);
            anim.SetFloat("Speed", inAttack ? 0f : 1f);
            anim.SetFloat("Direction", 0f);

            // 新增：攻擊時播放攻擊特效
            if (inAttack)
                TrySpawnAttackEffect();

            // 新增：檢查子彈距離
            DetectBulletNear();
        }

        // --- 播放被攻擊音效 ---
        private void PlayHitSound()
        {
            Debug.Log("iswhijwgowjoghwjnfoiwpint2");
            if (hitAudioClip != null && audioSourceHit != null)
            {
                audioSourceHit.volume = hitAudioVolume;
                audioSourceHit.PlayOneShot(hitAudioClip);

               
                if (debugLogs)
                    Debug.Log("[HitSound] Played hit sound: 51_Flee_02");
            }
        }


        // --- 新增：如果附近有子彈距離 < 0.01，就噴血並播放受擊音效 ---
        private void DetectBulletNear()
        {
            // 找出所有名子包含 "bullet" 的物件
            var bullets = GameObject.FindObjectsOfType<GameObject>()
                .Where(obj => obj.name.ToLower().Contains("bullet"))
                .ToArray();

            foreach (var b in bullets)
            {
                float dist = Vector3.Distance(transform.position, b.transform.position);
                if (dist < 0.01f)
                {
                    if (debugLogs)
                        Debug.Log($"[DetectBulletNear] Bullet too close! dist={dist:F5}, obj={b.name}");

                    Vector3 hitPos = b.transform.position;
                    SpawnBloodAt(hitPos, Quaternion.identity);

                    //播放受擊音效
                    PlayHitSound();

                    if (destroyBulletAfterHit)
                        Destroy(b);

                    break; 
                }
            }
        }

        // --- 隨機轉向 ---
        private void DoRandomTurn()
        {
            
            rotateTimer -= Time.fixedDeltaTime;
            if (rotateTimer <= 0f)
            {
               Debug.Log("turn by timer.");
               Debug.Log("turn by timer.");
                float deltaYaw = UnityEngine.Random.Range(-120f, 120f);
                targetFacing = Quaternion.Euler(0f, transform.eulerAngles.y + deltaYaw, 0f);
                rotateTimer = UnityEngine.Random.Range(rotateIntervalRange.x, rotateIntervalRange.y);
            }

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetFacing,
                rotateSpeedDegPerSec * Time.fixedDeltaTime
            );
        }

      
        // --- 碰到邊緣時固定轉 180 度 ---
        private void BigTurnAway()
        {
            Debug.Log("[EdgeGuard] Turned away from edge.");
            float deltaYaw = 90f; 
            if (UnityEngine.Random.value < 0.5f)
                deltaYaw = -deltaYaw;

            targetFacing = Quaternion.Euler(0f, transform.eulerAngles.y + deltaYaw, 0f);
            rotateTimer = UnityEngine.Random.Range(rotateIntervalRange.x, rotateIntervalRange.y);
        }


        // --- 面向指定方向 ---
        private void FaceTowards(Vector3 worldPos, float degPerSec)
        {
            Vector3 to = worldPos - transform.position;
            to.y = 0f;
            if (to.sqrMagnitude < 1e-6f) return;

            Quaternion desired = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desired, degPerSec * Time.fixedDeltaTime);
        }

        // --- 黏在地面 ---
        private void StickToGround()
        {
            Vector3 rayStart = transform.position + Vector3.up * yRayHeight;
            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, yRayHeight * 2f))
            {
                if (hit.collider != null && hit.collider.CompareTag(groundTag))
                {
                    var pos = rb.position;
                    pos.y = hit.point.y + groundSnapSkin;
                    rb.position = pos;
                }
            }
        }

        // --- 檢查下一步是否仍在地面 ---
        private bool WillStepStayOnGround(Vector3 delta)
        {
            Vector3 nextPos = rb.position + delta;
            Vector3 probeStart = nextPos + Vector3.up * yRayHeight;
            float rayLen = Mathf.Max(yRayHeight * 2f, 10f);
            Debug.DrawLine(probeStart, probeStart + Vector3.down * rayLen, Color.yellow, 0.1f);

            if (Physics.Raycast(probeStart, Vector3.down, out RaycastHit hit, rayLen, groundMask, QueryTriggerInteraction.Ignore))
            {
                if (!hit.collider.CompareTag(groundTag)) return false;

                float drop = rb.position.y - hit.point.y;
                float maxDrop = yRayHeight * 0.9f;
                if (drop > maxDrop) return false;

                return true;
            }
            return false;
        }

        // 前方暢通 -> true；有障礙 -> false
        private bool CanMoveForward(Vector3 delta)
        {
            Vector3 start = rb.position + Vector3.up * 1.0f;
            float checkDist = Mathf.Max(delta.magnitude + 0.2f, 0.5f);

            // 建議用 SphereCast，側擦牆也抓得到
            float bodyRadius = 0.4f; // 依角色寬度調整
            Debug.DrawRay(start, transform.forward * checkDist, Color.green, 0.1f);

            if (Physics.SphereCast(start, bodyRadius, transform.forward, out RaycastHit hit, checkDist))
            {
                var go = hit.collider.gameObject;

                // 排除不算障礙的對象
                if (go == gameObject) return true;
                if (go.CompareTag("Player")) return true;
                if (go.CompareTag("Ground")) return true;  // 地面不算障礙

                // 其餘都視為障礙 -> 不能前進
                Debug.DrawLine(start, hit.point, Color.yellow, 0.2f);
                Debug.Log($"[CanMoveForward] blocked by {go.name} (Tag:{go.tag}) at {hit.distance:F2}m");
                return false;
            }

            // 沒打到任何東西 -> 可前進
            return true;
        }



        private float PlanarDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f; b.y = 0f;
            return Vector3.Distance(a, b);
        }

        private bool IsBulletByName(string nameStr)
        {
            string s = nameStr.ToLowerInvariant();
            if (s == "bullet" || s == "bullet(clone)") return true;
            if (s.StartsWith("bullet") && s.EndsWith("(clone)")) return true;
            return false;
        }

        private void SpawnBloodAt(Vector3 pos, Quaternion rot)
        {
            if (enemyCBloodPrefab == null) return;
            // 在原本位置基礎上 Y 軸上移 2
            Vector3 spawnPos = pos + bloodOffset;
            spawnPos.y += 1.0f;

            Instantiate(enemyCBloodPrefab, spawnPos, rot);
            PlayHitSound();

        }

        private void OnCollisionEnter(Collision collision)
        {
            GameObject other = collision.gameObject;
            Debug.Log($"[OnCollisionEnter] Hit object name = {other.name}");

            if (other.CompareTag(playerTag))
            {
                var php = other.GetComponent<PlayerHP>();
                if (php != null)
                {
                    php.TakeDamage(10);
                    if (debugLogs) Debug.Log($"[TouchDamage] -10");
                }
                return;
            }

            if (!IsBulletByName(other.name))
                return;

            Debug.Log("onCollisionEnter: bullet detected!");

            Vector3 hitPoint = (collision.contactCount > 0)
                ? collision.GetContact(0).point
                : transform.position;

            Vector3 away = (collision.relativeVelocity.sqrMagnitude > 0f)
                ? -collision.relativeVelocity.normalized
                : Vector3.up;

            Quaternion rot = Quaternion.LookRotation(away, Vector3.up);
            SpawnBloodAt(hitPoint, rot);

            if (destroyBulletAfterHit)
                Destroy(other);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsBulletByName(other.gameObject.name)) return;
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            SpawnBloodAt(hitPoint, Quaternion.identity);
            if (destroyBulletAfterHit)
                Destroy(other.gameObject);
        }
    }
}
