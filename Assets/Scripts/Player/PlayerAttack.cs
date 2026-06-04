using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>玩家攻击：按当前武器执行剑/杖/弓攻击与伤害判定。</summary>
public class PlayerAttack : MonoBehaviour
{
    // 武器索引常量，与 GlobalData.chosenWeaponIndex 以及各祭坛的 weaponIndex 对齐。
    private const int WEAPON_SWORD = 0;
    private const int WEAPON_STAFF = 1;
    private const int WEAPON_BOW = 2;

    [Header("References")]
    [Tooltip("武器轴心 Transform，代码会旋转/平移它来驱动各职业的攻击动作。")]
    public Transform weaponPivot;

    [Tooltip("伤害判定的中心点（通常放在剑刃前方的空物体）。")]
    public Transform attackPoint;

    [Header("Audio")]
    [Tooltip("用于播放攻击音效的 AudioSource（通常挂在主角身上）。")]
    public AudioSource audioSource;

    [Tooltip("大剑挥击音效。")]
    public AudioClip swordSfx;

    [Tooltip("法杖开火音效。")]
    public AudioClip staffSfx;

    [Tooltip("弓箭射击音效。")]
    public AudioClip bowSfx;

    [Header("Sword (骑士大剑)")]
    [Tooltip("大剑挥砍总时长（前半段劈下 + 后半段收剑）。")]
    public float swordSwingDuration = 0.2f;

    [Tooltip("大剑挥砍到达的目标角度（度，负值向下劈砍）。")]
    public float swordSwingAngle = -120f;

    [Header("Staff (法师法杖)")]
    [Tooltip("法杖小幅挥动的总时长。")]
    public float staffSwingDuration = 0.1f;

    [Tooltip("法杖小幅挥动的目标角度（度），通常 20~30 度即可。")]
    public float staffSwingAngle = 25f;

    [Tooltip("法师发射的火球预制体（需挂载 Projectile 脚本 + 勾选 Is Trigger 的 2D 碰撞器）。")]
    public GameObject fireballPrefab;

    [Tooltip("法杖的火球发射点 Transform（通常放在法杖尖端）。")]
    public Transform staffFirePoint;

    [Header("Bow (弓箭手弓)")]
    [Tooltip("弓的拉弓/复位总时长。")]
    public float bowRecoilDuration = 0.1f;

    [Tooltip("弓拉弓时向角色后方平移的距离（本地空间 X 负方向），越大后坐力越明显。")]
    public float bowRecoilDistance = 0.15f;

    [Tooltip("弓箭手发射的箭矢预制体（需挂 Projectile 脚本 + 勾选 Is Trigger 的 2D 碰撞器）。")]
    public GameObject arrowPrefab;

    [Tooltip("弓的箭矢发射点 Transform（通常放在弓的箭槽处）。")]
    public Transform bowFirePoint;

    [Header("Cooldown")]
    [Tooltip("两次攻击之间的冷却时间（秒），所有武器共用。")]
    public float attackCooldown = 0.2f;

    [Header("Damage")]
    [Tooltip("伤害判定扇形的半径（米）。")]
    public float attackRange = 0.8f;

    [Tooltip("伤害判定扇形的总张角（度），例如 90 表示身前一个直角扇形。")]
    public float attackAngle = 90f;

    [Tooltip("只检测这些图层上的碰撞体（例如 Enemy），避免误伤自己或环境。")]
    public LayerMask enemyLayers;

    [Tooltip("大剑每次近战造成的伤害值。")]
    public int attackDamage = 15;
    [Tooltip("对 Final Boss 的近战单次伤害（按心值设计，默认 1 点）。")]
    public float bossMeleeDamagePerHit = 1f;
    [Tooltip("法师火球每发造成的伤害值。")]
    public float fireballDamage = 20f;
    [Tooltip("弓箭每发造成的伤害值。")]
    public float arrowDamage = 10f;

    private bool isAttacking = false;

