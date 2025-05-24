using UnityEngine;

namespace ARPetGame
{
    public class GameUIController : MonoBehaviour
    {
        [Header("UI Components")]
        public UIManager uiManager;
        public ScreenDimmer screenDimmer;
        
        [Header("Game Components")]
        public ARGameManager gameManager;
        public PetPlacer petPlacer;
        
        private void Start()
        {
            SetupComponents();
            ConnectEvents();
        }
        
        private void SetupComponents()
        {
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();
                
            if (screenDimmer == null)
                screenDimmer = FindObjectOfType<ScreenDimmer>();
                
            if (gameManager == null)
                gameManager = FindObjectOfType<ARGameManager>();
                
            if (petPlacer == null)
                petPlacer = FindObjectOfType<PetPlacer>();
        }
        
        private void ConnectEvents()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged += OnGameStateChanged;
            }
            
            if (petPlacer != null)
            {
                petPlacer.OnPetPlaced += OnPetPlaced;
                petPlacer.OnPlacementReady += OnPlacementReady;
            }
            
            if (uiManager != null)
            {
                uiManager.OnStartButtonPressed += OnStartButtonPressed;
            }
        }
        
        private void OnDestroy()
        {
            if (gameManager != null)
            {
                gameManager.OnGameStateChanged -= OnGameStateChanged;
            }
            
            if (petPlacer != null)
            {
                petPlacer.OnPetPlaced -= OnPetPlaced;
                petPlacer.OnPlacementReady -= OnPlacementReady;
            }
            
            if (uiManager != null)
            {
                uiManager.OnStartButtonPressed -= OnStartButtonPressed;
            }
        }
        
        private void OnGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.Scanning:
                    if (screenDimmer != null)
                        screenDimmer.UndimScreen();
                    break;
                    
                case GameState.GameReady:
                    if (screenDimmer != null)
                        screenDimmer.DimScreen();
                    break;
                    
                case GameState.Playing:
                    if (screenDimmer != null)
                        screenDimmer.UndimScreen();
                    break;
            }
        }
        
        private void OnPetPlaced(GameObject pet)
        {
            if (screenDimmer != null)
            {
                screenDimmer.DimScreen();
            }
            
            Debug.Log("Pet placed - dimming screen");
        }
        
        private void OnPlacementReady()
        {
            Debug.Log("Placement ready for UI");
        }
        
        private void OnStartButtonPressed()
        {
            Debug.Log("Start button pressed - beginning game");
        }
        
        public void UpdateGameScore(int score)
        {
            if (uiManager != null)
            {
                uiManager.UpdateScore(score);
            }
        }
        
        public void AddGameScore(int points)
        {
            if (uiManager != null)
            {
                uiManager.AddScore(points);
            }
        }
    }
}