using UnityEngine;

public class Actor_MedKit : InterfaceBase_IItem
{
    [Header("Heal Settings")]
    public float HealAmount = 100f;

    public override void OnUse()
    {
        PlayerHealth playerHealth = PlayerManager.Instance.GetComponent<PlayerHealth>();

        if (playerHealth == null)
        {
            Debug.LogWarning("[MedKit] PlayerHealth not found.");
            return;
        }

        playerHealth.Heal(HealAmount);

        ItemBehavior itemBehavior = GetComponentInParent<ItemBehavior>();
        if (itemBehavior != null)
        {
            itemBehavior.ConsumeEquippedItem();
        }
    }
}