using UnityEngine;

public class Actor_Rifle : InterfaceBase_IItem
{
    [Header("Shoot Options")]
    public Transform FirePoint;
    public GameObject Bullet;
    public float BulletSpeed = 100f;
    public float BulletDamage = 5f;

    [Header("Ammo Options")]
    public int MagazineSize = 30;
    public int CurrentAmmo = 30;
    public int ReserveAmmo = 90;

    private bool isFiring = false;
    private float lastFireTime;

    public override void OnEquip(GameObject itemHolder)
    {
        base.OnEquip(itemHolder);
        UpdateAmmoUI();
    }

    public override void OnUnEquip(GameObject sender)
    {
        isFiring = false;
        base.OnUnEquip(sender);

        if (AmmoUI.Instance != null)
        {
            AmmoUI.Instance.HideAmmo();
        }
    }

    public override void OnUse()
    {
        isFiring = true;
    }

    public override void OnStopUse()
    {
        isFiring = false;
    }

    public override void OnReload()
    {
        Reload();
    }

    private void Update()
    {
        if (!isFiring) return;

        if (Time.time >= lastFireTime + itemData.FireRate)
        {
            Fire();
            lastFireTime = Time.time;
        }
    }

    void Fire()
    {
        if (CurrentAmmo <= 0)
        {
            isFiring = false;
            Debug.Log("[Rifle] No ammo. Reload!");
            UpdateAmmoUI();
            return;
        }

        CurrentAmmo--;
        UpdateAmmoUI();

        Vector3 pos = FirePoint.position;
        Quaternion dir = FirePoint.rotation;

        GameObject bulletClone = Instantiate(Bullet, pos, dir);
        bulletClone.GetComponent<Actor_Bullet>().SetDamage(BulletDamage);

        Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(FirePoint.forward * BulletSpeed, ForceMode.VelocityChange);
        }

        Destroy(bulletClone, 2f);
    }

    void Reload()
    {
        if (CurrentAmmo >= MagazineSize) return;
        if (ReserveAmmo <= 0) return;

        int needAmmo = MagazineSize - CurrentAmmo;
        int reloadAmmo = Mathf.Min(needAmmo, ReserveAmmo);

        CurrentAmmo += reloadAmmo;
        ReserveAmmo -= reloadAmmo;

        UpdateAmmoUI();

        Debug.Log($"[Rifle] Reloaded: {CurrentAmmo}/{ReserveAmmo}");
    }

    void UpdateAmmoUI()
    {
        if (AmmoUI.Instance != null)
        {
            AmmoUI.Instance.ShowAmmo(CurrentAmmo, ReserveAmmo);
        }
    }

    public void AddReserveAmmo(int amount)
    {
        ReserveAmmo += amount;
        UpdateAmmoUI();

        Debug.Log($"[Rifle] Ammo picked up: +{amount}. ReserveAmmo={ReserveAmmo}");
    }
}