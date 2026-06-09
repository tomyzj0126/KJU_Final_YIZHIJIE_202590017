using UnityEngine;

public class Actor_Pistol : InterfaceBase_IItem
{
    [Header("Shoot Options")]
    public Transform FirePoint;
    public GameObject Bullet;
    public float BulletSpeed = 100f;
    public float BulletDamage = 10f;

    [Header("Bullet Visual")]
    public string BulletVisualModelResourcePath = "Models/PistolBullet/source/Bullet_45_ACP_sketchfab";
    public string BulletVisualBaseColorTextureResourcePath = "Models/PistolBullet/textures/Bullets_45_acp__for_texturing_Bullet_45_Ba";
    public string BulletVisualHiddenNameFragments = "Bullet_02;Main_bullet2;Bullet_case2;Back_part2";
    public Vector3 BulletVisualLocalPosition = Vector3.zero;
    public Vector3 BulletVisualLocalEulerAngles = new Vector3(0f, -90f, 0f);
    public Vector3 BulletVisualForwardFlipEulerAngles = Vector3.zero;
    public bool AutoAlignBulletVisual = false;
    public bool AutoFitBulletVisual = true;
    public float BulletVisualTargetLength = 0.18f;
    public bool HideDefaultBulletVisuals = true;

    [Header("Ammo Options")]
    public int MagazineSize = 7;
    public int CurrentAmmo = 7;
    public int ReserveAmmo = 21;

    [Header("Audio Options")]
    public AudioClip FireSound;
    public AudioClip ReloadSound;
    [Range(0f, 1f)] public float FireVolume = 0.9f;
    [Range(0f, 1f)] public float ReloadVolume = 0.8f;

    [Header("Visual Model")]
    public string WeaponVisualResourcePath = "Models/Revolver/source/Revolver";
    public string WeaponVisualBaseColorTextureResourcePath = "Models/Revolver/textures/rev_d.tga";
    public Vector3 WeaponVisualLocalPosition = Vector3.zero;
    public Vector3 WeaponVisualLocalEulerAngles = Vector3.zero;
    public Vector3 WeaponVisualForwardFlipEulerAngles = new Vector3(0f, 180f, 90f);
    public bool AutoAlignWeaponVisual = true;
    public bool AutoFitWeaponVisual = true;
    public float WeaponVisualTargetLength = 0.55f;
    public bool HideDefaultWeaponVisuals = true;

    private AudioSource audioSource;
    private const float TwoShotEnemyBulletDamage = 10f;
    private const string PistolFireResourcePath = "Audio/SFX/Pistol_Fire";
    private const string RifleFireResourcePath = "Audio/SFX/Weapon_Fire";
    private const string RevolverVisualResourcePath = "Models/Revolver/source/Revolver";
    private const string RevolverBaseColorTextureResourcePath = "Models/Revolver/textures/rev_d.tga";
    private const string PistolBulletVisualResourcePath = "Models/PistolBullet/source/Bullet_45_ACP_sketchfab";
    private const string PistolBulletBaseColorTextureResourcePath = "Models/PistolBullet/textures/Bullets_45_acp__for_texturing_Bullet_45_Ba";
    private const string PistolBulletHiddenNameFragments = "Bullet_02;Main_bullet2;Bullet_case2;Back_part2";
    private const string SniperBulletVisualResourcePath = "Models/SniperBullet/source/Bala";
    private static readonly Vector3 RevolverForwardCorrection = new Vector3(0f, 180f, 90f);
    private static readonly Vector3 PistolBulletLocalEulerCorrection = new Vector3(0f, -90f, 0f);
    private static readonly Vector3 PistolBulletForwardCorrection = Vector3.zero;
    private const float RevolverTargetLength = 0.45f;
    private const float PistolBulletTargetLength = 0.18f;
    private static readonly Vector3[] WeaponVisualAlignmentRotations =
    {
        Vector3.zero,
        new Vector3(90f, 0f, 0f),
        new Vector3(-90f, 0f, 0f),
        new Vector3(0f, 90f, 0f),
        new Vector3(0f, -90f, 0f),
        new Vector3(0f, 0f, 90f),
        new Vector3(0f, 0f, -90f),
        new Vector3(180f, 0f, 0f),
        new Vector3(0f, 180f, 0f),
        new Vector3(0f, 0f, 180f)
    };

    private void Awake()
    {
        ApplyRuntimeWeaponVisualCalibration();
        ApplyRuntimeBulletVisualCalibration();
        ApplyRuntimeDamageCalibration();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;

        if (FireSound == null || FireSound.name == "Weapon_Fire")
        {
            FireSound = Resources.Load<AudioClip>(PistolFireResourcePath);
            if (FireSound == null)
            {
                FireSound = Resources.Load<AudioClip>(RifleFireResourcePath);
            }
        }

        if (ReloadSound == null)
        {
            ReloadSound = Resources.Load<AudioClip>("Audio/SFX/Pistol_Reload");
            if (ReloadSound == null)
            {
                ReloadSound = Resources.Load<AudioClip>("Audio/SFX/Weapon_Reload");
            }
        }

        SetupWeaponVisualModel();
    }

