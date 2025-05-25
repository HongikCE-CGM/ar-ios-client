// Assets/BallUIButtonCreator.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BallUIButtonCreator : MonoBehaviour
{
    [Header("Optional: Assign an existing Canvas, or leave empty to auto-create")]
    public Canvas canvas;

    private bool ballOn = false;
    private Image toggleButtonImage;

    void Start()
    {
        // 1) Canvas 없으면 생성
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem 없으면 생성
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }
        }

        // 2) Ball 토글 버튼 생성
        var btnGO = new GameObject("BallToggleButton");
        btnGO.transform.SetParent(canvas.transform, false);

        // RectTransform 세팅 (Feed 버튼 위, 300×200)
        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        // feed 버튼이 (-10,10) 위치에 높이 200이므로, 위로 210px 올립니다
        rt.anchoredPosition = new Vector2(-10, 10 + 200 + 10);

        // 버튼 이미지 (사각형)
        toggleButtonImage = btnGO.AddComponent<Image>();
        toggleButtonImage.color = new Color(1f, 1f, 1f, 0.5f); // 초기 off 상태

        // Button 컴포넌트 & 클릭 리스너
        var button = btnGO.AddComponent<Button>();
        button.targetGraphic = toggleButtonImage;
        button.onClick.AddListener(OnToggleBall);

        // 버튼 텍스트
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(btnGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero;
        txtRT.offsetMax = Vector2.zero;
        var txt = txtGO.AddComponent<Text>();
        txt.text = "Ball";
        txt.alignment = TextAnchor.MiddleCenter;
        txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        txt.fontSize = 30;
        txt.color = Color.black;
    }

    private void OnToggleBall()
    {
        ballOn = !ballOn;
        toggleButtonImage.color = ballOn 
            ? new Color(1f, 1f, 1f, 1f)   // On
            : new Color(1f, 1f, 1f, 0.5f);
    }

    /// <summary>
    /// 외부에서 현재 Ball 발사 가능 상태를 조회
    /// </summary>
    public bool IsBallOn() => ballOn;
}