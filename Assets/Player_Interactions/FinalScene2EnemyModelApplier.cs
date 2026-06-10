using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class FinalScene2EnemyModelApplier : MonoBehaviour
{
    private const string TargetSceneName = "FINAL_Scene2";
    private const string ModelChildName = "CS2_Terrorist_Animated_Model";
    private const string PreviousAnimatedModelChildName = "CopZombie_Animated_Model";
    private const string PreviousStaticModelChildName = "CS2_Terrorist_Model";
    private const string WeaponChildName = "AK47_In_Hand";
    private const string WeaponVisualChildName = "AK47_Model";
    private const string AnimationDriverChildName = "CopZombie_Animation_Driver";
    private const string RunnerName = "FINAL_Scene2_Enemy_Model_Applier";
    private const string ModelResourcePath = "Models/CS2Terrorist/source/csgoT";
    private const string AnimationDriverResourcePath = "Models/CopZombieAnimated/source/CopZombie_RifleIdle";
    private const string IdleClipPath = "Models/CopZombieAnimated/source/CopZombie_RifleIdle";
    private const string WalkClipPath = "Models/CopZombieAnimated/source/CopZombie_WalkForward";
    private const string AimInClipPath = "Models/CopZombieAnimated/source/CopZombie_RifleDownToAim";
    private const string AimIdleClipPath = "Models/CopZombieAnimated/source/CopZombie_RifleAimingIdle";
    private const string WeaponResourcePath = "Models/CopZombieAnimated/AK47/source/AKM";
    private const string WeaponTexturePath = "Models/CopZombieAnimated/AK47/textures/Low_Poly_Base_Mesh_PogChampion!_Material__";
    private const string BodyTexturePath = "Models/CS2Terrorist/textures/tm_leet_v2_body_variantd_color";
    private const string LowerBodyTexturePath = "Models/CS2Terrorist/textures/tm_leet_v2_lower_body_variantd_color";
    private const string ShemaghTexturePath = "Models/CS2Terrorist/textures/tm_leet_v2_shemagh_variantd_color";
    private static readonly Vector3 ModelEuler = Vector3.zero;
    private static readonly Vector3 WeaponLocalPosition = new Vector3(0f, 0.06f, 0.14f);
    private static readonly Vector3 WeaponLocalEuler = new Vector3(90f, 0f, 90f);
    private const float WeaponAimRollDegrees = 0f;

    internal static Quaternion CurrentWeaponVisualRotation => Quaternion.Euler(WeaponLocalEuler);
    internal static Quaternion CurrentWeaponAimRoll => Quaternion.AngleAxis(WeaponAimRollDegrees, Vector3.forward);

    private static GameObject modelPrefab;
    private static GameObject weaponPrefab;
    private static GameObject animationDriverPrefab;
    private static AnimationClip idleClip;
    private static AnimationClip walkClip;
    private static AnimationClip aimInClip;
    private static AnimationClip aimIdleClip;
    private static Material weaponMaterial;
    private static Material bodyMaterial;
    private static Material lowerBodyMaterial;
    private static Material shemaghMaterial;
    private static FinalScene2EnemyModelApplyRunner runner;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyToScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene(scene);
    }

    private static void ApplyToScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        HashSet<Transform> enemies = FindEnemyRoots(scene);
        if (scene.name != TargetSceneName && enemies.Count == 0)
        {
            return;
        }

        modelPrefab = Resources.Load<GameObject>(ModelResourcePath);
        if (modelPrefab == null)
        {
            Debug.LogWarning($"[{nameof(FinalScene2EnemyModelApplier)}] Could not load model at Resources/{ModelResourcePath}.");
            return;
        }

        PrepareAssets();

        int appliedCount = 0;
        foreach (Transform enemy in enemies)
        {
            if (ApplyModel(enemy))
            {
                appliedCount++;
            }
        }

        EnsureRunner(scene);
        Debug.Log($"[{nameof(FinalScene2EnemyModelApplier)}] Animated enemy model check finished in scene '{scene.name}'. Applied/updated: {appliedCount}, enemies found: {enemies.Count}.");
    }

    private static HashSet<Transform> FindEnemyRoots(Scene scene)
    {
        HashSet<Transform> enemies = new HashSet<Transform>();

        foreach (EnemyRandomMove move in Resources.FindObjectsOfTypeAll<EnemyRandomMove>())
        {
            AddSceneObject(enemies, move, scene);
        }

        foreach (Actor_Enemy actor in Resources.FindObjectsOfTypeAll<Actor_Enemy>())
        {
            AddSceneObject(enemies, actor, scene);
        }

        foreach (EnemyHealth health in Resources.FindObjectsOfTypeAll<EnemyHealth>())
        {
            AddSceneObject(enemies, health, scene);
        }

        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform.gameObject.scene == scene && transform.name.StartsWith("Character_Enemy"))
            {
                enemies.Add(transform);
            }
        }

        return enemies;
    }

    private static void AddSceneObject(HashSet<Transform> enemies, Component component, Scene scene)
    {
        if (component != null && component.gameObject.scene == scene)
        {
            enemies.Add(component.transform);
        }
    }

    private static void EnsureRunner(Scene scene)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return;
        }