    private void ApplyRuntimeDamageCalibration()
    {
        if (BulletDamage < TwoShotEnemyBulletDamage)
        {
            BulletDamage = TwoShotEnemyBulletDamage;
        }
    }

    private void ApplyRuntimeWeaponVisualCalibration()
    {
        if (string.IsNullOrWhiteSpace(WeaponVisualResourcePath) ||
            WeaponVisualResourcePath == RevolverVisualResourcePath)
        {
            WeaponVisualResourcePath = RevolverVisualResourcePath;
            WeaponVisualBaseColorTextureResourcePath = RevolverBaseColorTextureResourcePath;
            WeaponVisualLocalPosition = Vector3.zero;
            WeaponVisualLocalEulerAngles = Vector3.zero;
            WeaponVisualForwardFlipEulerAngles = RevolverForwardCorrection;
            AutoAlignWeaponVisual = true;
            AutoFitWeaponVisual = true;
            WeaponVisualTargetLength = RevolverTargetLength;
            HideDefaultWeaponVisuals = true;
        }
    }

    private void ApplyRuntimeBulletVisualCalibration()
    {
        if (string.IsNullOrWhiteSpace(BulletVisualModelResourcePath) ||
            BulletVisualModelResourcePath == PistolBulletVisualResourcePath ||
            BulletVisualModelResourcePath == SniperBulletVisualResourcePath)
        {
            BulletVisualModelResourcePath = PistolBulletVisualResourcePath;
            BulletVisualBaseColorTextureResourcePath = PistolBulletBaseColorTextureResourcePath;
            BulletVisualHiddenNameFragments = PistolBulletHiddenNameFragments;
            BulletVisualLocalPosition = Vector3.zero;
            BulletVisualLocalEulerAngles = PistolBulletLocalEulerCorrection;
            BulletVisualForwardFlipEulerAngles = PistolBulletForwardCorrection;
            AutoAlignBulletVisual = false;
            AutoFitBulletVisual = true;
            BulletVisualTargetLength = PistolBulletTargetLength;
            HideDefaultBulletVisuals = true;
        }
    }

    private void SetupWeaponVisualModel()
    {
        if (string.IsNullOrWhiteSpace(WeaponVisualResourcePath))
        {
            return;
        }

        GameObject visualPrefab = Resources.Load<GameObject>(WeaponVisualResourcePath);
        if (visualPrefab == null)
        {
            Debug.LogWarning($"Weapon visual model not found in Resources: {WeaponVisualResourcePath}", this);
            return;
        }

        if (HideDefaultWeaponVisuals)
        {
            foreach (Renderer defaultRenderer in GetComponentsInChildren<Renderer>(true))
            {
                defaultRenderer.enabled = false;
            }
        }

        GameObject visualModel = Instantiate(visualPrefab, transform);
        visualModel.name = "Revolver_Model";
        visualModel.transform.localPosition = WeaponVisualLocalPosition;
        visualModel.transform.localRotation = Quaternion.Euler(WeaponVisualLocalEulerAngles);
        visualModel.transform.localScale = Vector3.one;

        foreach (Collider visualCollider in visualModel.GetComponentsInChildren<Collider>())
        {
            visualCollider.enabled = false;
        }

        foreach (Rigidbody visualRigidbody in visualModel.GetComponentsInChildren<Rigidbody>())
        {
            Destroy(visualRigidbody);
        }

        ApplyWeaponVisualTexture(visualModel);

        if (AutoAlignWeaponVisual)
        {
            AlignWeaponVisualForward(visualModel);
        }

        if (WeaponVisualForwardFlipEulerAngles != Vector3.zero)
        {
            visualModel.transform.localRotation *= Quaternion.Euler(WeaponVisualForwardFlipEulerAngles);
        }

        if (AutoFitWeaponVisual)
        {
            FitWeaponVisualModel(visualModel);
        }
    }

