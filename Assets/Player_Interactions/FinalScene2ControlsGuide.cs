using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FinalScene2ControlsGuide : MonoBehaviour
{
    private const string TargetSceneName = "FINAL_Scene2";
    private const string CanvasName = "FINAL_Scene2_Controls_Guide_Canvas";

    private static bool appliedForCurrentLoad;
    private static Font guideFont;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplyForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        appliedForCurrentLoad = false;
        ApplyForScene(scene);
    }

    private static void ApplyForScene(Scene scene)
    {
        if (appliedForCurrentLoad || !scene.IsValid() || scene.name != TargetSceneName)
        {
            return;
        }

        appliedForCurrentLoad = true;
        CreateControlsGuide();
    }

    private static void CreateControlsGuide()
    {
        if (GameObject.Find(CanvasName) != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject(CanvasName);
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 80;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject panel = new GameObject("Controls Guide Panel");
        panel.transform.SetParent(canvasObject.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1f, 1f);
        panelRect.anchorMax = new Vector2(1f, 1f);
        panelRect.pivot = new Vector2(1f, 1f);
        panelRect.anchoredPosition = new Vector2(-24f, -24f);
        panelRect.sizeDelta = new Vector2(410f, 430f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.58f);

        GameObject textObject = new GameObject("Controls Guide Text");
        textObject.transform.SetParent(panel.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(22f, 18f);
        textRect.offsetMax = new Vector2(-22f, -18f);

        Text text = textObject.AddComponent<Text>();
        text.font = GetGuideFont();
        text.fontSize = 18;
        text.resizeTextForBestFit = true;
        text.resizeTextMinSize = 13;
        text.resizeTextMaxSize = 20;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.lineSpacing = 1.08f;
        text.text =
            "按键说明\n\n" +
            "WASD / 方向键    移动\n" +
            "鼠标移动         视角\n" +
            "Shift            奔跑\n" +
            "Space            跳跃\n" +
            "鼠标左键         开火 / 使用\n" +
            "R                换弹\n" +
            "鼠标滚轮 / Tab   切换物品\n" +
            "C                装备 / 收起\n" +
            "X                收起物品\n" +
            "G                丢弃物品\n" +
            "Z                加入背包\n" +
            "Esc              暂停\n" +
            "T + 鼠标右键     传送\n" +
            "F + 鼠标左键     远距离抓取\n" +
            "U + 鼠标左键     远距离拉取\n" +
            "P + 鼠标左键     远距离点击";
    }

    private static Font GetGuideFont()
    {
        if (guideFont != null)
        {
            return guideFont;
        }

        guideFont = Font.CreateDynamicFontFromOSFont(new[] { "Microsoft YaHei", "SimHei", "Arial" }, 18);
        if (guideFont == null)
        {
            guideFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return guideFont;
    }
}
