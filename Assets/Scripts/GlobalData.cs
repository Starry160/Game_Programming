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
}
