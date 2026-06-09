using TMPro;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WallTextSpawner : MonoBehaviour
{
    [Header("Wall Text")]
    [TextArea(2, 5)]
    public string Message = "FIND THE EXIT";

    [Header("Placement On This Wall")]
    public Vector3 LocalPosition = new Vector3(0f, 0f, -0.51f);
    public Vector3 LocalRotation = new Vector3(0f, 180f, 0f);
    public Vector2 Size = new Vector2(4f, 1.2f);
    public float FontSize = 0.45f;
    public Color TextColor = new Color(1f, 0.92f, 0.25f, 1f);
    public bool SpawnOnStart = true;

    private TextMeshPro spawnedText;

    private void Start()
    {
        if (SpawnOnStart)
        {
            Spawn();
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall -= ApplyTextSettingsInEditor;
            EditorApplication.delayCall += ApplyTextSettingsInEditor;
            return;
        }
#endif

        if (spawnedText == null)
        {
            spawnedText = GetComponentInChildren<TextMeshPro>();
        }

        if (spawnedText != null)
        {
            ApplyTextSettings();
        }
    }

#if UNITY_EDITOR
    private void ApplyTextSettingsInEditor()
    {
        if (this == null)
        {
            return;
        }

        spawnedText = GetComponentInChildren<TextMeshPro>();

        if (spawnedText != null)
        {
            ApplyTextSettings();
        }
    }
#endif

    [ContextMenu("Update Wall Text")]
    public void UpdateWallText()
    {
        Spawn();
    }

    public void SetText(string message)
    {
        Message = message;

        if (spawnedText == null)
        {
            Spawn();
            return;
        }

        spawnedText.text = Message;
    }

    public void Spawn()
    {
        if (spawnedText == null)
        {
            spawnedText = GetComponentInChildren<TextMeshPro>();
        }

        if (spawnedText == null)
        {
            GameObject textObject = new GameObject("Wall Text");
            textObject.transform.SetParent(transform, false);
            spawnedText = textObject.AddComponent<TextMeshPro>();
        }

        ApplyTextSettings();
    }

    private void ApplyTextSettings()
    {
        Transform textTransform = spawnedText.transform;
        textTransform.SetParent(transform, false);
        textTransform.localPosition = LocalPosition;
        textTransform.localRotation = Quaternion.Euler(LocalRotation);
        textTransform.localScale = Vector3.one;

        RectTransform rectTransform = spawnedText.rectTransform;
        rectTransform.sizeDelta = Size;

        spawnedText.text = Message;
        spawnedText.fontSize = FontSize;
        spawnedText.color = TextColor;
        spawnedText.alignment = TextAlignmentOptions.Center;
        spawnedText.enableWordWrapping = true;
        spawnedText.overflowMode = TextOverflowModes.Overflow;
        spawnedText.raycastTarget = false;
    }
}
