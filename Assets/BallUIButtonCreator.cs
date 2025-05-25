// Assets/BallUIButtonCreator.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallUIButtonCreator : MonoBehaviour
{
    [Header("Optional: Assign existing Canvas or leave empty")]
    public Canvas canvas;

    bool ballOn = false;
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

        var btnGO = new GameObject("BallToggleButton");
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        // Feed(-10,10) 위에 배치
        rt.anchoredPosition = new Vector2(-10, 10 + 200 + 10);

        toggleButtonImage = btnGO.AddComponent<Image>();
        toggleButtonImage.color = new Color(1f, 1f, 1f, 0.5f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = toggleButtonImage;
        btn.onClick.AddListener(OnToggleBall);

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<Text>();
        txt.text = "공놀이";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 30;
        txt.color = Color.black;
    }

    void OnToggleBall()
    {
        ballOn = !ballOn;
        toggleButtonImage.color = ballOn ? new Color(1f,1f,1f,1f)
            : new Color(1f,1f,1f,0.5f);

        if (ballOn)
        {
            foreach (var f in FindObjectsOfType<FeedUIButtonCreator>())
                f.SetFeedOn(false);
            foreach (var s in FindObjectsOfType<SleepUIButtonCreator>())
                s.SetSleepOn(false);
        }
    }

    public bool IsBallOn() => ballOn;

    public void SetBallOn(bool value)
    {
        ballOn = value;
        toggleButtonImage.color = ballOn ? new Color(1f,1f,1f,1f)
            : new Color(1f,1f,1f,0.5f);
    }
}