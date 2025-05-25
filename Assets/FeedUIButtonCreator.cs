using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FeedUIButtonCreator : MonoBehaviour
{
    // 이미 씬에 Canvas가 있으면 할당, 없으면 Start()에서 생성
    public Canvas canvas;

    private bool feedOn = false;
    private Image toggleButtonImage;

    void Start()
    {
        // Canvas 생성 및 설정
        if (canvas == null)
        {
            var canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            // EventSystem이 없으면 생성
            if (FindObjectOfType<EventSystem>() == null)
            {
                var esGO = new GameObject("EventSystem");
                esGO.AddComponent<EventSystem>();
                esGO.AddComponent<StandaloneInputModule>();
            }
        }

        // 버튼 생성
        var buttonGO = new GameObject("FeedToggleButton");
        buttonGO.transform.SetParent(canvas.transform, false);

        // RectTransform 설정 (300×200, 우측 하단)
        var rt = buttonGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        rt.anchorMin = new Vector2(1, 0);
        rt.anchorMax = new Vector2(1, 0);
        rt.pivot = new Vector2(1, 0);
        rt.anchoredPosition = new Vector2(-10, 10);


    toggleButtonImage = buttonGO.AddComponent<Image>();
    toggleButtonImage.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");
    toggleButtonImage.type = Image.Type.Simple;
    toggleButtonImage.preserveAspect = false;
    toggleButtonImage.color = new Color(1f, 1f, 1f, 0.5f); // Off 상태: 반투명

        // 버튼 컴포넌트 & 클릭 리스너
        var button = buttonGO.AddComponent<Button>();
        button.targetGraphic = toggleButtonImage;
        button.onClick.AddListener(OnToggleFeed);

        // 버튼 텍스트
        var textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform, false);
        var textRT = textGO.AddComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        var text = textGO.AddComponent<Text>();
        text.text = "먹이주기";
        text.alignment = TextAnchor.MiddleCenter;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 30;
        text.color = Color.black;
    }

    private void OnToggleFeed()
    {
        feedOn = !feedOn;
        // On 상태: 완전 불투명, Off 상태: 반투명
        toggleButtonImage.color = feedOn 
            ? new Color(1f, 1f, 1f, 1f)   // On
            : new Color(1f, 1f, 1f, 0.5f); // Off
    }

    // FeedSpawner에서 상태 확인용
    public bool IsFeedOn() => feedOn;
}