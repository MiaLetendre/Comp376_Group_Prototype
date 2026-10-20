using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Self-contained, runtime-created pause overlay. No scene/prefab editing required.
/// Add this component to a GameObject in each gameplay scene, or use KDPolishInstaller.
/// </summary>
public class KDPolishMenu : MonoBehaviour
{
    private GameObject overlay;
    private bool paused;
    private CursorLockMode previousLock;
    private bool previousVisibility;
    private Font font;

    private void Awake()
    {
        // Prevent duplicated overlays if this component is added twice.
        if (FindObjectsByType<KDPolishMenu>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(this);
            return;
        }
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        BuildUI();
        overlay.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) SetPaused(!paused);
    }

    private void OnDisable() { if (paused) RestoreTimeAndCursor(); }
    private void OnDestroy() { if (paused) RestoreTimeAndCursor(); }

    private void BuildUI()
    {
        overlay = new GameObject("KD Pause Menu", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        overlay.transform.SetParent(transform, false);
        var canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;
        var scaler = overlay.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var dim = MakeRect("Dim", overlay.transform, Vector2.zero, Vector2.one);
        dim.gameObject.AddComponent<Image>().color = new Color(0.015f, 0.025f, 0.055f, 0.82f);

        var panel = MakeRect("Panel", overlay.transform, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
        panel.sizeDelta = new Vector2(540, 640);
        panel.gameObject.AddComponent<Image>().color = new Color(.075f, .10f, .16f, .97f);
        MakeText("PAUSED", panel, 0, 228, 46, new Color(.86f, .94f, 1f));
        MakeText("Take a breath. Then keep swinging.", panel, 0, 174, 21, new Color(.60f, .74f, .85f));
        MakeButton("RESUME", panel, 80, () => SetPaused(false));
        MakeButton("RESTART LEVEL", panel, -10, () => ChangeScene(SceneManager.GetActiveScene().buildIndex));
        MakeButton("MAIN MENU", panel, -100, () => ChangeScene(0));
        MakeButton("QUIT GAME", panel, -190, Quit);
        MakeText("ESC  •  PAUSE / RESUME", panel, 0, -278, 18, new Color(.55f, .68f, .77f));

        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("KD EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static RectTransform MakeRect(string name, Transform parent, Vector2 min, Vector2 max)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return rect;
    }

    private void MakeText(string value, Transform parent, float x, float y, int size, Color color)
    {
        var rect = MakeRect(value, parent, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
        rect.sizeDelta = new Vector2(510, 65);
        rect.anchoredPosition = new Vector2(x, y);
        var text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = color;
        text.raycastTarget = false;
    }

    private void MakeButton(string title, Transform parent, float y, UnityEngine.Events.UnityAction action)
    {
        var rect = MakeRect(title, parent, new Vector2(.5f, .5f), new Vector2(.5f, .5f));
        rect.sizeDelta = new Vector2(420, 68);
        rect.anchoredPosition = new Vector2(0, y);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = new Color(.13f, .25f, .37f);
        var button = rect.gameObject.AddComponent<Button>();
        var colors = button.colors;
        colors.highlightedColor = new Color(.23f, .45f, .59f);
        colors.pressedColor = new Color(.08f, .18f, .28f);
        button.colors = colors;
        button.onClick.AddListener(action);
        MakeText(title, rect, 0, 0, 25, Color.white);
    }

    private void SetPaused(bool value)
    {
        if (value == paused) return;
        paused = value;
        if (value)
        {
            previousLock = Cursor.lockState;
            previousVisibility = Cursor.visible;
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            overlay.SetActive(true);
        }
        else
        {
            overlay.SetActive(false);
            RestoreTimeAndCursor();
        }
    }

    private void RestoreTimeAndCursor()
    {
        Time.timeScale = 1f;
        Cursor.lockState = previousLock;
        Cursor.visible = previousVisibility;
        paused = false;
    }

    private void ChangeScene(int index)
    {
        if (index < 0 || index >= SceneManager.sceneCountInBuildSettings) return;
        SetPaused(false);
        SceneManager.LoadScene(index);
    }

    private void Quit()
    {
        SetPaused(false);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
