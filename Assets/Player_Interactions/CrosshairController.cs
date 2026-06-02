using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    public GameObject Crosshair;

    void Update()
    {
        if (PlayerManager.Instance == null || Crosshair == null) return;

        GameObject currentObject = PlayerManager.Instance.CurrentObject;

        bool hasWeapon = false;

        if (currentObject != null)
        {
            InterfaceBase_IItem item = currentObject.GetComponent<InterfaceBase_IItem>();

            if (item != null && item.itemData != null)
            {
                hasWeapon = item.itemData.Type == ItemType.Weapon;
            }
        }

        Crosshair.SetActive(hasWeapon);
    }
}