#endif

        if (runner != null)
        {
            return;
        }

        GameObject runnerObject = new GameObject(RunnerName);
        SceneManager.MoveGameObjectToScene(runnerObject, scene);
        runner = runnerObject.AddComponent<FinalScene2EnemyModelApplyRunner>();
        runner.Initialize(scene);
    }

    public static void RefreshScene(Scene scene)
    {
        ApplyToScene(scene);
    }

    internal static void DestroyGeneratedObject(UnityEngine.Object target)
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            UnityEngine.Object.DestroyImmediate(target);
            return;
        }
#endif

        UnityEngine.Object.Destroy(target);
    }

    private static bool ApplyModel(Transform enemy)
    {
        if (enemy == null)
        {
            return false;
        }

        RemoveGeneratedChild(enemy, PreviousAnimatedModelChildName);
        RemoveGeneratedChild(enemy, PreviousStaticModelChildName);
        HideOriginalRenderers(enemy);

        Transform existingModel = enemy.Find(ModelChildName);
        if (existingModel != null)
        {
            AttachWeapon(existingModel);
            ConfigureAnimation(existingModel.gameObject, enemy);
            return false;
        }

        GameObject model = Instantiate(modelPrefab, enemy, false);
        model.name = ModelChildName;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(ModelEuler);
        model.transform.localScale = Vector3.one;

        ApplyImportedMaterials(model);
        FitModelToEnemy(model.transform, enemy);
        AttachWeapon(model.transform);
        ConfigureAnimation(model, enemy);
        return true;
    }

    private static void RemoveGeneratedChild(Transform enemy, string childName)
    {
        Transform child = enemy.Find(childName);
        if (child != null)
        {
            DestroyGeneratedObject(child.gameObject);
        }
    }

    private static void HideOriginalRenderers(Transform enemy)
    {
        Transform currentModel = enemy.Find(ModelChildName);

        foreach (Renderer renderer in enemy.GetComponentsInChildren<Renderer>(true))
        {
            if (currentModel != null && renderer.transform.IsChildOf(currentModel))
            {
                renderer.enabled = true;
                continue;
            }

            renderer.enabled = false;
        }
    }

    private static void PrepareAssets()
    {
        if (weaponPrefab == null)
        {
            weaponPrefab = Resources.Load<GameObject>(WeaponResourcePath);
        }

        if (animationDriverPrefab == null)
        {
            animationDriverPrefab = Resources.Load<GameObject>(AnimationDriverResourcePath);
        }

        if (idleClip == null)
        {
            idleClip = LoadAnimationClip(IdleClipPath);
        }

        if (walkClip == null)
        {
            walkClip = LoadAnimationClip(WalkClipPath);
        }

        if (aimInClip == null)
        {
            aimInClip = LoadAnimationClip(AimInClipPath);
        }

        if (aimIdleClip == null)
        {
            aimIdleClip = LoadAnimationClip(AimIdleClipPath);
        }

        if (weaponMaterial == null)
        {
            weaponMaterial = CreateRuntimeMaterial("AK47_Material", Resources.Load<Texture2D>(WeaponTexturePath));
        }

        if (bodyMaterial == null)
        {
            bodyMaterial = CreateRuntimeMaterial("CS2_Terrorist_Body", Resources.Load<Texture2D>(BodyTexturePath));
        }

        if (lowerBodyMaterial == null)
        {
            lowerBodyMaterial = CreateRuntimeMaterial("CS2_Terrorist_LowerBody", Resources.Load<Texture2D>(LowerBodyTexturePath));
        }

        if (shemaghMaterial == null)
        {
            shemaghMaterial = CreateRuntimeMaterial("CS2_Terrorist_Shemagh", Resources.Load<Texture2D>(ShemaghTexturePath));
        }
    }

    private static AnimationClip LoadAnimationClip(string resourcePath)
    {
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>(resourcePath);
        AnimationClip preferredClip = null;
        AnimationClip fallbackClip = null;
        int preferredScore = int.MinValue;

        foreach (AnimationClip clip in clips)
        {
            if (clip == null)
            {
                continue;
            }

            string clipName = clip.name.ToLowerInvariant();
            if (clipName.Contains("__preview__"))
            {
                continue;
            }

            if (fallbackClip == null || clip.length > fallbackClip.length)
            {
                fallbackClip = clip;
            }

            int score = ScoreAnimationClip(clip, resourcePath);
            if (score > preferredScore || score == preferredScore && preferredClip != null && clip.length > preferredClip.length)
            {
                preferredClip = clip;
                preferredScore = score;
            }
        }

        AnimationClip bestClip = preferredClip != null ? preferredClip : fallbackClip;
        if (bestClip == null)
        {
            Debug.LogWarning($"[{nameof(FinalScene2EnemyModelApplier)}] Could not load animation clip at Resources/{resourcePath}.");
        }
        else
        {
            Debug.Log($"[{nameof(FinalScene2EnemyModelApplier)}] Using animation clip '{bestClip.name}' ({bestClip.length:0.00}s) from Resources/{resourcePath}.");
        }

        return bestClip;
    }

    private static int ScoreAnimationClip(AnimationClip clip, string resourcePath)
    {
        string clipName = clip.name.ToLowerInvariant();
        string path = resourcePath.ToLowerInvariant();
        int score = 0;

        if (clip.length > 0.03f)
        {
            score += 10;
        }

        if (clipName.Contains("mixamo"))
        {
            score += 80;
        }

        if (clipName.Contains("take 001") || clipName.Contains("default take"))
        {
            score -= 60;
        }

        if (path.Contains("walk") && clipName.Contains("walk"))
        {
            score += 30;
        }

        if (path.Contains("aim") && (clipName.Contains("aim") || clipName.Contains("rifle")))
        {
            score += 30;
        }

        if (path.Contains("idle") && clipName.Contains("idle"))
        {
            score += 30;
        }

        return score;
    }

    private static void AttachWeapon(Transform model)
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning($"[{nameof(FinalScene2EnemyModelApplier)}] Could not load AK47 at Resources/{WeaponResourcePath}.");
            return;
        }

        Transform hand = FindRightHand(model);
        if (hand == null)
        {
            hand = model;
        }

        bool createdWeapon = false;
        GameObject weapon = FindOrCreateWeaponAnchor(model, hand, out createdWeapon);
        weapon.transform.SetParent(model, true);
        weapon.transform.position = hand.position;
        weapon.transform.rotation = model.rotation;
        weapon.transform.localScale = Vector3.one;

        ApplyWeaponMaterial(weapon);
        RemoveWeaponPhysics(weapon);
        Quaternion weaponVisualRotation = PrepareWeaponVisual(weapon.transform, model);
        ConfigureWeaponFollower(weapon, model, hand, weaponVisualRotation);

        if (createdWeapon)
        {
            Debug.Log($"[{nameof(FinalScene2EnemyModelApplier)}] AK47 attached to '{hand.name}' on {model.name}.");
        }
    }

    private static GameObject FindOrCreateWeaponAnchor(Transform model, Transform hand, out bool createdWeapon)
    {
        GameObject keptWeapon = null;
        createdWeapon = false;

        foreach (Transform child in model.GetComponentsInChildren<Transform>(true))
        {
            if (child.name != WeaponChildName)
            {
                continue;
            }

            if (child.Find(WeaponVisualChildName) == null)
            {
                DestroyGeneratedObject(child.gameObject);
                continue;
            }

            if (keptWeapon == null)
            {
                keptWeapon = child.gameObject;
                continue;
            }

            DestroyGeneratedObject(child.gameObject);
        }

        if (keptWeapon != null)
        {
            if (keptWeapon.transform.parent != model)
            {
                keptWeapon.transform.SetParent(model, true);
            }

            return keptWeapon;
        }

        createdWeapon = true;
        GameObject weaponAnchor = new GameObject(WeaponChildName);
        weaponAnchor.transform.SetParent(hand, false);

        GameObject weaponVisual = Instantiate(weaponPrefab, weaponAnchor.transform, false);
        weaponVisual.name = WeaponVisualChildName;
        return weaponAnchor;
    }

    private static void ConfigureWeaponFollower(GameObject weapon, Transform model, Transform hand, Quaternion weaponVisualRotation)
    {
        FinalScene2EnemyWeaponFollower follower = weapon.GetComponent<FinalScene2EnemyWeaponFollower>();
        if (follower == null)
        {
            follower = weapon.AddComponent<FinalScene2EnemyWeaponFollower>();
        }

        follower.Configure(model, hand, WeaponChildName, WeaponVisualChildName, AnimationDriverChildName, WeaponLocalPosition, weaponVisualRotation);
    }

    private static Quaternion PrepareWeaponVisual(Transform weaponAnchor, Transform model)
    {
        Transform weaponVisual = weaponAnchor.Find(WeaponVisualChildName);
        Quaternion selectedRotation = CurrentWeaponVisualRotation;
        if (weaponVisual == null)
        {
            return selectedRotation;
        }

        weaponVisual.localPosition = Vector3.zero;
        selectedRotation = CurrentWeaponVisualRotation;
        weaponVisual.localRotation = selectedRotation;
        weaponVisual.localScale = Vector3.one;

        foreach (Renderer renderer in weaponVisual.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = true;
        }

        if (!TryGetRendererBounds(weaponVisual, out Bounds weaponBounds))
        {
            return selectedRotation;
        }

        float longestSide = Mathf.Max(weaponBounds.size.x, Mathf.Max(weaponBounds.size.y, weaponBounds.size.z));
        if (longestSide <= 0.001f)
        {
            return selectedRotation;
        }

        float targetLength = 0.78f;
        if (TryGetRendererBounds(model, weaponAnchor, out Bounds modelBounds))
        {
            targetLength = Mathf.Clamp(modelBounds.size.y * 0.43f, 0.65f, 0.95f);
        }

        float scale = Mathf.Clamp(targetLength / longestSide, 0.001f, 20f);
        weaponVisual.localScale = Vector3.one * scale;

        if (!TryGetRendererBounds(weaponVisual, out weaponBounds))
        {
            return selectedRotation;
        }

        Vector3 localCenter = weaponAnchor.InverseTransformPoint(weaponBounds.center);
        weaponVisual.localPosition -= localCenter;
        return selectedRotation;
    }

    private static Quaternion PickWeaponRotation(Transform weaponAnchor, Transform weaponVisual)
    {
        Quaternion baseRotation = CurrentWeaponVisualRotation;
        Transform barrelEnd = FindWeaponPart(weaponVisual, "barrelend", "barrelaim", "barrel");
        Transform stock = FindWeaponPart(weaponVisual, "stockwood", "stockmetal", "stockbase", "stock");
        if (barrelEnd == null || stock == null)
        {
            return baseRotation;
        }

        Quaternion[] candidates =
        {
            baseRotation,
            Quaternion.AngleAxis(180f, Vector3.up) * baseRotation,
            Quaternion.AngleAxis(180f, Vector3.right) * baseRotation,
            Quaternion.AngleAxis(180f, Vector3.forward) * baseRotation,
            baseRotation * Quaternion.AngleAxis(180f, Vector3.up),
            baseRotation * Quaternion.AngleAxis(180f, Vector3.right),
            baseRotation * Quaternion.AngleAxis(180f, Vector3.forward)
        };

        Quaternion originalRotation = weaponVisual.localRotation;
        Vector3 originalPosition = weaponVisual.localPosition;
        Vector3 originalScale = weaponVisual.localScale;
        Quaternion bestRotation = baseRotation;
        float bestScore = float.NegativeInfinity;

        weaponVisual.localPosition = Vector3.zero;
        weaponVisual.localScale = Vector3.one;

        foreach (Quaternion candidate in candidates)
        {
            weaponVisual.localRotation = candidate;
            float score = weaponAnchor.InverseTransformPoint(barrelEnd.position).z
                - weaponAnchor.InverseTransformPoint(stock.position).z;

            if (score > bestScore)
            {
                bestScore = score;
                bestRotation = candidate;
            }
        }

        weaponVisual.localRotation = originalRotation;
        weaponVisual.localPosition = originalPosition;
        weaponVisual.localScale = originalScale;
        return bestRotation;
    }

    private static Transform FindWeaponPart(Transform root, params string[] preferredNameParts)
    {
        Transform best = null;
        int bestScore = 0;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            string normalizedName = NormalizeBoneName(child.name);
            for (int i = 0; i < preferredNameParts.Length; i++)
            {
                string partName = NormalizeBoneName(preferredNameParts[i]);
                if (!normalizedName.Contains(partName))
                {
                    continue;
                }

                int score = preferredNameParts.Length - i;
                if (score > bestScore)
                {
                    best = child;
                    bestScore = score;
                }
            }
        }

        return best;
    }

    private static Transform FindRightHand(Transform root)
    {
        Transform best = null;
        int bestScore = 0;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (HasAncestor(child, AnimationDriverChildName) || HasAncestor(child, WeaponChildName))
            {
                continue;
            }

            string name = NormalizeBoneName(child.name);
            if (name.Contains("thumb") || name.Contains("index") || name.Contains("middle") || name.Contains("ring") || name.Contains("pinky"))
            {
                continue;
            }

            int score = 0;
            if (name.EndsWith("righthand") || name.EndsWith("rhand"))
            {
                score = 100;
            }
            else if (name.Contains("righthand") || name.Contains("rhand") || name.Contains("handr"))
            {
                score = 80;
            }

            if (score > bestScore)
            {
                best = child;
                bestScore = score;
            }
        }

        return best;
    }

    private static bool HasAncestor(Transform transform, string ancestorName)
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name == ancestorName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private static string NormalizeBoneName(string boneName)
    {
        return boneName
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace(".", string.Empty)
            .Replace(":", string.Empty);
    }

    private static void ApplyWeaponMaterial(GameObject weapon)
    {
        if (weaponMaterial == null)
        {
            return;
        }

        foreach (Renderer renderer in weapon.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = weaponMaterial;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static void RemoveWeaponPhysics(GameObject weapon)
    {
        foreach (Collider collider in weapon.GetComponentsInChildren<Collider>(true))
        {
            DestroyGeneratedObject(collider);
        }

        foreach (Rigidbody rigidbody in weapon.GetComponentsInChildren<Rigidbody>(true))
        {
            DestroyGeneratedObject(rigidbody);
        }
    }

    private static void FitWeaponToCharacter(Transform weapon, Transform model)
    {
        if (!TryGetRendererBounds(weapon, out Bounds weaponBounds) || !TryGetRendererBounds(model, out Bounds modelBounds))
        {
            return;
        }

        float longestSide = Mathf.Max(weaponBounds.size.x, Mathf.Max(weaponBounds.size.y, weaponBounds.size.z));
        if (longestSide <= 0.001f)
        {
            return;
        }

        float targetLength = Mathf.Clamp(modelBounds.size.y * 0.46f, 0.55f, 1.0f);
        float scale = Mathf.Clamp(targetLength / longestSide, 0.001f, 10f);
        weapon.localScale *= scale;
    }

    private static void ApplyImportedMaterials(GameObject model)
    {
        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = PickTerroristMaterial(materials[i] != null ? materials[i].name : string.Empty, i);
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static Material PickTerroristMaterial(string sourceName, int slotIndex)
    {
        string lowerName = sourceName.ToLowerInvariant();

        if (lowerName.Contains("lower") || lowerName.Contains("leg") || lowerName.Contains("pant"))
        {
            return lowerBodyMaterial;
        }

        if (lowerName.Contains("shemagh") || lowerName.Contains("head") || lowerName.Contains("face"))
        {
            return shemaghMaterial;
        }

        if (slotIndex == 1)
        {
            return lowerBodyMaterial;
        }

        if (slotIndex == 2)
        {
            return shemaghMaterial;
        }

        return bodyMaterial;
    }

    private static Material CreateRuntimeMaterial(string materialName, Texture2D texture)
    {
        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Diffuse");
        }

        Material material = new Material(shader);
        material.name = materialName;

        if (texture != null)
        {
            material.mainTexture = texture;
        }

        return material;
    }

    private static void ConfigureAnimation(GameObject model, Transform enemy)
    {
        FinalScene2EnemyVisualAnimator visualAnimator = model.GetComponent<FinalScene2EnemyVisualAnimator>();
        if (visualAnimator == null)
        {
            visualAnimator = model.AddComponent<FinalScene2EnemyVisualAnimator>();
        }

        visualAnimator.Configure(
            enemy,
            enemy.GetComponent<Actor_Enemy>(),
            enemy.GetComponent<EnemyRandomMove>(),
            animationDriverPrefab,
            idleClip,
            walkClip,
            aimInClip,
            aimIdleClip);
    }

    private static void FitModelToEnemy(Transform model, Transform enemy)
    {
        if (!TryGetRendererBounds(model, out Bounds modelBounds))
        {
            return;
        }

        float targetHeight = GetTargetHeight(enemy);
        if (modelBounds.size.y > 0.001f)
        {
            float scale = Mathf.Clamp(targetHeight / modelBounds.size.y, 0.001f, 10f);
            model.localScale *= scale;
        }

        if (!TryGetRendererBounds(model, out modelBounds))
        {
            return;
        }

        float baseY = GetBaseY(enemy);
        Vector3 offset = enemy.position - modelBounds.center;
        offset.y = baseY - modelBounds.min.y;
        model.position += offset;
    }

    private static float GetTargetHeight(Transform enemy)
    {
        if (TryGetColliderBounds(enemy, out Bounds bounds) && bounds.size.y > 0.2f)
        {
            return Mathf.Clamp(bounds.size.y * 0.95f, 1.4f, 2.2f);
        }

        return 1.8f;
    }

    private static float GetBaseY(Transform enemy)
    {
        if (TryGetColliderBounds(enemy, out Bounds bounds))
        {
            return bounds.min.y;
        }

        return enemy.position.y;
    }

    private static bool TryGetRendererBounds(Transform root, out Bounds bounds)
    {
        return TryGetRendererBounds(root, null, out bounds);
    }

    private static bool TryGetRendererBounds(Transform root, Transform excludedRoot, out Bounds bounds)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
        bounds = default;
        bool hasBounds = false;

        foreach (Renderer renderer in renderers)
        {
            if (excludedRoot != null && renderer.transform.IsChildOf(excludedRoot))
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        return hasBounds;
    }

    private static bool TryGetColliderBounds(Transform root, out Bounds bounds)
    {
        Collider[] colliders = root.GetComponentsInChildren<Collider>(true);
        bounds = default;
        bool hasBounds = false;

        foreach (Collider collider in colliders)
        {
            if (collider.isTrigger)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }
}

public class FinalScene2EnemyModelApplyRunner : MonoBehaviour
{
    private const float RefreshDuration = 8f;
    private const float RefreshInterval = 0.6f;

    private Scene scene;
    private float elapsed;
    private float timer;

    public void Initialize(Scene ownerScene)
    {
        scene = ownerScene;
    }

    private void Start()
    {
        timer = RefreshInterval;
    }

    private void Update()
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            FinalScene2EnemyModelApplier.DestroyGeneratedObject(gameObject);
            return;
        }

        elapsed += Time.deltaTime;
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = RefreshInterval;
            FinalScene2EnemyModelApplier.RefreshScene(scene);
        }

        if (elapsed >= RefreshDuration)
        {
            FinalScene2EnemyModelApplier.DestroyGeneratedObject(gameObject);
        }
    }
}

