// Assets/FeedUIButtonCreator.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FeedUIButtonCreator : MonoBehaviour
{
    [Header("Optional: Assign existing Canvas or leave empty")]
    public Canvas canvas;

    bool feedOn = false;
    Image toggleButtonImage;

    void Start()
    {
        // Canvas 생성/확인
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

        // 버튼 생성
        var btnGO = new GameObject("FeedToggleButton");
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-10, 10);

        // 이미지 & 초기 색상
        toggleButtonImage = btnGO.AddComponent<Image>();
        toggleButtonImage.color = new Color(1f, 1f, 1f, 0.5f);

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = toggleButtonImage;
        btn.onClick.AddListener(OnToggleFeed);

        // 텍스트
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = txtRT.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<Text>();
        txt.text = "먹이주기";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 30;
        txt.color = Color.black;
    }

    void OnToggleFeed()
    {
        feedOn = !feedOn;
        toggleButtonImage.color = feedOn
            ? new Color(1f,1f,1f,1f)
            : new Color(1f,1f,1f,0.5f);

        if (feedOn)
        {
            // 다른 버튼 꺼주기
            foreach (var b in FindObjectsOfType<BallUIButtonCreator>())
                b.SetBallOn(false);
            foreach (var s in FindObjectsOfType<SleepUIButtonCreator>())
                s.SetSleepOn(false);
        }
    }

    public bool IsFeedOn() => feedOn;

    public void SetFeedOn(bool value)
    {
        feedOn = value;
        toggleButtonImage.color = feedOn
            ? new Color(1f,1f,1f,1f)
            : new Color(1f,1f,1f,0.5f);
    }
}