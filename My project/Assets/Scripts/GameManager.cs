using UnityEngine;

namespace ARPetGame
{
    public enum GamePhase
    {
        WaitingForBall,
        BallInPlay,
        PetHitting,
        PlayerTurn
    }
    
    public class GameManager : MonoBehaviour
    {
        [Header("Game Components")]
        public ARGameManager arGameManager;
        public ScoreManager scoreManager;
        public BallController ballController;
        
        [Header("Game Settings")]
        public float ballSpawnHeight = 2f;
        public int maxMissedBalls = 3;
        
        [Header("Game State")]
        public GamePhase currentPhase = GamePhase.WaitingForBall;
        public int missedBalls = 0;
        public bool isGameActive = false;
        
        public System.Action<GamePhase> OnGamePhaseChanged;
        public System.Action OnGameStarted;
        public System.Action OnGameEnded;
        
        private Camera arCamera;
        
        private void Start()
        {
            SetupComponents();
            ConnectEvents();
        }
        
        private void SetupComponents()
        {
            if (arGameManager == null)
                arGameManager = FindFirstObjectByType<ARGameManager>();
                
            if (scoreManager == null)
                scoreManager = FindFirstObjectByType<ScoreManager>();
                
            if (ballController == null)
                ballController = FindFirstObjectByType<BallController>();
                
            arCamera = Camera.main;
        }
        
        private void ConnectEvents()
        {
            if (arGameManager != null)
            {
                arGameManager.OnGameStateChanged += OnARGameStateChanged;
            }
            
            if (ballController != null)
            {
                ballController.OnBallHit += OnBallHit;
                ballController.OnBallMissed += OnBallMissedHandler;
            }
        }
        
        private void OnARGameStateChanged(GameState newState)
        {
            if (newState == GameState.Playing)
            {
                StartGame();
            }
            else if (newState == GameState.GameOver)
            {
                EndGame();
            }
        }
        
        public void StartGame()
        {
            if (isGameActive) return;
            
            isGameActive = true;
            missedBalls = 0;
            
            if (scoreManager != null)
                scoreManager.ResetScore();
            
            OnGameStarted?.Invoke();
            SpawnNewBall();
            
            Debug.Log("Game started!");
        }
        
        public void EndGame()
        {
            if (!isGameActive) return;
            
            isGameActive = false;
            
            if (scoreManager != null)
                scoreManager.SaveGameData();
            
            OnGameEnded?.Invoke();
            Debug.Log("Game ended!");
        }
        
        public void SpawnNewBall()
        {
            if (!isGameActive || ballController == null) return;
            
            GameObject petObject = GameObject.FindWithTag("Pet");
            if (petObject != null)
            {
                Vector3 spawnPos = petObject.transform.position + Vector3.up * ballSpawnHeight;
                ballController.transform.position = spawnPos;
                ballController.ResetBall();
                
                Vector3 direction = GetBallDirection();
                float speed = GetCurrentBallSpeed();
                ballController.LaunchBall(direction, speed);
            }
        }
        
        private Vector3 GetBallDirection()
        {
            if (arCamera == null) return Vector3.forward;
            
            GameObject pet = GameObject.FindWithTag("Pet");
            if (pet == null) return Vector3.forward;
            
            Vector3 dir = (arCamera.transform.position - pet.transform.position).normalized;
            dir.y = 0.2f;
            return dir.normalized;
        }
        
        private float GetCurrentBallSpeed()
        {
            float baseSpeed = ballController != null ? ballController.baseSpeed : 5f;
            float speedMultiplier = scoreManager != null ? scoreManager.GetCurrentSpeedMultiplier() : 1f;
            return baseSpeed * speedMultiplier;
        }
        
        private void OnBallHit(Vector3 hitPosition)
        {
            if (scoreManager != null)
            {
                scoreManager.AddScore();
            }
            
            Invoke(nameof(DelayedBallSpawn), 2f);
            Debug.Log("Ball hit!");
        }
        
        private void OnBallMissedHandler()
        {
            missedBalls++;
            
            if (scoreManager != null)
            {
                scoreManager.ResetCombo();
            }
            
            if (missedBalls >= maxMissedBalls)
            {
                EndGame();
            }
            else
            {
                Invoke(nameof(DelayedBallSpawn), 2f);
            }
            
            Debug.Log($"Ball missed! ({missedBalls}/{maxMissedBalls})");
        }
        
        private void DelayedBallSpawn()
        {
            if (isGameActive)
            {
                SpawnNewBall();
            }
        }
        
        public bool IsGameActive()
        {
            return isGameActive;
        }
        
        public int GetMissedBalls()
        {
            return missedBalls;
        }
        
        public int GetRemainingLives()
        {
            return Mathf.Max(0, maxMissedBalls - missedBalls);
        }
    }
}