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
    Text buttonText;
    ModernButtonAnimator modernAnimator;

    void Start()
    {
        if (canvas == null)
        {
            var cgo = new GameObject("Canvas");
            canvas = cgo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            cgo.AddComponent<CanvasScaler>();
            cgo.AddComponent<GraphicRaycaster>();
            if (FindFirstObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
        }

        CreateModernButton();
    }

    void CreateModernButton()
    {
        var btnGO = new GameObject("SleepToggleButton");
        btnGO.transform.SetParent(canvas.transform, false);

        var rt = btnGO.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200, 200); // 훨씬 더 큰 정사각형 모던 버튼
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        // 하단 중앙, 왼쪽 버튼 위치 (훨씬 더 위로)
        rt.anchoredPosition = new Vector2(-220, 180);

        // 모던한 둥근 버튼 배경
        toggleButtonImage = btnGO.AddComponent<Image>();
        toggleButtonImage.color = new Color(0.2f, 0.7f, 0.9f, 0.8f); // 모던한 블루
        toggleButtonImage.sprite = CreateRoundedSprite();

        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = toggleButtonImage;
        btn.onClick.AddListener(OnToggleSleep);

        // 그림자 효과
        var shadowGO = new GameObject("Shadow");
        shadowGO.transform.SetParent(btnGO.transform, false);
        var shadowRT = shadowGO.AddComponent<RectTransform>();
        shadowRT.anchorMin = Vector2.zero;
        shadowRT.anchorMax = Vector2.one;
        shadowRT.offsetMin = new Vector2(4, -4);
        shadowRT.offsetMax = new Vector2(4, -4);
        var shadowImg = shadowGO.AddComponent<Image>();
        shadowImg.color = new Color(0, 0, 0, 0.3f);
        shadowImg.sprite = CreateRoundedSprite();
        shadowGO.transform.SetAsFirstSibling();

        // 아이콘 + 텍스트 컨테이너
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(btnGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero;
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        // 텍스트 라벨 (원의 완전한 정중앙에 배치)
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(contentGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.sizeDelta = new Vector2(180, 40);
        txtRT.anchorMin = new Vector2(0.5f, 0.5f);
        txtRT.anchorMax = new Vector2(0.5f, 0.5f);
        txtRT.anchoredPosition = new Vector2(0, 0); // 완전한 정중앙!!!
        buttonText = txtGO.AddComponent<Text>();
        buttonText.text = "😴 쓰다듬기";
        buttonText.alignment = TextAnchor.MiddleCenter;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 20; // 더 큰 텍스트
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;

        // 모던 버튼 애니메이션 효과
        modernAnimator = btnGO.AddComponent<ModernButtonAnimator>();
        modernAnimator.SetColors(
            new Color(0.2f, 0.7f, 0.9f, 0.8f), // normal
            new Color(0.3f, 0.8f, 1.0f, 0.9f), // hover
            new Color(0.1f, 0.5f, 0.7f, 1.0f)  // press
        );
    }

    Sprite CreateRoundedSprite()
    {
        // 간단한 원형 스프라이트 생성
        var texture = new Texture2D(100, 100);
        var center = new Vector2(50, 50);
        var radius = 45;

        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), center);
                var alpha = distance <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 100, 100), new Vector2(0.5f, 0.5f));
    }

    void OnToggleSleep()
    {
        sleepOn = !sleepOn;
        UpdateButtonAppearance();

        if (sleepOn)
        {
            foreach (var f in FindObjectsByType<FeedUIButtonCreator>(FindObjectsSortMode.None))
                f.SetFeedOn(false);
            foreach (var b in FindObjectsByType<BallUIButtonCreator>(FindObjectsSortMode.None))
                b.SetBallOn(false);
        }
    }

    void UpdateButtonAppearance()
    {
        if (sleepOn)
        {
            var activeColor = new Color(0.1f, 0.5f, 0.7f, 1f); // 더 진한 블루
            toggleButtonImage.color = activeColor;
            buttonText.color = Color.white;
            if (modernAnimator != null)
            {
                modernAnimator.SetColors(
                    activeColor,
                    new Color(0.2f, 0.6f, 0.8f, 1f), // hover
                    new Color(0.05f, 0.4f, 0.6f, 1f) // press
                );
            }
        }
        else
        {
            var normalColor = new Color(0.2f, 0.7f, 0.9f, 0.8f); // 원래 블루
            toggleButtonImage.color = normalColor;
            buttonText.color = Color.white;
            if (modernAnimator != null)
            {
                modernAnimator.SetColors(
                    normalColor,
                    new Color(0.3f, 0.8f, 1.0f, 0.9f), // hover
                    new Color(0.1f, 0.5f, 0.7f, 1.0f)  // press
                );
            }
        }
    }

    public bool IsSleepOn() => sleepOn;

    public void SetSleepOn(bool value)
    {
        sleepOn = value;
        UpdateButtonAppearance();
    }
}