public class FinalScene2EnemyVisualAnimator : MonoBehaviour
{
    private const string AnimationDriverChildName = "CopZombie_Animation_Driver";
    private const float MovingStartSpeed = 0.08f;
    private const float MovingStopSpeed = 0.025f;
    private const float MovingHoldSeconds = 0.24f;
    private const float AimHoldSeconds = 0.35f;
    private const float AimExitDistancePadding = 0.85f;

    private struct BonePair
    {
        public readonly Transform Source;
        public readonly Transform Target;

        public BonePair(Transform source, Transform target)
        {
            Source = source;
            Target = target;
        }
    }

    private Transform enemyRoot;
    private Actor_Enemy actorEnemy;
    private EnemyRandomMove enemyMover;
    private GameObject animationDriverPrefab;
    private GameObject animationDriver;
    private AnimationClip idleClip;
    private AnimationClip walkClip;
    private AnimationClip aimInClip;
    private AnimationClip aimIdleClip;
    private Animator animator;
    private PlayableGraph graph;
    private AnimationClipPlayable currentPlayable;
    private AnimationClip currentClip;
    private bool currentClipLoops;
    private bool wasAiming;
    private Vector3 lastEnemyPosition;
    private Transform playerTarget;
    private readonly List<BonePair> bonePairs = new List<BonePair>();
    private bool configured;
    private bool movingState;
    private bool aimingState;
    private float smoothedSpeed;
    private float movingHoldUntil;
    private float aimingHoldUntil;

