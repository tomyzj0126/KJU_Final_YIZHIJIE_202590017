using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class FinalScene2MedKitModelApplier : MonoBehaviour
{
    private const string ModelResourcePath = "Models/MedicalKitBag/source/MedicalKitBag";
    private const string TextureResourcePath = "Models/MedicalKitBag/textures/gltf_embedded_0";
    private const string VisualChildName = "MedicalKitBag_Visual";

    private static GameObject modelPrefab;
    private static Material modelMaterial;
    private static bool warnedMissingModel;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyToLoadedScenes();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyToScene(scene);
    }

    public static void ApplyToLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            ApplyToScene(SceneManager.GetSceneAt(i));
        }
    }

    public static void ApplyToScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded || !LoadModel())
        {
            return;
        }

        Actor_MedKit[] medKits = Resources.FindObjectsOfTypeAll<Actor_MedKit>();
        for (int i = 0; i < medKits.Length; i++)
        {
            Actor_MedKit medKit = medKits[i];
            if (medKit == null || medKit.gameObject.scene != scene)
            {
                continue;
            }

            ApplyToMedKit(medKit.transform);
        }
    }

    private static bool LoadModel()
    {
        if (modelPrefab == null)
        {
            modelPrefab = Resources.Load<GameObject>(ModelResourcePath);
        }

        if (modelPrefab == null)
        {
            if (!warnedMissingModel)
            {
                warnedMissingModel = true;
                Debug.LogWarning("[FinalScene2MedKitModelApplier] Medical kit bag model not found at Resources/" + ModelResourcePath);
            }

            return false;
        }

        return true;
    }

    private static void ApplyToMedKit(Transform medKit)
    {
        Transform existingVisual = medKit.Find(VisualChildName);
        if (existingVisual != null)
        {
            DestroyNow(existingVisual.gameObject);
        }

        GameObject visual = Instantiate(modelPrefab, medKit, false);
        visual.name = VisualChildName;
        visual.transform.localPosition = new Vector3(0f, 0.28f, 0f);
        visual.transform.localRotation = Quaternion.identity;
        visual.transform.localScale = Vector3.one * 0.28f;

        RemoveRuntimePhysics(visual);
        ApplyMaterial(visual);
        HideOriginalRenderers(medKit, visual.transform);
    }

    private static void HideOriginalRenderers(Transform medKit, Transform newVisual)
    {
        Renderer[] renderers = medKit.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];
            if (renderer == null || renderer.transform.IsChildOf(newVisual) || ShouldKeepRenderer(renderer))
            {
                continue;
            }

            renderer.enabled = false;
        }
    }

    private static bool ShouldKeepRenderer(Renderer renderer)
    {
        string objectName = renderer.gameObject.name.ToLowerInvariant();
        if (objectName.Contains("text") || objectName.Contains("label") || objectName.Contains("wall text"))
        {
            return true;
        }

        Component[] components = renderer.GetComponents<Component>();
        for (int i = 0; i < components.Length; i++)
        {
            Component component = components[i];
            if (component != null && component.GetType().Name.Contains("TextMeshPro"))
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveRuntimePhysics(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);
        for (int i = 0; i < colliders.Length; i++)
        {
            DestroyNow(colliders[i]);
        }

        Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>(true);
        for (int i = 0; i < rigidbodies.Length; i++)
        {
            DestroyNow(rigidbodies[i]);
        }
    }

    private static void ApplyMaterial(GameObject visual)
    {
        Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Material material = GetModelMaterial();
        if (material == null)
        {
            return;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] materials = renderers[i].sharedMaterials;
            if (materials == null || materials.Length == 0)
            {
                materials = new Material[] { material };
            }
            else
            {
                for (int j = 0; j < materials.Length; j++)
                {
                    materials[j] = material;
                }
            }

            renderers[i].sharedMaterials = materials;
        }
    }

    private static Material GetModelMaterial()
    {
        if (modelMaterial != null)
        {
            return modelMaterial;
        }

        Shader shader = Shader.Find("Standard");
        if (shader == null)
        {
            return null;
        }

        modelMaterial = new Material(shader)
        {
            name = "MedicalKitBag_Runtime_Mat"
        };

        Texture2D texture = Resources.Load<Texture2D>(TextureResourcePath);
        if (texture != null)
        {
            modelMaterial.mainTexture = texture;
        }

        if (modelMaterial.HasProperty("_Glossiness"))
        {
            modelMaterial.SetFloat("_Glossiness", 0.28f);
        }

        return modelMaterial;
    }

    private static void DestroyNow(Object target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }
}

#if UNITY_EDITOR
[InitializeOnLoad]
public static class FinalScene2MedKitModelApplierEditorSync
{
    static FinalScene2MedKitModelApplierEditorSync()
    {
        EditorApplication.delayCall -= RefreshLoadedScenes;
        EditorApplication.delayCall += RefreshLoadedScenes;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.sceneOpened += OnSceneOpened;
    }

    [MenuItem("Tools/FINAL Scene2/Sync MedKit Model Into Scene")]
    public static void RefreshLoadedScenes()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            return;
        }

        FinalScene2MedKitModelApplier.ApplyToLoadedScenes();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.IsValid() && scene.isLoaded)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall -= RefreshLoadedScenes;
        EditorApplication.delayCall += RefreshLoadedScenes;
    }
}
#endif
