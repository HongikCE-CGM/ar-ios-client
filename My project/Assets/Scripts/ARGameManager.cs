using UnityEngine;

namespace ARPetGame
{
    public enum GameState
    {
        Scanning,
        PlaneDetected,
        PetPlaced,
        GameReady,
        Playing,
        GameOver
    }
    
    public class ARGameManager : MonoBehaviour
    {
        [Header("Game State")]
        public GameState currentState = GameState.Scanning;
        
        [Header("Components")]
        public ImprovedARPlaneDetector planeDetector;
        public PetPlacer petPlacer;
        
        [Header("UI Elements")]
        public GameObject scanningUI;
        public GameObject planeDetectedUI;
        public GameObject gameReadyUI;
        
        [Header("Game Settings")]
        public bool debugMode = true;
        
        public System.Action<GameState> OnGameStateChanged;
        
        private void Start()
        {
            SetupComponents();
            SetGameState(GameState.Scanning);
        }
        
        private void SetupComponents()
        {
            if (planeDetector == null)
                planeDetector = FindFirstObjectByType<ImprovedARPlaneDetector>();
                
            if (petPlacer == null)
                petPlacer = FindFirstObjectByType<PetPlacer>();
            
            if (planeDetector != null)
            {
                planeDetector.OnValidPlaneDetected += OnPlaneDetected;
                planeDetector.OnValidPlaneLost += OnPlaneLost;
            }
            
            if (petPlacer != null)
            {
                petPlacer.OnPlacementReady += OnPlacementReady;
                petPlacer.OnPetPlaced += OnPetPlaced;
            }
        }
        
        private void OnPlaneDetected(Vector3 position, Quaternion rotation)
        {
            if (currentState == GameState.Scanning)
            {
                SetGameState(GameState.PlaneDetected);
            }
        }
        
        private void OnPlaneLost()
        {
            if (currentState == GameState.PlaneDetected)
            {
                SetGameState(GameState.Scanning);
            }
        }
        
        private void OnPlacementReady()
        {
            Debug.Log("Placement ready - waiting for pet placement");
        }
        
        private void OnPetPlaced(GameObject pet)
        {
            SetGameState(GameState.PetPlaced);
            Debug.Log("Pet placed - transitioning to game ready");
            
            Invoke(nameof(SetGameReady), 1f);
        }
        
        private void SetGameReady()
        {
            SetGameState(GameState.GameReady);
        }
        
        public void StartGame()
        {
            if (currentState == GameState.GameReady)
            {
                SetGameState(GameState.Playing);
                Debug.Log("Game started!");
            }
        }
        
        public void EndGame()
        {
            SetGameState(GameState.GameOver);
        }
        
        public void ResetGame()
        {
            if (petPlacer != null)
            {
                // Reset placement if needed
            }
            
            SetGameState(GameState.Scanning);
        }
        
        private void SetGameState(GameState newState)
        {
            if (currentState != newState)
            {
                GameState previousState = currentState;
                currentState = newState;
                
                UpdateUI();
                OnGameStateChanged?.Invoke(newState);
                
                if (debugMode)
                {
                    Debug.Log($"Game State: {previousState} -> {newState}");
                }
            }
        }
        
        private void UpdateUI()
        {
            if (scanningUI != null)
                scanningUI.SetActive(currentState == GameState.Scanning);
                
            if (planeDetectedUI != null)
                planeDetectedUI.SetActive(currentState == GameState.PlaneDetected);
                
            if (gameReadyUI != null)
                gameReadyUI.SetActive(currentState == GameState.GameReady);
        }
        
        public bool IsGamePlaying()
        {
            return currentState == GameState.Playing;
        }
        
        public bool IsPetPlaced()
        {
            return currentState >= GameState.PetPlaced;
        }
    }
}