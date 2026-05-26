using UnityEngine;

/// <summary>管理多把武器子物体，按索引显示/隐藏。</summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    [Tooltip("玩家可用的武器物体数组，索引与祭坛的 weaponIndex 对应。")]
    public GameObject[] weapons;

    // 隐藏全部武器，若有全局记录则装备对应索引。
    private void Start()
    {
        // 默认进入关卡时先全部隐藏，避免多把武器同时显示。
        HideAllWeapons();

        // 若全局库中已有玩家选择的武器记录，则自动装备对应武器。
        if (GlobalData.chosenWeaponIndex >= 0)
        {
            SwitchWeapon(GlobalData.chosenWeaponIndex);
        }
    }

    /// <summary>
    /// 切换到指定索引的武器，同时隐藏其他武器。
    /// </summary>
    public void SwitchWeapon(int index)
    {
        HideAllWeapons();

        if (weapons == null || index < 0 || index >= weapons.Length)
        {
            Debug.LogWarning($"[WeaponManager] 无效武器索引：{index}。", this);
            return;
        }

        if (weapons[index] != null)
        {
            weapons[index].SetActive(true);
        }
    }

    // 关闭 weapons 数组中所有子物体。
    private void HideAllWeapons()
    {
        if (weapons == null)
        {
            return;
        }

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null)
            {
                weapons[i].SetActive(false);
            }
        }
    }
}
