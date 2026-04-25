using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("Bow (弓箭手弓)")]
    [Tooltip("弓的拉弓/复位总时长。")]
    public float bowRecoilDuration = 0.1f;

    [Tooltip("弓拉弓时向角色后方平移的距离（本地空间 X 负方向），越大后坐力越明显。")]
    public float bowRecoilDistance = 0.15f;

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

    [Tooltip("每次近战攻击造成的伤害值，后续扣血逻辑直接使用此数值。")]
    public int attackDamage = 10;

    private bool isAttacking = false;

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

    private IEnumerator SwordAttack()
    {
        isAttacking = true;

        if (weaponPivot == null)
        {
            yield return new WaitForSeconds(swordSwingDuration + attackCooldown);
            isAttacking = false;
            yield break;
        }

        Quaternion startRotation = weaponPivot.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, swordSwingAngle);
        float halfDuration = Mathf.Max(0.0001f, swordSwingDuration * 0.5f);

        // 劈下瞬间触发扇形近战伤害判定。
        PerformDamage();

        yield return LerpRotation(startRotation, targetRotation, halfDuration);
        yield return LerpRotation(targetRotation, startRotation, halfDuration);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

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

        // TODO: 生成并在此处发射火球预制体

        yield return LerpRotation(startRotation, targetRotation, halfDuration);
        yield return LerpRotation(targetRotation, startRotation, halfDuration);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

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

        // TODO: 生成并在此处发射弓箭预制体

        yield return LerpPosition(startPosition, recoilPosition, halfDuration);
        yield return LerpPosition(recoilPosition, startPosition, halfDuration);

        yield return new WaitForSeconds(attackCooldown);
        isAttacking = false;
    }

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
                Debug.Log($"扇形击中敌人: {enemy.name}", enemy);
                // TODO: 之后在这里调用敌人扣血接口，例如 enemy.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
            }
        }
    }

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
