using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ModernUIManager : MonoBehaviour
{
    [Header("UI Settings")]
    public Canvas canvas;
    
    void Start()
    {
        CreateModernUIBackground();
    }
    
    void CreateModernUIBackground()
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
        
        // 버튼 그룹 배경 패널 생성
        var backgroundPanel = new GameObject("ButtonGroupBackground");
        backgroundPanel.transform.SetParent(canvas.transform, false);
        
        var bgRT = backgroundPanel.AddComponent<RectTransform>();
        bgRT.sizeDelta = new Vector2(800, 260); // 더욱 더 큰 배경 패널
        bgRT.anchorMin = new Vector2(0.5f, 0f);
        bgRT.anchorMax = new Vector2(0.5f, 0f);
        bgRT.pivot = new Vector2(0.5f, 0f);
        bgRT.anchoredPosition = new Vector2(0, 220); // 새로운 버튼 위치에 맞춤
        
        // 반투명 둥근 배경
        var bgImage = backgroundPanel.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.3f); // 반투명 검은색
        bgImage.sprite = CreateRoundedRectSprite();
        
        // 배경을 버튼들보다 뒤에 배치
        backgroundPanel.transform.SetAsFirstSibling();
        
        // 부드러운 그라데이션 효과를 위한 추가 레이어
        var gradientPanel = new GameObject("GradientOverlay");
        gradientPanel.transform.SetParent(backgroundPanel.transform, false);
        
        var gradRT = gradientPanel.AddComponent<RectTransform>();
        gradRT.anchorMin = Vector2.zero;
        gradRT.anchorMax = Vector2.one;
        gradRT.offsetMin = Vector2.zero;
        gradRT.offsetMax = Vector2.zero;
        
        var gradImage = gradientPanel.AddComponent<Image>();
        gradImage.color = new Color(1f, 1f, 1f, 0.1f); // 미세한 하이라이트
        gradImage.sprite = CreateGradientSprite();
    }
    
    Sprite CreateRoundedRectSprite()
    {
        var texture = new Texture2D(400, 130); // 더욱 더 큰 텍스처
        var cornerRadius = 60f;
        
        for (int x = 0; x < 400; x++)
        {
            for (int y = 0; y < 130; y++)
            {
                float alpha = 1f;
                
                // 둥근 모서리 계산
                if (x < cornerRadius) // 왼쪽 모서리
                {
                    var distX = Mathf.Max(0, cornerRadius - x);
                    var distY = y < cornerRadius ? Mathf.Max(0, cornerRadius - y) : 
                               y > (130 - cornerRadius) ? Mathf.Max(0, y - (130 - cornerRadius)) : 0;
                    var dist = Mathf.Sqrt(distX * distX + distY * distY);
                    alpha = dist <= cornerRadius ? 1f : 0f;
                }
                else if (x > (400 - cornerRadius)) // 오른쪽 모서리
                {
                    var distX = Mathf.Max(0, x - (400 - cornerRadius));
                    var distY = y < cornerRadius ? Mathf.Max(0, cornerRadius - y) : 
                               y > (130 - cornerRadius) ? Mathf.Max(0, y - (130 - cornerRadius)) : 0;
                    var dist = Mathf.Sqrt(distX * distX + distY * distY);
                    alpha = dist <= cornerRadius ? 1f : 0f;
                }
                
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 400, 130), new Vector2(0.5f, 0.5f));
    }
    
    Sprite CreateGradientSprite()
    {
        var texture = new Texture2D(400, 130); // 더욱 더 큰 텍스처
        
        for (int x = 0; x < 400; x++)
        {
            for (int y = 0; y < 130; y++)
            {
                float gradientValue = (float)y / 130f; // 위에서 아래로 그라데이션
                float alpha = Mathf.Lerp(0.3f, 0f, gradientValue);
                texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 400, 130), new Vector2(0.5f, 0.5f));
    }
} 