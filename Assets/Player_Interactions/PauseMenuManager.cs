using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class PauseMenuManager : MonoBehaviour
{
    [Header("Scene")]
    public string MainMenuSceneName = "FINAL_Scene1";

    [Header("Pause Menu")]
    public GameObject PauseMenuUI;
    public bool CreateMenuIfMissing = true;
    public string TitleText = "Pause Menu";
    public string ContinueButtonText = "Continue";
    public string MainMenuButtonText = "Main Menu";

    [Header("Player Control")]
    public bool DisableLookWhilePaused = true;

    private bool isPaused;
    private readonly List<Behaviour> disabledLookComponents = new List<Behaviour>();

    private void Awake()
    {
        Time.timeScale = 1f;

        if (PauseMenuUI == null && CreateMenuIfMissing)
        {
            PauseMenuUI = CreatePauseMenu();
        }

        if (PauseMenuUI != null)
        {
            PauseMenuUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (WasEscapePressed())
        {
            if (isPaused)
            {
                ContinueGame();
            }
            else
            {
                ShowPauseMenu();
            }
        }
    }

    public void ShowPauseMenu()
    {
        isPaused = true;
        Time.timeScale = 0f;
        SetLookControlsEnabled(false);

        if (PauseMenuUI != null)
        {
            PauseMenuUI.SetActive(true);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ContinueGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        SetLookControlsEnabled(true);

        if (PauseMenuUI != null)
        {
            PauseMenuUI.SetActive(false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetLookControlsEnabled(true);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private bool WasEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            return true;
        }
#endif

        return false;
    }

    private void SetLookControlsEnabled(bool enabled)
    {
        if (!DisableLookWhilePaused)
        {
            return;
        }

        if (!enabled)
        {
            disabledLookComponents.Clear();
            AddDisabledLookComponents(FindObjectsOfType<LookBehavior>(true));
            AddDisabledLookComponents(FindObjectsOfType<LookController_PC>(true));
            return;
        }

        foreach (Behaviour component in disabledLookComponents)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }

        disabledLookComponents.Clear();
    }

    private void AddDisabledLookComponents(Behaviour[] components)
    {
        foreach (Behaviour component in components)
        {
            if (component != null && component.enabled)
            {
                component.enabled = false;
                disabledLookComponents.Add(component);
            }
        }
    }

    private GameObject CreatePauseMenu()
    {
        EnsureEventSystem();

        GameObject canvasObject = new GameObject("Pause Menu Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject overlay = new GameObject("Pause Menu");
        overlay.transform.SetParent(canvasObject.transform, false);

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(520f, 360f);

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.08f, 0.08f, 0.09f, 0.95f);

        VerticalLayoutGroup layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(44, 44, 40, 40);
        layout.spacing = 24f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        AddText(panel.transform, TitleText, 44f, 70f);
        AddButton(panel.transform, ContinueButtonText, ContinueGame);
        AddButton(panel.transform, MainMenuButtonText, ReturnToMainMenu);

        return overlay;
    }

    private void AddText(Transform parent, string text, float fontSize, float height)
    {
        GameObject textObject = new GameObject(text);
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.color = Color.white;
        label.alignment = TextAlignmentOptions.Center;

        LayoutElement layout = textObject.AddComponent<LayoutElement>();
        layout.preferredHeight = height;
    }

    private void AddButton(Transform parent, string text, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObject = new GameObject(text);
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.92f, 0.92f, 0.92f, 1f);

        Button button = buttonObject.AddComponent<Button>();
        button.onClick.AddListener(action);

        LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
        layout.preferredHeight = 72f;

        GameObject labelObject = new GameObject("Text");
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.AddComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = 30f;
        label.color = new Color(0.08f, 0.08f, 0.09f, 1f);
        label.alignment = TextAlignmentOptions.Center;
    }

    private void EnsureEventSystem()
    {
        if (FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
#else
        eventSystemObject.AddComponent<StandaloneInputModule>();
#endif
    }
}
