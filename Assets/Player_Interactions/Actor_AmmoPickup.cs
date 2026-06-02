
using UnityEngine;
using UnityEngine.InputSystem;

public class Actor_AmmoPickup : MonoBehaviour
{
    public int AmmoAmount = 30;
    public bool DestroyAfterPickup = true;

    [Header("Pickup Keys")]
    public Key SelectKey = Key.F;
    public Key CollectKey = Key.Z;

    [Header("Optional UI")]
    public GameObject SelectText;
    public GameObject CollectText;

    private bool playerInRange = false;
    private bool selected = false;

    private void Start()
    {
        SetPrompt(false, false);
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (Keyboard.current == null) return;

        if (!selected && Keyboard.current[SelectKey].wasPressedThisFrame)
        {
            selected = true;
            SetPrompt(false, true);
            Debug.Log("[AmmoPickup] Ammo box selected.");
        }

        if (selected && Keyboard.current[CollectKey].wasPressedThisFrame)
        {
            PickUpAmmo();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<PlayerManager>() == null) return;

        playerInRange = true;
        selected = false;
        SetPrompt(true, false);

        Debug.Log("[AmmoPickup] Press F to select ammo box.");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<PlayerManager>() == null) return;

        playerInRange = false;
        selected = false;
        SetPrompt(false, false);

        Debug.Log("[AmmoPickup] Player left ammo box.");
    }

    public void PickUpAmmo()
    {
        ItemBehavior itemBehavior = PlayerManager.Instance.GetComponent<ItemBehavior>();

        if (itemBehavior == null)
        {
            Debug.LogWarning("[AmmoPickup] ItemBehavior not found.");
            return;
        }

        bool addedAmmo = false;

        foreach (InterfaceBase_IItem item in itemBehavior.inventory)
        {
            if (item == null) continue;

            Actor_Pistol pistol = item.GetComponent<Actor_Pistol>();
            if (pistol != null)
            {
                pistol.AddReserveAmmo(AmmoAmount);
                addedAmmo = true;
            }

            Actor_Rifle rifle = item.GetComponent<Actor_Rifle>();
            if (rifle != null)
            {
                rifle.AddReserveAmmo(AmmoAmount);
                addedAmmo = true;
            }
        }

        if (!addedAmmo)
        {
            Debug.Log("[AmmoPickup] No weapon in inventory.");
            return;
        }

        Consume();
    }

    private void SetPrompt(bool showSelect, bool showCollect)
    {
        if (SelectText != null) SelectText.SetActive(showSelect);
        if (CollectText != null) CollectText.SetActive(showCollect);
    }

    private void Consume()
    {
        if (DestroyAfterPickup)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}