using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARPetGame
{
    public class UISetupHelper : MonoBehaviour
    {
        [Header("UI Elements to Setup")]
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI statusText;
        public Button startButton;
        public Image dimOverlay;
        
        [Header("Setup on Start")]
        public bool setupOnStart = true;
        
        private void Start()
        {
            if (setupOnStart)
            {
                SetupAllUIElements();
            }
        }
        
        public void SetupAllUIElements()
        {
            SetupScoreText();
            SetupStatusText();
            SetupStartButton();
            SetupDimOverlay();
        }
        
        private void SetupScoreText()
        {
            if (scoreText == null)
            {
                GameObject scoreObj = GameObject.Find("ScoreText");
                if (scoreObj != null)
                    scoreText = scoreObj.GetComponent<TextMeshProUGUI>();
            }
            
            if (scoreText != null)
            {
                scoreText.text = "Score: 0";
                scoreText.fontSize = 36;
                scoreText.color = Color.white;
                scoreText.alignment = TextAlignmentOptions.Center;
                
                RectTransform rectTransform = scoreText.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 1f);
                    rectTransform.anchorMax = new Vector2(0.5f, 1f);
                    rectTransform.anchoredPosition = new Vector2(0, -100);
                    rectTransform.sizeDelta = new Vector2(300, 60);
                }
            }
        }
        
        private void SetupStatusText()
        {
            if (statusText == null)
            {
                GameObject statusObj = GameObject.Find("StatusText");
                if (statusObj != null)
                    statusText = statusObj.GetComponent<TextMeshProUGUI>();
            }
            
            if (statusText != null)
            {
                statusText.text = "바닥을 스캔하세요";
                statusText.fontSize = 28;
                statusText.color = Color.white;
                statusText.alignment = TextAlignmentOptions.Center;
                
                RectTransform rectTransform = statusText.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = new Vector2(0, 0);
                    rectTransform.sizeDelta = new Vector2(400, 80);
                }
            }
        }
        
        private void SetupStartButton()
        {
            if (startButton == null)
            {
                GameObject buttonObj = GameObject.Find("StartButton");
                if (buttonObj != null)
                    startButton = buttonObj.GetComponent<Button>();
            }
            
            if (startButton != null)
            {
                RectTransform rectTransform = startButton.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0f);
                    rectTransform.anchoredPosition = new Vector2(0, 150);
                    rectTransform.sizeDelta = new Vector2(200, 80);
                }
                
                TextMeshProUGUI buttonText = startButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = "START";
                    buttonText.fontSize = 24;
                    buttonText.color = Color.white;
                    buttonText.alignment = TextAlignmentOptions.Center;
                    
                    RectTransform textRect = buttonText.GetComponent<RectTransform>();
                    if (textRect != null)
                    {
                        textRect.anchorMin = Vector2.zero;
                        textRect.anchorMax = Vector2.one;
                        textRect.offsetMin = Vector2.zero;
                        textRect.offsetMax = Vector2.zero;
                    }
                }
                
                startButton.gameObject.SetActive(false);
            }
        }
        
        private void SetupDimOverlay()
        {
            if (dimOverlay == null)
            {
                GameObject dimObj = GameObject.Find("DimOverlay");
                if (dimObj != null)
                    dimOverlay = dimObj.GetComponent<Image>();
            }
            
            if (dimOverlay != null)
            {
                dimOverlay.color = new Color(0, 0, 0, 0.7f);
                
                RectTransform rectTransform = dimOverlay.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                }
                
                dimOverlay.gameObject.SetActive(false);
            }
        }
    }
}