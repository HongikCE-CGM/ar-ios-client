using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SleepUIButtonCreator : MonoBehaviour
{
    [Header("Optional: Assign existing Canvas, or leave empty to auto-create")]
    public Canvas canvas;

    private bool sleepOn = false;
    private Image toggleButtonImage;

    void Start()
    {
        // 1) Canvas 생성 및 설정
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }
        }

        // 2) Sleep 토글 버튼 생성
        var btnGO = new GameObject("SleepToggleButton");
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        // Feed(-10,10), Ball(-10,220), Sleep → 위로 200+10 더
        rt.anchoredPosition = new Vector2(-10, 430);

        toggleButtonImage = btnGO.AddComponent<Image>();
        toggleButtonImage.color = new Color(1f, 1f, 1f, 0.5f); // 초기 Off

        var button = btnGO.AddComponent<Button>();
        button.targetGraphic = toggleButtonImage;
        button.onClick.AddListener(OnToggleSleep);

        // 버튼 텍스트
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<Text>();
        txt.text = "Sleep";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 30;
        txt.color = Color.black;
    }

    private void OnToggleSleep()
    {
        sleepOn = !sleepOn;
        toggleButtonImage.color = sleepOn 
            ? new Color(1f, 1f, 1f, 1f)   // On
            : new Color(1f, 1f, 1f, 0.5f);;
    }

    /// <summary>
    /// 외부에서 현재 Sleep 토글 상태를 확인할 때
    /// </summary>
    public bool IsSleepOn() => sleepOn;
}