    public void Configure(
        Transform ownerEnemy,
        Actor_Enemy ownerActor,
        EnemyRandomMove ownerMover,
        GameObject driverPrefab,
        AnimationClip idle,
        AnimationClip walk,
        AnimationClip aimIn,
        AnimationClip aimIdle)
    {
        bool firstConfigure = !configured;
        enemyRoot = ownerEnemy;
        actorEnemy = ownerActor;
        enemyMover = ownerMover;
        animationDriverPrefab = driverPrefab;
        idleClip = idle;
        walkClip = walk;
        aimInClip = aimIn;
        aimIdleClip = aimIdle;
        if (firstConfigure)
        {
            lastEnemyPosition = enemyRoot != null ? enemyRoot.position : transform.position;
        }

        CachePlayer();
        InitializeDriverAnimator();
        BuildBonePairs();

        if (firstConfigure || currentClip == null)
        {
            PlayClip(idleClip, true);
        }

        configured = true;
    }

    private void Start()
    {
        InitializeDriverAnimator();
        BuildBonePairs();
        if (currentClip == null)
        {
            PlayClip(idleClip, true);
        }
    }

    private void Update()
    {
        InitializeDriverAnimator();

        bool isAiming = IsPlayerInAttackRange();
        if (isAiming)
        {
            UpdateAimAnimation();
        }
        else
        {
            PlayClip(IsMoving() ? walkClip : idleClip, true);
            wasAiming = false;
        }

        LoopCurrentClipIfNeeded();
        lastEnemyPosition = enemyRoot != null ? enemyRoot.position : transform.position;
    }

