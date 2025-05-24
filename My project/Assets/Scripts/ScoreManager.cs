using UnityEngine;

namespace ARPetGame
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Score Settings")]
        public int basePointsPerHit = 10;
        public int bonusPointsPerCombo = 5;
        public int maxComboMultiplier = 5;
        
        [Header("Score Tracking")]
        public int currentScore = 0;
        public int currentCombo = 0;
        public int highScore = 0;
        public int totalHits = 0;
        
        [Header("Round Management")]
        public int currentRound = 1;
        public int hitsRequiredPerRound = 10;
        public float roundSpeedIncrease = 0.2f;
        
        public System.Action<int> OnScoreChanged;
        public System.Action<int> OnComboChanged;
        public System.Action<int> OnRoundChanged;
        public System.Action<int> OnHighScoreAchieved;
        
        private void Start()
        {
            LoadHighScore();
            ResetScore();
        }
        
        public void AddScore(int hitNumber = 1)
        {
            int basePoints = basePointsPerHit * hitNumber;
            int comboBonus = currentCombo * bonusPointsPerCombo;
            int comboMultiplier = Mathf.Min(currentCombo / 3 + 1, maxComboMultiplier);
            
            int finalPoints = (basePoints + comboBonus) * comboMultiplier;
            
            currentScore += finalPoints;
            currentCombo++;
            totalHits++;
            
            OnScoreChanged?.Invoke(currentScore);
            OnComboChanged?.Invoke(currentCombo);
            
            CheckHighScore();
            CheckRoundProgression();
            
            Debug.Log($"Score added: {finalPoints} (Base: {basePoints}, Combo: {comboBonus}, Multiplier: x{comboMultiplier})");
        }
        
        public void ResetCombo()
        {
            if (currentCombo > 0)
            {
                Debug.Log($"Combo broken at {currentCombo}!");
                currentCombo = 0;
                OnComboChanged?.Invoke(currentCombo);
            }
        }
        
        public void ResetScore()
        {
            currentScore = 0;
            currentCombo = 0;
            totalHits = 0;
            currentRound = 1;
            
            OnScoreChanged?.Invoke(currentScore);
            OnComboChanged?.Invoke(currentCombo);
            OnRoundChanged?.Invoke(currentRound);
        }
        
        private void CheckHighScore()
        {
            if (currentScore > highScore)
            {
                highScore = currentScore;
                OnHighScoreAchieved?.Invoke(highScore);
                SaveHighScore();
                Debug.Log($"New high score: {highScore}!");
            }
        }
        
        private void CheckRoundProgression()
        {
            int requiredHits = currentRound * hitsRequiredPerRound;
            if (totalHits >= requiredHits)
            {
                currentRound++;
                OnRoundChanged?.Invoke(currentRound);
                Debug.Log($"Round {currentRound} reached! Speed increased.");
            }
        }
        
        public float GetCurrentSpeedMultiplier()
        {
            return 1f + (currentRound - 1) * roundSpeedIncrease;
        }
        
        public int GetPointsForHit()
        {
            int basePoints = basePointsPerHit;
            int comboBonus = currentCombo * bonusPointsPerCombo;
            int comboMultiplier = Mathf.Min(currentCombo / 3 + 1, maxComboMultiplier);
            
            return (basePoints + comboBonus) * comboMultiplier;
        }
        
        public string GetScoreText()
        {
            return $"Score: {currentScore}";
        }
        
        public string GetComboText()
        {
            return currentCombo > 0 ? $"Combo x{currentCombo}" : "";
        }
        
        public string GetRoundText()
        {
            return $"Round {currentRound}";
        }
        
        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("ARPetGame_HighScore", highScore);
            PlayerPrefs.Save();
        }
        
        private void LoadHighScore()
        {
            highScore = PlayerPrefs.GetInt("ARPetGame_HighScore", 0);
        }
        
        public void SaveGameData()
        {
            PlayerPrefs.SetInt("ARPetGame_HighScore", highScore);
            PlayerPrefs.SetInt("ARPetGame_LastScore", currentScore);
            PlayerPrefs.SetInt("ARPetGame_LastRound", currentRound);
            PlayerPrefs.Save();
        }
    }
}