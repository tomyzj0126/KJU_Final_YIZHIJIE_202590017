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

    [Header("Audio Options")]
    public AudioClip FireSound;
    public AudioClip ReloadSound;
    [Range(0f, 1f)] public float FireVolume = 0.85f;
    [Range(0f, 1f)] public float ReloadVolume = 0.8f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (FireSound == null)
        {
            FireSound = Resources.Load<AudioClip>("Audio/SFX/Weapon_Fire");
        }

        if (ReloadSound == null)
        {
            ReloadSound = Resources.Load<AudioClip>("Audio/SFX/Weapon_Reload");
        }
    }

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
        PlaySound(FireSound, FireVolume);

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
        PlaySound(ReloadSound, ReloadVolume);

        Debug.Log($"[Rifle] Reloaded: {CurrentAmmo}/{ReserveAmmo}");
    }

    void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
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
