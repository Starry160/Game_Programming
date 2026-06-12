using UnityEngine;

/// <summary>Applies the previously selected class animator when a level loads.</summary>
[RequireComponent(typeof(Animator))]
public class PlayerClassLoader : MonoBehaviour
{
    [SerializeField] private bool disableAttackAfterLoad = false;
    [SerializeField] private bool loadWeaponVisualAfterLoad = false;
    [SerializeField] private Sprite swordSprite;
    [SerializeField] private Sprite staffSprite;
    [SerializeField] private Sprite bowSprite;
    [SerializeField] private int weaponSortingOrder = 11;

    // Activates the character prefab that matches the class selected in the menu.
    private void Start()
    {
        ApplySelectedClass();

        if (loadWeaponVisualAfterLoad)
        {
            ApplySelectedWeaponVisual();
        }

        if (disableAttackAfterLoad)
        {
            DisableAttack();
        }
    }

    private void ApplySelectedClass()
    {
        if (GlobalData.chosenAnimatorController == null)
        {
            return;
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.runtimeAnimatorController = GlobalData.chosenAnimatorController;
        }
    }

    private void DisableAttack()
    {
        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null)
        {
            attack.enabled = false;
        }
    }

    private void ApplySelectedWeaponVisual()
    {
        WeaponVisualConfig config;
        if (!TryGetWeaponVisualConfig(GlobalData.chosenWeaponIndex, out config))
        {
            return;
        }

        Transform weaponPivot = new GameObject("WeaponPivot").transform;
        weaponPivot.SetParent(transform, false);
        weaponPivot.localPosition = new Vector3(0.14f, 0.33f, 0f);

        GameObject weaponObject = new GameObject(config.Name);
        weaponObject.transform.SetParent(weaponPivot, false);
        weaponObject.transform.localPosition = config.LocalPosition;
        weaponObject.transform.localScale = config.LocalScale;

        SpriteRenderer renderer = weaponObject.AddComponent<SpriteRenderer>();
        renderer.sprite = config.Sprite;
        renderer.sortingOrder = weaponSortingOrder;
    }

    private bool TryGetWeaponVisualConfig(int weaponIndex, out WeaponVisualConfig config)
    {
        switch (weaponIndex)
        {
            case 0:
                config = new WeaponVisualConfig("Sword", swordSprite, new Vector3(0.28f, 0.5f, 0f), new Vector3(0.7f, 0.9f, 1f));
                break;
            case 1:
                config = new WeaponVisualConfig("Staff", staffSprite, new Vector3(0.2f, 0.32f, 0f), new Vector3(0.7f, 0.8f, 1f));
                break;
            case 2:
                config = new WeaponVisualConfig("Bow", bowSprite, new Vector3(0.31f, 0.27f, 0f), new Vector3(0.7f, 0.7f, 1f));
                break;
            default:
                config = default(WeaponVisualConfig);
                return false;
        }

        return config.Sprite != null;
    }

    private struct WeaponVisualConfig
    {
        public readonly string Name;
        public readonly Sprite Sprite;
        public readonly Vector3 LocalPosition;
        public readonly Vector3 LocalScale;

        public WeaponVisualConfig(string name, Sprite sprite, Vector3 localPosition, Vector3 localScale)
        {
            Name = name;
            Sprite = sprite;
            LocalPosition = localPosition;
            LocalScale = localScale;
        }
    }
}