    private void LateUpdate()
    {
        if (graph.IsValid())
        {
            graph.Evaluate(0f);
        }

        ApplyDriverPoseToVisibleModel();
    }

    private void InitializeDriverAnimator()
    {
        if (animator != null)
        {
            HideDriverRenderers();
            return;
        }

        if (animationDriver == null && animationDriverPrefab != null)
        {
            animationDriver = Instantiate(animationDriverPrefab, transform, false);
            animationDriver.name = AnimationDriverChildName;
            animationDriver.transform.localPosition = Vector3.zero;
            animationDriver.transform.localRotation = Quaternion.identity;
            animationDriver.transform.localScale = Vector3.one;
            RemoveDriverPhysics();
        }

        if (animationDriver != null)
        {
            animator = animationDriver.GetComponent<Animator>();
            if (animator == null)
            {
                animator = animationDriver.GetComponentInChildren<Animator>();
            }
        }

        if (animator == null && animationDriver != null)
        {
            animator = animationDriver.AddComponent<Animator>();
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        if (animator != null)
        {
            animator.enabled = true;
            animator.applyRootMotion = false;
            animator.runtimeAnimatorController = null;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }

        DisableVisibleModelAnimators();
        HideDriverRenderers();
    }

    private void DisableVisibleModelAnimators()
    {
        if (animationDriver == null)
        {
            return;
        }

        foreach (Animator visibleAnimator in GetComponentsInChildren<Animator>(true))
        {
            if (visibleAnimator == null || visibleAnimator == animator)
            {
                continue;
            }

            if (visibleAnimator.transform.IsChildOf(animationDriver.transform))
            {
                continue;
            }

            visibleAnimator.enabled = false;
        }
    }

    private void BuildBonePairs()
    {
        if (animationDriver == null || bonePairs.Count > 0)
        {
            return;
        }

        Dictionary<string, Transform> sourceBones = BuildBoneMap(animationDriver.transform, null);
        Dictionary<string, Transform> targetBones = BuildBoneMap(transform, animationDriver.transform);

        foreach (KeyValuePair<string, Transform> source in sourceBones)
        {
            if (targetBones.TryGetValue(source.Key, out Transform target))
            {
                bonePairs.Add(new BonePair(source.Value, target));
            }
        }

        Debug.Log($"[{nameof(FinalScene2EnemyVisualAnimator)}] Bone driver pairs: {bonePairs.Count} on {name}.");
    }

    private Dictionary<string, Transform> BuildBoneMap(Transform root, Transform excludedRoot)
    {
        Dictionary<string, Transform> bones = new Dictionary<string, Transform>();

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (excludedRoot != null && child.IsChildOf(excludedRoot))
            {
                continue;
            }

            if (!IsLikelyHumanBone(child.name))
            {
                continue;
            }

            string normalizedName = NormalizeBoneName(child.name);
            if (!bones.ContainsKey(normalizedName))
            {
                bones.Add(normalizedName, child);
            }
        }

        return bones;
    }

