using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinalScene2MedKitModelApplier : MonoBehaviour
{
    private const string TargetSceneName = "FINAL_Scene2";
    private const string ModelChildName = "MedicalKit_Bag_Model";
    private const string ModelResourcePath = "Models/MedicalKitBag/medical_kit_bag";
    private const string BaseColorPath = "Models/MedicalKitBag/MedicalKitBag_BaseColor";
    private static readonly Vector3 ModelLocalEuler = Vector3.zero;

    private static GameObject modelPrefab;
    private static Material modelMaterial;

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

    public static void RefreshScene(Scene scene)
    {
        ApplyToScene(scene);
    }

    private static void ApplyToScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        HashSet<Transform> medKits = FindMedKitRoots(scene);
        if (scene.name != TargetSceneName && medKits.Count == 0)
        {
            return;
        }

        PrepareAssets();
        if (modelPrefab == null)
        {
            Debug.LogWarning($"[{nameof(FinalScene2MedKitModelApplier)}] Could not load model at Resources/{ModelResourcePath}.");
            return;
        }

        int appliedCount = 0;
        foreach (Transform medKit in medKits)
        {
            if (ApplyModel(medKit))
            {
                appliedCount++;
            }
        }

        Debug.Log($"[{nameof(FinalScene2MedKitModelApplier)}] Medical kit model check finished in scene '{scene.name}'. Applied/updated: {appliedCount}, medkits found: {medKits.Count}.");
    }

    private static HashSet<Transform> FindMedKitRoots(Scene scene)
    {
        HashSet<Transform> medKits = new HashSet<Transform>();

        foreach (Actor_MedKit medKit in Resources.FindObjectsOfTypeAll<Actor_MedKit>())
        {
            AddSceneObject(medKits, medKit, scene);
        }

        foreach (Transform transform in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (transform.gameObject.scene == scene && transform.name.StartsWith("MedKit"))
            {
                medKits.Add(transform);
            }
        }

        return medKits;
    }

    private static void AddSceneObject(HashSet<Transform> medKits, Component component, Scene scene)
    {
        if (component != null && component.gameObject.scene == scene)
        {
            medKits.Add(component.transform);
        }
    }

    private static void PrepareAssets()
    {
        if (modelPrefab == null)
        {
            modelPrefab = Resources.Load<GameObject>(ModelResourcePath);
        }

        if (modelMaterial == null)
        {
            modelMaterial = CreateModelMaterial();
        }
    }

    private static bool ApplyModel(Transform medKit)
    {
        if (medKit == null)
        {
            return false;
        }

        Bounds targetBounds = GetTargetBounds(medKit);
        Transform existingModel = medKit.Find(ModelChildName);
        bool created = existingModel == null;
        GameObject model = created ? Instantiate(modelPrefab, medKit, false) : existingModel.gameObject;

        model.name = ModelChildName;
        model.transform.SetParent(medKit, false);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(ModelLocalEuler);
        model.transform.localScale = Vector3.one;
        model.SetActive(true);

        RemoveGeneratedPhysics(model);
        ApplyMaterial(model);
        HideOriginalRenderers(medKit, model.transform);
        FitModelToBounds(model.transform, targetBounds);
        return created;
    }

    private static Material CreateModelMaterial()
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
        material.name = "MedicalKitBag_Runtime_Material";
        Texture2D baseColor = Resources.Load<Texture2D>(BaseColorPath);
        if (baseColor != null)
        {
            material.mainTexture = baseColor;
        }

        material.color = Color.white;
        material.SetFloat("_Glossiness", 0.24f);
        material.SetFloat("_Metallic", 0f);
        return material;
    }

    private static void ApplyMaterial(GameObject model)
    {
        if (modelMaterial == null)
        {
            return;
        }

        foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = modelMaterial;
            }

            renderer.sharedMaterials = materials;
            renderer.enabled = true;
        }
    }

    private static void HideOriginalRenderers(Transform medKit, Transform visibleModel)
    {
        foreach (Renderer renderer in medKit.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer.transform.IsChildOf(visibleModel))
            {
                renderer.enabled = true;
                continue;
            }

            renderer.enabled = false;
        }
    }

    private static Bounds GetTargetBounds(Transform medKit)
    {
        if (TryGetRendererBounds(medKit, null, out Bounds rendererBounds))
        {
            return rendererBounds;
        }

        if (TryGetColliderBounds(medKit, out Bounds colliderBounds))
        {
            return colliderBounds;
        }

        return new Bounds(medKit.position, new Vector3(0.55f, 0.35f, 0.4f));
    }

    private static void FitModelToBounds(Transform model, Bounds targetBounds)
    {
        if (!TryGetRendererBounds(model, null, out Bounds modelBounds))
        {
            return;
        }

        float modelLongest = Mathf.Max(modelBounds.size.x, Mathf.Max(modelBounds.size.y, modelBounds.size.z));
        float targetLongest = Mathf.Max(targetBounds.size.x, Mathf.Max(targetBounds.size.y, targetBounds.size.z));
        if (modelLongest > 0.001f && targetLongest > 0.001f)
        {
            float scale = Mathf.Clamp(targetLongest / modelLongest, 0.001f, 25f);
            model.localScale *= scale;
        }

        if (!TryGetRendererBounds(model, null, out modelBounds))
        {
            return;
        }

        Vector3 offset = targetBounds.center - modelBounds.center;
        offset.y = targetBounds.min.y - modelBounds.min.y;
        model.position += offset;
    }

    private static bool TryGetRendererBounds(Transform root, Transform excludedRoot, out Bounds bounds)
    {
        bounds = new Bounds(root.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || !renderer.enabled)
            {
                continue;
            }

            if (excludedRoot != null && renderer.transform.IsChildOf(excludedRoot))
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(renderer.bounds);
        }

        return hasBounds;
    }

    private static bool TryGetColliderBounds(Transform root, out Bounds bounds)
    {
        bounds = new Bounds(root.position, Vector3.zero);
        bool hasBounds = false;

        foreach (Collider collider in root.GetComponentsInChildren<Collider>(true))
        {
            if (collider == null || !collider.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
                continue;
            }

            bounds.Encapsulate(collider.bounds);
        }

        return hasBounds;
    }

    private static void RemoveGeneratedPhysics(GameObject model)
    {
        foreach (Collider collider in model.GetComponentsInChildren<Collider>(true))
        {
            DestroyGeneratedObject(collider);
        }

        foreach (Rigidbody rigidbody in model.GetComponentsInChildren<Rigidbody>(true))
        {
            DestroyGeneratedObject(rigidbody);
        }
    }

    private static void DestroyGeneratedObject(Object target)
    {
        if (target == null)
        {
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            DestroyImmediate(target);
            return;
        }
#endif

        Destroy(target);
    }
}

#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
internal static class FinalScene2MedKitModelApplierEditorSync
{
    private const string TargetSceneName = "FINAL_Scene2";

    static FinalScene2MedKitModelApplierEditorSync()
    {
        UnityEditor.EditorApplication.delayCall += RefreshLoadedScenes;
        UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    [UnityEditor.MenuItem("Tools/FINAL Scene2/Sync Medical Kit Model")]
    private static void SyncMedicalKitModel()
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

            FinalScene2MedKitModelApplier.RefreshScene(scene);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        }
    }
}
#endif
