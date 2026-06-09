using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    public GameObject Crosshair;

    [Header("CS2 Crosshair")]
    public Vector2 ScreenOffset = new Vector2(0f, -8f);
    public Color CrosshairColor = new Color(0f, 1f, 0.18f, 0.95f);
    public Color OutlineColor = new Color(0f, 0f, 0f, 0.85f);
    public float LineLength = 12f;
    public float LineThickness = 3f;
    public float CenterGap = 6f;
    public float OutlineThickness = 1.5f;

    private const string CrosshairRootName = "CS2_Crosshair_Root";
    private bool crosshairStyleReady;

    void Awake()
    {
        SetupCrosshairStyle();
    }

    void Update()
    {
        SetupCrosshairStyle();
        ApplyCrosshairTransform();

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

    private void SetupCrosshairStyle()
    {
        if (Crosshair == null || crosshairStyleReady)
        {
            return;
        }

        RectTransform crosshairRect = Crosshair.GetComponent<RectTransform>();
        if (crosshairRect == null)
        {
            return;
        }

        float totalSize = (CenterGap + LineLength + OutlineThickness + 2f) * 2f;
        ApplyCrosshairTransform(crosshairRect, totalSize);

        Image oldImage = Crosshair.GetComponent<Image>();
        if (oldImage != null)
        {
            oldImage.enabled = false;
            oldImage.raycastTarget = false;
        }

        Transform existingRoot = Crosshair.transform.Find(CrosshairRootName);
        if (existingRoot != null)
        {
            Destroy(existingRoot.gameObject);
        }

        RectTransform root = CreateRect(CrosshairRootName, Crosshair.transform, Vector2.zero, new Vector2(totalSize, totalSize));

        CreateSegment(root, "Right", new Vector2(CenterGap + LineLength * 0.5f, 0f), new Vector2(LineLength, LineThickness));
        CreateSegment(root, "Left", new Vector2(-CenterGap - LineLength * 0.5f, 0f), new Vector2(LineLength, LineThickness));
        CreateSegment(root, "Top", new Vector2(0f, CenterGap + LineLength * 0.5f), new Vector2(LineThickness, LineLength));
        CreateSegment(root, "Bottom", new Vector2(0f, -CenterGap - LineLength * 0.5f), new Vector2(LineThickness, LineLength));

        crosshairStyleReady = true;
    }

    private void ApplyCrosshairTransform()
    {
        if (Crosshair == null)
        {
            return;
        }

        RectTransform crosshairRect = Crosshair.GetComponent<RectTransform>();
        if (crosshairRect == null)
        {
            return;
        }

        float totalSize = (CenterGap + LineLength + OutlineThickness + 2f) * 2f;
        ApplyCrosshairTransform(crosshairRect, totalSize);
    }

    private void ApplyCrosshairTransform(RectTransform crosshairRect, float totalSize)
    {
        crosshairRect.anchorMin = new Vector2(0.5f, 0.5f);
        crosshairRect.anchorMax = new Vector2(0.5f, 0.5f);
        crosshairRect.pivot = new Vector2(0.5f, 0.5f);
        crosshairRect.anchoredPosition = ScreenOffset;
        crosshairRect.sizeDelta = new Vector2(totalSize, totalSize);
        crosshairRect.localRotation = Quaternion.identity;
        crosshairRect.localScale = Vector3.one;
    }

    private void CreateSegment(RectTransform parent, string name, Vector2 position, Vector2 size)
    {
        CreateImage($"{name}_Outline", parent, position, size + Vector2.one * OutlineThickness * 2f, OutlineColor);
        CreateImage(name, parent, position, size, CrosshairColor);
    }

    private RectTransform CreateImage(string name, RectTransform parent, Vector2 position, Vector2 size, Color color)
    {
        RectTransform rect = CreateRect(name, parent, position, size);
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        obj.layer = Crosshair.layer;
        obj.transform.SetParent(parent, false);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;
        return rect;
    }
}