    private void ApplyDriverPoseToVisibleModel()
    {
        if (bonePairs.Count == 0)
        {
            return;
        }

        foreach (BonePair pair in bonePairs)
        {
            if (pair.Source == null || pair.Target == null)
            {
                continue;
            }

            pair.Target.localRotation = pair.Source.localRotation;
        }
    }

    private void HideDriverRenderers()
    {
        if (animationDriver == null)
        {
            return;
        }

        foreach (Renderer renderer in animationDriver.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = false;
        }
    }

    private void RemoveDriverPhysics()
    {
        if (animationDriver == null)
        {
            return;
        }

        foreach (Collider collider in animationDriver.GetComponentsInChildren<Collider>(true))
        {
            FinalScene2EnemyModelApplier.DestroyGeneratedObject(collider);
        }

        foreach (Rigidbody rigidbody in animationDriver.GetComponentsInChildren<Rigidbody>(true))
        {
            FinalScene2EnemyModelApplier.DestroyGeneratedObject(rigidbody);
        }
    }

    private bool IsLikelyHumanBone(string boneName)
    {
        string normalizedName = NormalizeBoneName(boneName);
        return normalizedName.Contains("mixamorig")
            || normalizedName.Contains("hips")
            || normalizedName.Contains("spine")
            || normalizedName.Contains("head")
            || normalizedName.Contains("neck")
            || normalizedName.Contains("shoulder")
            || normalizedName.Contains("arm")
            || normalizedName.Contains("hand")
            || normalizedName.Contains("leg")
            || normalizedName.Contains("foot")
            || normalizedName.Contains("toe");
    }

