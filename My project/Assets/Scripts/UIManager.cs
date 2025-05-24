using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ARPetGame
{
    public class UIManager : MonoBehaviour
    {
        [Header("UI Panels")]
        public GameObject scanningPanel;
        public GameObject gameReadyPanel;
        public GameObject gamePlayingPanel;
        
        [Header("UI Elements")]
        public Button startButton;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI statusText;
        public Image dimOverlay;
        
        [Header("Settings")]
        public float dimAlpha = 0.7f;
        
        private ARGameManager gameManager;
        private int currentScore = 0;
        
        public System.Action OnStartButtonPressed;
        
        private void Start()
        {
            gameManager = FindObjectOfType<ARGameManager>();
            
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
            }
            
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartPressed);
            }
            
            UpdateScore(0);
            SetDimOverlay(false);
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Scanning:
                    ShowScanningUI();
                    break;
                case GameState.GameReady:
                    ShowGameReadyUI();
                    break;
                case GameState.Playing:
                    ShowGamePlayingUI();
                    break;
            }
        }
        
        private void ShowScanningUI()
        {
            SetAllPanelsActive(false);
            if (scanningPanel != null) scanningPanel.SetActive(true);
            SetStatusText("바닥을 스캔하세요");
            SetDimOverlay(false);
        }
        
        private void ShowGameReadyUI()
        {
            SetAllPanelsActive(false);
            if (gameReadyPanel != null) gameReadyPanel.SetActive(true);
            SetStatusText("게임 준비 완료!");
            
            if (startButton != null)
            {
                startButton.gameObject.SetActive(true);
                startButton.interactable = true;
            }
            
            SetDimOverlay(true);
        }
        
        private void ShowGamePlayingUI()
        {
            SetAllPanelsActive(false);
            if (gamePlayingPanel != null) gamePlayingPanel.SetActive(true);
            SetStatusText("게임 플레이 중");
            
            if (startButton != null)
            {
                startButton.gameObject.SetActive(false);
            }
            
            SetDimOverlay(false);
        }
        
        private void SetAllPanelsActive(bool active)
        {
            if (scanningPanel != null) scanningPanel.SetActive(active);
            if (gameReadyPanel != null) gameReadyPanel.SetActive(active);
            if (gamePlayingPanel != null) gamePlayingPanel.SetActive(active);
        }
        
        private void SetStatusText(string text)
        {
            if (statusText != null)
                statusText.text = text;
        }
        
        public void UpdateScore(int score)
        {
            currentScore = score;
            if (scoreText != null)
                scoreText.text = $"Score: {score}";
        }
        
        private void SetDimOverlay(bool enabled)
        {
            if (dimOverlay != null)
            {
                dimOverlay.gameObject.SetActive(enabled);
                if (enabled)
                {
                    Color color = dimOverlay.color;
                    color.a = dimAlpha;
                    dimOverlay.color = color;
                }
            }
        }
        
        private void OnStartPressed()
        {
            OnStartButtonPressed?.Invoke();
            
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }
    }
}