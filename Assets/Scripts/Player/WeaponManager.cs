using UnityEngine;

/// <summary>
/// Keeps the visible weapon object in sync with the selected class. It hides all weapon prefabs
/// first, then activates only the sword, staff, or bow object requested by the class system.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    [Header("Weapons")]
    [Tooltip("Weapon objects indexed by the class pedestal weapon index.")]
    public GameObject[] weapons;

    // Enables only the weapon visual that matches the selected class.
    private void Start()
    {
        HideAllWeapons();

        if (GlobalData.chosenWeaponIndex >= 0)
        {
            SwitchWeapon(GlobalData.chosenWeaponIndex);
        }
    }

    /// <summary>
    /// Activates the requested weapon and hides every other weapon.
    /// </summary>
    public void SwitchWeapon(int index)
    {
        HideAllWeapons();

        if (weapons == null || index < 0 || index >= weapons.Length)
        {
            Debug.LogWarning($"[WeaponManager] Invalid weapon index: {index}.", this);
            return;
        }

        if (weapons[index] != null)
        {
            weapons[index].SetActive(true);
        }
    }

    // Turns off every weapon object before enabling the selected one.
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