    private string NormalizeBoneName(string boneName)
    {
        return boneName
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace(".", string.Empty)
            .Replace(":", string.Empty);
    }

    private void UpdateAimAnimation()
    {
        if (!wasAiming)
        {
            wasAiming = true;
            PlayClip(aimInClip != null ? aimInClip : aimIdleClip, false);
            return;
        }

        if (currentClip == aimInClip && IsCurrentClipFinished())
        {
            PlayClip(aimIdleClip != null ? aimIdleClip : idleClip, true);
            return;
        }

        if (currentClip != aimInClip && currentClip != aimIdleClip)
        {
            PlayClip(aimIdleClip != null ? aimIdleClip : idleClip, true);
        }
    }

    private bool IsPlayerInAttackRange()
    {
        if (actorEnemy == null)
        {
            return false;
        }

        if (playerTarget == null)
        {
            CachePlayer();
        }

        if (playerTarget == null || enemyRoot == null)
        {
            return false;
        }

        float range = actorEnemy.AttackRange + (aimingState ? AimExitDistancePadding : 0f);
        if (Vector3.Distance(enemyRoot.position, playerTarget.position) <= range)
        {
            aimingHoldUntil = Time.time + AimHoldSeconds;
        }

        aimingState = Time.time <= aimingHoldUntil;
        return aimingState;
    }

    private bool IsMoving()
    {
        Transform root = enemyRoot != null ? enemyRoot : transform;
        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        float rawSpeed = Vector3.Distance(root.position, lastEnemyPosition) / deltaTime;
        smoothedSpeed = Mathf.Lerp(smoothedSpeed, rawSpeed, Mathf.Clamp01(Time.deltaTime * 12f));

        float startSpeed = MovingStartSpeed;
        float stopSpeed = MovingStopSpeed;
        if (enemyMover != null && enemyMover.enabled)
        {
            startSpeed *= 0.75f;
            stopSpeed *= 0.75f;
        }

        bool speedSaysMoving = movingState ? smoothedSpeed > stopSpeed : smoothedSpeed > startSpeed;
        if (speedSaysMoving)
        {
            movingHoldUntil = Time.time + MovingHoldSeconds;
        }

        movingState = Time.time <= movingHoldUntil;
        return movingState;
    }

    private void CachePlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        playerTarget = player != null ? player.transform : null;
    }

    private void PlayClip(AnimationClip clip, bool loop)
    {
        if (clip == null || animator == null || currentClip == clip && currentClipLoops == loop)
        {
            return;
        }

        DestroyGraph();
        clip.wrapMode = loop ? WrapMode.Loop : WrapMode.Once;

        graph = PlayableGraph.Create($"{name}_AnimationGraph");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        currentPlayable = AnimationClipPlayable.Create(graph, clip);
        currentPlayable.SetApplyFootIK(false);
        currentPlayable.SetApplyPlayableIK(false);
        currentPlayable.SetTime(0d);
        currentPlayable.SetSpeed(1d);

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        output.SetSourcePlayable(currentPlayable);
        graph.Play();
        graph.Evaluate(0f);

        currentClip = clip;
        currentClipLoops = loop;
    }

    private void LoopCurrentClipIfNeeded()
    {
        if (!currentClipLoops || currentClip == null || !currentPlayable.IsValid() || currentClip.length <= 0.001f)
        {
            return;
        }

        double time = currentPlayable.GetTime();
        if (time >= currentClip.length)
        {
            currentPlayable.SetTime(time % currentClip.length);
            if (graph.IsValid())
            {
                graph.Evaluate(0f);
            }
        }
    }

    private bool IsCurrentClipFinished()
    {
        return currentClip == null || !currentPlayable.IsValid() || currentPlayable.GetTime() >= currentClip.length;
    }

    private void OnDisable()
    {
        DestroyGraph();
    }

    private void OnDestroy()
    {
        DestroyGraph();
    }

    private void DestroyGraph()
    {
        if (graph.IsValid())
        {
            graph.Destroy();
        }

        currentClip = null;
    }
}

public class FinalScene2EnemyWeaponFollower : MonoBehaviour
{
    private Transform modelRoot;
    private Transform rightHand;
    private Transform leftHand;
    private Transform chest;
    private string weaponChildName;
    private string weaponVisualChildName;
    private string animationDriverChildName;
    private Vector3 worldOffset;
    private Quaternion weaponVisualRotation = Quaternion.identity;

    public void Configure(
        Transform ownerModel,
        Transform ownerRightHand,
        string weaponName,
        string visualName,
        string driverChildName,
        Vector3 mountOffset,
        Quaternion visualRotation)
    {
        modelRoot = ownerModel;
        rightHand = ownerRightHand;
        leftHand = FindHand(modelRoot, false);
        chest = FindChest(modelRoot);
        weaponChildName = weaponName;
        weaponVisualChildName = visualName;
        animationDriverChildName = driverChildName;
        worldOffset = mountOffset;
        weaponVisualRotation = visualRotation;
        ApplyMount();
    }

