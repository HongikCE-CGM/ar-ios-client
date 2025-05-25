// Assets/SleepUIButtonCreator.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SleepUIButtonCreator : MonoBehaviour
{
    [Header("Optional: Assign existing Canvas or leave empty")]
    public Canvas canvas;

    bool sleepOn = false;
    Image toggleButtonImage;

    void Start()
    {
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            if (FindObjectOfType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        var btnGO = new GameObject("SleepToggleButton");
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        // Ball(-10,220) 위에 배치: 10 + 200 + 10 + 200 + 10 = 430
        rt.anchoredPosition = new Vector2(-10, 430);

        toggleButtonImage = btnGO.AddComponent<Image>();
        toggleButtonImage.color = new Color(1f, 1f, 1f, 0.5f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = toggleButtonImage;
        btn.onClick.AddListener(OnToggleSleep);

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<Text>();
        txt.text = "쓰다듬기";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 30;
        txt.color = Color.black;
    }

    void OnToggleSleep()
    {
        sleepOn = !sleepOn;
        toggleButtonImage.color = sleepOn ? new Color(1f,1f,1f,1f)
            : new Color(1f,1f,1f,0.5f);

        if (sleepOn)
        {
            foreach (var f in FindObjectsOfType<FeedUIButtonCreator>())
                f.SetFeedOn(false);
            foreach (var b in FindObjectsOfType<BallUIButtonCreator>())
                b.SetBallOn(false);
        }
    }

    public bool IsSleepOn() => sleepOn;

    public void SetSleepOn(bool value)
    {
        sleepOn = value;
        toggleButtonImage.color = sleepOn ?  new Color(1f,1f,1f,1f)
            : new Color(1f,1f,1f,0.5f);
    }
}