    private void ApplyWeaponVisualTexture(GameObject visualModel)
    {
        if (string.IsNullOrWhiteSpace(WeaponVisualBaseColorTextureResourcePath))
        {
            return;
        }

        Texture2D baseColorTexture = Resources.Load<Texture2D>(WeaponVisualBaseColorTextureResourcePath);
        if (baseColorTexture == null)
        {
            return;
        }

        foreach (Renderer visualRenderer in visualModel.GetComponentsInChildren<Renderer>())
        {
            Material[] materials = visualRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null && materials[i].HasProperty("_MainTex"))
                {
                    materials[i].mainTexture = baseColorTexture;
                }
            }
        }
    }

    private void AlignWeaponVisualForward(GameObject visualModel)
    {
        Quaternion baseRotation = Quaternion.Euler(WeaponVisualLocalEulerAngles);
        Quaternion bestRotation = baseRotation;
        float bestScore = float.NegativeInfinity;

        for (int i = 0; i < WeaponVisualAlignmentRotations.Length; i++)
        {
            visualModel.transform.localRotation = baseRotation * Quaternion.Euler(WeaponVisualAlignmentRotations[i]);

            if (!TryGetWeaponVisualBoundsInItemSpace(visualModel, out Bounds bounds))
            {
                continue;
            }

            float sideSize = Mathf.Max(bounds.size.x, bounds.size.y);
            float score = bounds.size.z - sideSize;
            if (score > bestScore)
            {
                bestScore = score;
                bestRotation = visualModel.transform.localRotation;
            }
        }

        visualModel.transform.localRotation = bestRotation;
    }

    private void FitWeaponVisualModel(GameObject visualModel)
    {
        if (!TryGetWeaponVisualBoundsInItemSpace(visualModel, out Bounds bounds) || WeaponVisualTargetLength <= 0f)
        {
            return;
        }

        float longestSide = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
        if (longestSide > 0.001f)
        {
            float scaleMultiplier = WeaponVisualTargetLength / longestSide;
            visualModel.transform.localScale *= scaleMultiplier;
        }

        if (TryGetWeaponVisualBoundsInItemSpace(visualModel, out bounds))
        {
            visualModel.transform.localPosition += WeaponVisualLocalPosition - bounds.center;
        }
    }

    private bool TryGetWeaponVisualBoundsInItemSpace(GameObject visualModel, out Bounds bounds)
    {
        Renderer[] renderers = visualModel.GetComponentsInChildren<Renderer>();
        bounds = new Bounds();

        if (renderers.Length == 0)
        {
            return false;
        }

        bool hasBounds = false;
        for (int i = 0; i < renderers.Length; i++)
        {
            Bounds rendererBounds = renderers[i].bounds;
            Vector3 min = rendererBounds.min;
            Vector3 max = rendererBounds.max;

            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, min.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, min.y, max.z)), ref bounds, ref hasBounds);
            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, max.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(min.x, max.y, max.z)), ref bounds, ref hasBounds);
            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, min.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, min.y, max.z)), ref bounds, ref hasBounds);
            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, max.y, min.z)), ref bounds, ref hasBounds);
            EncapsulateItemLocalPoint(transform.InverseTransformPoint(new Vector3(max.x, max.y, max.z)), ref bounds, ref hasBounds);
        }

        return hasBounds;
    }

    private void EncapsulateItemLocalPoint(Vector3 point, ref Bounds bounds, ref bool hasBounds)
    {
        if (!hasBounds)
        {
            bounds = new Bounds(point, Vector3.zero);
            hasBounds = true;
            return;
        }

        bounds.Encapsulate(point);
    }

    public override void OnEquip(GameObject itemHolder)
    {
        base.OnEquip(itemHolder);
        UpdateAmmoUI();
    }

    public override void OnUnEquip(GameObject sender)
    {
        base.OnUnEquip(sender);

        if (AmmoUI.Instance != null)
        {
            AmmoUI.Instance.HideAmmo();
        }
    }

    public override void OnUse()
    {
        Fire();
    }

    public override void OnReload()
    {
        Reload();
    }

    void Fire()
    {
        if (CurrentAmmo <= 0)
        {
            Debug.Log("[Pistol] No ammo. Reload!");
            return;
        }

        CurrentAmmo--;
        UpdateAmmoUI();
        PlaySound(FireSound, FireVolume);

        Vector3 pos = FirePoint.position;
        Quaternion dir = FirePoint.rotation;

        GameObject bulletClone = Instantiate(Bullet, pos, dir);
        Actor_Bullet bulletActor = bulletClone.GetComponent<Actor_Bullet>();
        if (bulletActor != null)
        {
            bulletActor.SetDamage(BulletDamage);
            ConfigurePistolBulletVisual(bulletActor);
        }

        Rigidbody rb = bulletClone.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(FirePoint.forward * BulletSpeed, ForceMode.VelocityChange);
        }

        Destroy(bulletClone, 2f);
    }

    private void ConfigurePistolBulletVisual(Actor_Bullet bulletActor)
    {
        if (string.IsNullOrWhiteSpace(BulletVisualModelResourcePath))
        {
            return;
        }

        bulletActor.SetVisualModel(
            BulletVisualModelResourcePath,
            BulletVisualBaseColorTextureResourcePath,
            BulletVisualHiddenNameFragments,
            BulletVisualLocalPosition,
            BulletVisualLocalEulerAngles,
            BulletVisualForwardFlipEulerAngles,
            AutoAlignBulletVisual,
            AutoFitBulletVisual,
            BulletVisualTargetLength,
            HideDefaultBulletVisuals,
            "PistolBullet_Model");
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

        Debug.Log($"[Pistol] Reloaded: {CurrentAmmo}/{ReserveAmmo}");
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

        Debug.Log($"[Pistol] Ammo picked up: +{amount}. ReserveAmmo={ReserveAmmo}");
    }
}