    private void LateUpdate()
    {
        if (modelRoot == null)
        {
            return;
        }

        if (rightHand == null || rightHand.name == weaponChildName || rightHand.IsChildOf(transform))
        {
            rightHand = FindHand(modelRoot, true);
        }

        if (leftHand == null || leftHand.name == weaponChildName || leftHand.IsChildOf(transform))
        {
            leftHand = FindHand(modelRoot, false);
        }

        if (chest == null || chest.name == weaponChildName || chest.IsChildOf(transform))
        {
            chest = FindChest(modelRoot);
        }

        ApplyMount();
    }

    private void ApplyMount()
    {
        if (rightHand == null && leftHand == null)
        {
            return;
        }

        if (transform.parent != modelRoot)
        {
            transform.SetParent(modelRoot, true);
        }

        Vector3 up = modelRoot.up;
        Vector3 handCenter = GetHandCenter();
        Vector3 forward = GetAimForward(handCenter);
        Vector3 right = Vector3.Cross(up, forward).normalized;
        if (right.sqrMagnitude <= 0.001f)
        {
            right = modelRoot.right;
        }

        transform.position = handCenter
            + right * worldOffset.x
            + up * worldOffset.y
            + forward * worldOffset.z;
        transform.rotation = Quaternion.LookRotation(-forward, up) * FinalScene2EnemyModelApplier.CurrentWeaponAimRoll;

        Transform weaponVisual = transform.Find(weaponVisualChildName);
        if (weaponVisual != null)
        {
            weaponVisual.localRotation = FinalScene2EnemyModelApplier.CurrentWeaponVisualRotation;
        }
    }

    private Vector3 GetHandCenter()
    {
        if (rightHand != null && leftHand != null)
        {
            return Vector3.Lerp(rightHand.position, leftHand.position, 0.5f);
        }

        if (rightHand != null)
        {
            return rightHand.position;
        }

        return leftHand.position;
    }

    private Vector3 GetAimForward(Vector3 handCenter)
    {
        if (chest != null)
        {
            Vector3 chestToHands = handCenter - chest.position;
            if (chestToHands.sqrMagnitude > 0.0025f)
            {
                return chestToHands.normalized;
            }
        }

        if (modelRoot.forward.sqrMagnitude > 0.001f)
        {
            return modelRoot.forward;
        }

        return Vector3.forward;
    }

    private Transform FindHand(Transform root, bool rightSide)
    {
        Transform best = null;
        int bestScore = 0;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child == transform || child.IsChildOf(transform) || HasAncestor(child, animationDriverChildName) || HasAncestor(child, weaponVisualChildName))
            {
                continue;
            }

            string name = NormalizeBoneName(child.name);
            if (name.Contains("thumb") || name.Contains("index") || name.Contains("middle") || name.Contains("ring") || name.Contains("pinky"))
            {
                continue;
            }

            int score = 0;
            if (rightSide && (name.EndsWith("righthand") || name.EndsWith("rhand")))
            {
                score = 100;
            }
            else if (!rightSide && (name.EndsWith("lefthand") || name.EndsWith("lhand")))
            {
                score = 100;
            }
            else if (rightSide && (name.Contains("righthand") || name.Contains("rhand") || name.Contains("handr")))
            {
                score = 80;
            }
            else if (!rightSide && (name.Contains("lefthand") || name.Contains("lhand") || name.Contains("handl")))
            {
                score = 80;
            }

            if (score > bestScore)
            {
                best = child;
                bestScore = score;
            }
        }

        return best;
    }

    private Transform FindChest(Transform root)
    {
        Transform best = null;
        int bestScore = 0;

        foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
        {
            if (child == transform || child.IsChildOf(transform) || HasAncestor(child, animationDriverChildName) || HasAncestor(child, weaponVisualChildName))
            {
                continue;
            }

            string name = NormalizeBoneName(child.name);
            int score = 0;
            if (name.EndsWith("spine2") || name.EndsWith("chest"))
            {
                score = 100;
            }
            else if (name.EndsWith("spine1") || name.Contains("upperchest"))
            {
                score = 80;
            }
            else if (name.EndsWith("spine"))
            {
                score = 60;
            }

            if (score > bestScore)
            {
                best = child;
                bestScore = score;
            }
        }

        return best;
    }

    private bool HasAncestor(Transform transformToCheck, string ancestorName)
    {
        if (string.IsNullOrEmpty(ancestorName))
        {
            return false;
        }

        Transform current = transformToCheck;
        while (current != null)
        {
            if (current.name == ancestorName)
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private string NormalizeBoneName(string boneName)
    {
        return boneName
            .ToLowerInvariant()
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Replace(".", string.Empty)
            .Replace(":", string.Empty);
    }
}

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
internal static class FinalScene2EnemyModelApplierEditorSync
{
    private const string TargetSceneName = "FINAL_Scene2";

    static FinalScene2EnemyModelApplierEditorSync()
    {
        UnityEditor.EditorApplication.delayCall += RefreshLoadedScenes;
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    [UnityEditor.MenuItem("Tools/FINAL Scene2/Sync Game Visuals Into Scene")]
    private static void SyncGameVisualsIntoScene()
    {
        RefreshLoadedScenes();
    }

    private static void OnSceneOpened(Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
    {
        UnityEditor.EditorApplication.delayCall += RefreshLoadedScenes;
    }

    private static void RefreshLoadedScenes()
    {
        if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.IsValid() || !scene.isLoaded || scene.name != TargetSceneName)
            {
                continue;
            }

            FinalScene2EnemyModelApplier.RefreshScene(scene);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
#endif