    // 自动补齐攻击音源：优先 Inspector 引用，其次当前物体组件，最后运行时自动添加。
    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;
    }

    // 左键按下时按 GlobalData.chosenWeaponIndex 分发攻击协程。
    private void Update()
    {
        if (isAttacking)
        {
            return;
        }

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return;
        }

        // 根据跨场景记录的职业索引，分发到对应武器的攻击协程。
        switch (GlobalData.chosenWeaponIndex)
        {
            case WEAPON_SWORD:
                StartCoroutine(SwordAttack());
                break;
            case WEAPON_STAFF:
                StartCoroutine(StaffAttack());
                break;
            case WEAPON_BOW:
                StartCoroutine(BowAttack());
                break;
            default:
                // 未选择职业（-1）或未知索引时，默认用骑士大剑作为兜底表现。
                StartCoroutine(SwordAttack());
                break;
        }
    }

    // 大剑挥砍：旋转武器 + 扇形近战伤害。
    private IEnumerator SwordAttack()
    {
        isAttacking = true;

        if (weaponPivot == null)
        {
            yield return new WaitForSeconds(swordSwingDuration + attackCooldown);
            isAttacking = false;
            yield break;
        }

        // 使用固定常量作为初始旋转，避免每次读取 localRotation 导致累积浮点误差，
        // 造成多次挥剑后武器逐渐"下垂"的漂移 Bug。
        Quaternion defaultRotation = Quaternion.Euler(0f, 0f, 0f);
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, swordSwingAngle);
        float halfDuration = Mathf.Max(0.0001f, swordSwingDuration * 0.5f);

        // 劈下瞬间触发扇形近战伤害判定。
        PerformDamage();
        PlayAttackSfx(swordSfx);

        yield return LerpRotation(defaultRotation, targetRotation, halfDuration);
        yield return LerpRotation(targetRotation, defaultRotation, halfDuration);

        // 强制归位：无论 Lerp 结束时是否有帧误差，这一行都会把武器精确拉回默认角度。
        weaponPivot.localRotation = defaultRotation;

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // 法杖小幅挥动并发射火球。
    private IEnumerator StaffAttack()
    {
        isAttacking = true;

        if (weaponPivot == null)
        {
            yield return new WaitForSeconds(staffSwingDuration + attackCooldown);
            isAttacking = false;
            yield break;
        }

        Quaternion startRotation = weaponPivot.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, staffSwingAngle);
        float halfDuration = Mathf.Max(0.0001f, staffSwingDuration * 0.5f);

        // 挥动瞬间留出一个极短的"蓄力"间隔，再发射火球，手感更有节奏。
        yield return new WaitForSeconds(0.05f);
        SpawnFireball();
        PlayAttackSfx(staffSfx);

        yield return LerpRotation(startRotation, targetRotation, halfDuration);
        yield return LerpRotation(targetRotation, startRotation, halfDuration);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // 弓拉弦后坐并发射箭矢。
    private IEnumerator BowAttack()
    {
        isAttacking = true;

        if (weaponPivot == null)
        {
            yield return new WaitForSeconds(bowRecoilDuration + attackCooldown);
            isAttacking = false;
            yield break;
        }

        Vector3 startPosition = weaponPivot.localPosition;
        // 本地空间沿 -X 平移模拟向后拉弓，Player 翻转时会自动跟随。
        Vector3 recoilPosition = startPosition + new Vector3(-bowRecoilDistance, 0f, 0f);
        float halfDuration = Mathf.Max(0.0001f, bowRecoilDuration * 0.5f);

        // 拉弓瞬间发射箭矢，复用与火球相同的精确瞄准逻辑。
        SpawnProjectileTowardMouse(arrowPrefab, bowFirePoint, "Arrow", arrowDamage);
        PlayAttackSfx(bowSfx);

        yield return LerpPosition(startPosition, recoilPosition, halfDuration);
        yield return LerpPosition(recoilPosition, startPosition, halfDuration);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

    // 在法杖发射点生成朝向鼠标的火球。
    private void SpawnFireball()
    {
        SpawnProjectileTowardMouse(fireballPrefab, staffFirePoint, "Fireball", fireballDamage);
    }

    // 播放攻击音效（随机音高）。
    private void PlayAttackSfx(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        // 让音高在一个小范围内随机波动，减少重复播放时的机械感。
        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.PlayOneShot(clip);
    }

    // 实例化投射物并朝向鼠标世界坐标旋转。
    private void SpawnProjectileTowardMouse(GameObject prefab, Transform spawnPoint, string debugName, float projectileDamage)
    {
        // 安全检查：预制体或发射点缺失时打印警告，避免运行时静默失效或空引用异常。
        if (prefab == null)
        {
            Debug.LogWarning($"[PlayerAttack] {debugName} 预制体未配置，无法发射。", this);
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning($"[PlayerAttack] {debugName} 的发射点 Transform 未配置，无法发射。", this);
            return;
        }

        // 精确瞄准鼠标位置：基于鼠标世界坐标与发射点的向量，做 360° 角度换算。
        Camera mainCamera = Camera.main;
        if (mainCamera == null || Mouse.current == null)
        {
            return;
        }

        // 修复 ScreenToWorldPoint 丢失 Z 深度的经典 Bug：
        // 必须显式传入相机与发射点平面之间的距离，否则结果会坍缩到相机位置。
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        float distanceToCamera = Mathf.Abs(mainCamera.transform.position.z - spawnPoint.position.z);
        Vector3 screenPosWithZ = new Vector3(mouseScreenPos.x, mouseScreenPos.y, distanceToCamera);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPosWithZ);

        Vector2 aimDir = (Vector2)(mouseWorldPos - spawnPoint.position);
        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;

        GameObject projectileObj = Instantiate(prefab, spawnPoint.position, Quaternion.Euler(0f, 0f, angle));
        Projectile projectile = projectileObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.damage = projectileDamage;
        }
    }

    // 武器 pivot 旋转插值。
    private IEnumerator LerpRotation(Quaternion from, Quaternion to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            weaponPivot.localRotation = Quaternion.Slerp(from, to, t);
            yield return null;
        }
        weaponPivot.localRotation = to;
    }

    // 武器 pivot 位移插值（弓后坐）。
    private IEnumerator LerpPosition(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            weaponPivot.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }
        weaponPivot.localPosition = to;
    }

    // 扇形范围内对敌人造成伤害（优先 EnemyHealth）。
    private void PerformDamage()
    {
        if (attackPoint == null)
        {
            return;
        }

        // 先用圆形范围粗筛，再按扇形张角精筛。
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers);

        // 角色朝向：由 PlayerFacing 通过 localScale.x 的正负号控制。
        Vector2 facingDir = transform.localScale.x > 0f ? Vector2.right : Vector2.left;
        float halfAngle = attackAngle * 0.5f;

        for (int i = 0; i < hitEnemies.Length; i++)
        {
            Collider2D enemy = hitEnemies[i];
            if (enemy == null)
            {
                continue;
            }

            Vector2 dirToEnemy = ((Vector2)enemy.transform.position - (Vector2)attackPoint.position).normalized;
            float angle = Vector2.Angle(facingDir, dirToEnemy);

            if (angle <= halfAngle)
            {
                FinalBossController bossController = enemy.GetComponent<FinalBossController>();
                if (bossController == null)
                {
                    bossController = enemy.GetComponentInParent<FinalBossController>();
                }

                if (bossController != null)
                {
                    bossController.TakeDamage(Mathf.Max(0f, bossMeleeDamagePerHit));
                    continue;
                }

                EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
                if (enemyHealth == null)
                {
                    enemyHealth = enemy.GetComponentInParent<EnemyHealth>();
                }

                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(1);
                }
                else
                {
                    EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
                    if (enemyAI == null)
                    {
                        enemyAI = enemy.GetComponentInParent<EnemyAI>();
                    }

                    if (enemyAI != null)
                    {
                        enemyAI.TakeDamage(attackDamage);
                    }
                    else
                    {
                        Debug.LogWarning($"[PlayerAttack] 扇形命中 {enemy.name}，但未找到 EnemyHealth/EnemyAI。", enemy);
                    }
                }
            }
        }
    }

    // 编辑器中绘制近战扇形范围 Gizmo。
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        {
            return;
        }

        Gizmos.color = Color.red;

        // 外围参考圆：直观表示扇形的半径。
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        // 根据当前朝向（localScale.x 正负号）绘制扇形的上下两条边界线，构成披萨饼的切口。
        Vector3 facingDir = transform.localScale.x > 0f ? Vector3.right : Vector3.left;
        Vector3 upperLine = Quaternion.Euler(0f, 0f, attackAngle / 2f) * facingDir * attackRange;
        Vector3 lowerLine = Quaternion.Euler(0f, 0f, -attackAngle / 2f) * facingDir * attackRange;

        Gizmos.DrawLine(attackPoint.position, attackPoint.position + upperLine);
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + lowerLine);
    }
}
