using UnityEngine;

/// <summary>
/// 跨场景常驻内存的全局数据容器。
/// 纯静态类，不依赖 MonoBehaviour，不随场景销毁而丢失。
/// 用于在关卡切换间保存玩家已选职业等持久化状态。
/// </summary>
public static class GlobalData
{
    /// <summary>
    /// 玩家最后一次在祭坛选择的职业动画控制器；
    /// 新关卡出生时由 PlayerClassLoader 读取并应用到玩家 Animator 上。
    /// </summary>
    public static RuntimeAnimatorController chosenAnimatorController;

    /// <summary>
    /// 玩家最后一次在祭坛选择的武器索引；
    /// -1 表示尚未选择，新关卡出生时 WeaponManager 读取并自动装备对应武器。
    /// </summary>
    public static int chosenWeaponIndex = -1;

    /// <summary>
    /// 是否已有跨场景继承的生命值。
    /// false 时表示首次进入流程，PlayerStats 应使用 maxHealth 初始化生命值。
    /// </summary>
    public static bool hasPersistedHealth = false;

    /// <summary>
    /// 跨场景继承的当前生命值（同一次运行内有效）。
    /// </summary>
    public static float persistedHealth = 0f;

    /// <summary>
    /// 是否已有跨场景继承的生命上限。
    /// false 时表示使用 PlayerStats Inspector 默认值。
    /// </summary>
    public static bool hasPersistedMaxHealth = false;

    /// <summary>
    /// 跨场景继承的生命上限（同一次运行内有效）。
    /// </summary>
    public static float persistedMaxHealth = 0f;

    /// <summary>
    /// 是否已有跨场景继承的护盾上限。
    /// false 时表示使用 PlayerStats Inspector 默认值。
    /// </summary>
    public static bool hasPersistedMaxShield = false;

    /// <summary>
    /// 跨场景继承的护盾上限（同一次运行内有效）。
    /// </summary>
    public static float persistedMaxShield = 0f;

    /// <summary>
    /// 清空本局运行时的跨场景状态。
    /// 用于“回到主菜单后重新开始”这种应视为新开一局的场景。
    /// </summary>
    public static void ResetRunState()
    {
        chosenAnimatorController = null;
        chosenWeaponIndex = -1;
        hasPersistedHealth = false;
        persistedHealth = 0f;
        hasPersistedMaxHealth = false;
        persistedMaxHealth = 0f;
        hasPersistedMaxShield = false;
        persistedMaxShield = 0f;
    }
}
